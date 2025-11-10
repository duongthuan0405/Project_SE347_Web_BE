using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Work.Database.Entity;
using se347_be.Work.DTOs.AI;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Interfaces;

namespace se347_be.Work.Services.Implementations
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly IQuestionBankRepository _questionBankRepo;
        private readonly MyAppDbContext _context;
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        public GeminiAIService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<GeminiAIService> logger,
            IQuestionBankRepository questionBankRepo,
            MyAppDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _questionBankRepo = questionBankRepo;
            _context = context;
        }

        public async Task<GenerateQuizResponseDTO> GenerateQuestionsFromTextAsync(
            string textContent, 
            string fileName,
            int numberOfQuestions, 
            string? additionalInstructions = null)
        {
            var apiKey = _configuration["GEMINI_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("GEMINI_API_KEY is not configured");
            }

            var prompt = BuildPrompt(textContent, numberOfQuestions, additionalInstructions);
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 8192
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync(
                    $"{GEMINI_API_URL}?key={apiKey}",
                    jsonContent
                );

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                
                return ParseGeminiResponse(responseContent, fileName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to call Gemini API");
                throw new InvalidOperationException("Failed to generate questions from AI", ex);
            }
        }

        private string BuildPrompt(string textContent, int numberOfQuestions, string? additionalInstructions)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are an expert teacher creating a multiple-choice quiz.");
            prompt.AppendLine($"Based on the following text, generate EXACTLY {numberOfQuestions} multiple-choice questions.");
            prompt.AppendLine();
            prompt.AppendLine("Requirements:");
            prompt.AppendLine("- Each question must have exactly 4 answer options (A, B, C, D)");
            prompt.AppendLine("- Mark the correct answer clearly");
            prompt.AppendLine("- Questions should cover different aspects of the content");
            prompt.AppendLine("- Make questions clear and unambiguous");
            prompt.AppendLine();

            if (!string.IsNullOrWhiteSpace(additionalInstructions))
            {
                prompt.AppendLine($"Additional instructions: {additionalInstructions}");
                prompt.AppendLine();
            }

            prompt.AppendLine("Format your response EXACTLY like this (use JSON format):");
            prompt.AppendLine("[");
            prompt.AppendLine("  {");
            prompt.AppendLine("    \"question\": \"What is...?\",");
            prompt.AppendLine("    \"answers\": [");
            prompt.AppendLine("      { \"content\": \"Option A\", \"isCorrect\": false },");
            prompt.AppendLine("      { \"content\": \"Option B\", \"isCorrect\": true },");
            prompt.AppendLine("      { \"content\": \"Option C\", \"isCorrect\": false },");
            prompt.AppendLine("      { \"content\": \"Option D\", \"isCorrect\": false }");
            prompt.AppendLine("    ]");
            prompt.AppendLine("  }");
            prompt.AppendLine("]");
            prompt.AppendLine();
            prompt.AppendLine("Text content to analyze:");
            prompt.AppendLine("---");
            prompt.AppendLine(textContent);
            prompt.AppendLine("---");

            return prompt.ToString();
        }

        private GenerateQuizResponseDTO ParseGeminiResponse(string responseContent, string fileName)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                var candidates = jsonDoc.RootElement.GetProperty("candidates");
                
                if (candidates.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException("No response from Gemini API");
                }

                var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(text))
                {
                    throw new InvalidOperationException("Empty response from Gemini API");
                }

                // Extract JSON array from markdown code blocks if present
                var jsonMatch = Regex.Match(text, @"```(?:json)?\s*(\[[\s\S]*?\])\s*```", RegexOptions.Multiline);
                var jsonText = jsonMatch.Success ? jsonMatch.Groups[1].Value : text;

                // Remove any leading/trailing whitespace and try to find JSON array
                jsonText = jsonText.Trim();
                if (!jsonText.StartsWith("["))
                {
                    var arrayStart = jsonText.IndexOf('[');
                    if (arrayStart >= 0)
                    {
                        jsonText = jsonText.Substring(arrayStart);
                    }
                }

                var questions = JsonSerializer.Deserialize<List<GeneratedQuestionDTO>>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (questions == null || questions.Count == 0)
                {
                    throw new InvalidOperationException("Failed to parse questions from AI response");
                }

                return new GenerateQuizResponseDTO
                {
                    Questions = questions,
                    SourceFileName = fileName,
                    GeneratedCount = questions.Count
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini response as JSON");
                throw new InvalidOperationException("Failed to parse AI response. The AI might have returned an invalid format.", ex);
            }
        }

        public async Task<GenerateAndSaveQuizResponseDTO> GenerateAndSaveQuestionsAsync(
            Guid quizId,
            string textContent,
            string fileName,
            int numberOfQuestions,
            Guid creatorId,
            string? additionalInstructions = null)
        {
            _logger.LogInformation("Generating and saving {Count} questions for quiz {QuizId}", numberOfQuestions, quizId);

            // Step 1: Generate questions using AI
            var aiResponse = await GenerateQuestionsFromTextAsync(
                textContent,
                fileName,
                numberOfQuestions,
                additionalInstructions);

            if (!aiResponse.Questions.Any())
            {
                throw new InvalidOperationException("AI did not generate any questions");
            }

            var savedQuestionIds = new List<Guid>();

            // Step 2: Save each generated question to Question Bank
            foreach (var generatedQ in aiResponse.Questions)
            {
                try
                {
                    // Validate at least one correct answer
                    if (!generatedQ.Answers.Any(a => a.IsCorrect))
                    {
                        _logger.LogWarning("Skipping question without correct answer: {Question}", generatedQ.Question);
                        continue;
                    }

                    // Create Question entity
                    var question = new Question
                    {
                        Id = Guid.NewGuid(),
                        Content = generatedQ.Question,
                        Points = generatedQ.Points,
                        CreatorId = creatorId,
                        Category = "AI Generated", // Default category
                        IsDraft = false // Not draft, ready to use
                    };

                    // Create Answer entities
                    var answers = generatedQ.Answers.Select(a => new Answer
                    {
                        Id = Guid.NewGuid(),
                        Content = a.Content,
                        IsCorrectAnswer = a.IsCorrect,
                        QuestionId = question.Id
                    }).ToList();

                    question.Answers = answers;

                    // Save to Question Bank
                    await _questionBankRepo.CreateAsync(question);

                    // Step 3: Auto-link question to quiz via QuizQuestion
                    var maxOrder = await _context.QuizQuestions
                        .Where(qq => qq.QuizId == quizId)
                        .MaxAsync(qq => (int?)qq.OrderIndex) ?? 0;

                    var quizQuestion = new QuizQuestion
                    {
                        QuizId = quizId,
                        QuestionId = question.Id,
                        OrderIndex = maxOrder + 1
                    };

                    _context.QuizQuestions.Add(quizQuestion);
                    await _context.SaveChangesAsync();

                    savedQuestionIds.Add(question.Id);

                    _logger.LogInformation("Saved question {QuestionId} to bank and linked to quiz {QuizId}", question.Id, quizId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save question: {Question}", generatedQ.Question);
                    // Continue with next question
                }
            }

            if (!savedQuestionIds.Any())
            {
                throw new InvalidOperationException("Failed to save any questions. Check logs for details.");
            }

            return new GenerateAndSaveQuizResponseDTO
            {
                QuestionIds = savedQuestionIds,
                SavedCount = savedQuestionIds.Count,
                SourceFileName = fileName,
                Message = $"Successfully generated and saved {savedQuestionIds.Count} question(s) to Question Bank and added to quiz"
            };
        }
    }
}

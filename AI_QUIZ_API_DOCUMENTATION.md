# 🤖 AI Quiz Generation API Documentation

## Overview

Hệ thống AI Quiz Generation cho phép tạo quiz tự động từ tài liệu (PDF, DOCX, TXT) sử dụng Google Gemini AI.

### Features
- ✅ Upload tài liệu (PDF, DOCX, TXT)
- ✅ AI tự động tạo câu hỏi từ tài liệu
- ✅ Questions có category = "AI"
- ✅ Auto-save vào Question Bank khi publish quiz
- ✅ Hỗ trợ tiếng Việt

---

## 🔄 Complete Workflow

```
1. Create Quiz (Empty)
   POST /api/Quiz
   
2. Upload Document
   POST /api/Quiz/{quizId}/Document/upload
   
3. Generate Questions from Document
   POST /api/Quiz/{quizId}/generate-questions-from-document
   
4. Review Quiz
   GET /api/Quiz/{quizId}
   
5. Publish Quiz (Auto-save to bank)
   PUT /api/Quiz/{quizId}
   { "isPublish": true }
   
6. Verify in Question Bank
   GET /api/QuestionBank?category=AI
```

---

## 📋 API Endpoints

### 1. Create Quiz

**Endpoint:** `POST /api/Quiz`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "My AI Generated Quiz",
  "description": "Quiz created from document",
  "maxTimesCanAttempt": 3,
  "isPublish": false,
  "showScoreAfterSubmission": true,
  "scoringMode": "Standard"
}
```

**Response:** `200 OK`
```json
{
  "id": "quiz-guid",
  "title": "My AI Generated Quiz",
  "description": "Quiz created from document",
  "createAt": "2025-11-11T00:00:00",
  "maxTimesCanAttempt": 3,
  "isPublish": false,
  "accessCode": "ABC123",
  ...
}
```

---

### 2. Upload Document to Quiz

**Endpoint:** `POST /api/Quiz/{quizId}/Document/upload`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Request:**
- Form field: `file` (File)
- Supported types: `.pdf`, `.docx`, `.txt`
- Max size: Check your server configuration

**Response:** `200 OK`
```json
{
  "id": "doc-guid",
  "quizId": "quiz-guid",
  "fileName": "document.pdf",
  "storageUrl": "C:\\path\\to\\file.pdf",
  "status": "Uploaded",
  "uploadAt": "2025-11-11T00:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid file type or empty file
- `404 Not Found`: Quiz not found
- `401 Unauthorized`: Invalid token

---

### 3. Generate Questions from Document

**Endpoint:** `POST /api/Quiz/{quizId}/generate-questions-from-document`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "documentId": "doc-guid",
  "numberOfQuestions": 10,
  "additionalInstructions": "Generate questions in Vietnamese about business analysis"
}
```

**Parameters:**
- `documentId` (required): ID of uploaded document
- `numberOfQuestions` (required): 1-50 questions
- `additionalInstructions` (optional): Additional context for AI

**Response:** `200 OK`
```json
{
  "id": "quiz-guid",
  "title": "My AI Generated Quiz",
  "questions": [
    {
      "id": "question-guid",
      "content": "What is business analysis?",
      "quizId": "quiz-guid",
      "answers": [
        {
          "id": "answer-guid-1",
          "content": "A) Process of identifying business needs",
          "isCorrectAnswer": true
        },
        {
          "id": "answer-guid-2",
          "content": "B) Financial accounting",
          "isCorrectAnswer": false
        },
        {
          "id": "answer-guid-3",
          "content": "C) Marketing strategy",
          "isCorrectAnswer": false
        },
        {
          "id": "answer-guid-4",
          "content": "D) Human resources management",
          "isCorrectAnswer": false
        }
      ]
    }
  ]
}
```

**Notes:**
- Questions are created with `category = "AI"`
- Questions are linked to quiz but NOT saved to Question Bank yet
- `QuestionsSavedToBank = false` at this point

**Error Responses:**
- `400 Bad Request`: Invalid document ID or parameters
- `404 Not Found`: Quiz or document not found
- `500 Internal Server Error`: AI generation failed

**Common AI Errors:**
```json
{
  "message": "AI Generation failed: Content was blocked by Google's safety filters. Please try:\n- Using more neutral language\n- Avoiding sensitive topics\n- Simplifying your additional instructions"
}
```

---

### 4. Get Quiz Details

**Endpoint:** `GET /api/Quiz/{quizId}`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "id": "quiz-guid",
  "title": "My AI Generated Quiz",
  "questions": [...],
  "createAt": "2025-11-11T00:00:00",
  "isPublish": false,
  ...
}
```

---

### 5. Publish Quiz (Auto-save to Bank)

**Endpoint:** `PUT /api/Quiz/{quizId}`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "isPublish": true
}
```

**Response:** `200 OK`
```json
{
  "id": "quiz-guid",
  "isPublish": true,
  ...
}
```

**Backend Logic:**
```csharp
// Auto-save logic trong UpdateQuizAsync
if (quiz.IsPublish && wasUnpublished && !quiz.QuestionsSavedToBank)
{
    // Automatically save all AI questions to Question Bank
    await SaveQuizQuestionsToBankAsync(quizId, creatorId);
    quiz.QuestionsSavedToBank = true;
}
```

**What happens:**
1. ✅ Quiz is published
2. ✅ All questions automatically saved to Question Bank
3. ✅ Questions can be reused in other quizzes
4. ✅ `QuestionsSavedToBank` flag set to `true`

---

### 6. Verify Questions in Bank

**Endpoint:** `GET /api/QuestionBank?category=AI`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "questions": [
    {
      "id": "question-guid",
      "content": "What is business analysis?",
      "category": "AI",
      "points": 1,
      "creatorId": "user-guid",
      "answers": [...]
    }
  ]
}
```

---

### 7. Get Quiz Documents

**Endpoint:** `GET /api/Quiz/{quizId}/Document`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
[
  {
    "id": "doc-guid",
    "quizId": "quiz-guid",
    "fileName": "document.pdf",
    "storageUrl": "C:\\path\\to\\file.pdf",
    "status": "Uploaded",
    "uploadAt": "2025-11-11T00:00:00Z"
  }
]
```

---

### 8. Delete Document

**Endpoint:** `DELETE /api/Quiz/{quizId}/Document/{documentId}`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "message": "Document deleted successfully"
}
```

---

### 9. Manual Save to Bank (Optional)

**Endpoint:** `POST /api/Quiz/{quizId}/save-to-bank`

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "message": "Questions saved to bank successfully"
}
```

**Use Case:**
- Muốn save questions without publishing quiz
- Already published but want to ensure questions are in bank

---

## 🧪 Test Scenarios

### Scenario 1: Full Flow (Standard)

```bash
# 1. Create Quiz
POST /api/Quiz
{
  "title": "AI Quiz Test",
  "maxTimesCanAttempt": 3
}
→ quizId: "abc-123"

# 2. Upload Document
POST /api/Quiz/abc-123/Document/upload
[file: document.pdf]
→ documentId: "doc-456"

# 3. Generate Questions
POST /api/Quiz/abc-123/generate-questions-from-document
{
  "documentId": "doc-456",
  "numberOfQuestions": 5
}
→ 5 questions created (category = "AI")

# 4. Publish (Auto-save)
PUT /api/Quiz/abc-123
{
  "isPublish": true
}
→ Questions saved to bank automatically

# 5. Verify
GET /api/QuestionBank?category=AI
→ Should see 5 questions
```

---

### Scenario 2: Vietnamese Questions

```bash
POST /api/Quiz/abc-123/generate-questions-from-document
{
  "documentId": "doc-456",
  "numberOfQuestions": 10,
  "additionalInstructions": "Generate questions in Vietnamese about business analysis and IT features"
}
```

**Expected:**
- Questions in Vietnamese
- Focused on business analysis topics
- 4 answers per question

---

### Scenario 3: Multiple Documents

```bash
# Upload multiple documents to same quiz
POST /api/Quiz/abc-123/Document/upload
[file: doc1.pdf]

POST /api/Quiz/abc-123/Document/upload
[file: doc2.pdf]

# Generate from first document
POST /api/Quiz/abc-123/generate-questions-from-document
{
  "documentId": "doc1-guid",
  "numberOfQuestions": 5
}

# Generate from second document (adds to existing)
POST /api/Quiz/abc-123/generate-questions-from-document
{
  "documentId": "doc2-guid",
  "numberOfQuestions": 5
}

# Total: 10 questions
GET /api/Quiz/abc-123
→ Shows all 10 questions
```

---

## ⚙️ Configuration

### File Upload Settings

```json
// appsettings.json
{
  "FileUpload": {
    "Path": "uploads/documents",
    "MaxSize": 10485760  // 10MB
  }
}
```

### Gemini AI Settings

```env
# .env file
GEMINI_API_KEY=your_api_key_here
```

**Current Model:** `gemini-2.5-flash`

---

## 🔒 Security

### Authentication
All endpoints require JWT Bearer token:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Authorization
- Users can only access their own quizzes
- Document must belong to quiz
- Quiz must be owned by authenticated user

---

## 🐛 Error Handling

### Common Errors

#### 1. Document Not Found
```json
{
  "message": "Document not found or does not belong to this quiz"
}
```

**Solution:** Verify documentId and quizId match

---

#### 2. AI Generation Failed - Safety Block
```json
{
  "message": "AI Generation failed: Content was blocked by Google's safety filters. Please try:\n- Using more neutral language\n- Avoiding sensitive topics\n- Simplifying your additional instructions"
}
```

**Solutions:**
- Remove sensitive keywords
- Simplify additional instructions
- Use more neutral language

---

#### 3. Empty Sequence Error
```json
{
  "message": "Sequence contains no elements"
}
```

**Cause:** Fixed in latest version (check empty list before Max())

---

#### 4. File Type Not Supported
```json
{
  "message": "Unsupported file type. Supported: .txt, .pdf, .docx"
}
```

**Solution:** Convert file to supported format

---

## 📊 Database Schema

### QuizSourceDocument Table
```sql
CREATE TABLE QuizSourceDocument (
  Id uuid PRIMARY KEY,
  QuizId uuid NOT NULL,  -- FK to Quiz
  FileName varchar NOT NULL,
  StorageUrl varchar NOT NULL,
  Status varchar DEFAULT 'Uploaded',
  UploadAt timestamp
);
```

### Question Table (AI Questions)
```sql
CREATE TABLE Question (
  Id uuid PRIMARY KEY,
  Content varchar NOT NULL,
  Points int DEFAULT 1,
  CreatorId uuid,
  Category varchar,  -- "AI" for AI-generated
  IsDraft boolean DEFAULT false
);
```

### Quiz Table (New Fields)
```sql
ALTER TABLE Quiz ADD COLUMN QuestionsSavedToBank boolean DEFAULT false;
```

---

## 🚀 Performance Tips

### 1. Document Size
- Recommended: < 5MB
- Max: 10MB
- Large files may timeout

### 2. Number of Questions
- Recommended: 5-10 questions
- Max: 50 questions
- More questions = longer generation time

### 3. Additional Instructions
- Keep it concise (< 200 characters)
- Be specific but not too detailed
- Avoid sensitive topics

---

## 📈 Monitoring

### Logs to Check

```bash
# Successful generation
info: GeminiAIService[0]
      Gemini API call successful, parsing response...
info: GeminiAIService[0]
      Gemini finish reason: STOP

# Safety block
warning: GeminiAIService[0]
         Gemini finish reason: SAFETY
```

---

## 🔄 Migration Guide

### From Old Flow to New Flow

**Old Flow (Deprecated):**
```
POST /api/Quiz/{quizId}/AIQuiz/generate
→ Upload + Generate in one step
→ Questions saved to bank immediately
```

**New Flow (Current):**
```
1. POST /api/Quiz/{quizId}/Document/upload
2. POST /api/Quiz/{quizId}/generate-questions-from-document
3. PUT /api/Quiz/{quizId} { "isPublish": true }
→ Questions saved to bank on publish
```

**Benefits:**
- ✅ Follows database schema (QuizSourceDocument)
- ✅ Separate upload and generation
- ✅ Better control over when questions go to bank
- ✅ Can generate from multiple documents

---

## 📞 Support

### Common Issues

**Q: Questions generated in wrong language?**
A: Add language instruction in `additionalInstructions`:
```json
{
  "additionalInstructions": "Generate questions in Vietnamese"
}
```

**Q: Questions not in Question Bank after publish?**
A: Check `QuestionsSavedToBank` flag in database. If false, manually call:
```
POST /api/Quiz/{quizId}/save-to-bank
```

**Q: AI generation timeout?**
A: Reduce number of questions or simplify document content.

---

## 📝 Change Log

### Version 2.0 (Current)
- ✅ Added DocumentController
- ✅ Separate upload and generation
- ✅ Auto-save on publish
- ✅ Support for PDF/DOCX extraction
- ✅ Better error handling
- ✅ Safety filter detection

### Version 1.0 (Deprecated)
- Upload + Generate in one step
- Immediate save to bank
- No document management

---

## 🎯 Frontend Integration Guide

### React Example

```typescript
// 1. Create Quiz
const createQuiz = async () => {
  const response = await fetch('/api/Quiz', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'AI Quiz',
      maxTimesCanAttempt: 3
    })
  });
  const quiz = await response.json();
  return quiz.id;
};

// 2. Upload Document
const uploadDocument = async (quizId, file) => {
  const formData = new FormData();
  formData.append('file', file);
  
  const response = await fetch(`/api/Quiz/${quizId}/Document/upload`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  const doc = await response.json();
  return doc.id;
};

// 3. Generate Questions
const generateQuestions = async (quizId, documentId) => {
  const response = await fetch(
    `/api/Quiz/${quizId}/generate-questions-from-document`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        documentId,
        numberOfQuestions: 10,
        additionalInstructions: 'Vietnamese questions'
      })
    }
  );
  return await response.json();
};

// 4. Publish Quiz
const publishQuiz = async (quizId) => {
  await fetch(`/api/Quiz/${quizId}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      isPublish: true
    })
  });
};
```

---

## 🎉 Success Criteria

✅ Document uploaded successfully  
✅ Questions generated with meaningful content  
✅ Questions have category = "AI"  
✅ Questions linked to quiz  
✅ Quiz can be published  
✅ Questions appear in Question Bank after publish  
✅ Questions can be reused in other quizzes

---

**Last Updated:** November 11, 2025  
**API Version:** 2.0  
**Gemini Model:** gemini-2.5-flash

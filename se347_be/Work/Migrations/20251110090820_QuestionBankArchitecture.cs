using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace se347_be.Migrations
{
    /// <inheritdoc />
    public partial class QuestionBankArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // IDEMPOTENT MIGRATION - Check before create/add
            // Based on actual database schema provided
            // ============================================
            
            // 1. Add IsDraft column to Question if it doesn't exist (MISSING IN SCHEMA)
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Question' 
                        AND column_name = 'IsDraft'
                    ) THEN
                        ALTER TABLE ""Question"" ADD COLUMN ""IsDraft"" boolean NOT NULL DEFAULT false;
                    END IF;
                END $$;
            ");

            // 2. Add AccessType column to Quiz if it doesn't exist (MISSING IN SCHEMA)
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Quiz' 
                        AND column_name = 'AccessType'
                    ) THEN
                        ALTER TABLE ""Quiz"" ADD COLUMN ""AccessType"" varchar(20) NOT NULL DEFAULT 'Public';
                    END IF;
                END $$;
            ");

            // 3. Create QuizInvitation table if it doesn't exist (NOT IN SCHEMA)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""QuizInvitation"" (
                    ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
                    ""QuizId"" uuid NOT NULL,
                    ""Email"" varchar(255) NOT NULL,
                    ""StudentId"" varchar(100),
                    ""FullName"" varchar(255),
                    ""InvitedAt"" timestamp NOT NULL DEFAULT now(),
                    CONSTRAINT ""QuizInvitation_pkey"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_QuizInvitation_Quiz_QuizId"" FOREIGN KEY (""QuizId"") REFERENCES ""Quiz""(""Id"") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ""IX_QuizInvitation_QuizId"" ON ""QuizInvitation""(""QuizId"");
            ");

            // 4. Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AnswerSelection_ParticipationId_AnswerId"" 
                ON ""AnswerSelection""(""ParticipationId"", ""AnswerId"");
            ");

            // Note: Skipping IX_QuizParticipation_QuizId_StudentId unique index
            // Database may have duplicate entries (multiple attempts allowed)

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Answer_QuestionId"" ON ""Answer""(""QuestionId"");
                CREATE INDEX IF NOT EXISTS ""IX_AnswerSelection_AnswerId"" ON ""AnswerSelection""(""AnswerId"");
                CREATE INDEX IF NOT EXISTS ""IX_AnswerSelection_ParticipationId"" ON ""AnswerSelection""(""ParticipationId"");
                CREATE INDEX IF NOT EXISTS ""IX_Participant_ListId"" ON ""Participant""(""ListId"");
                CREATE INDEX IF NOT EXISTS ""IX_ParticipantList_CreatorId"" ON ""ParticipantList""(""CreatorId"");
                CREATE INDEX IF NOT EXISTS ""IX_Question_CreatorId"" ON ""Question""(""CreatorId"");
                CREATE INDEX IF NOT EXISTS ""IX_Quiz_CreatorId"" ON ""Quiz""(""CreatorId"");
                CREATE INDEX IF NOT EXISTS ""IX_QuizParticipation_QuizId"" ON ""QuizParticipation""(""QuizId"");
                CREATE INDEX IF NOT EXISTS ""IX_QuizQuestion_QuizId_QuestionId"" ON ""QuizQuestion""(""QuizId"", ""QuestionId"");
                CREATE INDEX IF NOT EXISTS ""IX_QuizQuestion_QuestionId"" ON ""QuizQuestion""(""QuestionId"");
                CREATE INDEX IF NOT EXISTS ""IX_QuizSourceDocument_QuizId"" ON ""QuizSourceDocument""(""QuizId"");
                CREATE INDEX IF NOT EXISTS ""IX_UserProfile_Id"" ON ""UserProfile""(""Id"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_User_CreatorId",
                table: "Question");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "QuizInvitation");

            migrationBuilder.DropTable(
                name: "QuizQuestion");

            migrationBuilder.DropTable(
                name: "QuizSourceDocument");

            migrationBuilder.DropTable(
                name: "ParticipantList");

            migrationBuilder.DropIndex(
                name: "IX_QuizParticipation_QuizId_StudentId",
                table: "QuizParticipation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnswerSelection",
                table: "AnswerSelection");

            migrationBuilder.DropIndex(
                name: "IX_AnswerSelection_AnswerId",
                table: "AnswerSelection");

            migrationBuilder.DropIndex(
                name: "IX_AnswerSelection_ParticipationId_AnswerId",
                table: "AnswerSelection");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "QuizParticipation");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizParticipation");

            migrationBuilder.DropColumn(
                name: "ShuffledAnswersJson",
                table: "QuizParticipation");

            migrationBuilder.DropColumn(
                name: "ShuffledQuestionsJson",
                table: "QuizParticipation");

            migrationBuilder.DropColumn(
                name: "AccessCode",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "AllowNavigationBack",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "DurationInMinutes",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "PresentationMode",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "SendResultEmail",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAnswersMode",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "ShowScoreAfterSubmission",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AnswerSelection");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Question",
                newName: "QuizId");

            migrationBuilder.RenameIndex(
                name: "IX_Question_CreatorId",
                table: "Question",
                newName: "IX_Question_QuizId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnswerSelection",
                table: "AnswerSelection",
                columns: new[] { "AnswerId", "ParticipationId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizParticipation_QuizId",
                table: "QuizParticipation",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSelection_ParticipationId",
                table: "AnswerSelection",
                column: "ParticipationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Quiz_QuizId",
                table: "Question",
                column: "QuizId",
                principalTable: "Quiz",
                principalColumn: "Id");
        }
    }
}

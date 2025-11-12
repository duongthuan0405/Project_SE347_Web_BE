using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace se347_be.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionsSavedToBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add QuestionsSavedToBank field to Quiz table
            migrationBuilder.AddColumn<bool>(
                name: "QuestionsSavedToBank",
                table: "Quiz",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove QuestionsSavedToBank field from Quiz table
            migrationBuilder.DropColumn(
                name: "QuestionsSavedToBank",
                table: "Quiz");
        }
    }
}

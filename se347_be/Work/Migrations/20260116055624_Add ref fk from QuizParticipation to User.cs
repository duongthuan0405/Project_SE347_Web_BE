using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace se347_be.Migrations
{
    /// <inheritdoc />
    public partial class AddreffkfromQuizParticipationtoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "QuizParticipation",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizParticipation_UserId",
                table: "QuizParticipation",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizParticipation_User_UserId",
                table: "QuizParticipation",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizParticipation_User_UserId",
                table: "QuizParticipation");

            migrationBuilder.DropIndex(
                name: "IX_QuizParticipation_UserId",
                table: "QuizParticipation");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "QuizParticipation");
        }
    }
}

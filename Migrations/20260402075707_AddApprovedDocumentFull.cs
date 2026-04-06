using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChairmanOMS.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedDocumentFull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "ApprovedDocuments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "ApprovedDocuments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovedDocuments_ApprovedByUserId",
                table: "ApprovedDocuments",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovedDocuments_CreatedById",
                table: "ApprovedDocuments",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovedDocuments_AspNetUsers_ApprovedByUserId",
                table: "ApprovedDocuments",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovedDocuments_AspNetUsers_CreatedById",
                table: "ApprovedDocuments",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovedDocuments_AspNetUsers_ApprovedByUserId",
                table: "ApprovedDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovedDocuments_AspNetUsers_CreatedById",
                table: "ApprovedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ApprovedDocuments_ApprovedByUserId",
                table: "ApprovedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ApprovedDocuments_CreatedById",
                table: "ApprovedDocuments");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "ApprovedDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "ApprovedDocuments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}

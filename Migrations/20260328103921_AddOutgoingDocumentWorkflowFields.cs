using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChairmanOMS.Migrations
{
    /// <inheritdoc />
    public partial class AddOutgoingDocumentWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConveyerName",
                table: "OutgoingDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "OutgoingDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "OutgoingDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConveyerName",
                table: "OutgoingDocuments");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "OutgoingDocuments");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "OutgoingDocuments");
        }
    }
}

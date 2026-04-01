using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChairmanOMS.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "IncomingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "IncomingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnderProcessBy",
                table: "IncomingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HandedTo",
                table: "IncomingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Other",
                table: "IncomingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "IncomingDocuments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfReturn",
                table: "IncomingDocuments",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

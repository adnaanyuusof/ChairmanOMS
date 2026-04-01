using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChairmanOMS.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OutgoingDocuments missing fields
            migrationBuilder.AddColumn<string>(
                name: "ConveyerName",
                table: "OutgoingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "OutgoingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "OutgoingDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedIncomingDocumentId",
                table: "OutgoingDocuments",
                type: "integer",
                nullable: true);

            // Appointments missing fields
            migrationBuilder.AddColumn<string>(
                name: "Masuulka",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorStatus",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Appointments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "Appointments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

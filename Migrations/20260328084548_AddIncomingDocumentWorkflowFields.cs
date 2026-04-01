using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChairmanOMS.Migrations
{
    /// <inheritdoc />
    public partial class AddIncomingDocumentWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfReturn",
                table: "IncomingDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "IncomingDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HandedTo",
                table: "IncomingDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Other",
                table: "IncomingDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "IncomingDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "IncomingDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnderProcessBy",
                table: "IncomingDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfReturn",
                table: "IncomingDocuments");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "IncomingDocuments");

            migrationBuilder.DropColumn(
                name: "HandedTo",
                table: "IncomingDocuments");

            migrationBuilder.DropColumn(
                name: "Other",
                table: "IncomingDocuments");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "IncomingDocuments");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "IncomingDocuments");

            migrationBuilder.DropColumn(
                name: "UnderProcessBy",
                table: "IncomingDocuments");
        }
    }
}

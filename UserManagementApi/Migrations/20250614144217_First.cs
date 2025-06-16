using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Admin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123");

            migrationBuilder.InsertData(
                table: "Users",
                columns:
                [
                    "Id", "Login", "Password", "Name", "Gender", "Birthday", "Admin",
                    "CreatedOn", "CreatedBy", "ModifiedOn", "ModifiedBy", "RevokedOn", "RevokedBy"
                ],
                values:
                [
                    Guid.NewGuid(),
                    "admin",
                    passwordHash,
                    "Administrator",
                    2,
                    null,
                    true,
                    new DateTime(2025, 6, 16, 0, 0, 0, DateTimeKind.Utc),
                    "System",
                    new DateTime(2025, 6, 16, 0, 0, 0, DateTimeKind.Utc),
                    "System",
                    null,
                    null
                ]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomHome.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTokenQueueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceTokens",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTokens_Status_CreatedAt_Id",
                table: "ServiceTokens",
                columns: new[] { "Status", "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceTokens_Status_CreatedAt_Id",
                table: "ServiceTokens");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceTokens",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}

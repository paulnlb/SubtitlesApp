using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubtitlesServer.IdentityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "SubscriptionLevel",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionLevel",
                table: "AspNetUsers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoMercy.Database.Migrations
{
    /// <inheritdoc />
    public partial class Test3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetadataId",
                table: "Albums",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_MetadataId",
                table: "Albums",
                column: "MetadataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Metadata_MetadataId",
                table: "Albums",
                column: "MetadataId",
                principalTable: "Metadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Metadata_MetadataId",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_MetadataId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "MetadataId",
                table: "Albums");
        }
    }
}

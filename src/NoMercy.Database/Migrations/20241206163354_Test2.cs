using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoMercy.Database.Migrations
{
    /// <inheritdoc />
    public partial class Test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Metadata_VideoFiles_VideoFileId",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_VideoFileId",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "VideoFileId",
                table: "Metadata");

            migrationBuilder.AlterColumn<string>(
                name: "MetadataId",
                table: "VideoFiles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AudioTrackId",
                table: "Metadata",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFiles_MetadataId",
                table: "VideoFiles",
                column: "MetadataId");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoFiles_Metadata_MetadataId",
                table: "VideoFiles",
                column: "MetadataId",
                principalTable: "Metadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoFiles_Metadata_MetadataId",
                table: "VideoFiles");

            migrationBuilder.DropIndex(
                name: "IX_VideoFiles_MetadataId",
                table: "VideoFiles");

            migrationBuilder.AlterColumn<int>(
                name: "MetadataId",
                table: "VideoFiles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AudioTrackId",
                table: "Metadata",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoFileId",
                table: "Metadata",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_VideoFileId",
                table: "Metadata",
                column: "VideoFileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Metadata_VideoFiles_VideoFileId",
                table: "Metadata",
                column: "VideoFileId",
                principalTable: "VideoFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

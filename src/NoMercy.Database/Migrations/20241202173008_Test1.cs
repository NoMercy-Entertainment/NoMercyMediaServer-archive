using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoMercy.Database.Migrations
{
    /// <inheritdoc />
    public partial class Test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileLibrary");

            migrationBuilder.DropTable(
                name: "FileMovie");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Specials");

            migrationBuilder.AddColumn<int>(
                name: "MetadataId",
                table: "VideoFiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Iso31661",
                table: "Translations",
                type: "TEXT",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "FolderId",
                table: "Tracks",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetadataId",
                table: "Tracks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Audio",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AudioTrackId",
                table: "Metadata",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ChaptersFile",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Metadata",
                type: "TEXT",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Filename",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Folder",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FolderSize",
                table: "Metadata",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Fonts",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FontsFile",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HostFolder",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Previews",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subtitles",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Metadata",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Metadata",
                type: "TEXT",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Video",
                table: "Metadata",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoFileId",
                table: "Metadata",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HostFolder",
                table: "Artists",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HostFolder",
                table: "Albums",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_AudioTrackId",
                table: "Metadata",
                column: "AudioTrackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_Filename_HostFolder",
                table: "Metadata",
                columns: new[] { "Filename", "HostFolder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_Type",
                table: "Metadata",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_VideoFileId",
                table: "Metadata",
                column: "VideoFileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Metadata_Tracks_AudioTrackId",
                table: "Metadata",
                column: "AudioTrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Metadata_VideoFiles_VideoFileId",
                table: "Metadata",
                column: "VideoFileId",
                principalTable: "VideoFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Metadata_Tracks_AudioTrackId",
                table: "Metadata");

            migrationBuilder.DropForeignKey(
                name: "FK_Metadata_VideoFiles_VideoFileId",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_AudioTrackId",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_Filename_HostFolder",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_Type",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_VideoFileId",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "MetadataId",
                table: "VideoFiles");

            migrationBuilder.DropColumn(
                name: "MetadataId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Audio",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "AudioTrackId",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "ChaptersFile",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Filename",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Folder",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "FolderSize",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Fonts",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "FontsFile",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "HostFolder",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Previews",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Subtitles",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "Video",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "VideoFileId",
                table: "Metadata");

            migrationBuilder.AlterColumn<string>(
                name: "Iso31661",
                table: "Translations",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FolderId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Specials",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HostFolder",
                table: "Artists",
                type: "TEXT",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "HostFolder",
                table: "Albums",
                type: "TEXT",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileLibrary",
                columns: table => new
                {
                    FileId = table.Column<string>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLibrary", x => new { x.FileId, x.LibraryId });
                    table.ForeignKey(
                        name: "FK_FileLibrary_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileLibrary_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileMovie",
                columns: table => new
                {
                    FileId = table.Column<string>(type: "TEXT", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileMovie", x => new { x.FileId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_FileMovie_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileMovie_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileLibrary_FileId",
                table: "FileLibrary",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileLibrary_LibraryId",
                table: "FileLibrary",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_FileMovie_FileId",
                table: "FileMovie",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileMovie_MovieId",
                table: "FileMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_EpisodeId",
                table: "Files",
                column: "EpisodeId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoMercy.Database.Migrations.Queue
{
    /// <inheritdoc />
    public partial class AddEncoderV2Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Iso31661 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Rating = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Meaning = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Headquarters = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", nullable: true),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OriginCountry = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ParentCompany = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncoderNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    NodeId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NodeName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NetworkAddress = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NetworkPort = table.Column<int>(type: "INTEGER", nullable: false),
                    UseHttps = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHealthy = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastHeartbeat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HasGPU = table.Column<bool>(type: "INTEGER", nullable: false),
                    GPUModel = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CPUCores = table.Column<int>(type: "INTEGER", nullable: false),
                    MemoryGB = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncoderNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncoderProfile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Container = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Param = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    VideoProfile = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AudioProfile = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SubtitleProfile = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncoderProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncodingNodeAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TaskId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NodeId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncodingNodeAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Task = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EpisodeCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: true),
                    CreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Keyword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keyword", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Iso6391 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EnglishName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Library",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ChapterImages = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtractChapters = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtractChaptersDuring = table.Column<bool>(type: "INTEGER", nullable: false),
                    Image = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AutoRefreshInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: true),
                    PerfectSubtitleMatch = table.Column<bool>(type: "INTEGER", nullable: false),
                    Realtime = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpecialSeasonName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicGenre",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGenre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Network",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OriginCountry = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Headquarters = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Network", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlsoKnownAs = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Biography = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    BirthDay = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeathDay = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ImdbId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    KnownForDepartment = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: false),
                    Profile = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalIds = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Special",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Backdrop = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Creator = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Special", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Manage = table.Column<bool>(type: "INTEGER", nullable: false),
                    Owner = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Allowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    AudioTranscoding = table.Column<bool>(type: "INTEGER", nullable: false),
                    VideoTranscoding = table.Column<bool>(type: "INTEGER", nullable: false),
                    NoTranscoding = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchProvider",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DisplayPriority = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchProvider", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncodingJobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    InputFilePath = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    OutputFolder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ProfileId = table.Column<string>(type: "TEXT", nullable: true),
                    ProfileSnapshotJson = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExecutionTimeMs = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncodingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncodingJobs_EncoderProfile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "EncoderProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EncoderProfileFolder",
                columns: table => new
                {
                    EncoderProfileId = table.Column<string>(type: "TEXT", nullable: false),
                    FolderId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncoderProfileFolder", x => new { x.EncoderProfileId, x.FolderId });
                    table.ForeignKey(
                        name: "FK_EncoderProfileFolder_EncoderProfile_EncoderProfileId",
                        column: x => x.EncoderProfileId,
                        principalTable: "EncoderProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EncoderProfileFolder_Folder_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TrackNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Cover = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: true),
                    Lyrics = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HostFolder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    FolderId = table.Column<string>(type: "TEXT", nullable: false),
                    MetadataId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Track", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Track_Folder_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Artist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Disambiguation = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Cover = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HostFolder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: true),
                    FolderId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artist_Folder_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Artist_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Collection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Backdrop = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Parts = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collection_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderLibrary",
                columns: table => new
                {
                    FolderId = table.Column<string>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderLibrary", x => new { x.FolderId, x.LibraryId });
                    table.ForeignKey(
                        name: "FK_FolderLibrary_Folder_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderLibrary_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageLibrary",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageLibrary", x => new { x.LanguageId, x.LibraryId });
                    table.ForeignKey(
                        name: "FK_LanguageLibrary_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageLibrary_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    Show = table.Column<bool>(type: "INTEGER", nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Adult = table.Column<bool>(type: "INTEGER", nullable: false),
                    Backdrop = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Budget = table.Column<int>(type: "INTEGER", nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ImdbId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OriginalTitle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OriginalLanguage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Revenue = table.Column<long>(type: "INTEGER", nullable: true),
                    Runtime = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Tagline = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Trailer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Video = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: true),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movie_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Disambiguation = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Cover = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseGroup_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    HaveEpisodes = table.Column<int>(type: "INTEGER", nullable: true),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Backdrop = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    FirstAirDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ImdbId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    InProduction = table.Column<bool>(type: "INTEGER", nullable: true),
                    LastEpisodeToAir = table.Column<int>(type: "INTEGER", nullable: true),
                    MediaType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NextEpisodeToAir = table.Column<int>(type: "INTEGER", nullable: true),
                    NumberOfEpisodes = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberOfSeasons = table.Column<int>(type: "INTEGER", nullable: true),
                    OriginCountry = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    OriginalLanguage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Popularity = table.Column<double>(type: "REAL", nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SpokenLanguages = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Tagline = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Trailer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TvdbId = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: true),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tv_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryUser",
                columns: table => new
                {
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryUser", x => new { x.LibraryId, x.UserId });
                    table.ForeignKey(
                        name: "FK_LibraryUser_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationUser",
                columns: table => new
                {
                    NotificationId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationUser", x => new { x.NotificationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_NotificationUser_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Cover = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlist_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialUser",
                columns: table => new
                {
                    SpecialId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialUser", x => new { x.SpecialId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SpecialUser_Special_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "Special",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EncodingTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    JobId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    AssignedNodeId = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DependenciesJson = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncodingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncodingTasks_EncoderNodes_AssignedNodeId",
                        column: x => x.AssignedNodeId,
                        principalTable: "EncoderNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EncodingTasks_EncodingJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "EncodingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryTrack",
                columns: table => new
                {
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryTrack", x => new { x.LibraryId, x.TrackId });
                    table.ForeignKey(
                        name: "FK_LibraryTrack_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Metadata",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    HostFolder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FolderSize = table.Column<long>(type: "INTEGER", nullable: false),
                    AudioTrackId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Previews = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Fonts = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    FontsFile = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ChaptersFile = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Chapters = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Video = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Audio = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Subtitles = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metadata_Track_AudioTrackId",
                        column: x => x.AudioTrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicGenreTrack",
                columns: table => new
                {
                    GenreId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGenreTrack", x => new { x.GenreId, x.TrackId });
                    table.ForeignKey(
                        name: "FK_MusicGenreTrack_MusicGenre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "MusicGenre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicGenreTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicPlay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicPlay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicPlay_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicPlay_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackUser",
                columns: table => new
                {
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackUser", x => new { x.TrackId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TrackUser_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistLibrary",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistLibrary", x => new { x.ArtistId, x.LibraryId });
                    table.ForeignKey(
                        name: "FK_ArtistLibrary_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistLibrary_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistMusicGenre",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MusicGenreId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistMusicGenre", x => new { x.ArtistId, x.MusicGenreId });
                    table.ForeignKey(
                        name: "FK_ArtistMusicGenre_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistMusicGenre_MusicGenre_MusicGenreId",
                        column: x => x.MusicGenreId,
                        principalTable: "MusicGenre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistTrack",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistTrack", x => new { x.ArtistId, x.TrackId });
                    table.ForeignKey(
                        name: "FK_ArtistTrack_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistUser",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistUser", x => new { x.ArtistId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ArtistUser_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionLibrary",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionLibrary", x => new { x.CollectionId, x.LibraryId });
                    table.ForeignKey(
                        name: "FK_CollectionLibrary_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionLibrary_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionUser",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionUser", x => new { x.CollectionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CollectionUser_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificationMovie",
                columns: table => new
                {
                    CertificationId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationMovie", x => new { x.CertificationId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_CertificationMovie_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificationMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionMovie",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionMovie", x => new { x.CollectionId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_CollectionMovie_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMovie",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMovie", x => new { x.CompanyId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_CompanyMovie_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenreMovie",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreMovie", x => new { x.GenreId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_GenreMovie_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeywordMovie",
                columns: table => new
                {
                    KeywordId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeywordMovie", x => new { x.KeywordId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_KeywordMovie_Keyword_KeywordId",
                        column: x => x.KeywordId,
                        principalTable: "Keyword",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KeywordMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryMovie",
                columns: table => new
                {
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryMovie", x => new { x.LibraryId, x.MovieId });
                    table.ForeignKey(
                        name: "FK_LibraryMovie_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieUser",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieUser", x => new { x.MovieId, x.UserId });
                    table.ForeignKey(
                        name: "FK_MovieUser_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistReleaseGroup",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReleaseGroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistReleaseGroup", x => new { x.ArtistId, x.ReleaseGroupId });
                    table.ForeignKey(
                        name: "FK_ArtistReleaseGroup_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistReleaseGroup_ReleaseGroup_ReleaseGroupId",
                        column: x => x.ReleaseGroupId,
                        principalTable: "ReleaseGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicGenreReleaseGroup",
                columns: table => new
                {
                    GenreId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReleaseGroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGenreReleaseGroup", x => new { x.GenreId, x.ReleaseGroupId });
                    table.ForeignKey(
                        name: "FK_MusicGenreReleaseGroup_MusicGenre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "MusicGenre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicGenreReleaseGroup_ReleaseGroup_ReleaseGroupId",
                        column: x => x.ReleaseGroupId,
                        principalTable: "ReleaseGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeTitle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Iso31661 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeTitle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeTitle_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlternativeTitle_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificationTv",
                columns: table => new
                {
                    CertificationId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationTv", x => new { x.CertificationId, x.TvId });
                    table.ForeignKey(
                        name: "FK_CertificationTv_Certification_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificationTv_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyTv",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyTv", x => new { x.CompanyId, x.TvId });
                    table.ForeignKey(
                        name: "FK_CompanyTv_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyTv_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Creator",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creator", x => new { x.PersonId, x.TvId });
                    table.ForeignKey(
                        name: "FK_Creator_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Creator_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenreTv",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreTv", x => new { x.GenreId, x.TvId });
                    table.ForeignKey(
                        name: "FK_GenreTv_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreTv_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KeywordTv",
                columns: table => new
                {
                    KeywordId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeywordTv", x => new { x.KeywordId, x.TvId });
                    table.ForeignKey(
                        name: "FK_KeywordTv_Keyword_KeywordId",
                        column: x => x.KeywordId,
                        principalTable: "Keyword",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KeywordTv_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryTv",
                columns: table => new
                {
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryTv", x => new { x.LibraryId, x.TvId });
                    table.ForeignKey(
                        name: "FK_LibraryTv_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryTv_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NetworkTv",
                columns: table => new
                {
                    NetworkId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkTv", x => new { x.NetworkId, x.TvId });
                    table.ForeignKey(
                        name: "FK_NetworkTv_Network_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Network",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NetworkTv_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybackPreference",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpecialId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Video = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Audio = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Subtitles = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybackPreference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybackPreference_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackPreference_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackPreference_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackPreference_Special_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "Special",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackPreference_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackPreference_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Backdrop = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvFromId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvToId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieFromId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieToId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendation_Movie_MovieFromId",
                        column: x => x.MovieFromId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recommendation_Movie_MovieToId",
                        column: x => x.MovieToId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recommendation_Tv_TvFromId",
                        column: x => x.TvFromId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recommendation_Tv_TvToId",
                        column: x => x.TvToId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Season",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AirDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EpisodeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Season", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Season_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Season_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Similar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Backdrop = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Poster = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TitleSort = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MediaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TvFromId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvToId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieFromId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieToId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Similar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Similar_Movie_MovieFromId",
                        column: x => x.MovieFromId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Similar_Movie_MovieToId",
                        column: x => x.MovieToId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Similar_Tv_TvFromId",
                        column: x => x.TvFromId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Similar_Tv_TvToId",
                        column: x => x.TvToId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvUser",
                columns: table => new
                {
                    TvId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvUser", x => new { x.TvId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TvUser_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TvUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchProviderMedia",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    WatchProviderId = table.Column<int>(type: "INTEGER", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ProviderType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Link = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchProviderMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchProviderMedia_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchProviderMedia_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchProviderMedia_WatchProvider_WatchProviderId",
                        column: x => x.WatchProviderId,
                        principalTable: "WatchProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistTrack",
                columns: table => new
                {
                    PlaylistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistTrack", x => new { x.PlaylistId, x.TrackId });
                    table.ForeignKey(
                        name: "FK_PlaylistTrack_Playlist_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EncodingProgress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ProgressPercentage = table.Column<double>(type: "REAL", nullable: false),
                    CurrentFrame = table.Column<long>(type: "INTEGER", nullable: false),
                    Fps = table.Column<double>(type: "REAL", nullable: false),
                    Speed = table.Column<double>(type: "REAL", nullable: false),
                    Bitrate = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CurrentTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EstimatedRemaining = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncodingProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncodingProgress_EncodingTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "EncodingTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Disambiguation = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Cover = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Tracks = table.Column<int>(type: "INTEGER", nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HostFolder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false),
                    FolderId = table.Column<string>(type: "TEXT", nullable: false),
                    MetadataId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Album_Folder_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Album_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Album_Metadata_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AirDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ImdbId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ProductionCode = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SeasonNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Still = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TvdbId = table.Column<int>(type: "INTEGER", nullable: true),
                    VoteAverage = table.Column<float>(type: "REAL", nullable: true),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episode_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Episode_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumArtist",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumArtist", x => new { x.AlbumId, x.ArtistId });
                    table.ForeignKey(
                        name: "FK_AlbumArtist_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumArtist_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumLibrary",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumLibrary", x => new { x.AlbumId, x.LibraryId });
                    table.ForeignKey(
                        name: "FK_AlbumLibrary_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumLibrary_Library_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumMusicGenre",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MusicGenreId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumMusicGenre", x => new { x.AlbumId, x.MusicGenreId });
                    table.ForeignKey(
                        name: "FK_AlbumMusicGenre_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumMusicGenre_MusicGenre_MusicGenreId",
                        column: x => x.MusicGenreId,
                        principalTable: "MusicGenre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumReleaseGroup",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReleaseGroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumReleaseGroup", x => new { x.AlbumId, x.ReleaseGroupId });
                    table.ForeignKey(
                        name: "FK_AlbumReleaseGroup_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumReleaseGroup_ReleaseGroup_ReleaseGroupId",
                        column: x => x.ReleaseGroupId,
                        principalTable: "ReleaseGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumTrack",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTrack", x => new { x.AlbumId, x.TrackId });
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumUser",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumUser", x => new { x.AlbumId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AlbumUser_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crew",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    JobId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crew", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crew_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crew_Job_JobId",
                        column: x => x.JobId,
                        principalTable: "Job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crew_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crew_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crew_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Crew_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestStar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestStar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestStar_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestStar_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialId = table.Column<string>(type: "TEXT", nullable: false),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialItem_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialItem_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialItem_Special_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "Special",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Translation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Iso31661 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Iso6391 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EnglishName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Overview = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Homepage = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Biography = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: true),
                    GenreId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translation_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Genre_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_ReleaseGroup_ReleaseGroupId",
                        column: x => x.ReleaseGroupId,
                        principalTable: "ReleaseGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Translation_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoFile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Filename = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HostFolder = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Languages = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Quality = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Share = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Subtitles = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Chapters = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    MetadataId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Track = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoFile_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoFile_Metadata_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "Metadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoFile_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Character = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EpisodeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: true),
                    CreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    GuestStarId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_GuestStar_GuestStarId",
                        column: x => x.GuestStarId,
                        principalTable: "GuestStar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Iso6391 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Site = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Size = table.Column<int>(type: "INTEGER", nullable: false),
                    Src = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: true),
                    VideoFileId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Media_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Media_VideoFile_VideoFileId",
                        column: x => x.VideoFileId,
                        principalTable: "VideoFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserData",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    LastPlayedDate = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Audio = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Subtitle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SubtitleType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Time = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpecialId = table.Column<string>(type: "TEXT", nullable: true),
                    VideoFileId = table.Column<string>(type: "TEXT", nullable: false),
                    SpecialItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserData_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserData_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserData_SpecialItem_SpecialItemId",
                        column: x => x.SpecialItemId,
                        principalTable: "SpecialItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserData_Special_SpecialId",
                        column: x => x.SpecialId,
                        principalTable: "Special",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserData_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserData_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserData_VideoFile_VideoFileId",
                        column: x => x.VideoFileId,
                        principalTable: "VideoFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cast",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cast", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cast_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cast_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cast_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cast_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cast_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cast_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AspectRatio = table.Column<double>(type: "REAL", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    Iso6391 = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Site = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Size = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    VoteAverage = table.Column<double>(type: "REAL", nullable: true),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    CastCreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CastId = table.Column<int>(type: "INTEGER", nullable: true),
                    CrewCreditId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CrewId = table.Column<int>(type: "INTEGER", nullable: true),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: true),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TvId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ColorPalette = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Image_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Artist_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Cast_CastId",
                        column: x => x.CastId,
                        principalTable: "Cast",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Crew_CrewId",
                        column: x => x.CrewId,
                        principalTable: "Crew",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Episode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Season_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Track_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Track",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Image_Tv_TvId",
                        column: x => x.TvId,
                        principalTable: "Tv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Album_FolderId",
                table: "Album",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Album_LibraryId",
                table: "Album",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Album_MetadataId",
                table: "Album",
                column: "MetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Album_Name",
                table: "Album",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Album_Year",
                table: "Album",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_AlbumId",
                table: "AlbumArtist",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_ArtistId",
                table: "AlbumArtist",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumLibrary_AlbumId",
                table: "AlbumLibrary",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumLibrary_LibraryId",
                table: "AlbumLibrary",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMusicGenre_AlbumId",
                table: "AlbumMusicGenre",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMusicGenre_MusicGenreId",
                table: "AlbumMusicGenre",
                column: "MusicGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumReleaseGroup_AlbumId",
                table: "AlbumReleaseGroup",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumReleaseGroup_ReleaseGroupId",
                table: "AlbumReleaseGroup",
                column: "ReleaseGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_AlbumId",
                table: "AlbumTrack",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_TrackId",
                table: "AlbumTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumUser_AlbumId",
                table: "AlbumUser",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumUser_UserId",
                table: "AlbumUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeTitle_MovieId",
                table: "AlternativeTitle",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeTitle_Title_MovieId",
                table: "AlternativeTitle",
                columns: new[] { "Title", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeTitle_Title_TvId",
                table: "AlternativeTitle",
                columns: new[] { "Title", "TvId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeTitle_TvId",
                table: "AlternativeTitle",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_Country",
                table: "Artist",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_FolderId",
                table: "Artist",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_LibraryId",
                table: "Artist",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_Name",
                table: "Artist",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_Year",
                table: "Artist",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistLibrary_ArtistId",
                table: "ArtistLibrary",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistLibrary_LibraryId",
                table: "ArtistLibrary",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMusicGenre_ArtistId",
                table: "ArtistMusicGenre",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMusicGenre_MusicGenreId",
                table: "ArtistMusicGenre",
                column: "MusicGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistReleaseGroup_ArtistId",
                table: "ArtistReleaseGroup",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistReleaseGroup_ReleaseGroupId",
                table: "ArtistReleaseGroup",
                column: "ReleaseGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistTrack_ArtistId",
                table: "ArtistTrack",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistTrack_TrackId",
                table: "ArtistTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistUser_ArtistId",
                table: "ArtistUser",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistUser_UserId",
                table: "ArtistUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_CreditId",
                table: "Cast",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_CreditId_EpisodeId_RoleId",
                table: "Cast",
                columns: new[] { "CreditId", "EpisodeId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cast_CreditId_MovieId_RoleId",
                table: "Cast",
                columns: new[] { "CreditId", "MovieId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cast_CreditId_SeasonId_RoleId",
                table: "Cast",
                columns: new[] { "CreditId", "SeasonId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cast_CreditId_TvId_RoleId",
                table: "Cast",
                columns: new[] { "CreditId", "TvId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cast_EpisodeId",
                table: "Cast",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_MovieId",
                table: "Cast",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_PersonId",
                table: "Cast",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_RoleId",
                table: "Cast",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_SeasonId",
                table: "Cast",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Cast_TvId",
                table: "Cast",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Certification_Iso31661_Rating",
                table: "Certification",
                columns: new[] { "Iso31661", "Rating" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certification_Order",
                table: "Certification",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Certification_Rating",
                table: "Certification",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationMovie_CertificationId",
                table: "CertificationMovie",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationMovie_MovieId",
                table: "CertificationMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTv_CertificationId",
                table: "CertificationTv",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTv_TvId",
                table: "CertificationTv",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_LibraryId",
                table: "Collection",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_Title",
                table: "Collection",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_TitleSort",
                table: "Collection",
                column: "TitleSort");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionLibrary_CollectionId",
                table: "CollectionLibrary",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionLibrary_LibraryId",
                table: "CollectionLibrary",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMovie_CollectionId",
                table: "CollectionMovie",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMovie_MovieId",
                table: "CollectionMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMovie_MovieId_CollectionId",
                table: "CollectionMovie",
                columns: new[] { "MovieId", "CollectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionUser_CollectionId",
                table: "CollectionUser",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionUser_UserId",
                table: "CollectionUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMovie_CompanyId_MovieId",
                table: "CompanyMovie",
                columns: new[] { "CompanyId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyMovie_MovieId",
                table: "CompanyMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyTv_CompanyId_TvId",
                table: "CompanyTv",
                columns: new[] { "CompanyId", "TvId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyTv_TvId",
                table: "CompanyTv",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Creator_TvId",
                table: "Creator",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_CreditId",
                table: "Crew",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_CreditId_EpisodeId_JobId",
                table: "Crew",
                columns: new[] { "CreditId", "EpisodeId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crew_CreditId_MovieId_JobId",
                table: "Crew",
                columns: new[] { "CreditId", "MovieId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crew_CreditId_SeasonId_JobId",
                table: "Crew",
                columns: new[] { "CreditId", "SeasonId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crew_CreditId_TvId_JobId",
                table: "Crew",
                columns: new[] { "CreditId", "TvId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crew_EpisodeId",
                table: "Crew",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_JobId",
                table: "Crew",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_MovieId",
                table: "Crew",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_PersonId",
                table: "Crew",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_SeasonId",
                table: "Crew",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Crew_TvId",
                table: "Crew",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_EncoderNodes_IsHealthy",
                table: "EncoderNodes",
                column: "IsHealthy");

            migrationBuilder.CreateIndex(
                name: "IX_EncoderNodes_NodeId",
                table: "EncoderNodes",
                column: "NodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncoderProfileFolder_EncoderProfileId",
                table: "EncoderProfileFolder",
                column: "EncoderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EncoderProfileFolder_FolderId",
                table: "EncoderProfileFolder",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_EncodingJobs_ProfileId",
                table: "EncodingJobs",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EncodingJobs_State",
                table: "EncodingJobs",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_EncodingProgress_TaskId_RecordedAt",
                table: "EncodingProgress",
                columns: new[] { "TaskId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EncodingTasks_AssignedNodeId",
                table: "EncodingTasks",
                column: "AssignedNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_EncodingTasks_JobId_State",
                table: "EncodingTasks",
                columns: new[] { "JobId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Episode_AirDate",
                table: "Episode",
                column: "AirDate");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_EpisodeNumber",
                table: "Episode",
                column: "EpisodeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_ImdbId",
                table: "Episode",
                column: "ImdbId");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_SeasonId",
                table: "Episode",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_SeasonNumber",
                table: "Episode",
                column: "SeasonNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_Title",
                table: "Episode",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_TvdbId",
                table: "Episode",
                column: "TvdbId");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_TvId",
                table: "Episode",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_TvId_SeasonNumber",
                table: "Episode",
                columns: new[] { "TvId", "SeasonNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Episode_TvId_SeasonNumber_EpisodeNumber",
                table: "Episode",
                columns: new[] { "TvId", "SeasonNumber", "EpisodeNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Folder_Path",
                table: "Folder",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolderLibrary_FolderId",
                table: "FolderLibrary",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderLibrary_LibraryId",
                table: "FolderLibrary",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Genre_Name",
                table: "Genre",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_GenreMovie_GenreId",
                table: "GenreMovie",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_GenreMovie_MovieId",
                table: "GenreMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_GenreTv_GenreId",
                table: "GenreTv",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_GenreTv_TvId",
                table: "GenreTv",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStar_CreditId",
                table: "GuestStar",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStar_CreditId_EpisodeId",
                table: "GuestStar",
                columns: new[] { "CreditId", "EpisodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestStar_EpisodeId",
                table: "GuestStar",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStar_PersonId",
                table: "GuestStar",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_AlbumId",
                table: "Image",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_ArtistId",
                table: "Image",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_CastCreditId",
                table: "Image",
                column: "CastCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_CastId",
                table: "Image",
                column: "CastId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_CollectionId",
                table: "Image",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_CollectionId_Type",
                table: "Image",
                columns: new[] { "CollectionId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Image_CrewCreditId",
                table: "Image",
                column: "CrewCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_CrewId",
                table: "Image",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_EpisodeId",
                table: "Image",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath",
                table: "Image",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_AlbumId",
                table: "Image",
                columns: new[] { "FilePath", "AlbumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_ArtistId",
                table: "Image",
                columns: new[] { "FilePath", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_CastCreditId",
                table: "Image",
                columns: new[] { "FilePath", "CastCreditId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_CollectionId",
                table: "Image",
                columns: new[] { "FilePath", "CollectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_CrewCreditId",
                table: "Image",
                columns: new[] { "FilePath", "CrewCreditId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_EpisodeId",
                table: "Image",
                columns: new[] { "FilePath", "EpisodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_MovieId",
                table: "Image",
                columns: new[] { "FilePath", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_PersonId",
                table: "Image",
                columns: new[] { "FilePath", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_SeasonId",
                table: "Image",
                columns: new[] { "FilePath", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_TrackId",
                table: "Image",
                columns: new[] { "FilePath", "TrackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_FilePath_TvId",
                table: "Image",
                columns: new[] { "FilePath", "TvId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_MovieId",
                table: "Image",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_MovieId_Type",
                table: "Image",
                columns: new[] { "MovieId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Image_PersonId",
                table: "Image",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_SeasonId",
                table: "Image",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_TrackId",
                table: "Image",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_TvId",
                table: "Image",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_TvId_Type",
                table: "Image",
                columns: new[] { "TvId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Image_Type_Iso6391",
                table: "Image",
                columns: new[] { "Type", "Iso6391" });

            migrationBuilder.CreateIndex(
                name: "IX_Job_CreditId",
                table: "Job",
                column: "CreditId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Keyword_Name",
                table: "Keyword",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_KeywordMovie_KeywordId",
                table: "KeywordMovie",
                column: "KeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_KeywordMovie_MovieId",
                table: "KeywordMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_KeywordTv_KeywordId",
                table: "KeywordTv",
                column: "KeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_KeywordTv_TvId",
                table: "KeywordTv",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Language_EnglishName",
                table: "Language",
                column: "EnglishName");

            migrationBuilder.CreateIndex(
                name: "IX_Language_Iso6391",
                table: "Language",
                column: "Iso6391",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Language_Name",
                table: "Language",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageLibrary_LanguageId",
                table: "LanguageLibrary",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageLibrary_LibraryId",
                table: "LanguageLibrary",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Library_Id",
                table: "Library",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Library_Order",
                table: "Library",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Library_Title",
                table: "Library",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Library_Type",
                table: "Library",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMovie_LibraryId",
                table: "LibraryMovie",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMovie_MovieId",
                table: "LibraryMovie",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryTrack_LibraryId",
                table: "LibraryTrack",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryTrack_TrackId",
                table: "LibraryTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryTv_LibraryId",
                table: "LibraryTv",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryTv_TvId",
                table: "LibraryTv",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryUser_LibraryId",
                table: "LibraryUser",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryUser_UserId",
                table: "LibraryUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_EpisodeId_Src",
                table: "Media",
                columns: new[] { "EpisodeId", "Src" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_MovieId_Src",
                table: "Media",
                columns: new[] { "MovieId", "Src" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_Name",
                table: "Media",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Media_PersonId_Src",
                table: "Media",
                columns: new[] { "PersonId", "Src" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_SeasonId_Src",
                table: "Media",
                columns: new[] { "SeasonId", "Src" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_Site",
                table: "Media",
                column: "Site");

            migrationBuilder.CreateIndex(
                name: "IX_Media_TvId_Src",
                table: "Media",
                columns: new[] { "TvId", "Src" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_Type",
                table: "Media",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Media_VideoFileId_Src",
                table: "Media",
                columns: new[] { "VideoFileId", "Src" },
                unique: true);

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
                name: "IX_Movie_ImdbId",
                table: "Movie",
                column: "ImdbId");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_LibraryId",
                table: "Movie",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_LibraryId_TitleSort",
                table: "Movie",
                columns: new[] { "LibraryId", "TitleSort" });

            migrationBuilder.CreateIndex(
                name: "IX_Movie_ReleaseDate",
                table: "Movie",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_Title",
                table: "Movie",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Movie_TitleSort",
                table: "Movie",
                column: "TitleSort");

            migrationBuilder.CreateIndex(
                name: "IX_MovieUser_MovieId",
                table: "MovieUser",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieUser_UserId",
                table: "MovieUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicGenre_Name",
                table: "MusicGenre",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MusicGenreReleaseGroup_GenreId",
                table: "MusicGenreReleaseGroup",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicGenreReleaseGroup_ReleaseGroupId",
                table: "MusicGenreReleaseGroup",
                column: "ReleaseGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicGenreTrack_GenreId",
                table: "MusicGenreTrack",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicGenreTrack_TrackId",
                table: "MusicGenreTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicPlay_TrackId",
                table: "MusicPlay",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicPlay_UserId",
                table: "MusicPlay",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Network_Name",
                table: "Network",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkTv_NetworkId_TvId",
                table: "NetworkTv",
                columns: new[] { "NetworkId", "TvId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkTv_TvId",
                table: "NetworkTv",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_NotificationId",
                table: "NotificationUser",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_UserId",
                table: "NotificationUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_BirthDay",
                table: "Person",
                column: "BirthDay");

            migrationBuilder.CreateIndex(
                name: "IX_Person_ImdbId",
                table: "Person",
                column: "ImdbId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Name",
                table: "Person",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Popularity",
                table: "Person",
                column: "Popularity");

            migrationBuilder.CreateIndex(
                name: "IX_Person_TitleSort",
                table: "Person",
                column: "TitleSort");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_CollectionId",
                table: "PlaybackPreference",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_LibraryId",
                table: "PlaybackPreference",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_MovieId",
                table: "PlaybackPreference",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_SpecialId",
                table: "PlaybackPreference",
                column: "SpecialId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_TvId",
                table: "PlaybackPreference",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_UserId_LibraryId",
                table: "PlaybackPreference",
                columns: new[] { "UserId", "LibraryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_UserId_MovieId",
                table: "PlaybackPreference",
                columns: new[] { "UserId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackPreference_UserId_TvId",
                table: "PlaybackPreference",
                columns: new[] { "UserId", "TvId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UserId",
                table: "Playlist",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTrack_PlaylistId",
                table: "PlaylistTrack",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTrack_TrackId",
                table: "PlaylistTrack",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_MediaId_MovieFromId",
                table: "Recommendation",
                columns: new[] { "MediaId", "MovieFromId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_MediaId_TvFromId",
                table: "Recommendation",
                columns: new[] { "MediaId", "TvFromId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_MovieFromId",
                table: "Recommendation",
                column: "MovieFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_MovieToId",
                table: "Recommendation",
                column: "MovieToId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_TvFromId",
                table: "Recommendation",
                column: "TvFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_TvToId",
                table: "Recommendation",
                column: "TvToId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseGroup_LibraryId",
                table: "ReleaseGroup",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_CreditId",
                table: "Role",
                column: "CreditId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_GuestStarId",
                table: "Role",
                column: "GuestStarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Season_AirDate",
                table: "Season",
                column: "AirDate");

            migrationBuilder.CreateIndex(
                name: "IX_Season_MovieId",
                table: "Season",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Season_SeasonNumber",
                table: "Season",
                column: "SeasonNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Season_Title",
                table: "Season",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Season_TvId",
                table: "Season",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Similar_MediaId_MovieFromId",
                table: "Similar",
                columns: new[] { "MediaId", "MovieFromId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Similar_MediaId_TvFromId",
                table: "Similar",
                columns: new[] { "MediaId", "TvFromId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Similar_MovieFromId",
                table: "Similar",
                column: "MovieFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Similar_MovieToId",
                table: "Similar",
                column: "MovieToId");

            migrationBuilder.CreateIndex(
                name: "IX_Similar_Title",
                table: "Similar",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Similar_TitleSort",
                table: "Similar",
                column: "TitleSort");

            migrationBuilder.CreateIndex(
                name: "IX_Similar_TvFromId",
                table: "Similar",
                column: "TvFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Similar_TvToId",
                table: "Similar",
                column: "TvToId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialItem_EpisodeId",
                table: "SpecialItem",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialItem_MovieId",
                table: "SpecialItem",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialItem_SpecialId_EpisodeId",
                table: "SpecialItem",
                columns: new[] { "SpecialId", "EpisodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialItem_SpecialId_MovieId",
                table: "SpecialItem",
                columns: new[] { "SpecialId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialUser_UserId",
                table: "SpecialUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Track_DiscNumber",
                table: "Track",
                column: "DiscNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Track_Filename",
                table: "Track",
                column: "Filename");

            migrationBuilder.CreateIndex(
                name: "IX_Track_Folder",
                table: "Track",
                column: "Folder");

            migrationBuilder.CreateIndex(
                name: "IX_Track_FolderId",
                table: "Track",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Track_Name",
                table: "Track",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Track_TrackNumber",
                table: "Track",
                column: "TrackNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TrackUser_TrackId",
                table: "TrackUser",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackUser_UserId",
                table: "TrackUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_AlbumId",
                table: "Translation",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_AlbumId_Iso31661",
                table: "Translation",
                columns: new[] { "AlbumId", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_ArtistId",
                table: "Translation",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_ArtistId_Iso31661",
                table: "Translation",
                columns: new[] { "ArtistId", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_CollectionId",
                table: "Translation",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_CollectionId_Iso6391_Iso31661",
                table: "Translation",
                columns: new[] { "CollectionId", "Iso6391", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_EpisodeId",
                table: "Translation",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_EpisodeId_Iso6391_Iso31661",
                table: "Translation",
                columns: new[] { "EpisodeId", "Iso6391", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_GenreId",
                table: "Translation",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_GenreId_Iso6391",
                table: "Translation",
                columns: new[] { "GenreId", "Iso6391" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_MovieId",
                table: "Translation",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_MovieId_Iso6391_Iso31661",
                table: "Translation",
                columns: new[] { "MovieId", "Iso6391", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_PersonId",
                table: "Translation",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_PersonId_Iso6391_Iso31661",
                table: "Translation",
                columns: new[] { "PersonId", "Iso6391", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_ReleaseGroupId",
                table: "Translation",
                column: "ReleaseGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_ReleaseGroupId_Iso31661",
                table: "Translation",
                columns: new[] { "ReleaseGroupId", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_SeasonId",
                table: "Translation",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_SeasonId_Iso6391_Iso31661",
                table: "Translation",
                columns: new[] { "SeasonId", "Iso6391", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TvId",
                table: "Translation",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_Translation_TvId_Iso6391_Iso31661",
                table: "Translation",
                columns: new[] { "TvId", "Iso6391", "Iso31661" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tv_FirstAirDate",
                table: "Tv",
                column: "FirstAirDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tv_ImdbId",
                table: "Tv",
                column: "ImdbId");

            migrationBuilder.CreateIndex(
                name: "IX_Tv_LibraryId",
                table: "Tv",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tv_LibraryId_TitleSort",
                table: "Tv",
                columns: new[] { "LibraryId", "TitleSort" });

            migrationBuilder.CreateIndex(
                name: "IX_Tv_Title",
                table: "Tv",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Tv_TitleSort",
                table: "Tv",
                column: "TitleSort");

            migrationBuilder.CreateIndex(
                name: "IX_Tv_TvdbId",
                table: "Tv",
                column: "TvdbId");

            migrationBuilder.CreateIndex(
                name: "IX_TvUser_TvId",
                table: "TvUser",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_TvUser_UserId",
                table: "TvUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Allowed",
                table: "User",
                column: "Allowed");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Manage",
                table: "User",
                column: "Manage");

            migrationBuilder.CreateIndex(
                name: "IX_User_Name",
                table: "User",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_User_Owner",
                table: "User",
                column: "Owner");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_CollectionId",
                table: "UserData",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_MovieId",
                table: "UserData",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_SpecialId",
                table: "UserData",
                column: "SpecialId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_SpecialItemId",
                table: "UserData",
                column: "SpecialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_TvId",
                table: "UserData",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_UserId",
                table: "UserData",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_UserId_LastPlayedDate",
                table: "UserData",
                columns: new[] { "UserId", "LastPlayedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserData_VideoFileId",
                table: "UserData",
                column: "VideoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_VideoFileId_UserId_CollectionId",
                table: "UserData",
                columns: new[] { "VideoFileId", "UserId", "CollectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserData_VideoFileId_UserId_MovieId",
                table: "UserData",
                columns: new[] { "VideoFileId", "UserId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserData_VideoFileId_UserId_SpecialId",
                table: "UserData",
                columns: new[] { "VideoFileId", "UserId", "SpecialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserData_VideoFileId_UserId_TvId",
                table: "UserData",
                columns: new[] { "VideoFileId", "UserId", "TvId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_Duration",
                table: "VideoFile",
                column: "Duration");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_EpisodeId",
                table: "VideoFile",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_EpisodeId_Folder",
                table: "VideoFile",
                columns: new[] { "EpisodeId", "Folder" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_Filename",
                table: "VideoFile",
                column: "Filename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_Folder",
                table: "VideoFile",
                column: "Folder");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_MetadataId",
                table: "VideoFile",
                column: "MetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_MovieId",
                table: "VideoFile",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_MovieId_Folder",
                table: "VideoFile",
                columns: new[] { "MovieId", "Folder" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoFile_Quality",
                table: "VideoFile",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProviderMedia_MovieId",
                table: "WatchProviderMedia",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProviderMedia_TvId",
                table: "WatchProviderMedia",
                column: "TvId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProviderMedia_WatchProviderId_CountryCode_ProviderType_MovieId_TvId",
                table: "WatchProviderMedia",
                columns: new[] { "WatchProviderId", "CountryCode", "ProviderType", "MovieId", "TvId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumArtist");

            migrationBuilder.DropTable(
                name: "AlbumLibrary");

            migrationBuilder.DropTable(
                name: "AlbumMusicGenre");

            migrationBuilder.DropTable(
                name: "AlbumReleaseGroup");

            migrationBuilder.DropTable(
                name: "AlbumTrack");

            migrationBuilder.DropTable(
                name: "AlbumUser");

            migrationBuilder.DropTable(
                name: "AlternativeTitle");

            migrationBuilder.DropTable(
                name: "ArtistLibrary");

            migrationBuilder.DropTable(
                name: "ArtistMusicGenre");

            migrationBuilder.DropTable(
                name: "ArtistReleaseGroup");

            migrationBuilder.DropTable(
                name: "ArtistTrack");

            migrationBuilder.DropTable(
                name: "ArtistUser");

            migrationBuilder.DropTable(
                name: "CertificationMovie");

            migrationBuilder.DropTable(
                name: "CertificationTv");

            migrationBuilder.DropTable(
                name: "CollectionLibrary");

            migrationBuilder.DropTable(
                name: "CollectionMovie");

            migrationBuilder.DropTable(
                name: "CollectionUser");

            migrationBuilder.DropTable(
                name: "CompanyMovie");

            migrationBuilder.DropTable(
                name: "CompanyTv");

            migrationBuilder.DropTable(
                name: "Creator");

            migrationBuilder.DropTable(
                name: "EncoderProfileFolder");

            migrationBuilder.DropTable(
                name: "EncodingNodeAssignments");

            migrationBuilder.DropTable(
                name: "EncodingProgress");

            migrationBuilder.DropTable(
                name: "FolderLibrary");

            migrationBuilder.DropTable(
                name: "GenreMovie");

            migrationBuilder.DropTable(
                name: "GenreTv");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropTable(
                name: "KeywordMovie");

            migrationBuilder.DropTable(
                name: "KeywordTv");

            migrationBuilder.DropTable(
                name: "LanguageLibrary");

            migrationBuilder.DropTable(
                name: "LibraryMovie");

            migrationBuilder.DropTable(
                name: "LibraryTrack");

            migrationBuilder.DropTable(
                name: "LibraryTv");

            migrationBuilder.DropTable(
                name: "LibraryUser");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "MovieUser");

            migrationBuilder.DropTable(
                name: "MusicGenreReleaseGroup");

            migrationBuilder.DropTable(
                name: "MusicGenreTrack");

            migrationBuilder.DropTable(
                name: "MusicPlay");

            migrationBuilder.DropTable(
                name: "NetworkTv");

            migrationBuilder.DropTable(
                name: "NotificationUser");

            migrationBuilder.DropTable(
                name: "PlaybackPreference");

            migrationBuilder.DropTable(
                name: "PlaylistTrack");

            migrationBuilder.DropTable(
                name: "Recommendation");

            migrationBuilder.DropTable(
                name: "Similar");

            migrationBuilder.DropTable(
                name: "SpecialUser");

            migrationBuilder.DropTable(
                name: "TrackUser");

            migrationBuilder.DropTable(
                name: "Translation");

            migrationBuilder.DropTable(
                name: "TvUser");

            migrationBuilder.DropTable(
                name: "UserData");

            migrationBuilder.DropTable(
                name: "WatchProviderMedia");

            migrationBuilder.DropTable(
                name: "Certification");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "EncodingTasks");

            migrationBuilder.DropTable(
                name: "Cast");

            migrationBuilder.DropTable(
                name: "Crew");

            migrationBuilder.DropTable(
                name: "Keyword");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "MusicGenre");

            migrationBuilder.DropTable(
                name: "Network");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Playlist");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "Artist");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "ReleaseGroup");

            migrationBuilder.DropTable(
                name: "Collection");

            migrationBuilder.DropTable(
                name: "SpecialItem");

            migrationBuilder.DropTable(
                name: "VideoFile");

            migrationBuilder.DropTable(
                name: "WatchProvider");

            migrationBuilder.DropTable(
                name: "EncoderNodes");

            migrationBuilder.DropTable(
                name: "EncodingJobs");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Job");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Special");

            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "EncoderProfile");

            migrationBuilder.DropTable(
                name: "GuestStar");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "Episode");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "Folder");

            migrationBuilder.DropTable(
                name: "Season");

            migrationBuilder.DropTable(
                name: "Movie");

            migrationBuilder.DropTable(
                name: "Tv");

            migrationBuilder.DropTable(
                name: "Library");
        }
    }
}

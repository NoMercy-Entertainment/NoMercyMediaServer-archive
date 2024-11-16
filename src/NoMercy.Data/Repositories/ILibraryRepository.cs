using System.Linq.Expressions;
using NoMercy.Database.Models;

namespace NoMercy.Data.Repositories;

public interface ILibraryRepository
{
    IQueryable<Library> GetLibraries(Guid userId);
    Task<Library> GetLibraryByIdAsync(Ulid libraryId, Guid userId, string language, int take, int page);
    IQueryable<Movie> GetLibraryMovies(Guid userId, Ulid libraryId, string language, int take, int page, Expression<Func<Movie, object>>? orderByExpression = null, string? direction = null);
    IQueryable<Tv> GetLibraryShows(Guid userId, Ulid libraryId, string language, int take, int page, Expression<Func<Tv, object>>? orderByExpression = null, string? direction = null);

    IOrderedQueryable<Library> GetDashboardLibrariesAsync(Guid userId);
    Task<Library?> GetLibraryByIdAsync(Ulid id);
    Task AddLibraryAsync(Library library, Guid userId);
    Task UpdateLibraryAsync(Library library);
    Task DeleteLibraryAsync(Library library);
    Task<List<Library>> GetAllLibrariesAsync();

    Task AddEncoderProfileFolderAsync(EncoderProfileFolder encoderProfileFolder);
    Task AddEncoderProfileFolderAsync(List<EncoderProfileFolder> encoderProfileFolders);
    Task AddEncoderProfileFolderAsync(EncoderProfileFolder[] encoderProfileFolders);
    Task AddLanguageLibraryAsync(LanguageLibrary[] languageLibraries);
    List<FolderDto> GetFoldersAsync();
}

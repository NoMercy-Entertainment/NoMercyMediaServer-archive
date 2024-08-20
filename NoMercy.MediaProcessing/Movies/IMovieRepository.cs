using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Common;
using NoMercy.Providers.TMDB.Models.Movies;

namespace NoMercy.MediaProcessing.Movies;

public interface IMovieRepository
{
    public Task AddAsync(Movie movie);
    public Task LinkToLibrary(Library library, Movie movie);
    public Task StoreAlternativeTitles(List<AlternativeTitle> alternativeTitles);
    public Task StoreTranslations(List<Translation> translations);
    public Task StoreContentRatings(List<CertificationMovie> certifications);
    public Task StoreSimilar(List<Similar> similar);
    public Task StoreRecommendations(List<Recommendation> recommendations);
    public Task StoreVideos(List<Media> videos);
    public Task StoreImages(List<Image> images);
    public Task StoreKeywords(List<Keyword> keywords);
    public Task LinkKeywordsToLibrary(List<KeywordMovie> keywordMovies);
    public Task StoreGenres(List<GenreMovie> genreMovies);
    
    public Task StoreWatchProviders();
    public Task StoreNetworks();
    public Task StoreCompanies();

    Task<List<CertificationMovie>> GetCertificationMovies(TmdbMovieAppends movie, List<CertificationCriteria> certificationCriteria);
}
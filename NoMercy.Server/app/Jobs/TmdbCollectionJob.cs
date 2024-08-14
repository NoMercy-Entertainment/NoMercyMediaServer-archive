using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Collections;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Server.app.Helper;
using NoMercy.Server.Logic;
using NoMercy.Server.system;

namespace NoMercy.Server.app.Jobs;

[Serializable]
public class TmdbCollectionJob : IShouldQueue
{
    public BelongsToCollection? BelongsToCollection { get; set; }
    public int Id { get; set; }
    public Library? Library { get; set; }

    public TmdbCollectionJob()
    {
        //
    }

    public TmdbCollectionJob(int id, Library? library)
    {
        Id = id;
        Library = library;
    }
    
    public TmdbCollectionJob(BelongsToCollection collectionAppends, Library? library)
    {
        BelongsToCollection = collectionAppends;
        Library = library;
    }

    public async Task Handle()
    {
        await using MediaContext context = new();

        if (Library is null) return;
        if (BelongsToCollection is null && Id is 0) return;
        
        var collectionClient = new TmdbCollectionClient(BelongsToCollection?.Id ?? Id);
        var collectionAppends = await collectionClient.WithAllAppends();
        
        if (collectionAppends is null) return;

        await using CollectionLogic collection = new(collectionAppends, Library);
        await collection.Process();
    }
}
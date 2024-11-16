using System.Linq.Expressions;
using NoMercy.Database.Models;

namespace NoMercy.Data.Repositories;

public interface ICollectionRepository
{
    Task<List<Collection>> GetCollectionsAsync(Guid userId, string language, int take, int page);
    Task<Collection?> GetCollectionAsync(Guid userId, int id, string? language, string country);
    IQueryable<Collection> GetCollectionItems(Guid userId, string? language, int take = 1,
        int page = 1, Expression<Func<Collection, object>>? orderByExpression = null, string? direction = null);
    Task<Collection?> GetAvailableCollectionAsync(Guid userId, int id);
    Task<Collection?> GetWatchCollectionAsync(Guid userId, int id, string language, string country);
    Task<bool> LikeCollectionAsync(int id, Guid userId, bool like);
}

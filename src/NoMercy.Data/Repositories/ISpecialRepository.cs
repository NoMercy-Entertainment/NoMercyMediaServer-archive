using System.Linq.Expressions;
using NoMercy.Database.Models;

namespace NoMercy.Data.Repositories;

public interface ISpecialRepository
{
    Task<List<Special>> GetSpecialsAsync(Guid userId, string language, int take, int page);
    Task<Special?> GetSpecialAsync(Guid userId, Ulid id);
    IQueryable<Special> GetSpecialItems(Guid userId, string? language, int take = 1, int page = 1,
        Expression<Func<Special, object>>? orderByExpression = null, string? direction = null);
}

using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IBaseRepository<T> where T : EntityBase
{
    Task<T> SoftDeleteAsync(T entity);
}
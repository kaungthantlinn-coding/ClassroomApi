using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IMaterialRepository : IBaseRepository<Material>
{
    Task<List<Material>> GetMaterialsByCourseIdAsync(int courseId);
    Task<Material?> GetByIdAsync(int materialId);
    Task<Material> CreateAsync(Material material);
    Task<Material> UpdateAsync(Material material);
    Task DeleteAsync(Material material);
    Task<bool> MaterialExistsAsync(int materialId);
    Task<bool> IsUserTeacherOfMaterialCourseAsync(int materialId, int userId);
    Task SaveChangesAsync();
}
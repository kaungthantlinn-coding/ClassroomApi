using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class MaterialRepository : IMaterialRepository
{
    private readonly ClassroomContext _context;

    public MaterialRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<List<Material>> GetMaterialsByCourseIdAsync(int courseId)
    {
        return await _context.Materials
            .Include(m => m.MaterialAttachments) // Include material attachments
            .Where(m => m.ClassId == courseId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Material?> GetByIdAsync(int materialId)
    {
        return await _context.Materials
            .Include(m => m.Class)
            .Include(m => m.MaterialAttachments) // Include material attachments
            .FirstOrDefaultAsync(m => m.MaterialId == materialId && !m.IsDeleted);
    }

    public async Task<Material> CreateAsync(Material material)
    {
        _context.Materials.Add(material);
        await SaveChangesAsync();
        return material;
    }

    public async Task<Material> UpdateAsync(Material material)
    {
        _context.Materials.Update(material);
        await SaveChangesAsync();
        return material;
    }

    public async Task DeleteAsync(Material material)
    {
        // Implement soft delete
        material.IsDeleted = true;
        material.UpdatedAt = DateTime.UtcNow;
        _context.Materials.Update(material);
        await SaveChangesAsync();
    }

    public async Task<Material> SoftDeleteAsync(Material material)
    {
        material.IsDeleted = true;
        material.UpdatedAt = DateTime.UtcNow;
        _context.Materials.Update(material);
        await SaveChangesAsync();
        return material;
    }

    public async Task<bool> MaterialExistsAsync(int materialId)
    {
        return await _context.Materials.AnyAsync(m => m.MaterialId == materialId && !m.IsDeleted);
    }

    public async Task<bool> IsUserTeacherOfMaterialCourseAsync(int materialId, int userId)
    {
        var material = await _context.Materials
            .Include(m => m.Class)
            .ThenInclude(c => c.CourseMembers)
            .FirstOrDefaultAsync(m => m.MaterialId == materialId && !m.IsDeleted);

        if (material == null || material.Class == null)
        {
            return false;
        }

        return material.Class.CourseMembers.Any(cm => cm.UserId == userId && cm.Role == "Teacher");
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
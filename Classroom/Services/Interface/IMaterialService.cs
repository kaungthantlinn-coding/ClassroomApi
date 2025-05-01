using Classroom.Dtos.Material;

namespace Classroom.Services.Interface;

public interface IMaterialService
{
    Task<List<MaterialDto>> GetCourseMaterialsAsync(int courseId, int userId);
    Task<MaterialDto?> GetMaterialByIdAsync(int materialId, int userId);
    Task<MaterialDto> CreateMaterialAsync(int courseId, CreateMaterialDto createMaterialDto, int teacherId);
    Task<MaterialDto?> UpdateMaterialAsync(int materialId, UpdateMaterialDto updateMaterialDto, int userId);
    Task<bool> DeleteMaterialAsync(int materialId, int userId);
}
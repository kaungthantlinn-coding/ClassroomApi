using Classroom.Dtos.Material;
using Classroom.Helpers;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Services.Implementation;

public class MaterialService(IMaterialRepository materialRepository, ICourseRepository courseRepository) : IMaterialService
{
    private readonly IMaterialRepository _materialRepository = materialRepository;
    private readonly ICourseRepository _courseRepository = courseRepository;

    public async Task<List<MaterialDto>> GetCourseMaterialsAsync(int courseId, int userId)
    {
        // Check if user is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (!isEnrolled)
        {
            return []; // Return empty list if user is not enrolled
        }

        var materials = await _materialRepository.GetMaterialsByCourseIdAsync(courseId);
        return materials.Select(MapMaterialToDto).ToList();
    }

    public async Task<MaterialDto?> GetMaterialByIdAsync(int materialId, int userId)
    {
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material is null || material.ClassId is null)
        {
            return null;
        }

        // Check if user is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(material.ClassId.Value, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        return MapMaterialToDto(material);
    }

    public async Task<MaterialDto> CreateMaterialAsync(int courseId, CreateMaterialDto createMaterialDto, int teacherId)
    {
        // Check if user is a teacher in the course
        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can create materials for this course");
        }

        // Get course details
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null)
        {
            throw new KeyNotFoundException($"Course with ID {courseId} not found");
        }

        // Generate random color if not provided or is the default placeholder "string"
        string materialColor = string.IsNullOrWhiteSpace(createMaterialDto.Color) || createMaterialDto.Color == "string"
            ? ColorHelper.GetRandomBackgroundColor()
            : createMaterialDto.Color;

        // Create new material
        var material = new Material
        {
            MaterialGuid = Guid.NewGuid(),
            Title = createMaterialDto.Title,
            Description = createMaterialDto.Description,
            Topic = createMaterialDto.Topic,
            ScheduledFor = createMaterialDto.ScheduledFor,
            ClassId = courseId,
            ClassName = course.Name,
            Section = course.Section,
            CreatedAt = DateTime.UtcNow,
            Color = materialColor
        };

        await _materialRepository.CreateAsync(material);
        return MapMaterialToDto(material);
    }

    public async Task<MaterialDto?> UpdateMaterialAsync(int materialId, UpdateMaterialDto updateMaterialDto, int userId)
    {
        // Check if material exists
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material is null)
        {
            return null;
        }

        // Check if user is a teacher in the course
        var isTeacher = await _materialRepository.IsUserTeacherOfMaterialCourseAsync(materialId, userId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can update materials for this course");
        }

        // Update material properties
        material.Title = updateMaterialDto.Title;
        material.Description = updateMaterialDto.Description;
        material.Topic = updateMaterialDto.Topic;
        material.ScheduledFor = updateMaterialDto.ScheduledFor;

        // Only update color if it's not the default placeholder "string"
        if (updateMaterialDto.Color != "string")
        {
            material.Color = updateMaterialDto.Color;
        }

        material.UpdatedAt = DateTime.UtcNow;

        await _materialRepository.UpdateAsync(material);
        return MapMaterialToDto(material);
    }

    public async Task<bool> DeleteMaterialAsync(int materialId, int userId)
    {
        // Check if material exists
        var material = await _materialRepository.GetByIdAsync(materialId);
        if (material is null)
        {
            return false;
        }

        // Check if user is a teacher in the course
        var isTeacher = await _materialRepository.IsUserTeacherOfMaterialCourseAsync(materialId, userId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can delete materials for this course");
        }

        await _materialRepository.DeleteAsync(material);
        return true;
    }

    private MaterialDto MapMaterialToDto(Material material)
    {
        var materialDto = new MaterialDto
        {
            MaterialId = material.MaterialId,
            MaterialGuid = material.MaterialGuid,
            Title = material.Title,
            Description = material.Description,
            Topic = material.Topic,
            ScheduledFor = material.ScheduledFor,
            CourseId = material.ClassId,
            CourseName = material.ClassName,
            Section = material.Section,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt,
            Color = material.Color
        };

        // Include file attachments if available
        if (material.MaterialAttachments != null && material.MaterialAttachments.Any())
        {
            // Map all attachments to the Files collection
            materialDto.Files = material.MaterialAttachments.Select(attachment => new MaterialFileInfo
            {
                AttachmentId = attachment.AttachmentId,
                Name = attachment.Name,
                Type = attachment.Type,
                Url = attachment.Url,
                // These properties might not be available in MaterialAttachment
                Size = 0, // Default value since MaterialAttachment might not have Size
                UploadDate = DateTime.UtcNow // Default value since MaterialAttachment might not have UploadDate
            }).ToList();

            // Set the first attachment as the main file for backward compatibility
            if (materialDto.Files.Count > 0)
            {
                materialDto.File = materialDto.Files[0];
            }
        }

        return materialDto;
    }
}
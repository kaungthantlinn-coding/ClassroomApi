using Classroom.Dtos.Announcement;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;

namespace Classroom.Services.Implementation;

public class AnnouncementService(IAnnouncementRepository announcementRepository, ICourseRepository courseRepository) : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepository = announcementRepository;
    private readonly ICourseRepository _courseRepository = courseRepository;

    public async Task<List<AnnouncementDto>> GetCourseAnnouncementsAsync(int courseId, int userId)
    {
        // Check if user is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (!isEnrolled)
        {
            return []; // Return empty list if user is not enrolled
        }

        var announcements = await _announcementRepository.GetAnnouncementsByCourseIdAsync(courseId);
        return announcements.Select(MapAnnouncementToDto).ToList();
    }

    public async Task<AnnouncementDto?> GetAnnouncementByIdAsync(int announcementId, int userId)
    {
        var announcement = await _announcementRepository.GetByIdAsync(announcementId);
        if (announcement is null)
        {
            return null;
        }

        // Check if user is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(announcement.ClassId, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        return MapAnnouncementToDto(announcement);
    }

    public async Task<AnnouncementDto> CreateAnnouncementAsync(int courseId, CreateAnnouncementDto createAnnouncementDto, int teacherId)
    {
        // Check if user is a teacher in the course
        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can create announcements for this course");
        }

        // Get course details
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null)
        {
            throw new KeyNotFoundException($"Course with ID {courseId} not found");
        }

        // Get teacher details
        var teacher = course.CourseMembers.FirstOrDefault(cm => cm.UserId == teacherId)?.User;
        if (teacher is null)
        {
            throw new KeyNotFoundException($"Teacher with ID {teacherId} not found in course");
        }

        // Create new announcement
        var announcement = new Announcement
        {
            AnnouncementGuid = Guid.NewGuid(),
            ClassId = courseId,
            Content = createAnnouncementDto.Content,
            AuthorId = teacherId,
            AuthorName = teacher.Name,
            AuthorAvatar = teacher.Avatar,
            CreatedAt = DateTime.UtcNow
        };

        await _announcementRepository.CreateAsync(announcement);
        return MapAnnouncementToDto(announcement);
    }

    public async Task<AnnouncementDto?> UpdateAnnouncementAsync(int announcementId, UpdateAnnouncementDto updateAnnouncementDto, int userId)
    {
        // Check if announcement exists
        var announcement = await _announcementRepository.GetByIdAsync(announcementId);
        if (announcement is null)
        {
            return null;
        }

        // Check if user is a teacher in the course
        var isTeacher = await _announcementRepository.IsUserTeacherOfAnnouncementCourseAsync(announcementId, userId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can update announcements for this course");
        }

        // Update announcement properties
        announcement.Content = updateAnnouncementDto.Content;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _announcementRepository.UpdateAsync(announcement);
        return MapAnnouncementToDto(announcement);
    }

    public async Task<bool> DeleteAnnouncementAsync(int announcementId, int userId)
    {
        // Check if announcement exists
        var announcement = await _announcementRepository.GetByIdAsync(announcementId);
        if (announcement is null)
        {
            return false;
        }

        // Check if user is a teacher in the course
        var isTeacher = await _announcementRepository.IsUserTeacherOfAnnouncementCourseAsync(announcementId, userId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can delete announcements for this course");
        }

        await _announcementRepository.DeleteAsync(announcement);
        return true;
    }

    private static AnnouncementDto MapAnnouncementToDto(Announcement announcement)
    {
        return new AnnouncementDto
        {
            AnnouncementId = announcement.AnnouncementId,
            AnnouncementGuid = announcement.AnnouncementGuid,
            CourseId = announcement.ClassId,
            CourseName = announcement.Class?.Name,
            Content = announcement.Content,
            AuthorId = announcement.AuthorId,
            AuthorName = announcement.AuthorName,
            AuthorAvatar = announcement.AuthorAvatar,
            CreatedAt = announcement.CreatedAt,
            UpdatedAt = announcement.UpdatedAt
        };
    }
}
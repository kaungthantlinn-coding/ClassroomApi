using Classroom.Dtos.Announcement;

namespace Classroom.Services.Interface;

public interface IAnnouncementService
{
    Task<List<AnnouncementDto>> GetCourseAnnouncementsAsync(int courseId, int userId);
    Task<AnnouncementDto?> GetAnnouncementByIdAsync(int announcementId, int userId);
    Task<AnnouncementDto> CreateAnnouncementAsync(int courseId, CreateAnnouncementDto createAnnouncementDto, int teacherId);
    Task<AnnouncementDto?> UpdateAnnouncementAsync(int announcementId, UpdateAnnouncementDto updateAnnouncementDto, int userId);
    Task<bool> DeleteAnnouncementAsync(int announcementId, int userId);
}
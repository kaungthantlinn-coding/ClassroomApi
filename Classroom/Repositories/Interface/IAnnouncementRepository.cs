using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IAnnouncementRepository
{
    Task<List<Announcement>> GetAnnouncementsByCourseIdAsync(int courseId);
    Task<Announcement?> GetByIdAsync(int announcementId);
    Task<Announcement> CreateAsync(Announcement announcement);
    Task<Announcement> UpdateAsync(Announcement announcement);
    Task DeleteAsync(Announcement announcement);
    Task<bool> AnnouncementExistsAsync(int announcementId);
    Task<bool> IsUserTeacherOfAnnouncementCourseAsync(int announcementId, int userId);
    Task SaveChangesAsync();
}
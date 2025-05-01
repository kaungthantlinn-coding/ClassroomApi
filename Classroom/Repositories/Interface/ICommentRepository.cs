using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface ICommentRepository
{
    Task<List<AnnouncementComment>> GetAnnouncementCommentsAsync(int announcementId);
    Task<AnnouncementComment?> GetCommentByIdAsync(int commentId);
    Task<AnnouncementComment> CreateCommentAsync(AnnouncementComment comment);
    Task<AnnouncementComment?> UpdateCommentAsync(AnnouncementComment comment);
    Task DeleteCommentAsync(AnnouncementComment comment);
    Task<bool> IsUserAuthorOfCommentAsync(int userId, int commentId);
    Task<bool> IsUserTeacherOfCommentCourseAsync(int userId, int commentId);
    Task<bool> IsUserInCourseAsync(int userId, int announcementId);
    Task SaveChangesAsync();
}
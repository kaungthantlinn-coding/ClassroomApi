using Classroom.Dtos.Announcement;

namespace Classroom.Services.Interface;

public interface ICommentService
{
    Task<List<CommentDto>> GetAnnouncementCommentsAsync(int announcementId, int userId);
    Task<CommentDto> CreateCommentAsync(int announcementId, CreateCommentDto createCommentDto, int userId);
    Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto, int userId);
    Task<bool> DeleteCommentAsync(int commentId, int userId);
}
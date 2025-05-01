using Classroom.Dtos.Announcement;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;

namespace Classroom.Services.Implementation;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IAnnouncementRepository _announcementRepository;

    public CommentService(ICommentRepository commentRepository, IAnnouncementRepository announcementRepository)
    {
        _commentRepository = commentRepository;
        _announcementRepository = announcementRepository;
    }

    public async Task<List<CommentDto>> GetAnnouncementCommentsAsync(int announcementId, int userId)
    {
        // Check if user is enrolled in the course
        var isEnrolled = await _commentRepository.IsUserInCourseAsync(userId, announcementId);
        if (!isEnrolled)
        {
            throw new UnauthorizedAccessException("You are not enrolled in this course");
        }

        var comments = await _commentRepository.GetAnnouncementCommentsAsync(announcementId);
        return comments.Select(MapCommentToDto).ToList();
    }

    public async Task<CommentDto> CreateCommentAsync(int announcementId, CreateCommentDto createCommentDto, int userId)
    {
        // Check if announcement exists
        var announcementExists = await _announcementRepository.AnnouncementExistsAsync(announcementId);
        if (!announcementExists)
        {
            throw new KeyNotFoundException($"Announcement with ID {announcementId} not found");
        }

        // Check if user is enrolled in the course
        var isEnrolled = await _commentRepository.IsUserInCourseAsync(userId, announcementId);
        if (!isEnrolled)
        {
            throw new UnauthorizedAccessException("You are not enrolled in this course");
        }

        // Get user details from the database
        // We'll just use the user ID for now, as we don't need to query for the user entity
        // The repository will include the user details when creating the comment

        // Create new comment
        var comment = new AnnouncementComment
        {
            AnnouncementId = announcementId,
            Content = createCommentDto.Content,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = createCommentDto.IsPrivate
        };

        // The repository will include the user details when fetching the comment

        await _commentRepository.CreateCommentAsync(comment);
        return MapCommentToDto(comment);
    }

    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateCommentDto, int userId)
    {
        // Check if comment exists
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment is null)
        {
            return null;
        }

        // Check if user is the author or a teacher
        var isAuthor = await _commentRepository.IsUserAuthorOfCommentAsync(userId, commentId);
        var isTeacher = await _commentRepository.IsUserTeacherOfCommentCourseAsync(userId, commentId);

        if (!isAuthor && !isTeacher)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this comment");
        }

        // Update comment properties
        comment.Content = updateCommentDto.Content;
        comment.IsPrivate = updateCommentDto.IsPrivate;
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateCommentAsync(comment);
        return MapCommentToDto(comment);
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int userId)
    {
        // Check if comment exists
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment is null)
        {
            return false;
        }

        // Check if user is the author or a teacher
        var isAuthor = await _commentRepository.IsUserAuthorOfCommentAsync(userId, commentId);
        var isTeacher = await _commentRepository.IsUserTeacherOfCommentCourseAsync(userId, commentId);

        if (!isAuthor && !isTeacher)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this comment");
        }

        await _commentRepository.DeleteCommentAsync(comment);
        return true;
    }

    private static CommentDto MapCommentToDto(AnnouncementComment comment)
    {
        return new CommentDto
        {
            CommentId = comment.CommentId,
            AnnouncementId = comment.AnnouncementId,
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            AuthorName = comment.AuthorName ?? "Unknown",
            AuthorAvatar = comment.AuthorAvatar,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsPrivate = comment.IsPrivate ?? false
        };
    }
}
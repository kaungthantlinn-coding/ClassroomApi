using Classroom.Dtos.Announcement;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;

namespace Classroom.Services.Implementation;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IAnnouncementRepository announcementRepository,
        IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _announcementRepository = announcementRepository;
        _userRepository = userRepository;
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
        var user = await _userRepository.GetByIdAsync(userId);

        // Create new comment
        var comment = new AnnouncementComment
        {
            AnnouncementId = announcementId,
            Content = createCommentDto.Content,
            AuthorId = userId,
            AuthorName = user?.Name, // Set the author name from the user object
            AuthorAvatar = user?.Avatar, // Set the author avatar from the user object
            CreatedAt = DateTime.UtcNow,
            IsPrivate = createCommentDto.IsPrivate
        };

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
            AuthorName = comment.Author?.Name ?? comment.AuthorName ?? "Unknown",
            AuthorAvatar = comment.Author?.Avatar ?? comment.AuthorAvatar,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsPrivate = comment.IsPrivate ?? false
        };
    }
}
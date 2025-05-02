using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class CommentRepository : ICommentRepository
{
    private readonly ClassroomContext _context;

    public CommentRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<List<AnnouncementComment>> GetAnnouncementCommentsAsync(int announcementId)
    {
        return await _context.AnnouncementComments
            .Where(c => c.AnnouncementId == announcementId && !c.IsDeleted)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<AnnouncementComment?> GetCommentByIdAsync(int commentId)
    {
        return await _context.AnnouncementComments
            .Include(c => c.Author)
            .Include(c => c.Announcement)
            .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);
    }

    public async Task<AnnouncementComment> CreateCommentAsync(AnnouncementComment comment)
    {
        _context.AnnouncementComments.Add(comment);
        await SaveChangesAsync();
        return comment;
    }

    public async Task<AnnouncementComment?> UpdateCommentAsync(AnnouncementComment comment)
    {
        _context.AnnouncementComments.Update(comment);
        await SaveChangesAsync();
        return comment;
    }

    public async Task DeleteCommentAsync(AnnouncementComment comment)
    {
        // Implement soft delete
        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        _context.AnnouncementComments.Update(comment);
        await SaveChangesAsync();
    }

    public async Task<AnnouncementComment> SoftDeleteAsync(AnnouncementComment comment)
    {
        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        _context.AnnouncementComments.Update(comment);
        await SaveChangesAsync();
        return comment;
    }

    public async Task<bool> IsUserAuthorOfCommentAsync(int userId, int commentId)
    {
        return await _context.AnnouncementComments
            .AnyAsync(c => c.CommentId == commentId && c.AuthorId == userId && !c.IsDeleted);
    }

    public async Task<bool> IsUserTeacherOfCommentCourseAsync(int userId, int commentId)
    {
        var comment = await _context.AnnouncementComments
            .Include(c => c.Announcement)
            .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

        if (comment == null)
        {
            return false;
        }

        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == comment.Announcement.ClassId && cm.UserId == userId && cm.Role == "Teacher");
    }

    public async Task<bool> IsUserInCourseAsync(int userId, int announcementId)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId && !a.IsDeleted);

        if (announcement == null)
        {
            return false;
        }

        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == announcement.ClassId && cm.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
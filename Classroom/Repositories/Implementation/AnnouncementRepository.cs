using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly ClassroomContext _context;

    public AnnouncementRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<List<Announcement>> GetAnnouncementsByCourseIdAsync(int courseId)
    {
        return await _context.Announcements
            .Where(a => a.ClassId == courseId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Announcement?> GetByIdAsync(int announcementId)
    {
        return await _context.Announcements
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId && !a.IsDeleted);
    }

    public async Task<Announcement> CreateAsync(Announcement announcement)
    {
        _context.Announcements.Add(announcement);
        await SaveChangesAsync();
        return announcement;
    }

    public async Task<Announcement> UpdateAsync(Announcement announcement)
    {
        _context.Announcements.Update(announcement);
        await SaveChangesAsync();
        return announcement;
    }

    public async Task DeleteAsync(Announcement announcement)
    {
        // Implement soft delete
        announcement.IsDeleted = true;
        announcement.UpdatedAt = DateTime.UtcNow;
        _context.Announcements.Update(announcement);
        await SaveChangesAsync();
    }

    public async Task<Announcement> SoftDeleteAsync(Announcement announcement)
    {
        announcement.IsDeleted = true;
        announcement.UpdatedAt = DateTime.UtcNow;
        _context.Announcements.Update(announcement);
        await SaveChangesAsync();
        return announcement;
    }

    public async Task<bool> AnnouncementExistsAsync(int announcementId)
    {
        return await _context.Announcements.AnyAsync(a => a.AnnouncementId == announcementId && !a.IsDeleted);
    }

    public async Task<bool> IsUserTeacherOfAnnouncementCourseAsync(int announcementId, int userId)
    {
        var announcement = await _context.Announcements
            .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);

        if (announcement == null)
        {
            return false;
        }

        return await _context.CourseMembers
            .AnyAsync(cm => cm.CourseId == announcement.ClassId && cm.UserId == userId && cm.Role == "Teacher");
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
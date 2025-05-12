using Classroom.Dtos.Submission;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Services.Implementation;

/// <summary>
/// Service for handling file operations related to submissions
/// </summary>
public class FileService : IFileService
{
    private readonly ClassroomContext _context;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly string _uploadsDirectory;

    public FileService(ClassroomContext context, ISubmissionRepository submissionRepository, IWebHostEnvironment env)
    {
        _context = context;
        _submissionRepository = submissionRepository;
        _uploadsDirectory = Path.Combine(env.ContentRootPath, "uploads", "submissions");

        // Ensure the uploads directory exists
        if (!Directory.Exists(_uploadsDirectory))
        {
            Directory.CreateDirectory(_uploadsDirectory);
        }
    }

   
    public async Task<SubmissionAttachment> SaveSubmissionFileAsync(IFormFile file, int submissionId)
    {
        Console.WriteLine($"SaveSubmissionFileAsync - SubmissionId: {submissionId}, File: {file.FileName}, Size: {file.Length} bytes");

        // Verify submission exists
        var submission = await _submissionRepository.GetSubmissionByIdAsync(submissionId);
        if (submission == null)
        {
            Console.WriteLine($"Submission with ID {submissionId} not found");
            throw new KeyNotFoundException($"Submission with ID {submissionId} not found");
        }

        Console.WriteLine($"Submission found - UserId: {submission.UserId}");

        // Generate a unique filename
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadsDirectory, fileName);

        Console.WriteLine($"Saving file to: {filePath}");

        // Save the file to disk
        try
        {
            // Ensure the directory exists (in case it was deleted after service initialization)
            if (!Directory.Exists(_uploadsDirectory))
            {
                Directory.CreateDirectory(_uploadsDirectory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                Console.WriteLine("File saved to disk successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file to disk: {ex.Message}");
            throw new IOException($"Error saving file to disk: {ex.Message}", ex);
        }

        // Create the attachment record
        var attachment = new SubmissionAttachment
        {
            SubmissionId = submissionId,
            Name = file.FileName,
            Type = file.ContentType,
            Size = (int)file.Length,
            Url = $"/submissions/files/{fileName}",
            UploadDate = DateTime.UtcNow
        };

        Console.WriteLine($"Creating attachment record - Name: {attachment.Name}, Type: {attachment.Type}, Size: {attachment.Size}, Url: {attachment.Url}");

        try
        {
            _context.SubmissionAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Attachment record created successfully - AttachmentId: {attachment.AttachmentId}");
        }
        catch (DbUpdateException ex)
        {
            // If we failed to create the database record, try to clean up the file
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Deleted file after database error: {filePath}");
                }
            }
            catch (Exception cleanupEx)
            {
                Console.WriteLine($"Error cleaning up file after database error: {cleanupEx.Message}");
            }

            Console.WriteLine($"Error creating attachment record: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating attachment record: {ex.Message}");
            throw;
        }

        return attachment;
    }

   
    public async Task<SubmissionAttachment?> GetSubmissionFileByIdAsync(int fileId)
    {
        return await _context.SubmissionAttachments
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);
    }

    
    public async Task<bool> DeleteSubmissionFileAsync(int fileId, int userId)
    {
        var attachment = await _context.SubmissionAttachments
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);

        if (attachment == null)
        {
            return false;
        }

        // Check if user owns the submission
        if (attachment.Submission.UserId != userId)
        {
            // Check if user is a teacher for this assignment
            var isTeacher = await _submissionRepository.IsTeacherForAssignmentAsync(userId, attachment.Submission.AssignmentId);
            if (!isTeacher)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this file");
            }
        }

        try
        {
            // Delete the physical file
            var fileName = Path.GetFileName(attachment.Url);
            var filePath = Path.Combine(_uploadsDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Remove the database record
            _context.SubmissionAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            throw;
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Error removing attachment record: {ex.Message}");
            throw;
        }
    }

    public async Task<Stream> GetSubmissionFileStreamAsync(int fileId)
    {
        var attachment = await _context.SubmissionAttachments.FindAsync(fileId);
        if (attachment == null)
        {
            throw new KeyNotFoundException($"File with ID {fileId} not found");
        }

        var fileName = Path.GetFileName(attachment.Url);
        var filePath = Path.Combine(_uploadsDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File {fileName} not found on server");
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }
    public async Task<List<SubmissionAttachment>> GetSubmissionFilesAsync(int submissionId)
    {
        return await _context.SubmissionAttachments
            .Where(a => a.SubmissionId == submissionId)
            .ToListAsync();
    }
}

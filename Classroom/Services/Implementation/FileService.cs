using Classroom.Dtos.Material;
using Classroom.Dtos.Submission;
using Classroom.Models;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Services.Implementation;

public class FileService : IFileService
{
    private readonly ClassroomContext _context;
    private readonly string _uploadsDirectory;

    public FileService(ClassroomContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _uploadsDirectory = Path.Combine(environment.ContentRootPath, "Uploads");

        // Ensure the uploads directory exists
        if (!Directory.Exists(_uploadsDirectory))
        {
            Directory.CreateDirectory(_uploadsDirectory);
        }
    }

    public async Task<SubmissionAttachment> SaveSubmissionFileAsync(IFormFile file, int submissionId, int? fileId = null)
    {
        // Check if submission exists
        var submission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null)
        {
            throw new KeyNotFoundException($"Submission with ID {submissionId} not found");
        }

        // If fileId is provided, only delete that specific file
        if (fileId.HasValue)
        {
            var existingFile = await _context.SubmissionAttachments
                .FirstOrDefaultAsync(a => a.AttachmentId == fileId.Value && a.SubmissionId == submissionId);

            if (existingFile != null)
            {
                // Extract the filename from the URL
                var urlParts = existingFile.Url.Split('/');
                var filename = urlParts[urlParts.Length - 1];
                var existingFilePath = Path.Combine(_uploadsDirectory, filename);

                // Delete the physical file if it exists
                if (File.Exists(existingFilePath))
                {
                    File.Delete(existingFilePath);
                }

                // Remove from database
                _context.SubmissionAttachments.Remove(existingFile);
            }
            else
            {
                // If the specified fileId doesn't exist, just continue without deleting anything
                Console.WriteLine($"Warning: File with ID {fileId.Value} not found for submission {submissionId}");
            }
        }
        else
        {
            // If no fileId is provided, delete all existing files (original behavior)
            var existingFiles = await _context.SubmissionAttachments
                .Where(a => a.SubmissionId == submissionId)
                .ToListAsync();

            // Delete existing files (both from database and disk)
            foreach (var existingFile in existingFiles)
            {
                // Extract the filename from the URL
                var urlParts = existingFile.Url.Split('/');
                var filename = urlParts[urlParts.Length - 1];
                var existingFilePath = Path.Combine(_uploadsDirectory, filename);

                // Delete the physical file if it exists
                if (File.Exists(existingFilePath))
                {
                    File.Delete(existingFilePath);
                }

                // Remove from database
                _context.SubmissionAttachments.Remove(existingFile);
            }
        }

        // Create a unique filename
        var fileName = Path.GetFileName(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var physicalPath = Path.Combine(_uploadsDirectory, uniqueFileName);

        // Save the file to disk
        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create the attachment record
        var attachment = new SubmissionAttachment
        {
            SubmissionId = submissionId,
            Name = fileName,
            Type = file.ContentType,
            Size = (int)file.Length,
            Url = $"/api/submissions/files/{uniqueFileName}",
            UploadDate = DateTime.UtcNow
        };

        // Store the physical path in a dictionary or other storage mechanism
        // since the SubmissionAttachment model doesn't have a FilePath property
        var filePathMapping = new Dictionary<string, string>
        {
            { uniqueFileName, physicalPath }
        };

        _context.SubmissionAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return attachment;
    }

    public async Task<SubmissionAttachment?> GetSubmissionFileByIdAsync(int fileId)
    {
        return await _context.SubmissionAttachments
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);
    }

    public async Task<bool> DeleteSubmissionFileAsync(int fileId, int userId)
    {
        var attachment = await _context.SubmissionAttachments
            .Include(a => a.Submission)
                .ThenInclude(s => s.Assignment)
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);

        if (attachment == null)
        {
            return false;
        }

        // Check if user owns the submission or is a teacher
        var isOwner = attachment.Submission.UserId == userId;
        var isTeacher = await _context.CourseMembers
            .AnyAsync(m => m.CourseId == attachment.Submission.Assignment.ClassId &&
                           m.UserId == userId &&
                           m.Role == "Teacher");

        if (!isOwner && !isTeacher)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this file");
        }

        // Extract the filename from the URL
        var urlParts = attachment.Url.Split('/');
        var filename = urlParts[urlParts.Length - 1];
        var physicalPath = Path.Combine(_uploadsDirectory, filename);

        // Delete the physical file if it exists
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        // Remove the database record
        _context.SubmissionAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Stream> GetSubmissionFileStreamAsync(int fileId)
    {
        var attachment = await _context.SubmissionAttachments
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);

        if (attachment == null)
        {
            throw new KeyNotFoundException($"File with ID {fileId} not found");
        }

        // Extract the filename from the URL
        var urlParts = attachment.Url.Split('/');
        var filename = urlParts[urlParts.Length - 1];
        var physicalPath = Path.Combine(_uploadsDirectory, filename);

        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException($"Physical file for attachment ID {fileId} not found");
        }

        return new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
    }

    public async Task<List<SubmissionAttachment>> GetSubmissionFilesAsync(int submissionId)
    {
        return await _context.SubmissionAttachments
            .Where(a => a.SubmissionId == submissionId)
            .ToListAsync();
    }

    public async Task<MaterialAttachment> SaveMaterialFileAsync(IFormFile file, int materialId)
    {
        // Check if material exists
        var material = await _context.Materials
            .FirstOrDefaultAsync(m => m.MaterialId == materialId && !m.IsDeleted);

        if (material == null)
        {
            throw new KeyNotFoundException($"Material with ID {materialId} not found");
        }

        // Create a unique filename
        var fileName = Path.GetFileName(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var physicalPath = Path.Combine(_uploadsDirectory, uniqueFileName);

        // Save the file to disk
        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create the attachment record
        var attachment = new MaterialAttachment
        {
            MaterialId = materialId,
            Name = fileName,
            Type = file.ContentType,
            Url = $"/api/materials/files/{uniqueFileName}",
            Thumbnail = null // No thumbnail for now
        };

        _context.MaterialAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return attachment;
    }

    public async Task<MaterialAttachment?> GetMaterialFileByIdAsync(int fileId)
    {
        return await _context.MaterialAttachments
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);
    }

    public async Task<bool> DeleteMaterialFileAsync(int fileId, int userId)
    {
        var attachment = await _context.MaterialAttachments
            .Include(a => a.Material)
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);

        if (attachment == null)
        {
            return false;
        }

        // Check if user is a teacher in the course
        var isTeacher = await _context.CourseMembers
            .AnyAsync(m => m.CourseId == attachment.Material.ClassId &&
                           m.UserId == userId &&
                           m.Role == "Teacher");

        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can delete material files");
        }

        // Extract the filename from the URL
        var urlParts = attachment.Url.Split('/');
        var filename = urlParts[urlParts.Length - 1];
        var physicalPath = Path.Combine(_uploadsDirectory, filename);

        // Delete the physical file if it exists
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        // Remove the database record
        _context.MaterialAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Stream> GetMaterialFileStreamAsync(int fileId)
    {
        var attachment = await _context.MaterialAttachments
            .FirstOrDefaultAsync(a => a.AttachmentId == fileId);

        if (attachment == null)
        {
            throw new KeyNotFoundException($"File with ID {fileId} not found");
        }

        // Extract the filename from the URL
        var urlParts = attachment.Url.Split('/');
        var filename = urlParts[urlParts.Length - 1];
        var physicalPath = Path.Combine(_uploadsDirectory, filename);

        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException($"Physical file for attachment ID {fileId} not found");
        }

        return new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
    }

    public async Task<List<MaterialAttachment>> GetMaterialFilesAsync(int materialId)
    {
        return await _context.MaterialAttachments
            .Where(a => a.MaterialId == materialId)
            .ToListAsync();
    }
}

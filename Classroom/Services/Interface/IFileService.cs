using Classroom.Dtos.Submission;
using Classroom.Models;
using Microsoft.AspNetCore.Http;

namespace Classroom.Services.Interface;

/// <summary>
/// Interface for file operations related to submissions
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Saves a file for a submission
    /// </summary>
    /// <param name="file">The file to save</param>
    /// <param name="submissionId">The ID of the submission</param>
    /// <returns>The created attachment record</returns>
    Task<SubmissionAttachment> SaveSubmissionFileAsync(IFormFile file, int submissionId);

    /// <summary>
    /// Gets a submission file by its ID
    /// </summary>
    /// <param name="fileId">The ID of the file to retrieve</param>
    /// <returns>The submission attachment or null if not found</returns>
    Task<SubmissionAttachment?> GetSubmissionFileByIdAsync(int fileId);

    /// <summary>
    /// Deletes a submission file
    /// </summary>
    /// <param name="fileId">The ID of the file to delete</param>
    /// <param name="userId">The ID of the user attempting to delete the file</param>
    /// <returns>True if the file was deleted, false if the file was not found</returns>
    Task<bool> DeleteSubmissionFileAsync(int fileId, int userId);

    /// <summary>
    /// Gets a file stream for a submission file
    /// </summary>
    /// <param name="fileId">The ID of the file</param>
    /// <returns>A stream containing the file data</returns>
    Task<Stream> GetSubmissionFileStreamAsync(int fileId);

    /// <summary>
    /// Gets all files for a submission
    /// </summary>
    /// <param name="submissionId">The ID of the submission</param>
    /// <returns>A list of submission attachments</returns>
    Task<List<SubmissionAttachment>> GetSubmissionFilesAsync(int submissionId);
}

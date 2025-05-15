using Classroom.Dtos.Submission;
using Classroom.Models;
using Microsoft.AspNetCore.Http;

namespace Classroom.Services.Interface;

/// <summary>
/// Interface for file operations related to submissions and materials
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Saves a file for a submission
    /// </summary>
    /// <param name="file">The file to save</param>
    /// <param name="submissionId">The ID of the submission</param>
    /// <param name="fileId">Optional: The ID of an existing file to replace. If not provided, a new file will be created.</param>
    /// <returns>The created attachment record</returns>
    Task<SubmissionAttachment> SaveSubmissionFileAsync(IFormFile file, int submissionId, int? fileId = null);

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

    /// <summary>
    /// Saves a file for a material
    /// </summary>
    /// <param name="file">The file to save</param>
    /// <param name="materialId">The ID of the material</param>
    /// <returns>The created attachment record</returns>
    Task<MaterialAttachment> SaveMaterialFileAsync(IFormFile file, int materialId);

    /// <summary>
    /// Gets a material file by its ID
    /// </summary>
    /// <param name="fileId">The ID of the file to retrieve</param>
    /// <returns>The material attachment or null if not found</returns>
    Task<MaterialAttachment?> GetMaterialFileByIdAsync(int fileId);

    /// <summary>
    /// Deletes a material file
    /// </summary>
    /// <param name="fileId">The ID of the file to delete</param>
    /// <param name="userId">The ID of the user attempting to delete the file</param>
    /// <returns>True if the file was deleted, false if the file was not found</returns>
    Task<bool> DeleteMaterialFileAsync(int fileId, int userId);

    /// <summary>
    /// Gets a file stream for a material file
    /// </summary>
    /// <param name="fileId">The ID of the file</param>
    /// <returns>A stream containing the file data</returns>
    Task<Stream> GetMaterialFileStreamAsync(int fileId);

    /// <summary>
    /// Gets all files for a material
    /// </summary>
    /// <param name="materialId">The ID of the material</param>
    /// <returns>A list of material attachments</returns>
    Task<List<MaterialAttachment>> GetMaterialFilesAsync(int materialId);
}

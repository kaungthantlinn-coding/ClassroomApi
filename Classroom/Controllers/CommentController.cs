using Classroom.Dtos.Announcement;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // GET: api/announcements/:id/comments
    // List comments on an announcement
    [HttpGet("announcements/{announcementId}/comments")]
    public async Task<IActionResult> GetAnnouncementComments(int announcementId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var comments = await _commentService.GetAnnouncementCommentsAsync(announcementId, currentUserId);
            return Ok(comments);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST: api/announcements/:id/comments
    // Add comment (student or teacher)
    [HttpPost("announcements/{announcementId}/comments")]
    public async Task<IActionResult> CreateComment(int announcementId, [FromBody] CreateCommentDto createCommentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var comment = await _commentService.CreateCommentAsync(announcementId, createCommentDto, currentUserId);
            return CreatedAtAction(nameof(GetAnnouncementComments), new { announcementId }, comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT: api/comments/:id
    // Edit comment (owner or teacher)
    [HttpPut("comments/{commentId}")]
    public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentDto updateCommentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var comment = await _commentService.UpdateCommentAsync(commentId, updateCommentDto, currentUserId);

            if (comment is null)
            {
                return NotFound($"Comment with ID {commentId} not found");
            }

            return Ok(comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // DELETE: api/comments/:id
    // Delete comment (owner or teacher)
    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _commentService.DeleteCommentAsync(commentId, currentUserId);

            if (!result)
            {
                return NotFound($"Comment with ID {commentId} not found");
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
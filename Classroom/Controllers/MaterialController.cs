using Classroom.Dtos.Material;
using Classroom.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Classroom.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class MaterialController : ControllerBase
{
    private readonly IMaterialService _materialService;

    public MaterialController(IMaterialService materialService)
    {
        _materialService = materialService;
    }

    // GET: api/courses/{courseId}/materials
    // List materials in a course
    [HttpGet("courses/{courseId}/materials")]
    public async Task<IActionResult> GetCourseMaterials(int courseId)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var materials = await _materialService.GetCourseMaterialsAsync(courseId, currentUserId);
        return Ok(materials);
    }

    // POST: api/courses/{courseId}/materials
    // Create material (teacher only)
    [HttpPost("courses/{courseId}/materials")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateMaterial(int courseId, [FromBody] CreateMaterialDto createMaterialDto)
    {
        // Validation removed as requested

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var material = await _materialService.CreateMaterialAsync(courseId, createMaterialDto, currentUserId);
            return CreatedAtAction(nameof(GetMaterialById), new { id = material.MaterialId }, material);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET: api/materials/{id}
    // Get material by ID
    [HttpGet("materials/{id}")]
    public async Task<IActionResult> GetMaterialById(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var material = await _materialService.GetMaterialByIdAsync(id, currentUserId);

        if (material == null)
        {
            return NotFound();
        }

        return Ok(material);
    }

    // PUT: api/materials/{id}
    // Update material (teacher only)
    [HttpPut("materials/{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateMaterial(int id, [FromBody] UpdateMaterialDto updateMaterialDto)
    {
        // Validation removed as requested

        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var material = await _materialService.UpdateMaterialAsync(id, updateMaterialDto, currentUserId);
            if (material == null)
            {
                return NotFound();
            }

            return Ok(material);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // DELETE: api/materials/{id}
    // Delete material (teacher only)
    [HttpDelete("materials/{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        try
        {
            var result = await _materialService.DeleteMaterialAsync(id, currentUserId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
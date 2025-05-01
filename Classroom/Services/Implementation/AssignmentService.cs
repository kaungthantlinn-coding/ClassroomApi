using Classroom.Dtos.Assignment;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;

namespace Classroom.Services.Implementation;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;

    public AssignmentService(IAssignmentRepository assignmentRepository, ICourseRepository courseRepository)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<List<AssignmentDto>> GetCourseAssignmentsAsync(int courseId, int userId)
    {
        // Check if user is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (!isEnrolled)
        {
            return new List<AssignmentDto>(); // Return empty list if user is not enrolled
        }

        var assignments = await _assignmentRepository.GetAssignmentsByCourseIdAsync(courseId);
        return assignments.Select(MapAssignmentToDto).ToList();
    }

    public async Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId, int userId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null || assignment.ClassId == null)
        {
            return null;
        }

        // Check if user is enrolled in the course
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(assignment.ClassId.Value, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        return MapAssignmentToDto(assignment);
    }

    public async Task<AssignmentDto> CreateAssignmentAsync(int courseId, CreateAssignmentDto createAssignmentDto, int teacherId)
    {
        // Check if user is a teacher of the course
        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, teacherId);
        if (!isTeacher)
        {
            throw new UnauthorizedAccessException("Only teachers can create assignments");
        }

        // Get course details
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            throw new KeyNotFoundException("Course not found");
        }

        // Create new assignment
        var assignment = new Assignment
        {
            AssignmentGuid = Guid.NewGuid(),
            Title = createAssignmentDto.Title,
            Instructions = createAssignmentDto.Instructions,
            Points = createAssignmentDto.Points,
            DueDate = createAssignmentDto.DueDate,
            DueTime = createAssignmentDto.DueTime,
            Topic = createAssignmentDto.Topic,
            ClassId = courseId,
            ClassName = course.Name,
            Section = course.Section,
            CreatedAt = DateTime.UtcNow,
            Status = createAssignmentDto.Status ?? "Published",
            AllowLateSubmissions = createAssignmentDto.AllowLateSubmissions,
            LateSubmissionPolicy = createAssignmentDto.LateSubmissionPolicy
        };

        var createdAssignment = await _assignmentRepository.CreateAsync(assignment);
        return MapAssignmentToDto(createdAssignment);
    }

    public async Task<AssignmentDto?> UpdateAssignmentAsync(int assignmentId, UpdateAssignmentDto updateAssignmentDto, int userId)
    {
        // Check if user is a teacher of the assignment's course
        var isTeacher = await _assignmentRepository.IsUserTeacherOfAssignmentCourseAsync(assignmentId, userId);
        if (!isTeacher)
        {
            return null; // User is not authorized to update this assignment
        }

        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            return null;
        }

        // Update assignment properties
        assignment.Title = updateAssignmentDto.Title;
        assignment.Instructions = updateAssignmentDto.Instructions;
        assignment.Points = updateAssignmentDto.Points;
        assignment.DueDate = updateAssignmentDto.DueDate;
        assignment.DueTime = updateAssignmentDto.DueTime;
        assignment.Topic = updateAssignmentDto.Topic;
        assignment.Status = updateAssignmentDto.Status;
        assignment.AllowLateSubmissions = updateAssignmentDto.AllowLateSubmissions;
        assignment.LateSubmissionPolicy = updateAssignmentDto.LateSubmissionPolicy;
        assignment.UpdatedAt = DateTime.UtcNow;

        var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment);
        return MapAssignmentToDto(updatedAssignment);
    }

    public async Task<bool> DeleteAssignmentAsync(int assignmentId, int userId)
    {
        // Check if user is a teacher of the assignment's course
        var isTeacher = await _assignmentRepository.IsUserTeacherOfAssignmentCourseAsync(assignmentId, userId);
        if (!isTeacher)
        {
            return false; // User is not authorized to delete this assignment
        }

        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            return false;
        }

        await _assignmentRepository.DeleteAsync(assignment);
        return true;
    }

    private static AssignmentDto MapAssignmentToDto(Assignment assignment)
    {
        return new AssignmentDto
        {
            AssignmentId = assignment.AssignmentId,
            Title = assignment.Title,
            Instructions = assignment.Instructions,
            Points = assignment.Points,
            DueDate = assignment.DueDate,
            DueTime = assignment.DueTime,
            Topic = assignment.Topic,
            CourseId = assignment.ClassId ?? 0,
            CourseName = assignment.ClassName ?? "Unknown Course",
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt,
            Status = assignment.Status,
            AllowLateSubmissions = assignment.AllowLateSubmissions,
            LateSubmissionPolicy = assignment.LateSubmissionPolicy
        };
    }
}
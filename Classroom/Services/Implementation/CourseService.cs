using Classroom.Dtos;
using Classroom.Dtos.Course;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;

namespace Classroom.Services.Implementation;

public class CourseService(ICourseRepository courseRepository) : ICourseService
{
    private readonly ICourseRepository _courseRepository = courseRepository;

    public async Task<List<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllCoursesAsync();
        return courses.Select(MapCourseToDto).ToList();
    }

    public async Task<List<CourseDto>> GetUserCoursesAsync(int userId)
    {
        var courses = await _courseRepository.GetCoursesByUserIdAsync(userId);
        return courses.Select(MapCourseToDto).ToList();
    }

    public async Task<CourseDto?> GetCourseByIdAsync(int courseId, int userId)
    {
        // Check if course exists and user is enrolled
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return null;
        }

        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        return MapCourseToDto(course);
    }

    public async Task<CourseDto> CreateCourseAsync(Classroom.Dtos.Course.CreateCourseDto createCourseDto, int teacherId)
    {
        // Generate a random enrollment code if not provided or empty
        string enrollmentCode = string.IsNullOrWhiteSpace(createCourseDto.EnrollmentCode)
            ? GenerateEnrollmentCode()
            : createCourseDto.EnrollmentCode;

        // Create new course
        var course = new Course
        {
            Name = createCourseDto.Name,
            Section = createCourseDto.Section,
            TeacherName = createCourseDto.TeacherName,
            CoverImage = createCourseDto.CoverImage,
            EnrollmentCode = enrollmentCode,
            Color = createCourseDto.Color,
            TextColor = createCourseDto.TextColor,
            Subject = createCourseDto.Subject,
            Room = createCourseDto.Room,
            CourseGuid = Guid.NewGuid()
        };

        var createdCourse = await _courseRepository.CreateAsync(course);

        // Add teacher as a course member
        var courseMember = new CourseMember
        {
            CourseId = createdCourse.CourseId,
            UserId = teacherId,
            Role = "Teacher"
        };

        await _courseRepository.AddMemberAsync(courseMember);

        return MapCourseToDto(createdCourse);
    }

    /// <summary>
    /// Generates a random enrollment code for a course
    /// </summary>
    /// <returns>A 6-character alphanumeric code</returns>
    private string GenerateEnrollmentCode()
    {
        // Define the characters that can be used in the enrollment code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Create a byte array for the random values
        byte[] randomBytes = new byte[6];

        // Fill the array with random values
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert random bytes to characters from our allowed set
        char[] result = new char[6];
        for (int i = 0; i < 6; i++)
        {
            // Ensure even distribution by using modulo
            result[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(result);
    }

    public async Task<CourseDto?> UpdateCourseAsync(int courseId, Classroom.Dtos.Course.UpdateCourseDto updateCourseDto, int userId)
    {
        // Check if course exists and user is the teacher
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return null;
        }

        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, userId);
        if (!isTeacher)
        {
            return null; // User is not the teacher of this course
        }

        // Update course properties
        course.Name = updateCourseDto.Name;
        course.Section = updateCourseDto.Section;
        course.TeacherName = updateCourseDto.TeacherName;
        course.CoverImage = updateCourseDto.CoverImage;
        // Only update enrollment code if a new one is provided
        if (!string.IsNullOrEmpty(updateCourseDto.EnrollmentCode))
        {
            course.EnrollmentCode = updateCourseDto.EnrollmentCode;
        }
        course.Color = updateCourseDto.Color;
        course.TextColor = updateCourseDto.TextColor;
        course.Subject = updateCourseDto.Subject;
        course.Room = updateCourseDto.Room;

        var updatedCourse = await _courseRepository.UpdateAsync(course);
        return MapCourseToDto(updatedCourse);
    }

    public async Task<bool> DeleteCourseAsync(int courseId, int userId)
    {
        // Check if course exists and user is the teacher
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return false;
        }

        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, userId);
        if (!isTeacher)
        {
            return false; // User is not the teacher of this course
        }

        await _courseRepository.DeleteAsync(course);
        return true;
    }

    public async Task<bool> EnrollInCourseAsync(int courseId, string enrollmentCode, int userId)
    {
        // Check if course exists
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return false;
        }

        // Verify enrollment code
        if (course.EnrollmentCode != enrollmentCode)
        {
            return false; // Invalid enrollment code
        }

        // Check if user is already enrolled
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (isEnrolled)
        {
            return false; // User is already enrolled
        }

        // Add user as a course member with Student role
        var courseMember = new CourseMember
        {
            CourseId = courseId,
            UserId = userId,
            Role = "Student"
        };

        return await _courseRepository.AddMemberAsync(courseMember);
    }

    public async Task<bool> UnenrollFromCourseAsync(int courseId, int userId)
    {
        // Check if course exists
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return false;
        }

        // Check if user is enrolled
        var courseMember = await _courseRepository.GetCourseMemberAsync(courseId, userId);
        if (courseMember == null)
        {
            return false; // User is not enrolled
        }

        // Check if user is a student (only students can unenroll themselves)
        if (courseMember.Role != "Student")
        {
            return false; // User is not a student
        }

        return await _courseRepository.RemoveMemberAsync(courseMember);
    }

    public async Task<List<Classroom.Dtos.Course.CourseMemberDto>> GetCourseMembersAsync(int courseId, int userId)
    {
        // Check if course exists and user is enrolled
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return [];
        }

        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (!isEnrolled)
        {
            return []; // User is not enrolled in this course
        }

        var members = await _courseRepository.GetCourseMembersAsync(courseId);
        return members.Select(m => new Classroom.Dtos.Course.CourseMemberDto
        {
            UserId = m.User.UserId,
            Name = m.User.Name,
            Email = m.User.Email,
            Avatar = m.User.Avatar,
            Role = m.Role
        }).ToList();
    }

    public async Task<bool> RemoveMemberFromCourseAsync(int courseId, int memberUserId, int requestingUserId)
    {
        // Check if course exists
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return false;
        }

        // Check if requesting user is the teacher of the course
        var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(courseId, requestingUserId);
        if (!isTeacher)
        {
            return false; // Requesting user is not the teacher
        }

        // Check if member exists
        var member = await _courseRepository.GetCourseMemberAsync(courseId, memberUserId);
        if (member == null)
        {
            return false; // Member not found
        }

        return await _courseRepository.RemoveMemberAsync(member);
    }

    private static Classroom.Dtos.Course.CourseDto MapCourseToDto(Course course)
    {
        return new Classroom.Dtos.Course.CourseDto
        {
            CourseId = course.CourseId,
            CourseGuid = course.CourseGuid,
            Name = course.Name,
            Section = course.Section,
            TeacherName = course.TeacherName,
            CoverImage = course.CoverImage,
            EnrollmentCode = course.EnrollmentCode,
            Color = course.Color,
            TextColor = course.TextColor,
            Subject = course.Subject,
            Room = course.Room
        };
    }
}
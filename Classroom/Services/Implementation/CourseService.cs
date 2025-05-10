using Classroom.Dtos;
using Classroom.Dtos.Course;
using Classroom.Helpers;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.EntityFrameworkCore;

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

    public async Task<CourseDto?> GetCourseByGuidAsync(Guid courseGuid, int userId)
    {
        // Check if course exists and user is enrolled
        var course = await _courseRepository.GetByGuidAsync(courseGuid);
        if (course == null)
        {
            return null;
        }

        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(course.CourseId, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        return MapCourseToDto(course);
    }

    public async Task<CourseDetailDto?> GetCourseDetailByGuidAsync(Guid courseGuid, int userId)
    {
        // Check if course exists and user is enrolled
        var course = await _courseRepository.GetByGuidAsync(courseGuid);
        if (course == null)
        {
            return null;
        }

        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(course.CourseId, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        // Get course statistics
        int totalMembers = await _courseRepository.GetCourseMemberCountAsync(course.CourseId);
        int totalStudents = await _courseRepository.GetCourseStudentCountAsync(course.CourseId);
        int totalTeachers = await _courseRepository.GetCourseTeacherCountAsync(course.CourseId);
        int totalAnnouncements = await _courseRepository.GetCourseAnnouncementCountAsync(course.CourseId);
        int totalAssignments = await _courseRepository.GetCourseAssignmentCountAsync(course.CourseId);
        int totalMaterials = await _courseRepository.GetCourseMaterialCountAsync(course.CourseId);

        // Get recent members
        var recentMembers = await _courseRepository.GetRecentMembersAsync(course.CourseId, 5);

        // Create the detailed course DTO
        var courseDetailDto = new CourseDetailDto
        {
            CourseId = course.CourseId,
            CourseGuid = course.CourseGuid,
            Name = course.Name,
            Section = course.Section,
            TeacherName = course.TeacherName,
            CoverImage = course.CoverImage,
            EnrollmentCode = course.EnrollmentCode,
            Subject = course.Subject,
            Room = course.Room,
            TotalMembers = totalMembers,
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalAnnouncements = totalAnnouncements,
            TotalAssignments = totalAssignments,
            TotalMaterials = totalMaterials,
            CreatedDate = DateTime.UtcNow, // Default value since it's not in the database
            LastUpdatedDate = null, // Default value since it's not in the database
            RecentMembers = recentMembers.Select(m => new Dtos.Course.CourseMemberDto
            {
                UserId = m.User.UserId,
                Name = m.User.Name,
                Email = m.User.Email,
                Avatar = m.User.Avatar,
                Role = m.Role
            }).ToList()
        };

        return courseDetailDto;
    }

    public async Task<CourseDto> CreateCourseAsync(Classroom.Dtos.Course.CreateCourseDto createCourseDto, int teacherId)
    {
        // Always generate a random enrollment code, ignoring any provided value
        string enrollmentCode = await GenerateEnrollmentCode();

        // Create new course
        var course = new Course
        {
            Name = createCourseDto.Name,
            Section = createCourseDto.Section,
            TeacherName = createCourseDto.TeacherName,
            CoverImage = createCourseDto.CoverImage,
            EnrollmentCode = enrollmentCode,
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
    /// Generates a unique enrollment code for a course
    /// </summary>
    /// <returns>A 6-character alphanumeric code that doesn't exist in the database</returns>
    private async Task<string> GenerateEnrollmentCode()
    {
        // Get all existing enrollment codes from the database
        var existingCodes = await _courseRepository.GetAllEnrollmentCodesAsync();

        // Generate a unique code that doesn't exist in the database
        return CodeGenerator.GenerateUniqueCode(existingCodes);
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
        course.Subject = updateCourseDto.Subject;
        course.Room = updateCourseDto.Room;

        // Update the last updated date
        course.LastUpdatedDate = DateTime.UtcNow;

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

    public async Task<bool> EnrollInCourseByCodeAsync(string enrollmentCode, int userId)
    {
        // Find course by enrollment code
        var course = await _courseRepository.GetByEnrollmentCodeAsync(enrollmentCode);
        if (course == null)
        {
            return false; // Course not found with this enrollment code
        }

        // Check if user is already enrolled
        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(course.CourseId, userId);
        if (isEnrolled)
        {
            return false; // User is already enrolled
        }

        // Add user as a course member with Student role
        var courseMember = new CourseMember
        {
            CourseId = course.CourseId,
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

    public async Task<bool> IsUserTeacherOfCourseAsync(int courseId, int userId)
    {
        return await _courseRepository.IsUserTeacherOfCourseAsync(courseId, userId);
    }

    public async Task<string> GenerateEnrollmentCodeAsync()
    {
        return await GenerateEnrollmentCode();
    }


    public async Task<string?> RegenerateEnrollmentCodeAsync(int courseId, int userId)
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

        // Generate a new enrollment code
        string newEnrollmentCode = await GenerateEnrollmentCode();

        // Update the course with the new code
        course.EnrollmentCode = newEnrollmentCode;
        course.LastUpdatedDate = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        return newEnrollmentCode;
    }




    public async Task<CourseDetailDto?> GetCourseDetailAsync(int courseId, int userId)
    {
        // Check if course exists and user is enrolled
        var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
        if (course == null)
        {
            return null;
        }

        var isEnrolled = await _courseRepository.IsUserEnrolledAsync(courseId, userId);
        if (!isEnrolled)
        {
            return null; // User is not enrolled in this course
        }

        // Get course statistics
        int totalMembers = await _courseRepository.GetCourseMemberCountAsync(courseId);
        int totalStudents = await _courseRepository.GetCourseStudentCountAsync(courseId);
        int totalTeachers = await _courseRepository.GetCourseTeacherCountAsync(courseId);
        int totalAnnouncements = await _courseRepository.GetCourseAnnouncementCountAsync(courseId);
        int totalAssignments = await _courseRepository.GetCourseAssignmentCountAsync(courseId);
        int totalMaterials = await _courseRepository.GetCourseMaterialCountAsync(courseId);

        // Get recent members
        var recentMembers = await _courseRepository.GetRecentMembersAsync(courseId, 5);

        // Create the detailed course DTO
        var courseDetailDto = new CourseDetailDto
        {
            CourseId = course.CourseId,
            CourseGuid = course.CourseGuid,
            Name = course.Name,
            Section = course.Section,
            TeacherName = course.TeacherName,
            CoverImage = course.CoverImage,
            EnrollmentCode = course.EnrollmentCode,
            Subject = course.Subject,
            Room = course.Room,
            TotalMembers = totalMembers,
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalAnnouncements = totalAnnouncements,
            TotalAssignments = totalAssignments,
            TotalMaterials = totalMaterials,
            CreatedDate = DateTime.UtcNow, // Default value since it's not in the database
            LastUpdatedDate = null, // Default value since it's not in the database
            RecentMembers = recentMembers.Select(m => new Dtos.Course.CourseMemberDto
            {
                UserId = m.User.UserId,
                Name = m.User.Name,
                Email = m.User.Email,
                Avatar = m.User.Avatar,
                Role = m.Role
            }).ToList()
        };

        return courseDetailDto;
    }

    private static CourseDto MapCourseToDto(Course course)
    {
        return new CourseDto
        {
            CourseId = course.CourseId,
            CourseGuid = course.CourseGuid,
            Name = course.Name,
            Section = course.Section,
            TeacherName = course.TeacherName,
            CoverImage = course.CoverImage,
            EnrollmentCode = course.EnrollmentCode,
            Subject = course.Subject,
            Room = course.Room,
            ThemeColor = course.ThemeColor ?? "#1976d2" // Provide a default color if ThemeColor is null
        };
    }

    public async Task<bool> UpdateCourseThemeAsync(CourseThemeDto themeDto, int userId)
    {
        try
        {
            // Parse the course ID (which could be a GUID)
            if (string.IsNullOrEmpty(themeDto.CourseId))
            {
                return false;
            }

            // Try to parse as GUID first
            if (Guid.TryParse(themeDto.CourseId, out Guid courseGuid))
            {
                // Get course by GUID
                var course = await _courseRepository.GetByGuidAsync(courseGuid);
                if (course == null)
                {
                    return false;
                }

                var isTeacher = await _courseRepository.IsUserTeacherOfCourseAsync(course.CourseId, userId);
                if (!isTeacher)
                {
                    return false; // User is not the teacher of this course
                }

                // Update course theme properties
                if (!string.IsNullOrEmpty(themeDto.HeaderImage))
                {
                    course.CoverImage = themeDto.HeaderImage;
                }

                if (!string.IsNullOrEmpty(themeDto.ThemeColor))
                {
                    course.ThemeColor = themeDto.ThemeColor;
                }

                // Update the last updated date
                course.LastUpdatedDate = DateTime.UtcNow;

                await _courseRepository.UpdateAsync(course);
                return true;
            }
            else if (int.TryParse(themeDto.CourseId, out int courseId))
            {
                // Get course by ID
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

                // Update course theme properties
                if (!string.IsNullOrEmpty(themeDto.HeaderImage))
                {
                    course.CoverImage = themeDto.HeaderImage;
                }

                if (!string.IsNullOrEmpty(themeDto.ThemeColor))
                {
                    course.ThemeColor = themeDto.ThemeColor;
                }

                // Update the last updated date
                course.LastUpdatedDate = DateTime.UtcNow;

                await _courseRepository.UpdateAsync(course);
                return true;
            }

            return false; // Invalid course ID format
        }
        catch (Exception)
        {
            return false; // Handle any exceptions
        }
    }
}
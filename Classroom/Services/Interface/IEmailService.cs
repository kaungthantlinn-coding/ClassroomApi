using Classroom.Dtos.Email;

namespace Classroom.Services.Interface;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendCourseInvitationAsync(CourseInvitationDto invitationDto);
    Task<bool> ValidateEmailAsync(string email);
}

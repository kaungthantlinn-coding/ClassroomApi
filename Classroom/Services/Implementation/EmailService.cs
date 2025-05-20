using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Classroom.Dtos.Email;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.Extensions.Options;

namespace Classroom.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                EnableSsl = _emailSettings.EnableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation($"Email sent successfully to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendCourseInvitationAsync(CourseInvitationDto invitationDto)
    {
        // Validate email
        if (!await ValidateEmailAsync(invitationDto.RecipientEmail))
        {
            _logger.LogWarning($"Invalid email format: {invitationDto.RecipientEmail}");
            return false;
        }

        // Get course details
        var course = await _courseRepository.GetByIdAsync(invitationDto.CourseId);
        if (course == null)
        {
            _logger.LogWarning($"Course not found: {invitationDto.CourseId}");
            return false;
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(invitationDto.RecipientEmail);
        
        // Create email subject and body
        string subject = $"Invitation to join {course.Name} on Classroom";
        string body = GenerateCourseInvitationEmail(course.Name, course.EnrollmentCode, invitationDto.CustomMessage, existingUser != null);

        // Send the email
        return await SendEmailAsync(invitationDto.RecipientEmail, subject, body);
    }

    public async Task<bool> ValidateEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Use a more comprehensive regex for email validation
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        bool isValidFormat = Regex.IsMatch(email, pattern);

        if (!isValidFormat)
            return false;

        // For more thorough validation, you could implement DNS MX record checking here
        // But for simplicity, we'll just check the format

        return await Task.FromResult(true);
    }

    private string GenerateCourseInvitationEmail(string courseName, string enrollmentCode, string? customMessage, bool userExists)
    {
        string enrollmentInstructions = userExists
            ? "Since you already have an account, simply log in and use the enrollment code below to join the course."
            : "You'll need to create an account first, then use the enrollment code below to join the course.";

        return $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #4285F4; color: white; padding: 10px 20px; border-radius: 5px 5px 0 0; }}
                .content {{ padding: 20px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 5px 5px; }}
                .code {{ background-color: #f5f5f5; padding: 10px; font-family: monospace; font-size: 18px; text-align: center; margin: 20px 0; border: 1px solid #ddd; border-radius: 5px; }}
                .footer {{ margin-top: 20px; font-size: 12px; color: #777; text-align: center; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Course Invitation</h2>
                </div>
                <div class='content'>
                    <p>You've been invited to join <strong>{courseName}</strong> on Classroom!</p>
                    
                    {(string.IsNullOrEmpty(customMessage) ? "" : $"<p>Message from the teacher: <em>{customMessage}</em></p>")}
                    
                    <p>{enrollmentInstructions}</p>
                    
                    <p>Your enrollment code is:</p>
                    <div class='code'>{enrollmentCode}</div>
                    
                    <p>To join the course:</p>
                    <ol>
                        <li>{(userExists ? "Log in to your account" : "Create an account or log in")}</li>
                        <li>Click on 'Join a course' or the '+' button</li>
                        <li>Enter the enrollment code above</li>
                    </ol>
                    
                    <p>If you have any questions, please contact the course teacher.</p>
                </div>
                <div class='footer'>
                    <p>This is an automated message. Please do not reply to this email.</p>
                </div>
            </div>
        </body>
        </html>";
    }
}

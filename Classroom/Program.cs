using System.Text;
using Classroom.Models;
using Classroom.Repositories.Implementation;
using Classroom.Repositories.Interface;
using Classroom.Services.Implementation;
using Classroom.Services.Interface;
using Classroom.Dtos.Submission;
using Classroom.Dtos.Announcement;
using Classroom.Dtos.Assignment;
using Classroom.Dtos.Material;
using Classroom.Dtos.Course;
using Classroom.Dtos;
using Classroom.Validators.Submission;
using Classroom.Validators.Comment;
using Classroom.Validators.Assignment;
using Classroom.Validators.Material;
using Classroom.Validators.Auth;
using Classroom.Validators.Announcement;
using Classroom.Validators.Course;
using Classroom.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Classroom
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Configure CORS for React app
            CorsConfiguration.ConfigureCors(builder.Services);

            // Configure JWT Authentication
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null)
            {
                throw new InvalidOperationException("JwtSettings configuration is missing");
            }

            // Set Secret equal to Key for backward compatibility
            jwtSettings.Secret = jwtSettings.Key;
            // Sync expiry times
            jwtSettings.ExpiryMinutes = jwtSettings.ExpiryInMinutes;
            jwtSettings.RefreshTokenExpiryDays = jwtSettings.RefreshTokenExpiryInDays;

            builder.Services.Configure<JwtSettings>(options =>
            {
                options.Key = jwtSettings.Key;
                options.Secret = jwtSettings.Key; // Make Secret the same as Key
                options.Issuer = jwtSettings.Issuer;
                options.Audience = jwtSettings.Audience;
                options.ExpiryMinutes = jwtSettings.ExpiryInMinutes;
                options.RefreshTokenExpiryDays = jwtSettings.RefreshTokenExpiryInDays;
                options.ExpiryInMinutes = jwtSettings.ExpiryInMinutes;
                options.RefreshTokenExpiryInDays = jwtSettings.RefreshTokenExpiryInDays;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Register DbContext
            builder.Services.AddDbContext<ClassroomContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
            builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
            builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
            builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();

            // Register Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IAssignmentService, AssignmentService>();
            builder.Services.AddScoped<IMaterialService, MaterialService>();
            builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
            builder.Services.AddScoped<ISubmissionService, SubmissionService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IValidationService, ValidationService>();

            // Register FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateSubmissionValidator>();

            // Submission validators
            builder.Services.AddScoped<IValidator<CreateSubmissionDto>, CreateSubmissionValidator>();
            builder.Services.AddScoped<IValidator<GradeSubmissionDto>, GradeSubmissionValidator>();
            builder.Services.AddScoped<IValidator<FeedbackSubmissionDto>, FeedbackSubmissionValidator>();

            // Comment validators
            builder.Services.AddScoped<IValidator<CreateCommentDto>, CreateCommentDtoValidator>();
            builder.Services.AddScoped<IValidator<UpdateCommentDto>, UpdateCommentDtoValidator>();

            // Assignment validators
            builder.Services.AddScoped<IValidator<CreateAssignmentDto>, CreateAssignmentValidator>();
            builder.Services.AddScoped<IValidator<UpdateAssignmentDto>, UpdateAssignmentValidator>();

            // Material validators
            builder.Services.AddScoped<IValidator<CreateMaterialDto>, CreateMaterialValidator>();
            builder.Services.AddScoped<IValidator<UpdateMaterialDto>, UpdateMaterialValidator>();

            // Auth validators
            builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
            builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
            builder.Services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenValidator>();
            builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();

            // Announcement validators
            builder.Services.AddScoped<IValidator<CreateAnnouncementDto>, CreateAnnouncementValidator>();
            builder.Services.AddScoped<IValidator<UpdateAnnouncementDto>, UpdateAnnouncementValidator>();

            // Course validators
            builder.Services.AddScoped<IValidator<CreateCourseDto>, CreateCourseValidator>();
            builder.Services.AddScoped<IValidator<UpdateCourseDto>, UpdateCourseValidator>();
            builder.Services.AddScoped<IValidator<EnrollCourseDto>, EnrollCourseValidator>();

            // Configure OpenAPI/Swagger
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Classroom API", Version = "v1" });

                // Configure Swagger to use JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Classroom API v1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at the root
                });
                app.MapOpenApi();
            }

            // Configure HTTPS redirection (only in production)
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Use CORS middleware with the React app policy
            app.UseCors("AllowReactApp");

            // Add Authentication Middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

using Classroom.Models;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Extensions
{
    public static class DbContextExtensions
    {
        public static string GetUploadsDirectory(this ClassroomContext context)
        {
            // Get the application's content root path
            var contentRootPath = AppDomain.CurrentDomain.BaseDirectory;
            
            // Combine with the Uploads directory
            return Path.Combine(contentRootPath, "Uploads");
        }
    }
}

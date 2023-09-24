using GrecoBot.Admin.MVC.Client.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GrecoBot.Admin.MVC.Client
{
    public class AdminUserDbContext : IdentityDbContext<User, Role, string>
    {
        private readonly IConfiguration _configuration;

        public AdminUserDbContext(DbContextOptions<AdminUserDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnectionAdmin"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Дополнительная конфигурация моделей Identity Framework, если необходимо
        }
    }
}

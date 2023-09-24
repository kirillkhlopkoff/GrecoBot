using GrecoBot.Admin.MVC.Client;
using GrecoBot.Admin.MVC.Client.Models;
using GrecoBot.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionStringAdmin = builder.Configuration.GetConnectionString("DefaultConnectionAdmin");

builder.Services.AddDbContext<AdminUserDbContext>(options =>
{
    options.LogTo(Console.WriteLine);
    options.UseNpgsql(connectionStringAdmin);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.LogTo(Console.WriteLine);
    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 10;
})
    .AddEntityFrameworkStores<AdminUserDbContext>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

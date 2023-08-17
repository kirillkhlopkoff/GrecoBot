using GrecoBot.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.LogTo(Console.WriteLine);
    options.UseNpgsql(connectionString);
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // ��������� ������������ JSON ��� ���������� ������ �� ������� � �������������� ����������� ������������
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // ��� ��-�� ��������� ������
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*builder.Services.AddScoped<MeetManager>(); //DI, � �� �����, ������ � ������� ��� ��������*/

// Configure the HTTP request pipeline.
var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

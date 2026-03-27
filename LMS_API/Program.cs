using System.Text;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.Assignmentset;
using LMS_API.Models.DTO.Teacher;
using LMS_API.Services;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(o =>
{
    o.CreateMap<Teacher, TeacherCreateDTO>().ReverseMap();
    o.CreateMap<Assignment, AssignmentCreateDTO>().ReverseMap();
    o.CreateMap<Assignment, AssignmentReadDTO>().ReverseMap();
    o.CreateMap<AssignmentSet, AssignmentSetCreateDTO>().ReverseMap();
    o.CreateMap<AssignmentSet, AssignmentSetReadDTO>()
    .ForMember(dest => dest.Assignments,
               opt => opt.MapFrom(src => src.AssignmentAssignmentSets.Select(x => x.Assignment)));
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IAssignmentSetService, AssignmentSetService>();

var app = builder.Build();

// Seed database in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

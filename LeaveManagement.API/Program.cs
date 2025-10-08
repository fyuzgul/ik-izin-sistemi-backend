using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL DateTime fix - treat unspecified as UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeaveManagement",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LeaveManagement",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong"))
        };
    });

builder.Services.AddAuthorization();

// Database
builder.Services.AddDbContext<LeaveManagementDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

    // Business Services
    builder.Services.AddScoped<LeaveManagement.Business.Interfaces.ILeaveRequestService, LeaveManagement.Business.Services.LeaveRequestService>();
    builder.Services.AddScoped<LeaveManagement.Business.Interfaces.IEmployeeService, LeaveManagement.Business.Services.EmployeeService>();
    builder.Services.AddScoped<LeaveManagement.Business.Interfaces.IAuthService, LeaveManagement.Business.Services.AuthService>();
    builder.Services.AddScoped<LeaveManagement.Business.Interfaces.IDepartmentService, LeaveManagement.Business.Services.DepartmentService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Swagger'ı her ortamda aktif et
app.UseSwagger();
app.UseSwaggerUI();

// CORS'u HTTPS redirection'dan önce koy
app.UseCors("AllowAll");

// HTTPS redirection disabled for testing
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Initialize database with default data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LeaveManagementDbContext>();
    await LeaveManagement.API.Data.DbInitializer.InitializeAsync(context);
}

app.Run();



using System.Threading.RateLimiting;
using FoodConnectAPI.Data;
using FoodConnectAPI.Interfaces.Repositories;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Repositories;
using FoodConnectAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", builder =>
    {
        builder.Window = TimeSpan.FromMinutes(1);
        builder.PermitLimit = 100; // Allow 100 requests per minute
        builder.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        builder.QueueLimit = 10; // Allow up to 10 requests in the queue
    });
});

// Add configuration validation
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>( options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodConnectDB")));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IPostTagRepository, PostTagRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITagService, TagService>();
//builder.Services.AddScoped<ILikeService, LikeService>();
//builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// Serve static files from wwwroot folder
app.UseStaticFiles();

// Use CORS policy
app.UseCors("AllowReactApp");

// Use Rate Limiting
app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed test data if database is empty
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedTestDataAsync(context);
}

app.Run();

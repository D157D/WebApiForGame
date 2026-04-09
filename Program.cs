using Crazy_Lobby.AppDataContext; 
using Crazy_Lobby.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH SERVICES ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); 
builder.Services.AddSingleton<IRoomService, RoomService>();

// Cấu hình Database với chiến lược Retry (để tránh lỗi khởi động chậm trên Cloud)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(connectionString, npgsqlOptions => {
        // Tự động thử lại 5 lần nếu kết nối thất bại tạm thời
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// Cấu hình CORS: Cho phép Unity Client truy cập
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Cấu hình JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Scrazy_Lobby",
        ValidAudience = "Client",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("definitely-a-very-secure-secret-key"))
    };
});

var app = builder.Build();

// --- 2. CẤU HÌNH CODE FIRST & AUTO-MIGRATION ---
// Đoạn này giúp Railway tự tạo bảng ngay khi App khởi động thành công
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Sử dụng Migrate() để áp dụng các thay đổi cấu trúc bảng (Migrations)
        // Nếu bạn chưa dùng Migration bao giờ, có thể tạm thay bằng context.Database.EnsureCreated();
        context.Database.Migrate(); 
        Console.WriteLine(">>> DATABASE OK: Migrations/Tables applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> DATABASE ERROR: {ex.Message}");
    }
}

// --- 3. CẤU HÌNH MIDDLEWARE (THỨ TỰ QUAN TRỌNG) ---

// Luôn bật Swagger trên Railway để bạn có thể test API qua trình duyệt
app.UseSwagger();
app.UseSwaggerUI();

// KHÔNG sử dụng UseHttpsRedirection trên Railway vì nó có thể gây lỗi vòng lặp Redirect
// app.UseHttpsRedirection(); 

app.UseCors("AllowAll"); // Cors phải đứng trước Auth

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// --- 4. CẤU HÌNH PORT ĐỘNG CHO RAILWAY ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
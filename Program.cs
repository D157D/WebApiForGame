using Crazy_Lobby.AppDataContext; 
using Crazy_Lobby.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); 

// Kết nối Database (Lấy từ biến môi trường DATABASE_URL của Railway nếu có)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 2. Cấu hình CORS (Phải cho phép mọi nguồn để Unity kết nối được)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Cấu hình Authentication
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("definitely-a-very-secure-secret-key"))
    };
});

var app = builder.Build();

// 4. Cấu hình Middleware (THỨ TỰ RẤT QUAN TRỌNG)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger"; // Truy cập qua đường dẫn /swagger
});

// Bật CORS trước khi Auth
app.UseCors("AllowAll");

// Tạm thời tắt Redirect để tránh lỗi vòng lặp trên Railway Proxy
// app.UseHttpsRedirection(); 

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// 5. Cấu hình Port động cho Railway
// Nếu không có biến PORT, mặc định chạy 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Application is starting on port: {port}");

app.Run($"http://0.0.0.0:{port}");
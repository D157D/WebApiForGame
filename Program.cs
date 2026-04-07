using Crazy_Lobby.AppDataContext; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Services cơ bản
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); 

// 2. Lấy Connection String (Sẽ ưu tiên lấy từ Biến môi trường trên Railway)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(connectionString, npgsqlOptions => {
        // Tự động thử lại nếu Database chưa sẵn sàng
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// 3. Cấu hình CORS cho Unity
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 4. Cấu hình Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
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

// 5. TỰ ĐỘNG TẠO BẢNG (FIX LỖI 500 DO THIẾU TABLE)
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    try {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); 
        Console.WriteLine(">>> DATABASE OK: Tables verified/created.");
    } catch (Exception ex) {
        Console.WriteLine($">>> DATABASE ERROR: {ex.Message}");
    }
}

// 6. Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll"); // Đặt trước Auth

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

// 7. Chạy trên Port do Railway cung cấp
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
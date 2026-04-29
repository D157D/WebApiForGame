# Crazy_Lobby Backend API

## Tổng quan dự án (Project Overview)
**Crazy_Lobby** là một hệ thống Backend Web API được xây dựng bằng **ASP.NET Core (.NET 10)**, đóng vai trò là máy chủ quản lý sảnh chờ (Lobby Server) cho các trò chơi trực tuyến. Dự án được thiết kế để xử lý xác thực người dùng, quản lý phòng chơi (Room), và hỗ trợ ghép trận (Matchmaking).

Dự án hiện đang được cấu hình để tối ưu hóa việc triển khai trên các nền tảng đám mây như **Railway**, với các tính năng tự động chạy Migration và hỗ trợ kết nối từ các Client game (như Unity).

## Công nghệ sử dụng (Tech Stack)
- **Framework**: .NET 10 Web API.
- **Database**: PostgreSQL thông qua Entity Framework Core (EF Core).
- **Authentication**: JWT Bearer Token.
- **API Documentation**: Swagger UI (Swashbuckle).
- **Hosting Support**: Cấu hình PORT động và CORS cho phép mọi nguồn (AllowAll) để phục vụ quá trình phát triển và tích hợp game client.

## Cấu trúc thư mục (Project Structure)
- **/Controller**: Chứa các API Endpoints xử lý các yêu cầu từ Client.
  - `AuthController`: Đăng ký, đăng nhập và quản lý Token.
  - `UserController`: Quản lý thông tin hồ sơ người dùng.
  - `RoomController`: Tạo, tham gia và quản lý trạng thái các phòng chơi.
  - `MatchController`: Xử lý logic ghép trận giữa các người chơi.
- **/Services**: Tầng logic nghiệp vụ (Business Logic Layer).
  - `AuthService`, `UserService`, `RoomService`.
- **/Models**: Định nghĩa các thực thể dữ liệu (Entities) và các DTOs (Data Transfer Objects).
- **/AppDbContext**: Cấu hình kết nối cơ sở dữ liệu và các bảng (Tables).
- **/Migrations**: Lưu vết các thay đổi cấu trúc Database.

## Các tính năng chính (Core Features)
1.  **Hệ thống xác thực (Authentication)**: Đảm bảo bảo mật bằng JWT.
2.  **Quản lý phòng chơi (Lobby Management)**:
    - Tạo phòng mới.
    - Tham gia/Rời phòng.
    - Theo dõi danh sách người chơi trong phòng.
3.  **Ghép trận (Matchmaking)**: Tìm kiếm và kết nối người chơi vào các trận đấu phù hợp.
4.  **Tự động hóa Database**: Tự động áp dụng Migration khi ứng dụng khởi động.

## Hướng dẫn cài đặt & Chạy dự án (Getting Started)
1.  **Yêu cầu hệ thống**:
    - .NET 10 SDK.
    - PostgreSQL Database (hoặc cấu hình kết nối tới Cloud DB).
2.  **Cấu hình**:
    - Cập nhật chuỗi kết nối trong `appsettings.json` tại mục `ConnectionStrings:DefaultConnection`.
    - Cấu hình các tham số JWT (`Issuer`, `Audience`, `Key`).
3.  **Chạy ứng dụng**:
    ```bash
    dotnet run
    ```
4.  **Kiểm tra API**: Truy cập đường dẫn `http://localhost:<port>/swagger` để xem tài liệu API chi tiết.

---
*Dự án được phát triển nhằm mục đích cung cấp một nền tảng Backend mạnh mẽ và dễ mở rộng cho các dự án Multiplayer Game.*

using SignalR.API.Hubs;
using SignalR.API.Services;
using SignalR.API.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// إضافة SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});

// إضافة CORS للسماح بالاتصال من التطبيقات الأخرى
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // سياسة أكثر أماناً للإنتاج
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// تسجيل خدمات الإشعارات
builder.Services.AddScoped<INotificationService, NotificationService>();

// إضافة قاعدة البيانات
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions => mySqlOptions
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
            .EnableStringComparisonTranslations()));

// تسجيل Repository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// إضافة Logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// استخدام الملفات الثابتة
app.UseStaticFiles();

// استخدام CORS
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

app.UseAuthorization();

// تعيين SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();

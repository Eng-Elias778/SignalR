# 🚀 نظام الإشعارات الفورية - SignalR ERP

نظام إشعارات فوري متكامل مبني باستخدام ASP.NET Core SignalR لإدارة التنبيهات والإشعارات في أنظمة ERP.

## 📋 المميزات الرئيسية

### 🔔 أنواع الإشعارات المدعومة
- **إشعارات الموافقة**: طلبات الإجازة، الموافقات المالية، الموافقات الإدارية
- **إشعارات إدارة المستخدمين**: إنشاء، تحديث، حذف المستخدمين وتغيير الصلاحيات
- **إشعارات تعديل البيانات**: تنبيهات عند تعديل أو حذف البيانات المهمة
- **إشعارات النظام**: تنبيهات الأخطاء والصيانة
- **إشعارات المهام**: تكليف المهام وتحديث حالتها
- **إشعارات مخصصة**: إشعارات قابلة للتخصيص حسب احتياجات النظام

### 🎯 المميزات التقنية
- **اتصال فوري**: استخدام SignalR للاتصال في الوقت الفعلي
- **دعم المجموعات**: إرسال الإشعارات لمجموعات محددة (أدوار، أقسام)
- **تتبع المستخدمين**: معرفة المستخدمين المتصلين حالياً
- **أولويات الإشعارات**: تصنيف الإشعارات حسب الأهمية
- **قوالب الإشعارات**: نظام قوالب قابل للتخصيص
- **قاعدة البيانات**: تخزين الإشعارات وتتبع حالتها
- **API متكامل**: RESTful API لإدارة الإشعارات
- **واجهة اختبار**: صفحة HTML للاختبار والاستخدام العملي

## 🛠️ التقنيات المستخدمة

- **ASP.NET Core 9.0**
- **SignalR** للاتصال الفوري
- **Entity Framework Core** لإدارة قاعدة البيانات
- **Swagger** لتوثيق API
- **CORS** لدعم التطبيقات المختلفة
- **HTML5 + JavaScript** لواجهة الاختبار

## 📁 هيكل المشروع

```
SignalR.API/
├── Controllers/
│   ├── NotificationsController.cs    # API للإشعارات
│   ├── GroupsController.cs           # API للمجموعات
│   └── ExamplesController.cs         # أمثلة عملية
├── Hubs/
│   └── NotificationHub.cs            # SignalR Hub الرئيسي
├── Models/
│   └── NotificationModels.cs        # نماذج البيانات
├── Services/
│   └── NotificationService.cs       # خدمة إدارة الإشعارات
├── Data/
│   └── NotificationDbContext.cs     # قاعدة البيانات
├── Middleware/
│   └── UserTrackingMiddleware.cs    # تتبع المستخدمين
├── wwwroot/
│   └── index.html                   # واجهة الاختبار
└── Program.cs                       # إعداد التطبيق
```

## 🚀 البدء السريع

### 1. تشغيل المشروع

```bash
# استنساخ المشروع
git clone <repository-url>
cd SignalR.API

# تشغيل المشروع
dotnet run
```

### 2. الوصول للواجهات

- **API Documentation**: `https://localhost:7016/swagger`
- **Test Interface**: `https://localhost:7016/index.html`
- **SignalR Hub**: `https://localhost:7016/notificationHub`

### 3. اختبار الاتصال

افتح صفحة الاختبار في المتصفح واتبع الخطوات:
1. أدخل معرف المستخدم
2. اختر المجموعة المناسبة
3. اضغط "اتصال"
4. جرب الأمثلة العملية

## 📡 استخدام API

### إرسال إشعار عام

```http
POST /api/notifications/send
Content-Type: application/json

{
  "title": "إشعار جديد",
  "message": "هذا إشعار تجريبي",
  "type": 1,
  "priority": 2,
  "targetUserId": "user123"
}
```

### إرسال إشعار موافقة

```http
POST /api/notifications/approval
?requestId=VACATION_123
&requestType=طلب إجازة
&targetUserId=user123
&approverName=مدير الموارد البشرية
```

### إرسال إشعار إدارة المستخدمين

```http
POST /api/notifications/user-management
?affectedUserId=newuser123
&affectedUserName=أحمد محمد
&type=10
&adminName=مدير النظام
```

### الحصول على إشعارات المستخدم

```http
GET /api/notifications/user/user123?page=1&pageSize=20
```

### تحديد إشعار كمقروء

```http
PUT /api/notifications/notificationId/read?userId=user123
```

## 🔌 استخدام SignalR في العميل

### JavaScript

```javascript
// إنشاء الاتصال
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

// بدء الاتصال
await connection.start();

// الانضمام لمجموعة
await connection.invoke("JoinGroup", "HR");

// استقبال الإشعارات
connection.on("ReceiveNotification", (notification) => {
    console.log("إشعار جديد:", notification);
    // عرض الإشعار في الواجهة
});

// إرسال إشعار لمستخدم محدد
await connection.invoke("SendNotificationToUser", "user123", {
    title: "إشعار مباشر",
    message: "هذا إشعار مباشر من SignalR"
});
```

### C# Client

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7016/notificationHub")
    .Build();

await connection.StartAsync();

connection.On<object>("ReceiveNotification", (notification) =>
{
    Console.WriteLine($"إشعار جديد: {notification}");
});

await connection.InvokeAsync("JoinGroup", "HR");
```

## 🎯 أمثلة الاستخدام في ERP

### 1. نظام الموافقات

```csharp
// إشعار طلب إجازة
await notificationService.SendApprovalNotificationAsync(
    "VACATION_123", 
    "طلب إجازة", 
    "manager123", 
    "أحمد محمد"
);

// إشعار موافقة مالية
await notificationService.SendNotificationAsync(new CreateNotificationRequest
{
    Title = "طلب موافقة مالية",
    Message = "لديك طلب موافقة مالية جديد بمبلغ 5000 ريال",
    Type = NotificationType.ApprovalRequest,
    Priority = NotificationPriority.High,
    TargetUserId = "finance_manager",
    ActionUrl = "/finance/approvals/123"
});
```

### 2. إدارة المستخدمين

```csharp
// إشعار مستخدم جديد
await notificationService.SendUserManagementNotificationAsync(
    "newuser123",
    "سارة أحمد",
    NotificationType.UserCreated,
    "مدير النظام"
);

// إشعار تغيير صلاحيات
await notificationService.SendUserManagementNotificationAsync(
    "user456",
    "محمد علي",
    NotificationType.UserRoleChanged,
    "مدير الموارد البشرية"
);
```

### 3. تعديل البيانات المهمة

```csharp
// إشعار تعديل بيانات موظف
await notificationService.SendDataChangeNotificationAsync(
    "Employee",
    "emp123",
    "Updated",
    "admin123",
    "مدير النظام"
);

// إشعار حذف بيانات مهمة
await notificationService.SendDataChangeNotificationAsync(
    "Invoice",
    "inv456",
    "Deleted",
    "user789",
    "محاسب"
);
```

## 🔧 التخصيص والتطوير

### إضافة أنواع إشعارات جديدة

1. أضف النوع الجديد إلى `NotificationType` enum
2. أنشئ نموذج مخصص في `Models/NotificationModels.cs`
3. أضف منطق الإرسال في `NotificationService`
4. أنشئ endpoint في `NotificationsController`

### إضافة قوالب إشعارات

```csharp
var template = new NotificationTemplate
{
    Name = "طلب إجازة",
    Title = "طلب إجازة جديد من {EmployeeName}",
    Message = "لديك طلب إجازة جديد من {EmployeeName} للفترة من {StartDate} إلى {EndDate}",
    Type = NotificationType.ApprovalRequest,
    Priority = NotificationPriority.High,
    Parameters = new Dictionary<string, object>
    {
        ["EmployeeName"] = "string",
        ["StartDate"] = "date",
        ["EndDate"] = "date"
    }
};
```

### تخصيص نظام المصادقة

قم بتعديل دالة `GetUserId()` في `NotificationHub.cs`:

```csharp
private string GetUserId()
{
    // استخدام JWT Token
    var token = Context.GetHttpContext()?.Request.Headers["Authorization"];
    // فك تشفير Token والحصول على User ID
    
    // أو استخدام Claims
    return Context.User?.FindFirst("sub")?.Value ?? "Anonymous";
}
```

## 🗄️ قاعدة البيانات

### إعداد MySQL

#### الطريقة السريعة - Docker:
```bash
# تشغيل MySQL و phpMyAdmin
docker-compose up -d

# تطبيق Migration
dotnet ef database update
```

#### الطريقة اليدوية:
1. تثبيت MySQL 8.0
2. تشغيل ملف SQL: `mysql -u root -p < database_setup.sql`
3. تطبيق Migration: `dotnet ef database update`

### إعدادات الاتصال

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SignalRNotificationsDB;Uid=signalr_app;Pwd=SignalR@2024!;CharSet=utf8mb4;"
  }
}
```

### الجداول المُنشأة

- **Notifications**: الإشعارات الأساسية
- **UserNotifications**: تتبع حالة الإشعار لكل مستخدم  
- **NotificationTemplates**: قوالب الإشعارات
- **UserSessions**: جلسات المستخدمين المتصلين

## 🔒 الأمان

### CORS Configuration

```csharp
// للتطوير
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// للإنتاج
options.AddPolicy("Production", policy =>
{
    policy.WithOrigins("https://yourdomain.com")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
});
```

### Authentication Integration

```csharp
// إضافة JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
```

## 📊 المراقبة والإحصائيات

### إحصائيات الإشعارات

```csharp
var stats = await notificationService.GetUserNotificationStatsAsync("user123");
Console.WriteLine($"إجمالي الإشعارات: {stats.TotalNotifications}");
Console.WriteLine($"غير مقروءة: {stats.UnreadNotifications}");
Console.WriteLine($"عالية الأولوية: {stats.HighPriorityNotifications}");
```

### المستخدمين المتصلين

```csharp
var connectedUsers = await notificationService.GetConnectedUsersAsync();
Console.WriteLine($"المستخدمين المتصلين: {connectedUsers.Count}");
```

## 🚀 النشر والإنتاج

### Docker Support

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SignalR.API.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalR.API.dll"]
```

### Environment Variables

```bash
# Production Settings
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Server=prod-server;Database=NotificationsDB;..."
ASPNETCORE_URLS="https://+:443;http://+:80"
```

## 🤝 المساهمة

1. Fork المشروع
2. أنشئ branch للميزة الجديدة (`git checkout -b feature/AmazingFeature`)
3. Commit التغييرات (`git commit -m 'Add some AmazingFeature'`)
4. Push إلى Branch (`git push origin feature/AmazingFeature`)
5. افتح Pull Request

## 📄 الترخيص

هذا المشروع مرخص تحت رخصة MIT - راجع ملف [LICENSE](LICENSE) للتفاصيل.

## 📞 الدعم

للدعم والاستفسارات:
- 📧 البريد الإلكتروني: support@company.com
- 💬 Slack: #signalr-support
- 📖 الوثائق: [docs.company.com](https://docs.company.com)

---
<img width="991" height="884" alt="image" src="https://github.com/user-attachments/assets/6878de57-04d0-47fa-8c2b-b7398585aa5b" />


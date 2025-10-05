# 🚀 دليل تشغيل نظام الإشعارات الفورية

## 📋 المتطلبات الأساسية

- .NET 9.0 SDK
- SQL Server LocalDB (أو أي قاعدة بيانات SQL Server)
- Visual Studio 2022 أو VS Code
- متصفح ويب حديث

## 🛠️ خطوات التشغيل

### 1. استنساخ وتشغيل المشروع

```bash
# استنساخ المشروع
git clone <repository-url>
cd SignalR.API

# استعادة الحزم
dotnet restore

# بناء المشروع
dotnet build

# تشغيل المشروع
dotnet run
```

### 2. إعداد قاعدة البيانات (اختياري)

```bash
# إنشاء Migration
dotnet ef migrations add InitialCreate

# تطبيق Migration على قاعدة البيانات
dotnet ef database update
```

### 3. الوصول للخدمات

بعد تشغيل المشروع، ستكون الخدمات متاحة على:

- **API Documentation**: `https://localhost:7016/swagger`
- **Test Interface**: `https://localhost:7016/index.html`
- **SignalR Hub**: `https://localhost:7016/notificationHub`

## 🧪 اختبار النظام

### 1. اختبار واجهة الويب

1. افتح المتصفح واذهب إلى `https://localhost:7016/index.html`
2. أدخل معرف المستخدم (مثل: `user123`)
3. اختر مجموعة مناسبة (مثل: `HR`)
4. اضغط "اتصال"
5. جرب الأزرار المختلفة لإرسال إشعارات تجريبية

### 2. اختبار API

#### إرسال إشعار عام
```bash
curl -X POST "https://localhost:7016/api/notifications/send" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "إشعار تجريبي",
    "message": "هذا إشعار للاختبار",
    "type": 1,
    "priority": 2,
    "targetUserId": "user123"
  }'
```

#### إرسال إشعار موافقة
```bash
curl -X POST "https://localhost:7016/api/notifications/approval?requestId=VACATION_123&requestType=طلب إجازة&targetUserId=user123&approverName=مدير الموارد البشرية"
```

#### الحصول على إشعارات المستخدم
```bash
curl -X GET "https://localhost:7016/api/notifications/user/user123"
```

### 3. اختبار SignalR مباشرة

```javascript
// في متصفح الويب (F12 -> Console)
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.start().then(() => {
    console.log("متصل بنجاح!");
    
    // الانضمام لمجموعة
    connection.invoke("JoinGroup", "HR");
    
    // استقبال الإشعارات
    connection.on("ReceiveNotification", (notification) => {
        console.log("إشعار جديد:", notification);
    });
});
```

## 📱 استخدام النظام في التطبيقات

### JavaScript/HTML

```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://unpkg.com/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>
</head>
<body>
    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7016/notificationHub")
            .build();

        connection.start().then(() => {
            console.log("متصل!");
            
            // استقبال الإشعارات
            connection.on("ReceiveNotification", (notification) => {
                // عرض الإشعار في الواجهة
                showNotification(notification);
            });
        });

        function showNotification(notification) {
            // إنشاء عنصر إشعار
            const notificationElement = document.createElement('div');
            notificationElement.innerHTML = `
                <h3>${notification.title}</h3>
                <p>${notification.message}</p>
                <small>${new Date(notification.createdAt).toLocaleString()}</small>
            `;
            
            // إضافة للصفحة
            document.body.appendChild(notificationElement);
        }
    </script>
</body>
</html>
```

### C# Client

```csharp
using Microsoft.AspNetCore.SignalR.Client;

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

## 🔧 التخصيص

### 1. تغيير إعدادات قاعدة البيانات

عدّل ملف `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### 2. تخصيص CORS

عدّل ملف `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### 3. إضافة نظام مصادقة

```csharp
// في Program.cs
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

## 🚀 النشر

### 1. نشر على IIS

```bash
# إنشاء ملف نشر
dotnet publish -c Release -o ./publish

# نسخ الملفات إلى مجلد IIS
# تكوين IIS لاستضافة ASP.NET Core
```

### 2. نشر باستخدام Docker

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

```bash
# بناء وتشغيل Docker
docker build -t signalr-api .
docker run -p 8080:80 signalr-api
```

## 🔍 استكشاف الأخطاء

### 1. مشاكل الاتصال

- تأكد من أن المنافذ 5118 و 7016 مفتوحة
- تحقق من إعدادات CORS
- تأكد من أن SignalR Hub يعمل بشكل صحيح

### 2. مشاكل قاعدة البيانات

- تحقق من اتصال قاعدة البيانات
- تأكد من تطبيق Migrations
- تحقق من صلاحيات المستخدم

### 3. مشاكل الإشعارات

- تحقق من أن المستخدم متصل بـ SignalR
- تأكد من أن المستخدم في المجموعة الصحيحة
- تحقق من logs للتأكد من إرسال الإشعارات

## 📞 الدعم

للمساعدة والدعم:
- 📧 البريد الإلكتروني: support@company.com
- 💬 Slack: #signalr-support
- 📖 الوثائق: [docs.company.com](https://docs.company.com)

---

**تم تطوير هذا النظام بواسطة فريق التطوير** 🚀

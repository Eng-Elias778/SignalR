# 🚀 دليل سريع لإعداد MySQL

## 🐳 الطريقة السريعة - Docker

### 1. تشغيل MySQL باستخدام Docker Compose

```bash
# تشغيل MySQL و phpMyAdmin
docker-compose up -d

# التحقق من حالة الخدمات
docker-compose ps
```

### 2. الوصول للخدمات

- **MySQL**: `localhost:3306`
- **phpMyAdmin**: `http://localhost:8080`
  - المستخدم: `signalr_app`
  - كلمة المرور: `SignalR@2024!`

### 3. تطبيق Migration

```bash
# تطبيق Migration على قاعدة البيانات
dotnet ef database update
```

### 4. تشغيل التطبيق

```bash
dotnet run
```

## 🛠️ الطريقة اليدوية - تثبيت MySQL

### 1. تثبيت MySQL

#### Windows:
- تحميل من: https://dev.mysql.com/downloads/mysql/
- تشغيل المثبت واتباع التعليمات

#### Linux:
```bash
sudo apt update
sudo apt install mysql-server
sudo mysql_secure_installation
```

#### macOS:
```bash
brew install mysql
brew services start mysql
```

### 2. إنشاء قاعدة البيانات

```bash
# الاتصال بـ MySQL
mysql -u root -p

# تشغيل ملف SQL
mysql -u root -p < database_setup.sql
```

### 3. تطبيق Migration

```bash
dotnet ef database update
```

## 🔧 إعدادات الاتصال

### ملف appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SignalRNotificationsDB;Uid=signalr_app;Pwd=SignalR@2024!;CharSet=utf8mb4;"
  }
}
```

### ملف appsettings.Development.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SignalRNotificationsDB_Dev;Uid=signalr_app;Pwd=SignalR@2024!;CharSet=utf8mb4;"
  }
}
```

## 🧪 اختبار النظام

### 1. اختبار الاتصال

```bash
# تشغيل التطبيق
dotnet run

# فتح المتصفح
# https://localhost:7016/swagger
# https://localhost:7016/index.html
```

### 2. اختبار قاعدة البيانات

```sql
-- الاتصال بقاعدة البيانات
USE SignalRNotificationsDB;

-- عرض الجداول
SHOW TABLES;

-- عرض الإشعارات التجريبية
SELECT * FROM Notifications LIMIT 5;

-- عرض الإحصائيات
SELECT * FROM NotificationStats;
```

## 📊 الجداول المُنشأة

1. **Notifications** - الإشعارات الأساسية
2. **UserNotifications** - تتبع حالة الإشعار لكل مستخدم
3. **NotificationTemplates** - قوالب الإشعارات
4. **UserSessions** - جلسات المستخدمين المتصلين

## 🔍 استكشاف الأخطاء

### مشكلة الاتصال:
```bash
# تحقق من حالة MySQL
docker-compose logs mysql

# أو إذا كان مثبت محلياً
sudo systemctl status mysql
```

### مشكلة Migration:
```bash
# إزالة Migration الحالي
dotnet ef migrations remove

# إنشاء Migration جديد
dotnet ef migrations add InitialCreate

# تطبيق Migration
dotnet ef database update
```

## 🎯 الخطوات التالية

1. ✅ تثبيت MySQL
2. ✅ إنشاء قاعدة البيانات
3. ✅ تطبيق Migration
4. ✅ تشغيل التطبيق
5. ✅ اختبار النظام

---

**النظام جاهز للاستخدام!** 🎉

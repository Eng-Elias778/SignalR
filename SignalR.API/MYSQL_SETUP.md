# 🗄️ إعداد قاعدة البيانات MySQL

## 📋 المتطلبات

- MySQL Server 8.0 أو أحدث
- MySQL Workbench (اختياري)
- أو استخدام MySQL عبر Docker

## 🚀 طرق التثبيت

### الطريقة 1: تثبيت MySQL مباشرة

#### Windows:
1. تحميل MySQL من: https://dev.mysql.com/downloads/mysql/
2. تشغيل المثبت واتباع التعليمات
3. تذكر كلمة المرور الجذرية

#### Linux (Ubuntu/Debian):
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

### الطريقة 2: استخدام Docker

```bash
# تشغيل MySQL في Docker
docker run --name mysql-signalr \
  -e MYSQL_ROOT_PASSWORD=yourpassword \
  -e MYSQL_DATABASE=SignalRNotificationsDB \
  -p 3306:3306 \
  -d mysql:8.0

# أو استخدام docker-compose
```

## 🔧 إعداد قاعدة البيانات

### 1. الاتصال بـ MySQL

```bash
# Windows/Linux/macOS
mysql -u root -p
```

### 2. إنشاء قاعدة البيانات

```sql
-- إنشاء قاعدة البيانات
CREATE DATABASE SignalRNotificationsDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- إنشاء قاعدة بيانات للتطوير
CREATE DATABASE SignalRNotificationsDB_Dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- إنشاء مستخدم مخصص (اختياري)
CREATE USER 'signalr_user'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON SignalRNotificationsDB.* TO 'signalr_user'@'localhost';
GRANT ALL PRIVILEGES ON SignalRNotificationsDB_Dev.* TO 'signalr_user'@'localhost';
FLUSH PRIVILEGES;
```

### 3. تحديث إعدادات الاتصال

عدّل ملف `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SignalRNotificationsDB;Uid=root;Pwd=your_password;CharSet=utf8mb4;"
  }
}
```

أو إذا كنت تستخدم مستخدم مخصص:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SignalRNotificationsDB;Uid=signalr_user;Pwd=your_password;CharSet=utf8mb4;"
  }
}
```

## 🐳 استخدام Docker Compose

أنشئ ملف `docker-compose.yml`:

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: signalr-mysql
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: SignalRNotificationsDB
      MYSQL_USER: signalr_user
      MYSQL_PASSWORD: signalr_password
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql
    command: --default-authentication-plugin=mysql_native_password

volumes:
  mysql_data:
```

ملف `init.sql`:

```sql
CREATE DATABASE IF NOT EXISTS SignalRNotificationsDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS SignalRNotificationsDB_Dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

تشغيل Docker Compose:

```bash
docker-compose up -d
```

## 🔄 تطبيق Migration

بعد إعداد MySQL:

```bash
# تطبيق Migration على قاعدة البيانات
dotnet ef database update

# أو إذا كنت تستخدم Docker
dotnet ef database update --connection "Server=localhost;Database=SignalRNotificationsDB;Uid=root;Pwd=rootpassword;CharSet=utf8mb4;"
```

## 🧪 اختبار الاتصال

### 1. اختبار من التطبيق

```bash
dotnet run
```

### 2. اختبار مباشر من MySQL

```sql
-- الاتصال بقاعدة البيانات
USE SignalRNotificationsDB;

-- عرض الجداول
SHOW TABLES;

-- فحص جدول الإشعارات
DESCRIBE Notifications;
```

## 🔧 استكشاف الأخطاء

### مشكلة الاتصال

```bash
# تحقق من حالة MySQL
# Windows
net start mysql

# Linux
sudo systemctl status mysql
sudo systemctl start mysql

# macOS
brew services start mysql
```

### مشكلة الصلاحيات

```sql
-- إعادة تعيين صلاحيات المستخدم
GRANT ALL PRIVILEGES ON SignalRNotificationsDB.* TO 'signalr_user'@'localhost';
FLUSH PRIVILEGES;
```

### مشكلة Character Set

```sql
-- تحقق من إعدادات Character Set
SHOW VARIABLES LIKE 'character_set%';
SHOW VARIABLES LIKE 'collation%';

-- تحديث قاعدة البيانات
ALTER DATABASE SignalRNotificationsDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

## 📊 مراقبة الأداء

### عرض العمليات النشطة

```sql
SHOW PROCESSLIST;
```

### عرض إحصائيات قاعدة البيانات

```sql
SELECT 
    table_schema AS 'Database',
    table_name AS 'Table',
    table_rows AS 'Rows',
    ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'Size (MB)'
FROM information_schema.tables
WHERE table_schema = 'SignalRNotificationsDB'
ORDER BY (data_length + index_length) DESC;
```

## 🔒 الأمان

### 1. تغيير كلمة مرور الجذر

```sql
ALTER USER 'root'@'localhost' IDENTIFIED BY 'new_strong_password';
FLUSH PRIVILEGES;
```

### 2. إنشاء مستخدم محدود الصلاحيات

```sql
CREATE USER 'signalr_readonly'@'localhost' IDENTIFIED BY 'readonly_password';
GRANT SELECT ON SignalRNotificationsDB.* TO 'signalr_readonly'@'localhost';
FLUSH PRIVILEGES;
```

### 3. إعداد SSL (اختياري)

```sql
-- تفعيل SSL للمستخدم
ALTER USER 'signalr_user'@'localhost' REQUIRE SSL;
```

## 📝 ملاحظات مهمة

1. **تأكد من تشغيل MySQL** قبل تشغيل التطبيق
2. **استخدم كلمات مرور قوية** في الإنتاج
3. **قم بعمل نسخ احتياطية** دورية لقاعدة البيانات
4. **راقب استخدام الذاكرة** ومساحة القرص
5. **استخدم Connection Pooling** في الإنتاج

## 🆘 الدعم

إذا واجهت مشاكل:

1. تحقق من logs التطبيق
2. تحقق من logs MySQL
3. تأكد من إعدادات Firewall
4. تحقق من إعدادات الشبكة

---

**تم إعداد هذا الدليل لمساعدتك في إعداد MySQL بنجاح** 🚀

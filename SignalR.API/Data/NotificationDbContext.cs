using Microsoft.EntityFrameworkCore;
using SignalR.API.Models;

namespace SignalR.API.Data
{
    /// <summary>
    /// DbContext لإدارة الإشعارات في قاعدة البيانات
    /// </summary>
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إعداد جدول الإشعارات
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Priority).IsRequired();
                entity.Property(e => e.SenderId).HasMaxLength(50);
                entity.Property(e => e.SenderName).HasMaxLength(100);
                entity.Property(e => e.TargetUserId).HasMaxLength(50);
                entity.Property(e => e.TargetGroup).HasMaxLength(50);
                entity.Property(e => e.ActionUrl).HasMaxLength(500);
                entity.Property(e => e.IconUrl).HasMaxLength(500);
                
                // تحويل Dictionary إلى JSON
                entity.Property(e => e.Metadata)
                    .HasConversion(
                        v => v != null ? System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!) : null,
                        v => v != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null!) : null
                    );
                
                entity.HasIndex(e => e.TargetUserId);
                entity.HasIndex(e => e.TargetGroup);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsRead);
            });

            // إعداد جدول إشعارات المستخدمين
            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NotificationId).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.NotificationId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });

            // إعداد جدول قوالب الإشعارات
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Priority).IsRequired();
                
                // تحويل Dictionary إلى JSON
                entity.Property(e => e.Parameters)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new Dictionary<string, object>()
                    );
                
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
            });

            // إعداد جدول جلسات المستخدمين
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConnectionId).HasMaxLength(100);
                entity.Property(e => e.IPAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ConnectionId);
                entity.HasIndex(e => e.LastActivity);
            });
        }
    }

    /// <summary>
    /// نموذج إشعارات المستخدمين (جدول منفصل لتتبع حالة الإشعار لكل مستخدم)
    /// </summary>
    public class UserNotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string NotificationId { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public Notification? Notification { get; set; }
    }

    /// <summary>
    /// نموذج قوالب الإشعارات
    /// </summary>
    public class NotificationTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
        public string? IconUrl { get; set; }
        public string? ActionUrl { get; set; }
    }

    /// <summary>
    /// نموذج جلسة المستخدم في قاعدة البيانات
    /// </summary>
    public class UserSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string? ConnectionId { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsConnected { get; set; } = true;
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DisconnectedAt { get; set; }
    }

    /// <summary>
    /// خدمة قاعدة البيانات للإشعارات
    /// </summary>
    public interface INotificationRepository
    {
        Task<string> CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<Notification?> GetNotificationByIdAsync(string notificationId);
        Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);
        Task<NotificationStats> GetUserNotificationStatsAsync(string userId);
        Task<List<UserSession>> GetActiveSessionsAsync();
        Task<UserSession?> GetUserSessionAsync(string userId);
        Task<bool> UpdateUserSessionAsync(UserSession session);
        Task<bool> RemoveUserSessionAsync(string userId);
        Task<List<NotificationTemplate>> GetNotificationTemplatesAsync();
        Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(string templateId);
        Task<string> CreateNotificationFromTemplateAsync(string templateId, Dictionary<string, object> parameters, string? targetUserId = null, string? targetGroup = null);
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateNotificationAsync(Notification notification)
        {
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                return await _context.UserNotifications
                    .Where(un => un.UserId == userId && un.DeletedAt == null)
                    .Include(un => un.Notification)
                    .OrderByDescending(un => un.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(un => un.Notification!)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                throw;
            }
        }

        public async Task<Notification?> GetNotificationByIdAsync(string notificationId)
        {
            try
            {
                return await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification by ID");
                throw;
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            try
            {
                var userNotification = await _context.UserNotifications
                    .FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.UserId == userId);

                if (userNotification != null)
                {
                    userNotification.IsRead = true;
                    userNotification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var userNotifications = await _context.UserNotifications
                    .Where(un => un.UserId == userId && !un.IsRead)
                    .ToListAsync();

                foreach (var userNotification in userNotifications)
                {
                    userNotification.IsRead = true;
                    userNotification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            try
            {
                var userNotification = await _context.UserNotifications
                    .FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.UserId == userId);

                if (userNotification != null)
                {
                    userNotification.DeletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw;
            }
        }

        public async Task<NotificationStats> GetUserNotificationStatsAsync(string userId)
        {
            try
            {
                var stats = new NotificationStats();
                var userNotifications = await _context.UserNotifications
                    .Where(un => un.UserId == userId && un.DeletedAt == null)
                    .Include(un => un.Notification)
                    .ToListAsync();

                stats.TotalNotifications = userNotifications.Count;
                stats.UnreadNotifications = userNotifications.Count(un => !un.IsRead);
                stats.HighPriorityNotifications = userNotifications.Count(un => un.Notification!.Priority == NotificationPriority.High);
                stats.CriticalNotifications = userNotifications.Count(un => un.Notification!.Priority == NotificationPriority.Critical);

                stats.NotificationsByType = userNotifications
                    .GroupBy(un => un.Notification!.Type)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.NotificationsByPriority = userNotifications
                    .GroupBy(un => un.Notification!.Priority)
                    .ToDictionary(g => g.Key, g => g.Count());

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notification stats");
                throw;
            }
        }

        public async Task<List<UserSession>> GetActiveSessionsAsync()
        {
            try
            {
                return await _context.UserSessions
                    .Where(s => s.IsConnected)
                    .OrderByDescending(s => s.LastActivity)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                throw;
            }
        }

        public async Task<UserSession?> GetUserSessionAsync(string userId)
        {
            try
            {
                return await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IsConnected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user session");
                throw;
            }
        }

        public async Task<bool> UpdateUserSessionAsync(UserSession session)
        {
            try
            {
                _context.UserSessions.Update(session);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user session");
                throw;
            }
        }

        public async Task<bool> RemoveUserSessionAsync(string userId)
        {
            try
            {
                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (session != null)
                {
                    session.IsConnected = false;
                    session.DisconnectedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user session");
                throw;
            }
        }

        public async Task<List<NotificationTemplate>> GetNotificationTemplatesAsync()
        {
            try
            {
                return await _context.NotificationTemplates
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates");
                throw;
            }
        }

        public async Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(string templateId)
        {
            try
            {
                return await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template by ID");
                throw;
            }
        }

        public async Task<string> CreateNotificationFromTemplateAsync(string templateId, Dictionary<string, object> parameters, string? targetUserId = null, string? targetGroup = null)
        {
            try
            {
                var template = await GetNotificationTemplateByIdAsync(templateId);
                if (template == null)
                {
                    throw new ArgumentException("Template not found");
                }

                var notification = new Notification
                {
                    Title = ReplaceParameters(template.Title, parameters),
                    Message = ReplaceParameters(template.Message, parameters),
                    Type = template.Type,
                    Priority = template.Priority,
                    TargetUserId = targetUserId,
                    TargetGroup = targetGroup,
                    IconUrl = template.IconUrl,
                    ActionUrl = ReplaceParameters(template.ActionUrl ?? "", parameters),
                    CreatedAt = DateTime.UtcNow
                };

                return await CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification from template");
                throw;
            }
        }

        private string ReplaceParameters(string text, Dictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                text = text.Replace($"{{{parameter.Key}}}", parameter.Value?.ToString() ?? "");
            }
            return text;
        }
    }
}

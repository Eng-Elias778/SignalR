using Microsoft.AspNetCore.SignalR;
using SignalR.API.Hubs;
using SignalR.API.Models;
using System.Collections.Concurrent;

namespace SignalR.API.Services
{
    /// <summary>
    /// خدمة إدارة الإشعارات الفورية
    /// تتعامل مع إرسال وتخزين وإدارة الإشعارات في نظام ERP
    /// </summary>
    public interface INotificationService
    {
        Task<string> SendNotificationAsync(CreateNotificationRequest request);
        Task<string> SendApprovalNotificationAsync(string requestId, string requestType, string targetUserId, string approverName);
        Task<string> SendUserManagementNotificationAsync(string affectedUserId, string affectedUserName, NotificationType type, string adminName);
        Task<string> SendDataChangeNotificationAsync(string entityType, string entityId, string changeType, string userId, string userName);
        Task<List<NotificationResponse>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<NotificationStats> GetUserNotificationStatsAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);
        Task<List<string>> GetConnectedUsersAsync();
        Task SendSystemAlertAsync(string title, string message, NotificationPriority priority = NotificationPriority.High);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        
        // في التطبيق الحقيقي، يجب استخدام قاعدة بيانات بدلاً من الذاكرة
        private static readonly ConcurrentDictionary<string, List<Notification>> _userNotifications = new();
        private static readonly ConcurrentDictionary<string, Notification> _notifications = new();

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// إرسال إشعار عام
        /// </summary>
        public async Task<string> SendNotificationAsync(CreateNotificationRequest request)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    Priority = request.Priority,
                    SenderId = request.SenderId,
                    SenderName = request.SenderName,
                    TargetUserId = request.TargetUserId,
                    TargetGroup = request.TargetGroup,
                    ExpiresAt = request.ExpiresAt,
                    Metadata = request.Metadata,
                    ActionUrl = request.ActionUrl,
                    IconUrl = request.IconUrl,
                    CreatedAt = DateTime.UtcNow
                };

                // حفظ الإشعار
                _notifications.TryAdd(notification.Id, notification);

                // إضافة الإشعار لمستخدم محدد
                if (!string.IsNullOrEmpty(notification.TargetUserId))
                {
                    AddNotificationToUser(notification.TargetUserId, notification);
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                }
                // إرسال لمجموعة
                else if (!string.IsNullOrEmpty(notification.TargetGroup))
                {
                    await _hubContext.Clients.Group(notification.TargetGroup).SendAsync("ReceiveNotification", notification);
                }
                // إرسال للجميع
                else
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                }

                _logger.LogInformation($"Notification sent: {notification.Id} - {notification.Title}");
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                throw;
            }
        }

        /// <summary>
        /// إرسال إشعار موافقة
        /// </summary>
        public async Task<string> SendApprovalNotificationAsync(string requestId, string requestType, string targetUserId, string approverName)
        {
            var request = new CreateNotificationRequest
            {
                Title = $"طلب موافقة جديد - {requestType}",
                Message = $"لديك طلب موافقة جديد من {approverName} يحتاج إلى مراجعة",
                Type = NotificationType.ApprovalRequest,
                Priority = NotificationPriority.High,
                TargetUserId = targetUserId,
                SenderName = approverName,
                ActionUrl = $"/approvals/{requestId}",
                Metadata = new Dictionary<string, object>
                {
                    ["RequestId"] = requestId,
                    ["RequestType"] = requestType,
                    ["ApproverName"] = approverName
                }
            };

            return await SendNotificationAsync(request);
        }

        /// <summary>
        /// إرسال إشعار إدارة المستخدمين
        /// </summary>
        public async Task<string> SendUserManagementNotificationAsync(string affectedUserId, string affectedUserName, NotificationType type, string adminName)
        {
            string title = type switch
            {
                NotificationType.UserCreated => $"مستخدم جديد تم إنشاؤه",
                NotificationType.UserUpdated => $"تم تحديث بيانات المستخدم",
                NotificationType.UserDeleted => $"تم حذف مستخدم",
                NotificationType.UserRoleChanged => $"تم تغيير صلاحيات المستخدم",
                _ => "تغيير في إدارة المستخدمين"
            };

            string message = type switch
            {
                NotificationType.UserCreated => $"تم إنشاء مستخدم جديد: {affectedUserName}",
                NotificationType.UserUpdated => $"تم تحديث بيانات المستخدم: {affectedUserName}",
                NotificationType.UserDeleted => $"تم حذف المستخدم: {affectedUserName}",
                NotificationType.UserRoleChanged => $"تم تغيير صلاحيات المستخدم: {affectedUserName}",
                _ => $"تغيير في المستخدم: {affectedUserName}"
            };

            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = type,
                Priority = NotificationPriority.High,
                SenderName = adminName,
                ActionUrl = $"/users/{affectedUserId}",
                Metadata = new Dictionary<string, object>
                {
                    ["AffectedUserId"] = affectedUserId,
                    ["AffectedUserName"] = affectedUserName,
                    ["AdminName"] = adminName
                }
            };

            return await SendNotificationAsync(request);
        }

        /// <summary>
        /// إرسال إشعار تغيير البيانات
        /// </summary>
        public async Task<string> SendDataChangeNotificationAsync(string entityType, string entityId, string changeType, string userId, string userName)
        {
            string title = changeType switch
            {
                "Created" => $"تم إنشاء {entityType} جديد",
                "Updated" => $"تم تحديث {entityType}",
                "Deleted" => $"تم حذف {entityType}",
                _ => $"تغيير في {entityType}"
            };

            string message = changeType switch
            {
                "Created" => $"تم إنشاء {entityType} جديد بواسطة {userName}",
                "Updated" => $"تم تحديث {entityType} بواسطة {userName}",
                "Deleted" => $"تم حذف {entityType} بواسطة {userName}",
                _ => $"تم تغيير {entityType} بواسطة {userName}"
            };

            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = NotificationType.DataModified,
                Priority = NotificationPriority.Normal,
                SenderName = userName,
                ActionUrl = $"/{entityType.ToLower()}/{entityId}",
                Metadata = new Dictionary<string, object>
                {
                    ["EntityType"] = entityType,
                    ["EntityId"] = entityId,
                    ["ChangeType"] = changeType,
                    ["UserId"] = userId,
                    ["UserName"] = userName
                }
            };

            return await SendNotificationAsync(request);
        }

        /// <summary>
        /// الحصول على إشعارات المستخدم
        /// </summary>
        public Task<List<NotificationResponse>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            if (_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToResponse)
                    .ToList());
            }

            return Task.FromResult(new List<NotificationResponse>());
        }

        /// <summary>
        /// الحصول على إحصائيات إشعارات المستخدم
        /// </summary>
        public Task<NotificationStats> GetUserNotificationStatsAsync(string userId)
        {
            var stats = new NotificationStats();

            if (_userNotifications.TryGetValue(userId, out var notifications))
            {
                stats.TotalNotifications = notifications.Count;
                stats.UnreadNotifications = notifications.Count(n => !n.IsRead);
                stats.HighPriorityNotifications = notifications.Count(n => n.Priority == NotificationPriority.High);
                stats.CriticalNotifications = notifications.Count(n => n.Priority == NotificationPriority.Critical);

                stats.NotificationsByType = notifications
                    .GroupBy(n => n.Type)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.NotificationsByPriority = notifications
                    .GroupBy(n => n.Priority)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            return Task.FromResult(stats);
        }

        /// <summary>
        /// تحديد إشعار كمقروء
        /// </summary>
        public Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            if (_notifications.TryGetValue(notificationId, out var notification) &&
                _userNotifications.TryGetValue(userId, out var userNotifications))
            {
                var userNotification = userNotifications.FirstOrDefault(n => n.Id == notificationId);
                if (userNotification != null)
                {
                    userNotification.IsRead = true;
                    userNotification.ReadAt = DateTime.UtcNow;
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// تحديد جميع الإشعارات كمقروءة
        /// </summary>
        public Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            if (_userNotifications.TryGetValue(userId, out var notifications))
            {
                foreach (var notification in notifications.Where(n => !n.IsRead))
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// حذف إشعار
        /// </summary>
        public Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            if (_userNotifications.TryGetValue(userId, out var notifications))
            {
                var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notifications.Remove(notification);
                    _notifications.TryRemove(notificationId, out _);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// الحصول على المستخدمين المتصلين
        /// </summary>
        public Task<List<string>> GetConnectedUsersAsync()
        {
            // يمكن تحسين هذا باستخدام قاعدة بيانات أو Redis
            return Task.FromResult(new List<string>());
        }

        /// <summary>
        /// إرسال تنبيه نظام
        /// </summary>
        public async Task SendSystemAlertAsync(string title, string message, NotificationPriority priority = NotificationPriority.High)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = NotificationType.SystemAlert,
                Priority = priority,
                SenderName = "النظام",
                IconUrl = "/icons/system-alert.png"
            };

            await SendNotificationAsync(request);
        }

        /// <summary>
        /// إضافة إشعار لمستخدم
        /// </summary>
        private void AddNotificationToUser(string userId, Notification notification)
        {
            _userNotifications.AddOrUpdate(userId,
                new List<Notification> { notification },
                (key, existingList) =>
                {
                    existingList.Add(notification);
                    return existingList;
                });
        }

        /// <summary>
        /// تحويل Notification إلى NotificationResponse
        /// </summary>
        private NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Priority = notification.Priority,
                SenderName = notification.SenderName,
                CreatedAt = notification.CreatedAt,
                ExpiresAt = notification.ExpiresAt,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                Metadata = notification.Metadata,
                ActionUrl = notification.ActionUrl,
                IconUrl = notification.IconUrl
            };
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SignalR.API.Models
{
    /// <summary>
    /// نموذج الإشعار الأساسي
    /// </summary>
    public class Notification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public string? SenderId { get; set; }
        
        public string? SenderName { get; set; }
        
        public string? TargetUserId { get; set; }
        
        public string? TargetGroup { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public string? ActionUrl { get; set; }
        
        public string? IconUrl { get; set; }
    }

    /// <summary>
    /// أنواع الإشعارات المختلفة في نظام ERP
    /// </summary>
    public enum NotificationType
    {
        // إشعارات الموافقة والرفض
        ApprovalRequest = 1,
        ApprovalApproved = 2,
        ApprovalRejected = 3,
        
        // إشعارات إدارة المستخدمين
        UserCreated = 10,
        UserUpdated = 11,
        UserDeleted = 12,
        UserRoleChanged = 13,
        
        // إشعارات البيانات المهمة
        DataModified = 20,
        DataDeleted = 21,
        DataCreated = 22,
        
        // إشعارات النظام
        SystemAlert = 30,
        SystemMaintenance = 31,
        SystemError = 32,
        
        // إشعارات العمل
        TaskAssigned = 40,
        TaskCompleted = 41,
        TaskOverdue = 42,
        
        // إشعارات مخصصة
        Custom = 99
    }

    /// <summary>
    /// أولوية الإشعار
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// نموذج لإنشاء إشعار جديد
    /// </summary>
    public class CreateNotificationRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public NotificationType Type { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        public string? SenderId { get; set; }
        
        public string? SenderName { get; set; }
        
        public string? TargetUserId { get; set; }
        
        public string? TargetGroup { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public string? ActionUrl { get; set; }
        
        public string? IconUrl { get; set; }
    }

    /// <summary>
    /// نموذج لتحديث حالة الإشعار
    /// </summary>
    public class UpdateNotificationRequest
    {
        public bool IsRead { get; set; }
        
        public DateTime? ReadAt { get; set; }
    }

    /// <summary>
    /// نموذج الاستجابة للإشعارات
    /// </summary>
    public class NotificationResponse
    {
        public string Id { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; }
        
        public NotificationPriority Priority { get; set; }
        
        public string? SenderName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsRead { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
        
        public string? ActionUrl { get; set; }
        
        public string? IconUrl { get; set; }
    }

    /// <summary>
    /// نموذج إحصائيات الإشعارات
    /// </summary>
    public class NotificationStats
    {
        public int TotalNotifications { get; set; }
        
        public int UnreadNotifications { get; set; }
        
        public int HighPriorityNotifications { get; set; }
        
        public int CriticalNotifications { get; set; }
        
        public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();
        
        public Dictionary<NotificationPriority, int> NotificationsByPriority { get; set; } = new();
    }

    /// <summary>
    /// نموذج لإشعارات الموافقة المحددة
    /// </summary>
    public class ApprovalNotification : Notification
    {
        public string RequestId { get; set; } = string.Empty;
        
        public string RequestType { get; set; } = string.Empty;
        
        public string? ApproverId { get; set; }
        
        public string? ApproverName { get; set; }
        
        public string? Comments { get; set; }
        
        public ApprovalNotification()
        {
            Type = NotificationType.ApprovalRequest;
        }
    }

    /// <summary>
    /// نموذج لإشعارات إدارة المستخدمين
    /// </summary>
    public class UserManagementNotification : Notification
    {
        public string AffectedUserId { get; set; } = string.Empty;
        
        public string AffectedUserName { get; set; } = string.Empty;
        
        public string? PreviousRole { get; set; }
        
        public string? NewRole { get; set; }
        
        public UserManagementNotification()
        {
            Type = NotificationType.UserCreated;
        }
    }

    /// <summary>
    /// نموذج لإشعارات تعديل البيانات
    /// </summary>
    public class DataChangeNotification : Notification
    {
        public string EntityType { get; set; } = string.Empty;
        
        public string EntityId { get; set; } = string.Empty;
        
        public string? PreviousValue { get; set; }
        
        public string? NewValue { get; set; }
        
        public string ChangeType { get; set; } = string.Empty; // Created, Updated, Deleted
        
        public DataChangeNotification()
        {
            Type = NotificationType.DataModified;
        }
    }
}

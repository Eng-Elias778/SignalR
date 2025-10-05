using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalR.API.Hubs;
using SignalR.API.Models;
using SignalR.API.Services;

namespace SignalR.API.Controllers
{
    /// <summary>
    /// Controller لإدارة الإشعارات الفورية
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// إرسال إشعار جديد
        /// </summary>
        /// <param name="request">بيانات الإشعار</param>
        /// <returns>معرف الإشعار المرسل</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                var notificationId = await _notificationService.SendNotificationAsync(request);
                return Ok(new { NotificationId = notificationId, Message = "تم إرسال الإشعار بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال الإشعار" });
            }
        }

        /// <summary>
        /// إرسال إشعار موافقة
        /// </summary>
        /// <param name="requestId">معرف الطلب</param>
        /// <param name="requestType">نوع الطلب</param>
        /// <param name="targetUserId">معرف المستخدم المستهدف</param>
        /// <param name="approverName">اسم الموافق</param>
        [HttpPost("approval")]
        public async Task<IActionResult> SendApprovalNotification(
            [FromQuery] string requestId,
            [FromQuery] string requestType,
            [FromQuery] string targetUserId,
            [FromQuery] string approverName)
        {
            try
            {
                var notificationId = await _notificationService.SendApprovalNotificationAsync(
                    requestId, requestType, targetUserId, approverName);
                
                return Ok(new { NotificationId = notificationId, Message = "تم إرسال إشعار الموافقة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending approval notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار الموافقة" });
            }
        }

        /// <summary>
        /// إرسال إشعار إدارة المستخدمين
        /// </summary>
        /// <param name="affectedUserId">معرف المستخدم المتأثر</param>
        /// <param name="affectedUserName">اسم المستخدم المتأثر</param>
        /// <param name="type">نوع الإشعار</param>
        /// <param name="adminName">اسم المدير</param>
        [HttpPost("user-management")]
        public async Task<IActionResult> SendUserManagementNotification(
            [FromQuery] string affectedUserId,
            [FromQuery] string affectedUserName,
            [FromQuery] NotificationType type,
            [FromQuery] string adminName)
        {
            try
            {
                var notificationId = await _notificationService.SendUserManagementNotificationAsync(
                    affectedUserId, affectedUserName, type, adminName);
                
                return Ok(new { NotificationId = notificationId, Message = "تم إرسال إشعار إدارة المستخدمين بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending user management notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار إدارة المستخدمين" });
            }
        }

        /// <summary>
        /// إرسال إشعار تغيير البيانات
        /// </summary>
        /// <param name="entityType">نوع الكيان</param>
        /// <param name="entityId">معرف الكيان</param>
        /// <param name="changeType">نوع التغيير</param>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="userName">اسم المستخدم</param>
        [HttpPost("data-change")]
        public async Task<IActionResult> SendDataChangeNotification(
            [FromQuery] string entityType,
            [FromQuery] string entityId,
            [FromQuery] string changeType,
            [FromQuery] string userId,
            [FromQuery] string userName)
        {
            try
            {
                var notificationId = await _notificationService.SendDataChangeNotificationAsync(
                    entityType, entityId, changeType, userId, userName);
                
                return Ok(new { NotificationId = notificationId, Message = "تم إرسال إشعار تغيير البيانات بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data change notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار تغيير البيانات" });
            }
        }

        /// <summary>
        /// الحصول على إشعارات المستخدم
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="page">رقم الصفحة</param>
        /// <param name="pageSize">حجم الصفحة</param>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(
            string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                return StatusCode(500, new { Message = "حدث خطأ أثناء جلب الإشعارات" });
            }
        }

        /// <summary>
        /// الحصول على إحصائيات إشعارات المستخدم
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        [HttpGet("user/{userId}/stats")]
        public async Task<IActionResult> GetUserNotificationStats(string userId)
        {
            try
            {
                var stats = await _notificationService.GetUserNotificationStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notification stats");
                return StatusCode(500, new { Message = "حدث خطأ أثناء جلب إحصائيات الإشعارات" });
            }
        }

        /// <summary>
        /// تحديد إشعار كمقروء
        /// </summary>
        /// <param name="notificationId">معرف الإشعار</param>
        /// <param name="userId">معرف المستخدم</param>
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(
            string notificationId,
            [FromQuery] string userId)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);
                if (result)
                {
                    return Ok(new { Message = "تم تحديد الإشعار كمقروء" });
                }
                return NotFound(new { Message = "الإشعار غير موجود" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { Message = "حدث خطأ أثناء تحديد الإشعار كمقروء" });
            }
        }

        /// <summary>
        /// تحديد جميع الإشعارات كمقروءة
        /// </summary>
        /// <param name="userId">معرف المستخدم</param>
        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead(string userId)
        {
            try
            {
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                if (result)
                {
                    return Ok(new { Message = "تم تحديد جميع الإشعارات كمقروءة" });
                }
                return NotFound(new { Message = "لا توجد إشعارات للمستخدم" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { Message = "حدث خطأ أثناء تحديد جميع الإشعارات كمقروءة" });
            }
        }

        /// <summary>
        /// حذف إشعار
        /// </summary>
        /// <param name="notificationId">معرف الإشعار</param>
        /// <param name="userId">معرف المستخدم</param>
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(
            string notificationId,
            [FromQuery] string userId)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
                if (result)
                {
                    return Ok(new { Message = "تم حذف الإشعار بنجاح" });
                }
                return NotFound(new { Message = "الإشعار غير موجود" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء حذف الإشعار" });
            }
        }

        /// <summary>
        /// الحصول على المستخدمين المتصلين
        /// </summary>
        [HttpGet("connected-users")]
        public async Task<IActionResult> GetConnectedUsers()
        {
            try
            {
                var connectedUsers = await _notificationService.GetConnectedUsersAsync();
                return Ok(connectedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connected users");
                return StatusCode(500, new { Message = "حدث خطأ أثناء جلب المستخدمين المتصلين" });
            }
        }

        /// <summary>
        /// إرسال تنبيه نظام
        /// </summary>
        /// <param name="title">عنوان التنبيه</param>
        /// <param name="message">رسالة التنبيه</param>
        /// <param name="priority">أولوية التنبيه</param>
        [HttpPost("system-alert")]
        public async Task<IActionResult> SendSystemAlert(
            [FromQuery] string title,
            [FromQuery] string message,
            [FromQuery] NotificationPriority priority = NotificationPriority.High)
        {
            try
            {
                await _notificationService.SendSystemAlertAsync(title, message, priority);
                return Ok(new { Message = "تم إرسال تنبيه النظام بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system alert");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال تنبيه النظام" });
            }
        }
    }

    /// <summary>
    /// Controller لإدارة مجموعات SignalR
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IHubContext<NotificationHub> hubContext, ILogger<GroupsController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// إرسال إشعار لمجموعة محددة
        /// </summary>
        /// <param name="groupName">اسم المجموعة</param>
        /// <param name="notification">بيانات الإشعار</param>
        [HttpPost("send-to-group/{groupName}")]
        public async Task<IActionResult> SendToGroup(string groupName, [FromBody] CreateNotificationRequest notification)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                return Ok(new { Message = $"تم إرسال الإشعار للمجموعة {groupName} بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to group");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال الإشعار للمجموعة" });
            }
        }

        /// <summary>
        /// إرسال إشعار لجميع المستخدمين المتصلين
        /// </summary>
        /// <param name="notification">بيانات الإشعار</param>
        [HttpPost("send-to-all")]
        public async Task<IActionResult> SendToAll([FromBody] CreateNotificationRequest notification)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                return Ok(new { Message = "تم إرسال الإشعار لجميع المستخدمين بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال الإشعار لجميع المستخدمين" });
            }
        }
    }
}

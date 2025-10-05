using Microsoft.AspNetCore.Mvc;
using SignalR.API.Services;
using SignalR.API.Models;

namespace SignalR.API.Controllers
{
    /// <summary>
    /// Controller للأمثلة العملية لاستخدام نظام الإشعارات في ERP
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExamplesController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ExamplesController> _logger;

        public ExamplesController(INotificationService notificationService, ILogger<ExamplesController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// مثال 1: إشعار موافقة على طلب إجازة
        /// </summary>
        [HttpGet("vacation-approval")]
        public async Task<IActionResult> SendVacationApprovalNotification(
            [FromQuery] string employeeId,
            [FromQuery] string employeeName,
            [FromQuery] string managerName,
            [FromQuery] string vacationStartDate,
            [FromQuery] string vacationEndDate)
        {
            try
            {
                var notificationId = await _notificationService.SendApprovalNotificationAsync(
                    $"VACATION_{employeeId}_{DateTime.UtcNow:yyyyMMdd}",
                    "طلب إجازة",
                    employeeId,
                    managerName);

                // إشعار إضافي للمدير المباشر
                await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                {
                    Title = "طلب إجازة جديد",
                    Message = $"لديك طلب إجازة جديد من {employeeName} للفترة من {vacationStartDate} إلى {vacationEndDate}",
                    Type = NotificationType.ApprovalRequest,
                    Priority = NotificationPriority.High,
                    TargetUserId = managerName, // يمكن استخدام معرف المدير
                    ActionUrl = $"/hr/vacation-requests/{employeeId}",
                    Metadata = new Dictionary<string, object>
                    {
                        ["EmployeeId"] = employeeId,
                        ["EmployeeName"] = employeeName,
                        ["StartDate"] = vacationStartDate,
                        ["EndDate"] = vacationEndDate,
                        ["RequestType"] = "Vacation"
                    }
                });

                return Ok(new { 
                    Message = "تم إرسال إشعار طلب الإجازة بنجاح",
                    NotificationId = notificationId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending vacation approval notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار طلب الإجازة" });
            }
        }

        /// <summary>
        /// مثال 2: إشعار عند إنشاء مستخدم جديد
        /// </summary>
        [HttpGet("new-user-created")]
        public async Task<IActionResult> SendNewUserCreatedNotification(
            [FromQuery] string newUserId,
            [FromQuery] string newUserName,
            [FromQuery] string newUserEmail,
            [FromQuery] string adminName,
            [FromQuery] string userRole)
        {
            try
            {
                var notificationId = await _notificationService.SendUserManagementNotificationAsync(
                    newUserId,
                    newUserName,
                    NotificationType.UserCreated,
                    adminName);

                // إشعار للمستخدم الجديد
                await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                {
                    Title = "مرحباً بك في النظام",
                    Message = $"مرحباً {newUserName}! تم إنشاء حسابك بنجاح. يمكنك الآن تسجيل الدخول باستخدام البريد الإلكتروني: {newUserEmail}",
                    Type = NotificationType.UserCreated,
                    Priority = NotificationPriority.Normal,
                    TargetUserId = newUserId,
                    ActionUrl = "/profile/setup",
                    IconUrl = "/icons/welcome.png",
                    Metadata = new Dictionary<string, object>
                    {
                        ["Email"] = newUserEmail,
                        ["Role"] = userRole,
                        ["CreatedBy"] = adminName
                    }
                });

                // إشعار لمدير النظام
                await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                {
                    Title = "مستخدم جديد تم إنشاؤه",
                    Message = $"تم إنشاء مستخدم جديد: {newUserName} ({newUserEmail}) بواسطة {adminName}",
                    Type = NotificationType.UserCreated,
                    Priority = NotificationPriority.Normal,
                    TargetGroup = "SystemAdmins",
                    ActionUrl = $"/admin/users/{newUserId}",
                    Metadata = new Dictionary<string, object>
                    {
                        ["NewUserId"] = newUserId,
                        ["NewUserName"] = newUserName,
                        ["NewUserEmail"] = newUserEmail,
                        ["Role"] = userRole,
                        ["CreatedBy"] = adminName
                    }
                });

                return Ok(new { 
                    Message = "تم إرسال إشعارات المستخدم الجديد بنجاح",
                    NotificationId = notificationId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new user notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار المستخدم الجديد" });
            }
        }

        /// <summary>
        /// مثال 3: إشعار عند تعديل بيانات مهمة
        /// </summary>
        [HttpGet("data-modified")]
        public async Task<IActionResult> SendDataModifiedNotification(
            [FromQuery] string entityType,
            [FromQuery] string entityId,
            [FromQuery] string entityName,
            [FromQuery] string changeType,
            [FromQuery] string userId,
            [FromQuery] string userName,
            [FromQuery] string? previousValue = null,
            [FromQuery] string? newValue = null)
        {
            try
            {
                var notificationId = await _notificationService.SendDataChangeNotificationAsync(
                    entityType,
                    entityId,
                    changeType,
                    userId,
                    userName);

                // إشعارات إضافية حسب نوع التغيير
                if (changeType == "Updated" && !string.IsNullOrEmpty(previousValue) && !string.IsNullOrEmpty(newValue))
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                    {
                        Title = $"تم تحديث {entityName}",
                        Message = $"تم تغيير {entityName} من '{previousValue}' إلى '{newValue}' بواسطة {userName}",
                        Type = NotificationType.DataModified,
                        Priority = NotificationPriority.Normal,
                        TargetGroup = GetRelevantGroup(entityType),
                        ActionUrl = $"/{entityType.ToLower()}/{entityId}",
                        Metadata = new Dictionary<string, object>
                        {
                            ["EntityType"] = entityType,
                            ["EntityId"] = entityId,
                            ["EntityName"] = entityName,
                            ["PreviousValue"] = previousValue,
                            ["NewValue"] = newValue,
                            ["ChangedBy"] = userName,
                            ["ChangedById"] = userId
                        }
                    });
                }

                return Ok(new { 
                    Message = "تم إرسال إشعار تعديل البيانات بنجاح",
                    NotificationId = notificationId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data modified notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار تعديل البيانات" });
            }
        }

        /// <summary>
        /// مثال 4: إشعار نظام عند حدوث خطأ
        /// </summary>
        [HttpGet("system-error")]
        public async Task<IActionResult> SendSystemErrorNotification(
            [FromQuery] string errorType,
            [FromQuery] string errorMessage,
            [FromQuery] string? affectedModule = null,
            [FromQuery] string? userId = null)
        {
            try
            {
                var priority = errorType switch
                {
                    "Critical" => NotificationPriority.Critical,
                    "High" => NotificationPriority.High,
                    "Medium" => NotificationPriority.Normal,
                    _ => NotificationPriority.Low
                };

                await _notificationService.SendSystemAlertAsync(
                    $"خطأ في النظام - {errorType}",
                    $"حدث خطأ في {affectedModule ?? "النظام"}: {errorMessage}",
                    priority);

                // إشعار للمطورين/المديرين التقنيين
                await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                {
                    Title = $"تنبيه تقني - {errorType}",
                    Message = $"خطأ {errorType}: {errorMessage}\nالمودول: {affectedModule}\nالمستخدم: {userId ?? "غير محدد"}",
                    Type = NotificationType.SystemError,
                    Priority = priority,
                    TargetGroup = "TechnicalTeam",
                    ActionUrl = "/admin/system-logs",
                    IconUrl = "/icons/error.png",
                    Metadata = new Dictionary<string, object>
                    {
                        ["ErrorType"] = errorType,
                        ["ErrorMessage"] = errorMessage,
                        ["AffectedModule"] = affectedModule ?? "",
                        ["UserId"] = userId ?? "",
                        ["Timestamp"] = DateTime.UtcNow
                    }
                });

                return Ok(new { Message = "تم إرسال إشعار خطأ النظام بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system error notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار خطأ النظام" });
            }
        }

        /// <summary>
        /// مثال 5: إشعارات مهام العمل
        /// </summary>
        [HttpGet("task-assignment")]
        public async Task<IActionResult> SendTaskAssignmentNotification(
            [FromQuery] string taskId,
            [FromQuery] string taskTitle,
            [FromQuery] string assignedToUserId,
            [FromQuery] string assignedToUserName,
            [FromQuery] string assignedByUserName,
            [FromQuery] string dueDate,
            [FromQuery] string priority = "Normal")
        {
            try
            {
                var notificationPriority = priority switch
                {
                    "High" => NotificationPriority.High,
                    "Critical" => NotificationPriority.Critical,
                    "Low" => NotificationPriority.Low,
                    _ => NotificationPriority.Normal
                };

                await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                {
                    Title = "مهمة جديدة مخصصة لك",
                    Message = $"تم تكليفك بمهمة جديدة: {taskTitle}\nالمكلف من: {assignedByUserName}\nتاريخ الاستحقاق: {dueDate}",
                    Type = NotificationType.TaskAssigned,
                    Priority = notificationPriority,
                    TargetUserId = assignedToUserId,
                    ActionUrl = $"/tasks/{taskId}",
                    IconUrl = "/icons/task.png",
                    Metadata = new Dictionary<string, object>
                    {
                        ["TaskId"] = taskId,
                        ["TaskTitle"] = taskTitle,
                        ["AssignedBy"] = assignedByUserName,
                        ["DueDate"] = dueDate,
                        ["Priority"] = priority
                    }
                });

                return Ok(new { Message = "تم إرسال إشعار تكليف المهمة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending task assignment notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار تكليف المهمة" });
            }
        }

        /// <summary>
        /// مثال 6: إشعارات الموافقات المالية
        /// </summary>
        [HttpGet("financial-approval")]
        public async Task<IActionResult> SendFinancialApprovalNotification(
            [FromQuery] string requestId,
            [FromQuery] string requestType,
            [FromQuery] string amount,
            [FromQuery] string currency,
            [FromQuery] string requesterName,
            [FromQuery] string approverUserId,
            [FromQuery] string status) // Approved, Rejected, Pending
        {
            try
            {
                var notificationType = status switch
                {
                    "Approved" => NotificationType.ApprovalApproved,
                    "Rejected" => NotificationType.ApprovalRejected,
                    _ => NotificationType.ApprovalRequest
                };

                var title = status switch
                {
                    "Approved" => "تم الموافقة على الطلب المالي",
                    "Rejected" => "تم رفض الطلب المالي",
                    _ => "طلب موافقة مالية جديد"
                };

                var message = status switch
                {
                    "Approved" => $"تم الموافقة على طلبك المالي ({requestType}) بمبلغ {amount} {currency}",
                    "Rejected" => $"تم رفض طلبك المالي ({requestType}) بمبلغ {amount} {currency}",
                    _ => $"لديك طلب موافقة مالية جديد من {requesterName} ({requestType}) بمبلغ {amount} {currency}"
                };

                var targetUserId = status == "Pending" ? approverUserId : requesterName;

                await _notificationService.SendNotificationAsync(new CreateNotificationRequest
                {
                    Title = title,
                    Message = message,
                    Type = notificationType,
                    Priority = NotificationPriority.High,
                    TargetUserId = targetUserId,
                    ActionUrl = $"/finance/requests/{requestId}",
                    IconUrl = status == "Approved" ? "/icons/approved.png" : 
                             status == "Rejected" ? "/icons/rejected.png" : "/icons/pending.png",
                    Metadata = new Dictionary<string, object>
                    {
                        ["RequestId"] = requestId,
                        ["RequestType"] = requestType,
                        ["Amount"] = amount,
                        ["Currency"] = currency,
                        ["RequesterName"] = requesterName,
                        ["Status"] = status,
                        ["ApproverUserId"] = approverUserId
                    }
                });

                return Ok(new { Message = "تم إرسال إشعار الموافقة المالية بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending financial approval notification");
                return StatusCode(500, new { Message = "حدث خطأ أثناء إرسال إشعار الموافقة المالية" });
            }
        }

        /// <summary>
        /// مساعد لتحديد المجموعة المناسبة حسب نوع الكيان
        /// </summary>
        private string GetRelevantGroup(string entityType)
        {
            return entityType.ToLower() switch
            {
                "employee" or "user" => "HR",
                "invoice" or "payment" or "expense" => "Finance",
                "product" or "inventory" => "Inventory",
                "customer" or "client" => "Sales",
                "project" or "task" => "ProjectManagement",
                _ => "General"
            };
        }
    }
}

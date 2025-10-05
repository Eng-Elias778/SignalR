using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalR.API.Hubs
{
    /// <summary>
    /// SignalR Hub لإدارة الإشعارات الفورية في نظام ERP
    /// يدعم إرسال الإشعارات للمستخدمين الفرديين والمجموعات
    /// </summary>
    public class NotificationHub : Hub
    {
        // قاموس لتتبع المستخدمين المتصلين مع معرفاتهم
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
        
        // قاموس لتتبع المجموعات (الأدوار/الأقسام)
        private static readonly ConcurrentDictionary<string, HashSet<string>> UserGroups = new();

        /// <summary>
        /// عند اتصال مستخدم جديد
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                ConnectedUsers.TryAdd(Context.ConnectionId, userId);
                
                // إضافة المستخدم إلى مجموعة المستخدمين المتصلين
                await Groups.AddToGroupAsync(Context.ConnectionId, "ConnectedUsers");
                
                // إشعار الآخرين بأن المستخدم متصل
                await Clients.Others.SendAsync("UserConnected", userId);
                
                Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// عند انقطاع اتصال مستخدم
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                ConnectedUsers.TryRemove(Context.ConnectionId, out _);
                
                // إشعار الآخرين بأن المستخدم غير متصل
                await Clients.Others.SendAsync("UserDisconnected", userId);
                
                Console.WriteLine($"User {userId} disconnected");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// انضمام مستخدم إلى مجموعة (مثل دور أو قسم)
        /// </summary>
        /// <param name="groupName">اسم المجموعة (مثل: "Admins", "HR", "Finance")</param>
        public async Task JoinGroup(string groupName)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                
                // إضافة المستخدم إلى قاموس المجموعات
                UserGroups.AddOrUpdate(groupName, 
                    new HashSet<string> { userId }, 
                    (key, existingSet) => 
                    {
                        existingSet.Add(userId);
                        return existingSet;
                    });
                
                Console.WriteLine($"User {userId} joined group: {groupName}");
            }
        }

        /// <summary>
        /// مغادرة مستخدم لمجموعة
        /// </summary>
        /// <param name="groupName">اسم المجموعة</param>
        public async Task LeaveGroup(string groupName)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                
                // إزالة المستخدم من قاموس المجموعات
                if (UserGroups.TryGetValue(groupName, out var group))
                {
                    group.Remove(userId);
                }
                
                Console.WriteLine($"User {userId} left group: {groupName}");
            }
        }

        /// <summary>
        /// إرسال إشعار لمستخدم محدد
        /// </summary>
        /// <param name="targetUserId">معرف المستخدم المستهدف</param>
        /// <param name="notification">بيانات الإشعار</param>
        public async Task SendNotificationToUser(string targetUserId, object notification)
        {
            // البحث عن connection ID للمستخدم المستهدف
            var targetConnectionId = ConnectedUsers.FirstOrDefault(x => x.Value == targetUserId).Key;
            
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveNotification", notification);
                Console.WriteLine($"Notification sent to user {targetUserId}");
            }
            else
            {
                Console.WriteLine($"User {targetUserId} is not connected");
            }
        }

        /// <summary>
        /// إرسال إشعار لمجموعة من المستخدمين
        /// </summary>
        /// <param name="groupName">اسم المجموعة</param>
        /// <param name="notification">بيانات الإشعار</param>
        public async Task SendNotificationToGroup(string groupName, object notification)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Notification sent to group: {groupName}");
        }

        /// <summary>
        /// إرسال إشعار لجميع المستخدمين المتصلين
        /// </summary>
        /// <param name="notification">بيانات الإشعار</param>
        public async Task SendNotificationToAll(object notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
            Console.WriteLine("Notification sent to all connected users");
        }

        /// <summary>
        /// الحصول على قائمة المستخدمين المتصلين حالياً
        /// </summary>
        /// <returns>قائمة بمعرفات المستخدمين المتصلين</returns>
        public Task<List<string>> GetConnectedUsers()
        {
            return Task.FromResult(ConnectedUsers.Values.Distinct().ToList());
        }

        /// <summary>
        /// الحصول على قائمة المجموعات المتاحة
        /// </summary>
        /// <returns>قائمة بأسماء المجموعات</returns>
        public Task<List<string>> GetAvailableGroups()
        {
            return Task.FromResult(UserGroups.Keys.ToList());
        }

        /// <summary>
        /// الحصول على معرف المستخدم من Context
        /// يمكن تخصيص هذا حسب نظام المصادقة المستخدم
        /// </summary>
        /// <returns>معرف المستخدم</returns>
        private string GetUserId()
        {
            // يمكن الحصول على معرف المستخدم من Claims أو Headers
            // هذا مثال بسيط - يجب تخصيصه حسب نظام المصادقة
            return Context.User?.Identity?.Name ?? Context.GetHttpContext()?.Request.Headers["User-Id"].FirstOrDefault() ?? "Anonymous";
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using SignalR.API.Hubs;
using System.Collections.Concurrent;

namespace SignalR.API.Middleware
{
    /// <summary>
    /// Middleware لتتبع المستخدمين المتصلين وإدارة الجلسات
    /// </summary>
    public class UserTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserTrackingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, UserSession> _activeSessions = new();

        public UserTrackingMiddleware(RequestDelegate next, ILogger<UserTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // استخراج معرف المستخدم من Header أو Claims
            var userId = ExtractUserId(context);
            
            if (!string.IsNullOrEmpty(userId))
            {
                // تحديث معلومات الجلسة
                UpdateUserSession(userId, context);
                
                // إضافة معرف المستخدم إلى Header للاستخدام في SignalR
                context.Request.Headers["User-Id"] = userId;
            }

            await _next(context);
        }

        /// <summary>
        /// استخراج معرف المستخدم من HttpContext
        /// يمكن تخصيص هذا حسب نظام المصادقة المستخدم
        /// </summary>
        private string? ExtractUserId(HttpContext context)
        {
            // الطريقة 1: من Header
            if (context.Request.Headers.TryGetValue("User-Id", out var userIdHeader))
            {
                return userIdHeader.FirstOrDefault();
            }

            // الطريقة 2: من Claims (إذا كان هناك نظام مصادقة)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("sub") ?? 
                                context.User.FindFirst("user_id") ?? 
                                context.User.FindFirst("id");
                
                if (userIdClaim != null)
                {
                    return userIdClaim.Value;
                }
            }

            // الطريقة 3: من Query String (للاستخدام في التطوير)
            if (context.Request.Query.TryGetValue("userId", out var userIdQuery))
            {
                return userIdQuery.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// تحديث معلومات جلسة المستخدم
        /// </summary>
        private void UpdateUserSession(string userId, HttpContext context)
        {
            var session = _activeSessions.AddOrUpdate(userId,
                new UserSession
                {
                    UserId = userId,
                    LastActivity = DateTime.UtcNow,
                    IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                    ConnectionId = context.Request.Headers["Connection-Id"].FirstOrDefault()
                },
                (key, existingSession) =>
                {
                    existingSession.LastActivity = DateTime.UtcNow;
                    existingSession.IPAddress = context.Connection.RemoteIpAddress?.ToString();
                    existingSession.UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
                    return existingSession;
                });

            _logger.LogDebug($"Updated session for user {userId}");
        }

        /// <summary>
        /// الحصول على الجلسات النشطة
        /// </summary>
        public static List<UserSession> GetActiveSessions()
        {
            return _activeSessions.Values.ToList();
        }

        /// <summary>
        /// الحصول على جلسة مستخدم محدد
        /// </summary>
        public static UserSession? GetUserSession(string userId)
        {
            _activeSessions.TryGetValue(userId, out var session);
            return session;
        }

        /// <summary>
        /// إزالة جلسة مستخدم
        /// </summary>
        public static bool RemoveUserSession(string userId)
        {
            return _activeSessions.TryRemove(userId, out _);
        }

        /// <summary>
        /// تنظيف الجلسات المنتهية الصلاحية
        /// </summary>
        public static void CleanupExpiredSessions(TimeSpan timeout)
        {
            var expiredSessions = _activeSessions
                .Where(kvp => DateTime.UtcNow - kvp.Value.LastActivity > timeout)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var userId in expiredSessions)
            {
                _activeSessions.TryRemove(userId, out _);
            }
        }
    }

    /// <summary>
    /// نموذج جلسة المستخدم
    /// </summary>
    public class UserSession
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ConnectionId { get; set; }
        public bool IsConnected { get; set; }
    }

    /// <summary>
    /// Extension methods لتسجيل Middleware
    /// </summary>
    public static class UserTrackingMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserTrackingMiddleware>();
        }
    }
}

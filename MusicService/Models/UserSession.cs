namespace MusicService.Models
{
    public static class UserSession
    {
        private static IHttpContextAccessor _accessor = new HttpContextAccessor();
        public static int? CurrentUserId
        {
            get => _accessor.HttpContext?.Session.GetInt32("UserId");
            set { if (value.HasValue) _accessor.HttpContext?.Session.SetInt32("UserId", value.Value); }
        }

        public static string? UserName
        {
            get => _accessor.HttpContext?.Session.GetString("UserName");
            set { if (value != null) _accessor.HttpContext?.Session.SetString("UserName", value); }
        }

        public static string? Role
        {
            get => _accessor.HttpContext?.Session.GetString("Role");
            set { if (value != null) _accessor.HttpContext?.Session.SetString("Role", value); }
        }

        public static void Logout()
        {
            _accessor.HttpContext?.Session.Clear();
        }
    }
}
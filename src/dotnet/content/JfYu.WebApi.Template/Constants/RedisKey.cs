namespace JfYu.WebApi.Template.Constants
{
    public static class RedisKey
    {
        public const string ApplicationName = "JfYu.WebApi.Template";
        public const string UserPermission = ApplicationName + ":USER_PERMISSION_{0}";
        //#if (EnableJWTRedis)
        public const string UserBlacklist = ApplicationName + ":USER_BLACKLIST_{0}";
        //#endif
    }
}

using WebApi.Options;

namespace WebApi.Extensions
{
    public static class OptionsExtension
    {
        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            //#if (EnableJWT)
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            //#endif           
            return services;
        }
    }
}

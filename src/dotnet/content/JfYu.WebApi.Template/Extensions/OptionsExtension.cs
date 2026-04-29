//#if (EnableJWT)
using JfYu.WebApi.Template.Options;
//#endif

namespace JfYu.WebApi.Template.Extensions
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

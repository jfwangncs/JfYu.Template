using WebApi.Extensions;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
//#if (EnableTelemetry)
using OpenTelemetry.Logs;
//#endif 
using Scalar.AspNetCore;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Info("Application Start");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddNLog();
    //#if (EnableTelemetry)
    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });
    //#endif

    builder.Services.AddControllers();
    builder.Services.AddCustomCoreAPI()
        .AddCustomCors()
        .AddCustomScalar()
        .AddCustomApiVersioning()
        .AddCustomFluentValidation()
        .AddMapster()
        //#if (EnableTelemetry)
        .AddCustomOpenTelemetry()
        //#endif
        .AddCustomNLog()
        .AddCustomOptions(builder.Configuration)
        //#if (EnableJWT)
        .AddCustomAuthentication(builder.Configuration)
        //#endif 
        .AddCustomInjection(builder.Configuration);

    var app = builder.Build();

    app.UseCors("AllowAll");

    app.UseHttpLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    //#if (EnableJWT)
    app.UseAuthentication();
    //#endif
    app.UseAuthorization();

    app.UseCustomExceptionHandler();

    app.UsePermissionSync();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application start failed");
    throw;
}
finally
{
    LogManager.Shutdown();
}

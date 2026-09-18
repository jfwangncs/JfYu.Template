using JfYu.WebApi.Template.Extensions;
//#if (EnableJWTRedis)
using JfYu.WebApi.Template.Infrastructure;
//#endif
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Model;
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
    builder.Services.AddHealthChecks()
        //#if (EnableRBAC)
        .AddCheck<JfYu.WebApi.Template.Infrastructure.DbHealthCheck>("database")
        //#endif
        ;
    builder.Services.AddCustomCoreAPI()
        .AddCustomLocalization()
        .AddCustomCors()
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

    app.UseRequestLocalization();

    app.UseHttpLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().WithDocumentPerVersion();
        app.MapScalarApiReference(options =>
        {
            var descriptions = app.DescribeApiVersions();
            for (var i = 0; i < descriptions.Count; i++)
            {
                var description = descriptions[i];
                var isDefault = i == 0;
                options.AddDocument(description.GroupName, description.GroupName, isDefault: isDefault);
            }
        });
    }
    //#if (EnableJWT)
    app.UseAuthentication();
    //#endif
    //#if (EnableJWTRedis)
    app.UseBlacklistMiddleware();
    //#endif
    app.UseAuthorization();

    app.UseCustomExceptionHandler();

    //#if (EnableRBAC)
    app.UsePermissionSync();
    //#endif

    //#if (EnableTelemetry)
    app.UseOpenTelemetryPrometheusScrapingEndpoint();
    //#endif

    app.MapHealthChecks("/api/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new BaseResponse<object>
            {
                Data = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration
                    }),
                    totalDuration = report.TotalDuration
                }
            };

            if (report.Status != HealthStatus.Healthy)
            {
                response.Code = ResponseCode.Failed;
                response.Message = ResponseCode.Failed.GetDescription();
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    });

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "Application start failed");
    LogManager.Shutdown();
    Environment.Exit(1);
}

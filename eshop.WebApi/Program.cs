using eshop.Application;
using eshop.Application.Order;
using eshop.Core.Cache;
using eshop.WebApi.Filters;
using eshop.WebApi.Middlewares;
using FluentResults;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Prometheus;
using Prometheus.DotNetRuntime;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .Enrich.WithProperty("ApplicationName", "eshop")
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File(new CompactJsonFormatter(), "logs.txt")
    .CreateLogger();

builder.Services.AddSerilog(logger);
Log.Logger = logger;

builder.Services.RegisterApplicationDependencies(builder.Configuration);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login.html");

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<IncomingRequestFilter>();
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var modelErrors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(x => x.Exception?.Message ?? x.ErrorMessage)
                .ToArray();

            var result = Result.Fail(modelErrors);
            
            return new BadRequestObjectResult(result.ToString());
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCaching();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<CacheKeysStorage>();

builder.Services.AddHostedService<ClearBasketsBackgroundService>();

var app = builder.Build();

DotNetRuntimeStatsBuilder.Default().StartCollecting();

app.UseAuthentication();
app.UseMiddleware<LogUserMiddleware>();

app.UseSerilogRequestLogging(ops =>
{
    ops.Logger = logger;
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMetricServer();
app.UseResponseCaching();
app.UseAuthorization();

app.MapControllers();

app.Run();

using eshop.Application;
using eshop.WebApi;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterApplicationDependencies(builder.Configuration);
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
            string[] modelErrors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(x => x.Exception?.Message ?? x.ErrorMessage)
                .ToArray();

            var result = Result.Fail(modelErrors);
            
            return new BadRequestObjectResult(result.ToString());
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("apikey", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "X-Api-Key",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "apikey"
                }
            }, Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
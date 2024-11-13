using eshop.Application;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterApplicationDependencies(builder.Configuration);
builder.Services.AddControllers()
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
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
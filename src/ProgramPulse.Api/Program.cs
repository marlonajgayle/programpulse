using ProgramPulse.Api.Infrastructure.ExceptionHandling;
using ProgramPulse.Api.Infrastructure.HealthChecks;
using ProgramPulse.Api.Infrastructure.Logging;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.Infrastructure.RateLimiting;
using ProgramPulse.Api.Infrastructure.SecurityHeaders;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.OpenApi;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddApiVersioningWithOpenApi();
builder.Services.AddGlobalExceptionHandling();
builder.Services.AddValidation();
builder.Services.AddEndpoints();
builder.Services.AddHealthCheckConfiguration(builder.Configuration);
builder.Services.AddRateLimitingConfiguration(builder.Configuration);
builder.Services.AddSecurityHeadersConfiguration(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.UseInitializeDatabaseAsync();

app.UseGlobalExceptionHandling();

app.UseSecurityHeadersConfiguration(builder.Configuration);

app.UseRequestPerformanceLogging();
app.UseRateLimitingConfiguration(builder.Configuration);
app.UseHealthCheckConfiguration(builder.Configuration);

// Configure the HTTP request pipeline.
app.MapApiDocumentation(app.Environment);

app.UseHttpsRedirection();

app.MapApiEndpoints();

app.Run();

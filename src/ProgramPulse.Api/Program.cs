using ProgramPulse.Api.Infrastructure.Logging;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.OpenApi;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddApiVersioningWithOpenApi();
builder.Services.AddValidation();
builder.Services.AddEndpoints();

var app = builder.Build();

app.UseRequestPerformanceLogging();

// Configure the HTTP request pipeline.
app.MapApiDocumentation(app.Environment);

app.UseHttpsRedirection();

app.MapApiEndpoints();

app.Run();

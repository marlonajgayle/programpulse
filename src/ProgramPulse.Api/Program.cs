using ProgramPulse.Api.Features.Authentication.ForgotPassword;
using ProgramPulse.Api.Features.Authentication.Login;
using ProgramPulse.Api.Features.Authentication.Logout;
using ProgramPulse.Api.Features.Authentication.ResetPassword;
using ProgramPulse.Api.Features.Faqs.Create;
using ProgramPulse.Api.Features.Faqs.Delete;
using ProgramPulse.Api.Features.Faqs.GetAll;
using ProgramPulse.Api.Features.Faqs.GetById;
using ProgramPulse.Api.Features.Faqs.Update;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.Infrastructure.ExceptionHandling;
using ProgramPulse.Api.Infrastructure.HealthChecks;
using ProgramPulse.Api.Infrastructure.Logging;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
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
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<LogoutCommandHandler>();
builder.Services.AddScoped<ForgotPasswordCommandHandler>();
builder.Services.AddScoped<ResetPasswordCommandHandler>();
builder.Services.AddScoped<CreateFaqCommandHandler>();
builder.Services.AddScoped<UpdateFaqCommandHandler>();
builder.Services.AddScoped<DeleteFaqCommandHandler>();
builder.Services.AddScoped<GetFaqByIdQueryHandler>();
builder.Services.AddScoped<GetAllFaqsQueryHandler>();
builder.Services.AddHealthCheckConfiguration(builder.Configuration);
builder.Services.AddRateLimitingConfiguration(builder.Configuration);
builder.Services.AddSecurityHeadersConfiguration(builder.Configuration);
builder.Services.AddCurrentUserService();
builder.Services.AddPersistence(builder.Configuration, builder.Environment);
builder.Services.AddIdentityServices(builder.Configuration, builder.Environment);
builder.Services.AddOutboxMessaging();
builder.Services.AddEmailConfiguration(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();

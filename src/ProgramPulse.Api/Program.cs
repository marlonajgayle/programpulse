using ProgramPulse.Api.Features.Authentication.ChangePassword;
using ProgramPulse.Api.Features.Authentication.ConfirmEmail;
using ProgramPulse.Api.Features.Authentication.CurrentUser;
using ProgramPulse.Api.Features.Authentication.ForgotPassword;
using ProgramPulse.Api.Features.Authentication.Login;
using ProgramPulse.Api.Features.Authentication.Logout;
using ProgramPulse.Api.Features.Authentication.Register;
using ProgramPulse.Api.Features.Authentication.ResetPassword;
using ProgramPulse.Api.Features.Dashboard.GetSummary;
using ProgramPulse.Api.Features.Faqs.Create;
using ProgramPulse.Api.Features.Faqs.Delete;
using ProgramPulse.Api.Features.Faqs.GetAll;
using ProgramPulse.Api.Features.Faqs.GetById;
using ProgramPulse.Api.Features.Faqs.Update;
using ProgramPulse.Api.Features.Programmes.Create;
using ProgramPulse.Api.Features.Programmes.Delete;
using ProgramPulse.Api.Features.Programmes.GetById;
using ProgramPulse.Api.Features.Programmes.List;
using ProgramPulse.Api.Features.Programmes.Update;
using ProgramPulse.Api.Features.Objectives.Create;
using ProgramPulse.Api.Features.Objectives.Delete;
using ProgramPulse.Api.Features.Objectives.List;
using ProgramPulse.Api.Features.Objectives.Update;
using ProgramPulse.Api.Features.Kpis.Create;
using ProgramPulse.Api.Features.Kpis.Delete;
using ProgramPulse.Api.Features.Kpis.List;
using ProgramPulse.Api.Features.Kpis.Update;
using ProgramPulse.Api.Features.Measurements.Create;
using ProgramPulse.Api.Features.Measurements.Delete;
using ProgramPulse.Api.Features.Measurements.List;
using ProgramPulse.Api.Features.Measurements.Update;
using ProgramPulse.Api.Features.MeasurementComments.Create;
using ProgramPulse.Api.Features.MeasurementComments.Delete;
using ProgramPulse.Api.Features.MeasurementComments.List;
using ProgramPulse.Api.Features.MeasurementComments.Update;
using ProgramPulse.Api.Features.Users.AddUser;
using ProgramPulse.Api.Features.Users.ListUsers;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Configuration;
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
builder.Services.AddScoped<ChangePasswordCommandHandler>();
builder.Services.AddScoped<ConfirmEmailCommandHandler>();
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<GetCurrentUserQueryHandler>();
builder.Services.AddScoped<AddUserCommandHandler>();
builder.Services.AddScoped<ListUsersQueryHandler>();
builder.Services.AddScoped<CreateFaqCommandHandler>();
builder.Services.AddScoped<UpdateFaqCommandHandler>();
builder.Services.AddScoped<DeleteFaqCommandHandler>();
builder.Services.AddScoped<GetFaqByIdQueryHandler>();
builder.Services.AddScoped<GetAllFaqsQueryHandler>();
builder.Services.AddScoped<CreateProgrammeCommandHandler>();
builder.Services.AddScoped<UpdateProgrammeCommandHandler>();
builder.Services.AddScoped<DeleteProgrammeCommandHandler>();
builder.Services.AddScoped<GetProgrammesQueryHandler>();
builder.Services.AddScoped<GetProgrammeByIdQueryHandler>();
builder.Services.AddScoped<CreateObjectiveCommandHandler>();
builder.Services.AddScoped<UpdateObjectiveCommandHandler>();
builder.Services.AddScoped<DeleteObjectiveCommandHandler>();
builder.Services.AddScoped<GetObjectivesQueryHandler>();
builder.Services.AddScoped<CreateKpiCommandHandler>();
builder.Services.AddScoped<UpdateKpiCommandHandler>();
builder.Services.AddScoped<DeleteKpiCommandHandler>();
builder.Services.AddScoped<GetObjectiveKpisQueryHandler>();
builder.Services.AddScoped<CreateMeasurementCommandHandler>();
builder.Services.AddScoped<UpdateMeasurementCommandHandler>();
builder.Services.AddScoped<DeleteMeasurementCommandHandler>();
builder.Services.AddScoped<GetMeasurementsQueryHandler>();
builder.Services.AddScoped<CreateMeasurementCommentCommandHandler>();
builder.Services.AddScoped<UpdateMeasurementCommentCommandHandler>();
builder.Services.AddScoped<DeleteMeasurementCommentCommandHandler>();
builder.Services.AddScoped<GetMeasurementCommentsQueryHandler>();
builder.Services.AddScoped<GetDashboardSummaryQueryHandler>();
builder.Services.AddHealthCheckConfiguration(builder.Configuration);
builder.Services.AddRateLimitingConfiguration(builder.Configuration);
builder.Services.AddSecurityHeadersConfiguration(builder.Configuration);
builder.Services.AddCurrentUserService();
builder.Services.AddPersistence(builder.Configuration, builder.Environment);
builder.Services.AddIdentityServices(builder.Configuration, builder.Environment);
builder.Services.AddOutboxMessaging();
builder.Services.AddEmailConfiguration(builder.Configuration);

builder.Services.Configure<FrontendOption>(
    builder.Configuration.GetSection(FrontendOption.SectionName));

const string WebCorsPolicy = "WebClient";
builder.Services.AddCors(options => options.AddPolicy(WebCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["https://localhost:7208", "http://localhost:5031"])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

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

app.UseCors(WebCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();

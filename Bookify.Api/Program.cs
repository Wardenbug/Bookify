using Bookify.Infrastructure;
using Bookify.Application;
using Bookify.Api.Extensions;
using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Bookify.Application.Abstractions.Data;
using Dapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Bookify.Api.Controllers.Bookings;
using Asp.Versioning.Builder;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.ApplyMigrations();
    // app.SeedData();
}

app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseCustomExtensionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
           .HasApiVersion(new ApiVersion(1))
           .ReportApiVersions()
           .Build();

var routeGroupBuilder = app.MapGroup("api/v{version:apiVersion}").
    WithApiVersionSet(apiVersionSet);

routeGroupBuilder.MapBookingEndpoints();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
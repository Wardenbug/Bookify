using Bookify.Infrastructure;
using Bookify.Application;
using Bookify.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);


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

app.UseCustomExtensionHandler();

app.MapControllers();

app.Run();

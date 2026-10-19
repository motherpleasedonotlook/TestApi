using Business;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Web;
using Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TaskItemDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddOptions<RabbitMqConfiguration>()
    .Bind(builder.Configuration.GetSection(RabbitMqConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value);

builder.Services.AddRepositories();
builder.Services.AddServices();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCustomExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskItemDbContext>();
    dbContext.Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapEndpoints();

app.Run();
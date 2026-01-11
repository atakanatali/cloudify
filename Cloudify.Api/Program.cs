using Cloudify.Application.Ports;
using Cloudify.Application.Services;
using Cloudify.Infrastructure.Options;
using Cloudify.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<EnvironmentStoreOptions>()
    .BindConfiguration(EnvironmentStoreOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddCloudifyPersistence("./data/cloudify.db");
builder.Services.AddSingleton<IEnvironmentRepository, InMemoryEnvironmentRepository>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<CloudifyDatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

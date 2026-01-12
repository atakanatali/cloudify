using Cloudify.Application.Ports;
using Cloudify.Application.Services;
using Cloudify.Infrastructure.Options;
using Cloudify.Infrastructure.Orchestration;
using Cloudify.Infrastructure.Ports;
using Cloudify.Infrastructure.Persistence;
using Cloudify.Infrastructure.Processes;
using Cloudify.Infrastructure.SystemProfiles;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<EnvironmentStoreOptions>()
    .BindConfiguration(EnvironmentStoreOptions.SectionName)
    .ValidateDataAnnotations();
builder.Services.AddOptions<DockerComposeOptions>()
    .BindConfiguration(DockerComposeOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddCloudifyPersistence("./data/cloudify.db");
builder.Services.AddSingleton<IEnvironmentRepository, InMemoryEnvironmentRepository>();
builder.Services.AddScoped<IAddResourceHandler, AddResourceHandler>();
builder.Services.AddScoped<ICreateEnvironmentHandler, CreateEnvironmentHandler>();
builder.Services.AddScoped<ICreateResourceGroupHandler, CreateResourceGroupHandler>();
builder.Services.AddScoped<IDeleteResourceHandler, DeleteResourceHandler>();
builder.Services.AddScoped<IGetEnvironmentOverviewHandler, GetEnvironmentOverviewHandler>();
builder.Services.AddScoped<IGetResourceHealthHandler, GetResourceHealthHandler>();
builder.Services.AddScoped<IGetResourceLogsHandler, GetResourceLogsHandler>();
builder.Services.AddScoped<IListEnvironmentsHandler, ListEnvironmentsHandler>();
builder.Services.AddScoped<IListResourceGroupsHandler, ListResourceGroupsHandler>();
builder.Services.AddScoped<IRestartResourceHandler, RestartResourceHandler>();
builder.Services.AddScoped<IScaleResourceHandler, ScaleResourceHandler>();
builder.Services.AddScoped<IStartResourceHandler, StartResourceHandler>();
builder.Services.AddScoped<IStopResourceHandler, StopResourceHandler>();
builder.Services.AddSingleton<ProcessRunner>();
builder.Services.AddScoped<IOrchestrator, DockerComposeOrchestrator>();
builder.Services.AddScoped<ITemplateRenderer, DockerComposeTemplateRenderer>();
builder.Services.AddScoped<IPortAllocator, PortAllocator>();
builder.Services.AddSingleton<ISystemProfileProvider, HostSystemProfileProvider>();

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

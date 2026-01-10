using Cloudify.Application.Ports;
using Cloudify.Application.Services;
using Cloudify.Infrastructure.Options;
using Cloudify.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<EnvironmentStoreOptions>()
    .BindConfiguration(EnvironmentStoreOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddSingleton<IEnvironmentRepository, InMemoryEnvironmentRepository>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

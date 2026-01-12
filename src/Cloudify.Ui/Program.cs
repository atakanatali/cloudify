using Cloudify.Ui.Options;
using Cloudify.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddOptions<ApiClientOptions>()
    .BindConfiguration(ApiClientOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddHttpClient("CloudifyApi", (serviceProvider, client) =>
{
    ApiClientOptions options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
});
builder.Services.AddScoped<CloudifyApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

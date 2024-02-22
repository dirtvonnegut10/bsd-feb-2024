using IssueTrackerApi;
using IssueTrackerApi.Services;
using Marten;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(b => b.AddService("issues-api"))
    .WithTracing(b =>
    {
        b.AddAspNetCoreInstrumentation();
        b.AddHttpClientInstrumentation();
        b.AddZipkinExporter();
        b.AddHttpClientInstrumentation();
        b.AddConsoleExporter();
        b.AddNpgsql();
        b.SetSampler(new AlwaysOnSampler());
    })
    .WithMetrics(opts =>
    {
        opts.AddPrometheusExporter();
        opts.AddHttpClientInstrumentation();
        opts.AddRuntimeInstrumentation();

        opts.AddAspNetCoreInstrumentation();
    });

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("issues")
        ?? throw new Exception("No Connection String for Issues");

var apiUrl = builder.Configuration.GetValue<string>("api")
        ?? throw new Exception("No Api Url");

//builder.Services.AddHttpClient(); // Global HTTP Client - used for every request made from this API

// "Named Client" - never have used this.
//builder.Services.AddHttpClient("google");

// This is a client that is ONLY for the url (apiUrl) This is called a "Typed Client"
builder.Services.AddHttpClient<BusinessClockHttpService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
})
    .AddPolicyHandler(BasicSrePolicies.GetDefaultRetryPolicy())
    .AddPolicyHandler(BasicSrePolicies.GetDefaultCircuitBreaker());

builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);

}).UseLightweightSessions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.UseHealthChecks("/healthz");
app.MapPrometheusScrapingEndpoint();
app.Run();

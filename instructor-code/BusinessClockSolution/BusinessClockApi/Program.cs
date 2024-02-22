using BusinessClockApi.Services;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args); // Hey, Microsoft, create me a 

builder.Services.AddOpenTelemetry()
    .ConfigureResource(b => b.AddService("business-clock-api"))
    .WithTracing(b =>
    {
        b.AddAspNetCoreInstrumentation();
        b.AddHttpClientInstrumentation();
        b.AddZipkinExporter();
        b.AddHttpClientInstrumentation();
        b.AddConsoleExporter();
        b.SetSampler(new AlwaysOnSampler());
    })
    .WithMetrics(opts =>
    {
        opts.AddPrometheusExporter();
        opts.AddHttpClientInstrumentation();
        opts.AddRuntimeInstrumentation();
        opts.AddAspNetCoreInstrumentation();
    });
// api with the basic configuration you think would be a good starting place.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Lazy Initialization
//builder.Services.AddSingleton<IProvideTheBusinessClock, AdvancedBusinessClock>();
// If you need to initialize this, but still provide it in a "lazy" way.
//builder.Services.AddSingleton<IProvideTheBusinessClock>(sp =>
//{
//    Console.WriteLine("Setting up the business clock");
//    Thread.Sleep(3000);
//    // do all the work to create this thing.
//    var clock = sp.GetRequiredService<ISystemTime>();
//    Console.WriteLine("Setting up the business clock is done");

//    return new AdvancedBusinessClock(clock);
//});

Console.WriteLine("Setting up the systemtime");

Thread.Sleep(3000);
var realClock = new SystemTime();
Console.WriteLine("Created the Clock");

builder.Services.AddSingleton<ISystemTime>(sp => realClock);
builder.Services.AddScoped<IProvideTheBusinessClock, AdvancedBusinessClock>();

// Above this line is "internal" configuration stuff.
var app = builder.Build();
// after this line is configuration for the "middleware" 
//   - how actual HTTP requests are handled and responses are sent.
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/support-info", ([FromServices] IProvideTheBusinessClock clock) =>
{

    if (clock.IsOpen())
    {
        return new SupportInfoResponse("Diane", "555-1212");
    }
    else
    {
        return new SupportInfoResponse("TechSupportPros", "800-STUF-BROKE");
    }
});

app.MapPrometheusScrapingEndpoint();
app.Run();


public record SupportInfoResponse(string Name, string Phone);

public interface IProvideTheBusinessClock
{
    bool IsOpen();
}
public partial class Program { }
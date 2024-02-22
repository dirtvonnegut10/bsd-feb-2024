using IssueTrackerApi;
using IssueTrackerApi.Services;
using Marten;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("issues") ?? throw new Exception("no connection string for issues");

var apiUrl = builder.Configuration.GetValue<string>("api") ?? throw new Exception("no api url");

//builder.Services.AddHttpClient();
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

app.Run();

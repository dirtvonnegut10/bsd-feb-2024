using BusinessClockApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IProvideTheBusinessClock, StandardBusinessClock>();
builder.Services.AddSingleton<ISystemTime, SystemTime>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// GET /
app.MapGet("/support-info", () =>
{
    return new SupportInfoResponse("Graham", "123-1234");
});

app.Run();


public record SupportInfoResponse(string Name, String Phone);

public interface IProvideTheBusinessClock
{
    bool IsOpen();
}
public partial class Program { }
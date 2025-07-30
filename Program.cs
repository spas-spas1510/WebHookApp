using WebHookApp.Logic;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddScoped<IPostionModifier, PositionModifier>();
builder.Services.AddScoped<IWebHookLogic, WebHookLogic>();
builder.Services.AddScoped<ILoginLogic, LoginLogic>();
builder.Services.AddScoped<IWebSocketListener, WebSocketListener>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

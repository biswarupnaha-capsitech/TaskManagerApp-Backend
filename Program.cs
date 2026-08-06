using Microsoft.AspNetCore.Hosting;
using TaskManager;

var builder = WebApplication.CreateBuilder(args);

// Delegate all logic to Startup
var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, builder.Environment);

app.Run();

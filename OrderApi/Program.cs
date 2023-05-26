using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monitoring;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using Shared;


var builder = WebApplication.CreateBuilder(args);

// Telemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("OrderApi")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(MonitorService.ServiceName))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter()
            .AddZipkinExporter(config =>
            {
                config.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
            }));

//register services for dependency injection

builder.Services.AddDbContext<OrderApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddSingleton<IConverter<Order, OrderDto>, OrderConverter>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();
// Register MessagePublisher (a messaging gateway) for dependency injection
builder.Services.AddSingleton<IMessagePublisher>(new
    MessagePublisher());

builder.Services.AddControllers().AddDapr();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();

// Configure the HTTP request pipeline.

// Initialize the database.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<OrderApiContext>();
    var dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
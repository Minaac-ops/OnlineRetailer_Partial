using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Base URL for the product service when the solution is executed using docker-compose.
// The product service (running as a container) listens on this URL for HTTP requests
// from other services specified in the docker compose file (which in this solution is
// the order service).

// RabbitMQ connection string (I use CloudAMQP as a RabbitMQ server).
// Remember to replace this connectionstring with your own.
string cloudAMQPConnectionString =
    "";

// Use this connection string if you want to run RabbitMQ server as a container
// (see docker-compose.yml)
//string cloudAMQPConnectionString = "host=rabbitmq";

//register services for dependency injection

builder.Services.AddDbContext<OrderApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddSingleton<IConverter<Order, OrderDto>, OrderConverter>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

// Register MessagePublisher (a messaging gateway) for dependency injection
builder.Services.AddSingleton<IMessagePublisher>(new
    MessagePublisher(cloudAMQPConnectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

Task.Factory.StartNew(() => new MessageListener(app.Services, cloudAMQPConnectionString).Start());

app.UseAuthorization();

app.MapControllers();

app.Run();

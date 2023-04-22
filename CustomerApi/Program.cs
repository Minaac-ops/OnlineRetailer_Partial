using System;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Infrastructure;
using CustomerApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Monitoring;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared;

var builder = WebApplication.CreateBuilder(args);
string cloudAMQPConnectionString =
    "host=sparrow-01.rmq.cloudamqp.com;virtualHost=dcsrkben;username=dcsrkben;password=btHFI057Mxuj4edjwE9aaG0DPatBSShP";
// Add services to the container.

builder.Services.AddDbContext<CustomerApiContext>(opt => opt.UseInMemoryDatabase("CustomersDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Customer>, CustomerRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConverter<Customer, CustomerDto>, CustomerConverter>();
var app = builder.Build();

// Configure the HTTP request pipeline.

// Initialize the database.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<CustomerApiContext>();
    var dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

//app.UseHttpsRedirection();

Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAMQPConnectionString).Start());


app.UseAuthorization();

app.MapControllers();

app.Run();
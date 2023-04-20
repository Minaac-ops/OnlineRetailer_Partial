using System.Threading.Tasks;
using EmailService.Infrastructure;
using EmailService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

var builder = WebApplication.CreateBuilder(args);


string cloudAMQPConnectionString =
    "";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var emailConfig = builder.Configuration
    .GetSection("MailConfig")
    .Get<EmailConfig>();

builder.Services.AddSingleton<EmailConfig>(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAMQPConnectionString).Start());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
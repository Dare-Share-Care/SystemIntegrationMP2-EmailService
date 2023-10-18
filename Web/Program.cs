using Web.Services;
using Microsoft.Extensions.Configuration;

var policyName = "AllowOrigin";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
        builder =>
        {
            builder
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

//Services
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(policyName);
app.UseAuthorization();

app.MapControllers();

app.Run();
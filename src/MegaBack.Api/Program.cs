// When decorated on an assembly, all controllers in the assembly will be treated
// as controllers with API behavior. 
//[assembly: Microsoft.AspNetCore.Mvc.ApiController]

using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
                .AddJsonOptions( o => 
                    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var logger = app.Logger;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogError("app.Environment.IsDevelopment() == true");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    logger.LogInformation("app.Environment.IsDevelopment() == false");
    
//TODO:
    logger.LogError("TODO: disable swagger for prod");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }

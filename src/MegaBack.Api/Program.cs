using Microsoft.OpenApi.Models;

// When decorated on an assembly, all controllers in the assembly will be treated
// as controllers with API behavior. 
[assembly: Microsoft.AspNetCore.Mvc.ApiController]



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddProblemDetails();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); //c =>
// {
//     c.SwaggerDoc("v0.1", new OpenApiInfo { Title = "My API - V0.1", Version = "v0.1" });
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1" });
//     c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API - V2", Version = "v2" });
// });
// builder.Services.AddApiVersioning();
//                     options =>
//                     {
//                         // reporting api versions will return the headers
//                         // "api-supported-versions" and "api-deprecated-versions"
//                         options.ReportApiVersions = true;
//                     });


var app = builder.Build();
ILogger logger = app.Logger;

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
    
    logger.LogError("disable swagger for prod");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();




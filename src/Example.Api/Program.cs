// When decorated on an assembly, all controllers in the assembly will be treated
// as controllers with API behavior. 
//[assembly: Microsoft.AspNetCore.Mvc.ApiController]

using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddControllers()
                .AddJsonOptions( o => 
                {
                    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.SnakeCaseLower));
                });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( o =>
{
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();
var logger = app.Logger;



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogWarning("app.Environment.IsDevelopment() == true");
    app.UseSwagger();
    app.UseSwaggerUI( o => 
        o.EnableDeepLinking()
    );
}
else
{
    logger.LogInformation("app.Environment.IsDevelopment() == false");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();


public partial class Program { }

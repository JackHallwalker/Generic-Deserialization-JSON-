using Generic_Deserialization_JSON.Core.Interfaces;
using Generic_Deserialization_JSON.Infrastructure.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add your services here
builder.Services.AddScoped(typeof(IObjectMapper<>), typeof(Generic_Deserialization_JSON.Infastructure.Mappers.XmlToObjectMapper<>));
builder.Services.AddScoped(typeof(JsonToObjectMapper<>), typeof(JsonToObjectMapper<>));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

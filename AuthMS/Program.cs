using Application.Interfaces.IServices;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Application.Interfaces.ICommand;
using Infrastructure.Command;
using Application.Interfaces.IQuery;
using Infrastructure.Query;
using Application.Interfaces.IServices.IReservationServices;
using Application.UseCase.ReservationServices;
using Application.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;
using Application.Interfaces.IServices.IVehicleServices;
using Infrastructure.HttpClients;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ReservationMS", Version = "1.0" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});


// Custom            

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

//Services
builder.Services.AddScoped<IReservationPostService, ReservationPostService>();
builder.Services.AddScoped<IReservationGetService, ReservationGetService>();
builder.Services.AddScoped<IReservationAvailabilityService, ReservationAvailabilityService>();

builder.Services.AddLogging();

builder.Services.AddHttpClient<IVehicleService, VehicleServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["VehicleService:BaseUrl"]);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});


//Command
builder.Services.AddScoped<IReservationCommand, ReservationCommand>();
builder.Services.AddScoped<IReservationEventCommand, ReservationEventCommand>();

//Query
builder.Services.AddScoped<IReservationQuery, ReservationQuery>();


//Validators
builder.Services.AddValidatorsFromAssembly(typeof(ReservationRequestValidator).Assembly);
builder.Services.AddFluentValidationAutoValidation();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();


app.Use(async (context, next) =>
{
    // Continúa con la solicitud
    await next();

    // Si el estado de la respuesta es 401 (No autorizado), añade los encabezados CORS
    if (context.Response.StatusCode == 401)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Content-Type");

    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();

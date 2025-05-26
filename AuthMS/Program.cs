using Application.Interfaces.IServices;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
using Application.Interfaces.IValidator;
using Application.Dtos.Request;
using System.Text;
using Infrastructure.Service;
using AuthMS.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using AuthMS.Handlers;
using System.Net.Http.Headers;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new DateTimeWithoutFractionConverter());
    });

//Services
builder.Services.AddScoped<IReservationPostService, ReservationPostService>();
builder.Services.AddScoped<IReservationGetService, ReservationGetService>();
builder.Services.AddScoped<IReservationPutService, ReservationPutService>();
builder.Services.AddScoped<IReservationAvailabilityService, ReservationAvailabilityService>();
builder.Services.AddSingleton<ITimeProvider, ArgentinaTimeProvider>();

builder.Services.AddLogging();

builder.Services.AddHttpClient<IVehicleService, VehicleServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["VehicleService:BaseUrl"]);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<INotificationService, NotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["NotificationService:BaseUrl"]!);
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
builder.Services.AddValidatorsFromAssemblyContaining<GetAvailableVehiclesRequestValidator>();
builder.Services.AddScoped<IValidatorHandler<GetAvailableVehiclesRequest>, ValidatorHandler<GetAvailableVehiclesRequest>>();
builder.Services.AddValidatorsFromAssemblyContaining<GetReservationsRequestValidator>();
builder.Services.AddScoped<IValidatorHandler<GetReservationsRequest>, ValidatorHandler<GetReservationsRequest>>();
//builder.Services.AddValidatorsFromAssemblyContaining<ReservationUpdateRequestValidator>();
//builder.Services.AddScoped<IValidatorHandler<ReservationUpdateRequest>, ValidatorHandler<ReservationUpdateRequest>>();





//TokenConfiguration
var jwtKey = builder.Configuration["JwtSettings:key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("No se encontró 'JwtSettings:key'. Configúralo en User Secrets o Variables de Entorno.");
}

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ActiveUser", policy => policy.RequireClaim("IsActive", "True"));
});

builder.Services.AddSingleton<IAuthorizationHandler, SameUserHandler>();

builder.Services.AddAuthorization(options =>
{
    // deja tu política ActiveUser si la necesitas...
    options.AddPolicy("SameUserPolicy", policy =>
        policy.Requirements.Add(new SameUserRequirement()));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<BearerTokenHandler>();

//Configurar el HttpClient y asociarle el handler
builder.Services
    .AddHttpClient<IVehicleService, VehicleServiceClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["VehicleService:BaseUrl"]);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddHttpMessageHandler<BearerTokenHandler>();

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

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();

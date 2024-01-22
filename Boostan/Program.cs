using System.Reflection;
using Application.Common;
using Boostan.Controllers;
using Boostan.Extensions;
using Boostan.SharedKernel.Extensions;
using DominClass.Entities.User;
using FluentValidation;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddSingleton(configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.ConfigurePersistence(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MediatRValidationBehavior<,>));
builder.Services.Configure<IdentitySettings>(configuration.GetSection(nameof(IdentitySettings)));

var identitySettings = configuration.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();

builder.Services.AddApplicationServices()
    .RegisterIdentityServices(identitySettings);


builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureCorsPolicy();
builder.Services.AddAutoMapper(typeof(User), typeof(JwtService), typeof(UserController));
builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

builder.Services.RegisterValidatorsAsServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
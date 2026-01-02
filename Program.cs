using InternalSystem_ModelContext;
using THMY_API.Models.DBContext;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using THMY_API.Middlewares;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using THM_Encryption;

//IHost host = Host.CreateDefaultBuilder().ConfigureServices(service => {
//    service.AddDbContext<DatabaseContext>(options => options.UseSqlite(@"Data Source=C:\\Users\\tzerming\\Documents\\SQLiteDB\\ticketing.db", b => b.MigrationsAssembly("THMY-API")));
//}).Build();

var builder = WebApplication.CreateBuilder(args);

Encryption encryption = new Encryption();

// Add services to the container.

#if DEBUG
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlite("Data Source=ticketing_new.db"));

// TODO: REMEMBER TO CHANGE THIS
builder.Services.AddDbContext<APIContext>(options =>
    options.UseSqlite("Data Source=API_prod.db"));

#else
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlite(encryption.Decrypt(builder.Configuration.GetConnectionString("ProductionConnection")), b => b.MigrationsAssembly("THMY-API")));

builder.Services.AddDbContext<APIContext>(options =>
        options.UseSqlite(encryption.Decrypt(builder.Configuration.GetConnectionString("APIProductionConnection")), b => b.MigrationsAssembly("THMY-API")));
#endif

//setup serilog

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "debug-.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


// Register deletion validator service
builder.Services.AddScoped<THMY_API.Services.IDeletionValidator, THMY_API.Services.DeletionValidator>();
builder.Services.AddSingleton<THM_Encryption.Encryption>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//run as services
builder.Host.UseWindowsService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    //app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

// Apply API key validation first
app.UseMiddleware<APIKeyMiddleware>();

// Apply deletion validation before controller actions
app.UseMiddleware<THMY_API.Middlewares.DeletionValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

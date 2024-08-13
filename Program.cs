using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using System.Text;
// using InventoryService.Models;
// using Microsoft.EntityFrameworkCore;



// Purpose: Entry point and startup configuration files for the application.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.WithProperty("ApplicationDbContext", Program.AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341")
        .CreateLogger();

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    //var host = CreateHostBuilder(configuration, args);
    var builder = WebApplication.CreateBuilder(args);
// DI IoC container configuration
// b lackbox to request a specifica type returns an instance of this type 
//knows gow to create a specidic type
// adding all the services to the blackbox 
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    // var CorsPolicy = "CorsPolicy";
    var configuration = builder.Configuration;

    builder.Host.UseSerilog((context, services, configuration) => configuration
      .Enrich.WithProperty("ApplicationDbContext", Program.AppName)
      .ReadFrom.Configuration(context.Configuration)
      .ReadFrom.Services(services)
      .Enrich.FromLogContext()
      .WriteTo.Console());

    var connectionString = configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
        });
    });

    builder.Services.AddDbContext<InventoryContext>(options =>
    {
        options.UseSqlServer(connectionString);
    });

    builder.Services.AddHealthChecks();

    // builder.Services.AddIdentity<>(options => options.SignIn.RequireConfirmedAccount = false)
    // .AddEntityFrameworkStores<ApplicationDbContext>()
    // .AddDefaultTokenProviders();



    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Password settings.
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;

        // User settings.
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    });


    //Default config for ASP.NET Core
    builder.Services.AddControllers(options =>
    {
        // options.Filters.Add(typeof(AbpExceptionFilter));
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomOperationIds(apiDesc =>
        {
            return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
        });
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }
                });
        // options.ParameterFilter<SwaggerNullableParameterFilter>();
    });

builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    }) //Add Be     arer
    .AddJwtBearer(options =>
    {
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateIssuer = true,
        //     ValidIssuer = configuration["Jwt:Issuer"],
        //     ValidateAudience = true,
        //     ValidAudience = configuration["Jwt:Audience"],
        //     ValidateLifetime = true,
        //     // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration("Jwt:Key"))),
        //     // ValidIssuer = configuration["Jwt:Issuer"],
        //     // ValidAudience = configuration["Jwt:Audience"],
        //     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
        //     ValidateIssuerSigningKey = true
        // }; 
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // ValidateLifetime = true,
            // ClockSkew = TimeSpan.FromSeconds(0),
            // ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
        };
      
    });
    builder.Services.AddAuthorization();

    builder.Services.AddRazorPages();

    // builder.Services.AddExceptionHandling();


    builder.Services.AddSignalR();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddMvc();
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

    // omitted
    var app = builder.Build();
// middleware - chain of object components that call eachother in sequence , returns on the way back - request pipeline
    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template
        options.MessageTemplate = "Handled {RequestPath}";

        // Emit debug-level events instead of the defaults
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

        // Attach additional properties to the request completion event
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseDeveloperExceptionPage();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            c.DisplayOperationId();
            c.DisplayRequestDuration();
        });
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    var supportedCultures = new[] { "vi-VN" };
    var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    app.UseRequestLocalization(localizationOptions);

    app.UseCors("AllowAll");
    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // app.UseMiddleware<GetTokenFromQueryStringMiddleware>();

    app.MapRazorPages();

    app.MapDefaultControllerRoute();

    app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

    app.MapControllers();

    // app.MapHub<UserHub>("/userHub");

    app.MapHealthChecks("/healthz");

    app.MapFallbackToFile("index.html");

    //Seeding data
    // app.MigrateDatabase();

    // omitted

    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

// public void ConfigureServices(IServiceCollection services){
//    var connection = Configuration.GetConnectionString("InventoryDatabase");
//    services.AddDbContext<InventoryContext>(options => options.UseSqlServer(connection));
//     services.AddControllers(); 
// }
// public class MyAuthenticationOptions :         
//     AuthenticationSchemeOptions
// {
//     public const string DefaultScheme = "MyAuthenticationScheme";
//     public string TokenHeaderName { get; set; } = "MyToken";
// }
public partial class Program
{
    // public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = "Namespace";
}

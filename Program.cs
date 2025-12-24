
using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;
using System.Text.Json;


namespace FarmAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.Configure<FarmGrowDatabaseSettings>(
            //    builder.Configuration.GetSection("FarmGrowDatabase"));


            // Program.cs (add before registering repositories)
            builder.Services.Configure<FarmGrowDatabaseSettings>(
                builder.Configuration.GetSection("FarmGrowDatabase"));

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<FarmGrowDatabaseSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            //builder.Services.AddSingleton<IMongoDatabase>(sp =>
            //{
            //    var settings = sp.GetRequiredService<IOptions<FarmGrowDatabaseSettings>>().Value;
            //    return sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName);
            //});

            builder.Services.AddScoped(sp =>
            {
                var mongoSettings = sp.GetRequiredService<IOptions<FarmGrowDatabaseSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoSettings.DatabaseName);
            });

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            builder.Services.AddScoped<OwnerService>();
            builder.Services.AddScoped<FarmService>();
            builder.Services.AddScoped<MaintainerService>();
            builder.Services.AddScoped<CropService>();
            builder.Services.AddScoped<ActivityService>();

            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<MagicLinkRepository>();
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

            var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
            var key = Encoding.UTF8.GetBytes(jwt.Key);

            // Program.cs - Read JWT from COOKIE (not Authorization header)
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };

                    //THIS EXTRACTS FROM COOKIE (missing in your config)
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["authToken"];
                            Console.WriteLine($"JWT COOKIE: {context.Token?.Substring(0, 20)}...");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine($"Token Validated: { context.Principal?.Identity?.Name}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("JWT VALIDATION FAILED");
                            Console.WriteLine($"Cookie: {context.Request.Cookies["authToken"]?.Substring(0, 20)}...");
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EasyGrow Farm API",
                    Version = "v1",
                    Description = "Farm Traceability HTTP API"
                });

                c.EnableAnnotations();
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy.WithOrigins("http://localhost:4200",
                        // Server machine Angular
                        "http://192.168.29.235:4200",
                        // Any device on your LAN running Angular
                        "http://192.168.29.*:4200",
                        // Allow all 192.168.29.x subnet (dev only)
                        "http://192.168.29.:4200"
                        )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("Set-Cookie");  //Allow Set-Cookie
                });
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();   // provides IDistributedCache
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".FarmAPI.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None; // if using https + cross-site
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // https
                options.IdleTimeout = TimeSpan.FromMinutes(20);
            });

            builder.Services.AddScoped<Fido2Service>();
            builder.Services.AddScoped<PasswordHasher>();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSession();
            app.UseCors("AllowAngularApp");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

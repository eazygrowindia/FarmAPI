
using FarmAPI.Models;
using FarmAPI.Services;

namespace FarmAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<FarmGrowDatabaseSettings>(
                builder.Configuration.GetSection("FarmGrowDatabase"));

            builder.Services.AddSingleton<OwnerService>();
            builder.Services.AddSingleton<FarmService>();
            builder.Services.AddSingleton<MaintainerService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //builder.Services.AddSwaggerGen(c =>
            //{
            //    c.EnableAnnotations();
            //    // ... other configuration
            //});
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    //policy.WithOrigins("http://localhost:4200")
                    //      .AllowAnyMethod()
                    //      .AllowAnyHeader();
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAngularApp");

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

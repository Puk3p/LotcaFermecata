using Microsoft.AspNetCore.Identity;
using SP25.Business.ModelMapping;
using SP25.Business.Services.Contracts;
using SP25.Business.Services.Implementations;
using SP25.Domain.Context;
using SP25.Domain.Models;
using SP25.Domain.Repository;
using SP25.WebApi.Authentication;

namespace SP25.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendDev", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:63140")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<MyDbContext>();

            builder.Services.ConfigureJwtAuthentication(builder.Configuration, logger: Serilog.Log.Logger);

            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IWorkZoneService, WorkZoneService>();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<ITestService, TestService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontendDev");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

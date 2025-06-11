using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Text;
using WebAPI.Repositories;
using ZLogger;
using ZLogger.Providers;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // SqlKata QueryFactory ���
            builder.Services.AddSingleton<IDbConnection>(o => new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<Compiler, MySqlCompiler>();
            builder.Services.AddSingleton<QueryFactory>();

            // �α� ���� �߰�/����
            builder.Logging.ClearProviders();
            builder.Logging.AddZLoggerConsole();
            builder.Logging.AddZLoggerRollingFile(options =>
            {
                // �α� ���� �̸� ��Ģ: logs/2025-06-11_000.log ����
                options.FilePathSelector = (timestamp, sequenceNo) => $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNo:000}.log";

                // �Ϸ�(Day) ������ ������ ���� ����
                options.RollingInterval = RollingInterval.Day;
            });
            // JWT ���� ���� ���
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
                    };
                });
            // Repository ���
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            // Add services to the container.
            builder.Services.AddControllers();
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
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

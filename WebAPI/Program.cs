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

            // SqlKata QueryFactory 등록
            builder.Services.AddSingleton<IDbConnection>(o => new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<Compiler, MySqlCompiler>();
            builder.Services.AddSingleton<QueryFactory>();

            // 로깅 설정 추가/수정
            builder.Logging.ClearProviders();
            builder.Logging.AddZLoggerConsole();
            builder.Logging.AddZLoggerRollingFile(options =>
            {
                // 로그 파일 이름 규칙: logs/2025-06-11_000.log 형식
                options.FilePathSelector = (timestamp, sequenceNo) => $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNo:000}.log";

                // 하루(Day) 단위로 파일을 새로 생성
                options.RollingInterval = RollingInterval.Day;
            });
            // JWT 인증 서비스 등록
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
            // Repository 등록
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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Text;
using OmokServer.Repositories;
using OmokServer.Services;
using ZLogger;
using ZLogger.Providers;

namespace OmokServer
{
    /// <summary>
    /// 애플리케이션의 진입점
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // SqlKata QueryFactory 설정
            builder.Services.AddSingleton<IDbConnection>(o => new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<Compiler, MySqlCompiler>();
            builder.Services.AddSingleton<QueryFactory>();
            // OpenAPI 설정
            builder.Services.AddEndpointsApiExplorer();

            // 로깅 설정
            builder.Logging.ClearProviders();
            builder.Logging.AddZLoggerConsole();
            builder.Logging.AddZLoggerRollingFile(options =>
            {
                // 로그 파일 이름 형식: logs/2025-06-11_000.log 형식
                options.FilePathSelector = (timestamp, sequenceNo) => $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNo:000}.log";

                // 일(Day) 단위로 로그 파일 생성
                options.RollingInterval = RollingInterval.Day;
            });
            // JWT 인증 설정
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
            // AuthService 등록
            builder.Services.AddScoped<IAuthService, AuthService>();
            // Match Repository 등록
            builder.Services.AddScoped<IMatchRepository, MatchRepository>();
            // 매칭 서비스 등록
            builder.Services.AddScoped<IMatchHistoryService, MatchHistoryService>();
            builder.Services.AddSingleton<GameRoomManager>();
            builder.Services.AddSingleton<MatchmakingService>();
            // SignalR 설정
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<UserConnectionManager>();
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Swagger 문서 설정
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Omok Server API", Version = "v1" });

                // JWT 인증 설정
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: \"Bearer 12345abcdef\""
                });

                // 보안 요구사항 설정
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // 개발 환경에서 Swagger UI 활성화
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 미들웨어 설정
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // 엔드포인트 설정
            app.MapControllers();
            app.MapHub<OmokServer.Hubs.GameHub>("/gamehub");
            app.Run();
        }
    }
}
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
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // SqlKata QueryFactory 등록
            builder.Services.AddSingleton<IDbConnection>(o => new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<Compiler, MySqlCompiler>();
            builder.Services.AddSingleton<QueryFactory>();
            // OpenAPI 서비스 등록
            builder.Services.AddEndpointsApiExplorer();

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
            // AuthService 등록
            builder.Services.AddScoped<IAuthService, AuthService>();
            // Match Repository 등록
            builder.Services.AddScoped<IMatchRepository, MatchRepository>();
            // 인게임 서비스 등록
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddSingleton<GameRoomManager>();
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Swagger 문서의 제목, 버전 등 정보를 설정합니다.
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Omok Server API", Version = "v1" });

                // 1. "Authorize" 버튼을 만들기 위한 설정입니다.
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

                // 2. 위에서 정의한 "Bearer" 인증을 사용하도록 설정하고, 자물쇠 아이콘을 표시합니다.
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
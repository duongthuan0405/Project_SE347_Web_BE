
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using se347_be.Database;
using se347_be.Work.JWT;
using se347_be.Work.Repositories.Implementations;
using se347_be.Work.Repositories.Interfaces;
using se347_be.Work.Services.Implementations;
using se347_be.Work.Services.Interfaces;

namespace se347_be;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region Load .env File
        Env.Load();
        #endregion

        #region Database Connection
        // Get Connection String from appsettings.json
        var connectionString = Environment.GetEnvironmentVariable("PSQLConnectionString");
        Console.WriteLine(connectionString);
        // Apply DbContext with PostgreSQL
        builder.Services.AddDbContext<MyAppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        #endregion

        #region JWT

        var jwtConfig = builder.Configuration.GetSection("Jwt");

        string JWTKey = Environment.GetEnvironmentVariable("JWTKey") ?? throw new Exception("No Key JWT");
        string JWTIssuer = jwtConfig["Issuer"] ?? throw new Exception("No Issuer JWT");
        string JWTAudience = jwtConfig["Audience"] ?? throw new Exception("No Audience JWT");
        double JWTExpireHours = double.Parse(jwtConfig["ExpireHours"] ?? "1");

        builder.Services.AddSingleton(new JWTHelper(JWTKey, JWTIssuer, JWTAudience, JWTExpireHours));
        #endregion
        
        #region Custom Services and Repositories
        builder.Services.AddScoped<ITestEntityRepository, TestEntityRepository>();
        builder.Services.AddScoped<ITestEntityService, TestEntityService>();
        builder.Services.AddScoped<IAppAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        #endregion

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // dev local
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = JWTIssuer,

                ValidateAudience = true,
                ValidAudience = JWTAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType = JwtRegisteredClaimNames.Sub,
            };
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
            // Apply any pending migrations
            dbContext.Database.Migrate();
        }
        
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}


using Microsoft.EntityFrameworkCore;
using se347_be.Database;
using se347_be.Database.DbEntity;
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

        #region Database Connection
        // Get Connection String from appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("PSQLConnectionString");
        Console.WriteLine(connectionString);
        // Apply DbContext with PostgreSQL
        builder.Services.AddDbContext<MyAppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        #endregion

        #region Custom Services and Repositories
        builder.Services.AddScoped<ITestEntityRepository, TestEntityRepository>();
        builder.Services.AddScoped<ITestEntityService, TestEntityService>();
        #endregion

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}

using HabitTracker.Application.Auth.Interfaces;
using HabitTracker.Application.Common;
using HabitTracker.Infrastructure.Auth;
using HabitTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ITokenHasher, Sha256TokenHasher>();

        return services;
    }

    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrEmpty(configured))
        {
            return configured;
        }

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            return ConvertDatabaseUrlToNpgsqlConnectionString(databaseUrl);
        }

        throw new InvalidOperationException(
            "No database connection string configured. Set ConnectionStrings__Postgres or DATABASE_URL.");
    }

    // Railway (and most managed Postgres providers) expose the connection as a URL,
    // e.g. postgresql://user:pass@host:port/db — Npgsql needs the key=value format instead.
    private static string ConvertDatabaseUrlToNpgsqlConnectionString(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        // "Prefer" negotiates SSL when the server offers it (e.g. Railway's managed Postgres)
        // and falls back to a plain connection otherwise, so this works against both managed
        // and local/non-SSL Postgres instances without needing separate configuration.
        return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Infrastructure.Persistence;
using FiscalPymeEC.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FiscalPymeEC.Infrastructure.Authentication;
using FiscalPymeEC.Application.Authentication;
using FiscalPymeEC.Infrastructure.Persistence.Repositories;

namespace FiscalPymeEC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {

        var connectionString =
            configuration.GetConnectionString("FiscalPyme")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'FiscalPyme'.");

        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<
            IPasswordHasher,
            AspNetCorePasswordHasher>();

        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options.UseNpgsql(connectionString);
            });

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<
            ILoginService,
            LoginService>();

        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        services.AddSingleton<
            ITokenGenerator,
            JwtTokenGenerator>();

        return services;
    }
}
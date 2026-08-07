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
using FiscalPymeEC.Application.Users;

namespace FiscalPymeEC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        services.AddDbContext<ApplicationDbContext>(
          (serviceProvider, options) =>
          {
              var effectiveConfiguration =
                  serviceProvider
                      .GetRequiredService<IConfiguration>();

              var connectionString =
                  effectiveConfiguration
                      .GetConnectionString("FiscalPyme")
                  ?? throw new InvalidOperationException(
                      "No se encontró la cadena de conexión 'FiscalPyme'.");

              options.UseNpgsql(connectionString);
          });

        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<
            IPasswordHasher,
            AspNetCorePasswordHasher>();
        
        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<
            ILoginService,
            LoginService>();

        services.AddScoped<
            ICreateUserService,
            CreateUserService>();

        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        services.AddSingleton<
            ITokenGenerator,
            JwtTokenGenerator>();

        services.AddScoped<DatabaseSeeder>();

        services.AddScoped<
            IAuthenticationAuditService,
            AuthenticationAuditService>();

        return services;
    }
}
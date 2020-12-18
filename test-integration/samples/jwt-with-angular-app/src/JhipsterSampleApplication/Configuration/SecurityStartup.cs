using System;
using JhipsterSampleApplication.Infrastructure.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using JhipsterSampleApplication.Infrastructure.Data;
using JhipsterSampleApplication.Domain;
using JhipsterSampleApplication.Security;
using JhipsterSampleApplication.Security.Jwt;
using JhipsterSampleApplication.Domain.Services;
using JhipsterSampleApplication.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthenticationService = JhipsterSampleApplication.Domain.Services.AuthenticationService;
using IAuthenticationService = JhipsterSampleApplication.Domain.Services.Interfaces.IAuthenticationService;

namespace JhipsterSampleApplication.Configuration {
    public static class SecurityStartup {

        public const string UserNameClaimType = JwtRegisteredClaimNames.Sub;

        public static IServiceCollection AddSecurityModule(this IServiceCollection services)
        {
            //TODO Retrieve the signing key properly (DRY with TokenProvider)
            var opt = services.BuildServiceProvider().GetRequiredService<IOptions<JHipsterSettings>>();
            var jhipsterSettings = opt.Value;
            byte[] keyBytes;
            var secret = jhipsterSettings.Security.Authentication.Jwt.Secret;

            if (!string.IsNullOrWhiteSpace(secret)) {
                keyBytes = Encoding.ASCII.GetBytes(secret);
            }
            else {
                keyBytes = Convert.FromBase64String(jhipsterSettings.Security.Authentication.Jwt.Base64Secret);
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims

            services.AddIdentity<User, Role>(options => {
                    options.SignIn.RequireConfirmedEmail = true;
                    options.ClaimsIdentity.UserNameClaimType = UserNameClaimType;
                })
                .AddEntityFrameworkStores<ApplicationDatabaseContext>()
                .AddUserStore<UserStore<User, Role, ApplicationDatabaseContext, string, IdentityUserClaim<string>,
                    UserRole, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>>()
                .AddRoleStore<RoleStore<Role, ApplicationDatabaseContext, string, UserRole, IdentityRoleClaim<string>>
                >()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg => {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ClockSkew = TimeSpan.Zero,/// remove delay of token when expire
                        NameClaimType = UserNameClaimType
                    };
                });

            services.AddScoped<IPasswordHasher<User>, BCryptPasswordHasher>();
            services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            return services;
        }

        public static IApplicationBuilder UseApplicationSecurity(this IApplicationBuilder app,
            JHipsterSettings jhipsterSettings)
        {
            app.UseCors(CorsPolicyBuilder(jhipsterSettings.Cors));
            app.UseAuthentication();
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
            return app;
        }

        private static Action<CorsPolicyBuilder> CorsPolicyBuilder(Cors config)
        {
            //TODO implement an url based cors policy rather than global or per controller
            return builder => {
                if (!config.AllowedOrigins.Equals("*"))
                {
                    if (config.AllowCredentials)
                    {
                        builder.AllowCredentials();
                    }
                    else
                    {
                        builder.DisallowCredentials();
                    }
                }

                builder.WithOrigins(config.AllowedOrigins)
                    .WithMethods(config.AllowedMethods)
                    .WithHeaders(config.AllowedHeaders)
                    .WithExposedHeaders(config.ExposedHeaders)
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(config.MaxAge));
            };
        }
    }
}

﻿using Aide.Extensions;
using Aide.Service;
using Aide.Service.ExcelSheetService;
using Aide.Service.GraphAPIService;
using Aide.Service.OneDriveService;
using Aide.Service.SupuervisedInfoAPIService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Aide
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public const string ObjectIdentifierType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantIdType = "http://schemas.microsoft.com/identity/claims/tenantid";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpContextAccessor(); // Add a default implemantation for IHttpContextAccessor service

            services.AddDistributedMemoryCache();
            // Add Session state configration to store access token and professor username when login
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(1800);
                options.Cookie.Name = "cookiesession";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddHttpClient();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            .AddCookie();

            // Add application services.
            services.AddScoped<IOneDriveService, OneDriveService>();
            services.AddScoped<IStudyPlan, StudyPlan>();
            services.AddScoped<ISupuervisedInfoService, SupuervisedInfoService>();

            services.AddSingleton<IGraphAuthProvider, GraphAuthProvider>();
            services.AddSingleton<IGraphServiceClientFactory, GraphServiceClientFactory>();
            services.AddTransient<IGraphService, GraphService>();

            services.Configure<HstsOptions>(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            /*services.AddHealthChecks();*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                /*app.UseStatusCodePages();*/
            }
            else
            {
                app.UseStatusCodePages();
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                /*endpoints.MapHealthChecks("/healthcheck");*/
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Accounts}/{action=Login}/{id?}");
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LimFx.Business.Exceptions;
using LimFx.Business.Extensions;
using LimFx.Business.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LimFxTemplate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 从appsettings.json 中获取mongodb的设置信息
            var config = Configuration.GetSection(nameof(DatabaseSettings));
            services.Configure<DatabaseSettings>(config);

            services.AddControllers();

            // swagger
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Host = "localhost";
                    document.Info.Version = "v1";
                    document.Info.Title = "Template API using LimFx.Common";
                    document.Info.Description = "ASP.NET Core web API";
                    document.Info.TermsOfService = "None";
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "珂学家",
                        Email = "1769712655@qq.com",
                        Url = "https://chronos.limfx.pro/"
                    };
                    document.Info.License = new NSwag.OpenApiLicense
                    {
                        Name = "No lisence, all rights reserved",
                        Url = string.Empty
                    };
                };
            });
            services.AddAuthentication(op =>
            {
                op.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(op =>
                {
                    op.Events.OnRedirectToLogin += (o) => throw new _403Exception("无法通过身份验证");
                    op.Events.OnRedirectToAccessDenied += (o) => throw new _403Exception("无法通过身份验证");
                });
            services.AddAuthorization(op =>
            {
                op.InvokeHandlersAfterFailure = false;
            });

            // 添加路径ratelimit
            services.AddEnhancedRateLimiter(ClaimTypes.UserData, 1000, 1000);

            // cookie持久化
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), ".secret")))
                .SetDefaultKeyLifetime(new TimeSpan(50, 0, 0, 0));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // 添加limfx错误处理中间件 https://chronos.limfx.pro/ReadArticle/457/limfxerrorhandler-shi-yong-wen-dang
            app.UseLimFxExceptionHandler();
            // 添加全局ratelimiter
            app.UseRateLimiter(maxRequest: 20, blockTime: 10000, maxReq: 200);
            app.UseHttpsRedirection();


            // 添加swagger ui
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthentication();

            app.UseCookiePolicy();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

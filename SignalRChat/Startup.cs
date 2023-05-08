using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SignalRChat.Services;

namespace SignalRChat
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddRazorPages();
            services.AddSingleton<IChatRoomService, ChatRoomService>();
            services.AddSignalR();
            services.AddSignalRCore();


            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                }).AddCookie(options => { options.LoginPath = "/Login"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }


            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting(); // middle ware ha raaat shavad mohem!!!!
            app.UseAuthentication();
            app.UseAuthorization();

            //? cross platform another domain
            //app.UseCors(builder =>
            //{
            //    builder.WithOrigins( /*domain https://www......*/)
            //        .AllowAnyHeader()
            //        .WithMethods("GET", "POST")
            //        .AllowCredentials();
            //});

            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapHub<ChatHub>("/ChatHub");

                endpoints.MapHub<AgentHub>("/AgentHub");
            });
        }
    }
}

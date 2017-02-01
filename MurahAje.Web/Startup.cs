using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using MurahAje.Web.Tools;
using MurahAje.Web;
using MurahAje.Web.Services;
using Redis.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;

namespace MurahAje.Web
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                //builder.AddUserSecrets();
            }
            Configuration = builder.Build();
            
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new SignalRContractResolver();

            var serializer = JsonSerializer.Create(settings);

            services.Add(new ServiceDescriptor(typeof(JsonSerializer),
                         provider => serializer,
                         ServiceLifetime.Transient));

            MurahAje.Web.Entities.SocialDb.RedisConStr = Configuration.GetConnectionString("RedisCon");
            //var fbid = Configuration["Authentication:Google:ClientId"];
            ObjectContainer.Register<SocialHub>(new SocialHub());
            //redis auth
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddRedisStores(MurahAje.Web.Entities.SocialDb.RedisConStr)
                .AddDefaultTokenProviders();

            UserStore<ApplicationUser>.AppNamespace = "urn:app:";

            services.Configure<IdentityOptions>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.AddSignalR(options =>
            {
                options.Hubs.EnableDetailedErrors = true;
            });

            services.AddCors();

            // Add framework services.
            services.AddMvc();
            
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Murah Aje API", Version = "v1", Description = "Rest service to access Murah Aje data",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Mif", Email = "mifmasterz@gmail.com", Url = "http://twitter.com/gravicode" },
                    License = new License { Name = "Free for Everyone", Url = "http://gravicode.com/murahaje" }
                });
                
            });
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.CookieHttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseSession();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseIdentity();
            app.UseWebSockets();
            app.UseSignalR();
            app.UseGoogleAuthentication(new GoogleOptions()
            {
                ClientId = Configuration["Authentication:Google:ClientId"],
                ClientSecret = Configuration["Authentication:Google:ClientSecret"]
            });
            app.UseFacebookAuthentication(new FacebookOptions()
            {
                AppId = Configuration["Authentication:Facebook:ClientId"],
                AppSecret = Configuration["Authentication:Facebook:ClientSecret"]
            });
            /*
            // Configure the HTTP request pipeline.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookie",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });*/
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}"
                );
                routes.MapRoute(
                       name: "api",
                       template: "api/{controller=Home}/{action=Index}/{id?}"
               );
            });
            if (env.IsDevelopment())
            {
              
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUi(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Murah Aje API V1");
            });

            app.UseCors(builder => builder.WithOrigins("http://murahaje.azurewebsites.net"));
        }
    }
}

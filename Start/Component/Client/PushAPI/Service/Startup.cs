namespace Game.Client.PushAPI.Service
{
    using Game.Client.PushAPI.Service.Hubs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Text.Json;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*---------------------------------------------------------------------------------------
             * EXCERCISE 5: add DAPR controllers
             *---------------------------------------------------------------------------------------*/

            // default json serializer settings
            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

            // add SignalR support
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            /*---------------------------------------------------------------------------------------
             * EXCERCISE 5: configure the app to handle cloud events
             *---------------------------------------------------------------------------------------*/

            app.UseEndpoints(endpoints =>
            {
                /*---------------------------------------------------------------------------------------
                 * EXCERCISE 5: 
                 * - add the pubsub subscription endpoint mapping
                 * - add the dapr controller mapping
                 *---------------------------------------------------------------------------------------*/

                endpoints.MapHub<ShapeHub>("/shapehub");
            });
        }
    }
}

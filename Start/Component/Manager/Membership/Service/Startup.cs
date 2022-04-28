namespace Game.Manager.Membership.Service
{
    using Dapr.Client;
    using Game.Actor.Shape.Interface;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Startup
    {
        private const string PubSubName = "pubsub";
        private const string ShapeListStateName = "statestore";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, JsonSerializerOptions serializerOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
            });
        }

        private async Task<List<Guid>> RetrieveShapeListFromState(string stateKey, DaprClient client)
        {
            var shapeList = await client.GetStateAsync<List<Guid>>(ShapeListStateName, stateKey);
            shapeList = shapeList?.Distinct().ToList() ?? new List<Guid>();
            return shapeList;
        }
    }
}

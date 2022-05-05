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
            services.AddDaprClient();
            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, JsonSerializerOptions serializerOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCloudEvents();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();

                endpoints.MapGet("{clientId}", Onboard);
                endpoints.MapPost("onCreatedShape", OnCreatedShape).WithTopic(PubSubName, "onCreatedShape");
            });

            async Task Onboard(HttpContext context)
            {
                Console.WriteLine("Enter Onboard");
                var client = context.RequestServices.GetRequiredService<DaprClient>();

                var clientId = (string)context.Request.RouteValues["clientId"];
                Console.WriteLine("Onboard ClientId {0}", clientId);

                var shapeList = await RetrieveShapeListFromState(clientId, client);
                Console.WriteLine("Number of shapes: {0}", shapeList.Count);

                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(context.Response.Body, shapeList, serializerOptions);
            }

            async Task OnCreatedShape(HttpContext context)
            {
                Console.WriteLine("Enter OnCreatedShape");
                var client = context.RequestServices.GetRequiredService<DaprClient>();

                var shapeActorId = await JsonSerializer.DeserializeAsync<ShapeActorId>(context.Request.Body, serializerOptions);
                Console.WriteLine("ShapeActorId: ClientId {0}, ShapeId {1}", shapeActorId.ClientId, shapeActorId.ShapeId);

                var shapeList = await RetrieveShapeListFromState(shapeActorId.ClientId.ToString("N"), client);
                Console.WriteLine("Number of shapes: {0}", shapeList.Count);

                if (!shapeList.Any(shapeId => shapeId == shapeActorId.ShapeId))
                {
                    shapeList.Add(shapeActorId.ShapeId);
                    await client.SaveStateAsync(ShapeListStateName, shapeActorId.ClientId.ToString("N"), shapeList);
                    Console.WriteLine("New shape added");
                }
            }
        }

        private async Task<List<Guid>> RetrieveShapeListFromState(string stateKey, DaprClient client)
        {
            var shapeList = await client.GetStateAsync<List<Guid>>(ShapeListStateName, stateKey);
            shapeList = shapeList?.Distinct().ToList() ?? new List<Guid>();
            return shapeList;
        }
    }
}

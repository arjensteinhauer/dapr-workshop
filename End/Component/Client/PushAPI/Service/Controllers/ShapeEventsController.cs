using Dapr;
using Game.Actor.Shape.Interface;
using Game.Client.PushAPI.Service.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Game.Client.PushAPI.Service.Controllers
{
    [ApiController]
    public class ShapeEventsController : ControllerBase
    {
        private const string PubSubName = "pubsub";

        private readonly IHubContext<ShapeHub, IShapeActorEvents> _hubContext;

        public ShapeEventsController(IHubContext<ShapeHub, IShapeActorEvents> hubContext)
        {
            _hubContext = hubContext;
        }

        [Topic(PubSubName, "onUpdatedShapeLocation")]
        [HttpPost("onUpdatedShapeLocation")]
        public async Task OnUpdatedShapeLocation(ShapeActorId shapeActorId)
        {
            await _hubContext.Clients.Group(shapeActorId.ClientId.ToString("N")).OnUpdatedShapeLocation(shapeActorId.ShapeId);
        }
    }
}

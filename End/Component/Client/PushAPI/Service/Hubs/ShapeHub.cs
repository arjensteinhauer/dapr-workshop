namespace Game.Client.PushAPI.Service.Hubs
{
    using Game.Actor.Shape.Interface;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    /// <summary>
    /// The SignalR shape hub.
    /// </summary>
    public class ShapeHub : Hub<IShapeActorEvents>
    {
        public async Task Subscribe(string clientId)
        {
            await this.Groups.AddToGroupAsync(Context.ConnectionId, clientId);
        }

        public async Task Unsubscribe(string clientId)
        {
            await this.Groups.RemoveFromGroupAsync(Context.ConnectionId, clientId);
        }
    }
}

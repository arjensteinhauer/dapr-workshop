namespace Game.Actor.Shape.Service
{
    using Dapr.Actors.Runtime;
    using Dapr.Client;
    using Game.Actor.Shape.Interface;
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class ShapeActor : Actor, IShapeActor
    {
        private const string ShapeStateName = "shape";

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        public ShapeActor(ActorHost host)
            : base(host)
        {
        }

        public Task<Shape> RetrievePosition()
        {
            throw new NotImplementedException("TODO: get from the actor state");
        }

        public Task MoveToNewPosition()
        {
            throw new NotImplementedException("TODO: calculate the new state");
        }
    }
}

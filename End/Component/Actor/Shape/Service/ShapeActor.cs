namespace Game.Actor.Shape.Service
{
    using Dapr.Actors.Runtime;
    using Dapr.Client;
    using Game.Actor.Shape.Interface;
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class ShapeActor : Actor, IShapeActor, IRemindable
    {
        private const string ShapeStateName = "shape";
        private const string CalculateNewPositionReminderName = "calculateNewPosition";

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
            // get from the actor state
            return StateManager.GetStateAsync<Shape>(ShapeStateName);
        }

        public Task MoveToNewPosition()
        {
            // get from the actor state
            return CalculateNewPosition(null);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                // reminder for calculating a new shape position
                case CalculateNewPositionReminderName:
                    await CalculateNewPosition(state);
                    break;
            }
        }

        protected override async Task OnActivateAsync()
        {
            Console.WriteLine($"Activate ShapeActor {Id.GetId()}");
            var actorIdParts = this.Id.GetId().Split('_');
            var clientId = actorIdParts[0];
            var shapeId = actorIdParts[1];

            // this is the first time the actor is activated --> initiate the actor state
            await StateManager.TryAddStateAsync(ShapeStateName, CreateNewShape());

            // start a reminder for calculating new positions
            await RegisterReminderAsync(
                CalculateNewPositionReminderName,
                null,
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(500));

            // add this shape instance to list (via publish)
            var client = new DaprClientBuilder().UseJsonSerializationOptions(_jsonOptions).Build();
            await client.PublishEventAsync("pubsub", "onCreatedShape", new { ClientId = Guid.Parse(clientId), ShapeId = Guid.Parse(shapeId) });

            // base
            await base.OnActivateAsync();
        }

        private async Task CalculateNewPosition(object state)
        {
            // get the current state
            var result = await StateManager.TryGetStateAsync<Shape>(ShapeStateName);
            if (result.HasValue)
            {
                // calculate the new position
                var shape = result.Value;

                if (shape.X > 900) shape.DiffX = -2;
                if (shape.X < 10) shape.DiffX = 2;
                if (shape.Y > 600) shape.DiffY = -2;
                if (shape.Y < 10) shape.DiffY = 2;

                shape.X += shape.DiffX;
                shape.Y += shape.DiffY;

                #region DEMO
                // demo: new implementation - let's also rotate the shape
                shape.Angle++;
                #endregion DEMO

                // save the new position in the state
                await StateManager.SetStateAsync(ShapeStateName, shape);

                // notify any subscribers the position has changed (publish on pub/sub)
                var actorIdParts = this.Id.GetId().Split('_');
                var clientId = actorIdParts[0];
                var shapeId = actorIdParts[1];
                var client = new DaprClientBuilder().UseJsonSerializationOptions(_jsonOptions).Build();
                await client.PublishEventAsync("pubsub", "onUpdatedShapeLocation", new { ClientId = Guid.Parse(clientId), ShapeId = Guid.Parse(shapeId) });
            }
            else
            {
                Console.WriteLine($"ShapeActor {Id.GetId()} can't get the state.");
            }
        }

        /// <summary>
        /// Create a new shape with a random position.
        /// </summary>
        /// <returns>The created shape.</returns>
        private Shape CreateNewShape()
        {
            var randomizer = new Random();
            var diff = new int[2] { -1, 1 };

            return new Shape()
            {
                X = randomizer.Next(10, 900),
                Y = randomizer.Next(10, 600),
                Angle = 0,
                DiffX = diff[randomizer.Next(0, 2)],
                DiffY = diff[randomizer.Next(0, 2)]
            };
        }
    }
}

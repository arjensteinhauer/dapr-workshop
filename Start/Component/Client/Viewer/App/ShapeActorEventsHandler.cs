namespace Game.Client.Viewer.App
{
    using Microsoft.AspNetCore.SignalR.Client;
    using System;
    using System.Configuration;
    using System.Threading.Tasks;

    public class ShapeActorEventsHandler : IShapeActorEventsHandler
    {
        /// <summary>
        /// SignalR hub connection.
        /// </summary>
        private readonly HubConnection _connection;

        /// <summary>
        /// Gets the client ID from the app settings.
        /// </summary>
        private Guid ClientId => Guid.Parse(ConfigurationManager.AppSettings["clientId"]);

        /// <summary>
        /// SignalR connection ID.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// Handler for shape location updated events.
        /// </summary>
        public event EventHandler<Guid> OnUpdatedShapeLocation;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="connection"></param>
        public ShapeActorEventsHandler(HubConnection connection)
        {
            _connection = connection;
            _connection.Closed += (ex) =>
            {
                // on disconnect, remove the current event handlers
                RemoveEventHandlersOnClosed(_connection);
                return Task.FromResult(true);
            };
            _connection.Reconnected += async (connectionId) =>
            {
                // save the new connection ID
                ConnectionId = connectionId;

                // on reconnect, subscribe on the events
                await Connect().ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Connect to signalR events.
        /// </summary>
        public async Task Connect()
        {
            AddEventHandlersOnConnect(_connection);

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync().ConfigureAwait(false);

                ConnectionId = _connection.ConnectionId;
            }

            await _connection.InvokeAsync("Subscribe", ClientId.ToString("N")).ConfigureAwait(false);
        }


        /// <summary>
        /// Disconnect the signalR connection.
        /// </summary>
        /// <returns>Async task.</returns>
        public async Task Disconnect()
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("Unsubscribe", ClientId.ToString("N")).ConfigureAwait(false);
                await _connection.StopAsync().ConfigureAwait(false);
            }
        }

        private void AddEventHandlersOnConnect(HubConnection connection)
        {
            connection.On<Guid>("OnUpdatedShapeLocation", (shapeId) =>
            {
                OnUpdatedShapeLocation?.Invoke(this, shapeId);
            });
        }

        private void RemoveEventHandlersOnClosed(HubConnection connection)
        {
            connection.Remove("OnUpdatedShapeLocation");
        }
    }
}

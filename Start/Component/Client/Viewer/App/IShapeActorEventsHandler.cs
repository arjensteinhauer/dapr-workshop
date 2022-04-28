namespace Game.Client.Viewer.App
{
    using System;
    using System.Threading.Tasks;

    public interface IShapeActorEventsHandler
    {
        /// <summary>
        /// SignalR connection ID.
        /// </summary>
        string ConnectionId { get; }

        /// <summary>
        /// Handler for shape location updated events.
        /// </summary>
        event EventHandler<Guid> OnUpdatedShapeLocation;

        /// <summary>
        /// Connect to signalR events.
        /// </summary>
        Task Connect();

        /// <summary>
        /// Disconnect the signalR connection.
        /// </summary>
        /// <returns>Async task.</returns>
        Task Disconnect();
    }
}

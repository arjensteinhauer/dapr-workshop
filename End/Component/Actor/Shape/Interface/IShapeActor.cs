namespace Game.Actor.Shape.Interface
{
    using Dapr.Actors;
    using System.Threading.Tasks;

    public interface IShapeActor : IActor
    {
        /// <summary>
        /// Gets the current shape position.
        /// </summary>
        /// <returns>The shape with the current position.</returns>
        Task<Shape> RetrievePosition();

        /// <summary>
        /// Moves the current shape to a new position.
        /// </summary>
        /// <returns>Async task.</returns>
        Task MoveToNewPosition();
    }
}

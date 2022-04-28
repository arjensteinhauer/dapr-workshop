using System;
using System.Threading.Tasks;

namespace Game.Actor.Shape.Interface
{
    /// <summary>
    /// Describes the shape location updated event.
    /// </summary>
    public interface IUpdatedShapeLocationEvent
    {
        /// <summary>
        /// Event fired when the shape location has been updated.
        /// </summary>
        /// <param name="shapeId">The shape ID</param>
        /// <returns>Async task.</returns>
        Task OnUpdatedShapeLocation(Guid shapeId);
    }
}

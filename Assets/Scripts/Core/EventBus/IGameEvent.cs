namespace Rapadura.Core.Events
{
    /// <summary>
    /// Marker interface implemented by every event dispatched through the EventBus.
    /// Keeping events as plain structs/classes decouples publishers from subscribers.
    /// </summary>
    public interface IGameEvent
    {
    }
}

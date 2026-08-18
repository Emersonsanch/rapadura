using System;
using System.Collections.Generic;

namespace Rapadura.Core.EventBus
{
    /// <summary>
    /// Static, type-safe publish/subscribe event bus used to decouple game systems.
    /// Any system can raise an event without knowing who (if anyone) is listening,
    /// and any system can listen for an event without knowing who raises it.
    /// This is the backbone that will later let networked systems mirror local
    /// events without touching gameplay code.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Subscribers = new Dictionary<Type, Delegate>();

        /// <summary>Subscribes a handler to a specific event type.</summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            Type eventType = typeof(TEvent);

            if (Subscribers.TryGetValue(eventType, out Delegate existing))
            {
                Subscribers[eventType] = Delegate.Combine(existing, handler);
            }
            else
            {
                Subscribers[eventType] = handler;
            }
        }

        /// <summary>Unsubscribes a previously registered handler. Always call this on destroy/disable to avoid leaks.</summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            Type eventType = typeof(TEvent);

            if (!Subscribers.TryGetValue(eventType, out Delegate existing))
            {
                return;
            }

            Delegate result = Delegate.Remove(existing, handler);

            if (result == null)
            {
                Subscribers.Remove(eventType);
            }
            else
            {
                Subscribers[eventType] = result;
            }
        }

        /// <summary>Publishes an event to every subscriber registered for its type.</summary>
        public static void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
        {
            Type eventType = typeof(TEvent);

            if (Subscribers.TryGetValue(eventType, out Delegate existing))
            {
                (existing as Action<TEvent>)?.Invoke(gameEvent);
            }
        }

        /// <summary>Clears every subscription. Intended for scene reloads / tests only.</summary>
        public static void Clear()
        {
            Subscribers.Clear();
        }
    }
}

namespace GameCore.Core;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private static readonly object _lock = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        lock (_lock)
        {
            var type = typeof (T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }
    }

    public static void Publish<T>(T evt)
    {
        List<Delegate>? handlersCopy;

        lock (_lock)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list))
                return;

            handlersCopy = list.ToList();
        }

        foreach (var handler in handlersCopy)
        {
            ((Action<T>)handler)(evt);
        }
    }
}
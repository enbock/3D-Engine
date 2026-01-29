namespace Application.Container;

public class ServiceContainer
{
    private readonly Dictionary<Type, object> _services = new();

    public void Register<TInterface, TImplementation>()
        where TImplementation : TInterface, new()
    {
        _services[typeof(TInterface)] = new TImplementation();
    }

    public void RegisterInstance<TInterface>(TInterface instance)
        where TInterface : notnull
    {
        _services[typeof(TInterface)] = instance;
    }

    public TInterface Resolve<TInterface>()
    {
        if (_services.TryGetValue(typeof(TInterface), out object? service)) return (TInterface)service;
        throw new InvalidOperationException($"Service {typeof(TInterface).Name} not registered");
    }

    public bool TryResolve<TInterface>(out TInterface? service)
    {
        if (_services.TryGetValue(typeof(TInterface), out object? obj))
        {
            service = (TInterface)obj;
            return true;
        }

        service = default;
        return false;
    }

    public void Clear()
    {
        foreach (object service in _services.Values)
            if (service is IDisposable disposable)
                disposable.Dispose();

        _services.Clear();
    }
}
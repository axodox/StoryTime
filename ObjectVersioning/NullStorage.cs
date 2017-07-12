using System;
using ObjectVersioning.Actions;
using System.Collections.Concurrent;

namespace ObjectVersioning
{
  class NullStorage : IHistoryStorage
  {
    public static IHistoryStorage Instance { get; } = new NullStorage();

    private ConcurrentDictionary<Guid, object> _objects = new ConcurrentDictionary<Guid, object>();

    private NullStorage() { }

    public void RecordAction(EditAction editAction)
    {

    }

    public bool RegisterObject(VersionedValue value)
    {
      return _objects.TryAdd(value.Id, value);
    }

    public object ResolveObject(Guid id)
    {
      return _objects.TryGetValue(id, out var value) ? value : null;
    }

    public bool UnregisterObject(VersionedValue value)
    {
      return _objects.TryRemove(value.Id, out _);
    }
  }
}

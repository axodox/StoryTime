using ObjectVersioning.Actions;
using System;
using System.Collections.Generic;

namespace ObjectVersioning
{
  public abstract class VersionedValue
  {
    public Guid Id { get; } = Guid.NewGuid();

    private readonly List<Guid> _references = new List<Guid>();

    private readonly VersionedReference _reference;
    
    protected readonly HistoryStorage _historyStorage;

    public VersionedValue(HistoryStorage historyStorage)
    {
      _historyStorage = historyStorage ?? throw new ArgumentNullException(nameof(historyStorage));
      _reference = new VersionedReference(Id);
    }

    public VersionedReference AddReference(VersionedValue owner)
    {
      _references.Add(owner.Id);
      return _reference;
    }

    public void RemoveReference(VersionedValue owner)
    {
      _references.Remove(owner.Id);
    }
  }
}
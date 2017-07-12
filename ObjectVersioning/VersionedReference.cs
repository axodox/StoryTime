using Newtonsoft.Json;
using System;

namespace ObjectVersioning
{
  public class VersionedReference : VersionedValue
  {
    public Guid TargetId { get; }

    private object _reference;
    public object Reference
    {
      get
      {
        return _reference ?? (_reference = HistoryStorage.ResolveObject(TargetId));
      }
    }

    [JsonConstructor]
    public VersionedReference(Guid id, Guid targetId)
      : base(id)
    {
      TargetId = targetId;
    }

    public VersionedReference(object value)
      : base((value as VersionedValue).Id)
    {
      _reference = value;
    }

    public VersionedReference(IHistoryStorage historyStorage, Guid targetId)
      : base(historyStorage)
    {
      TargetId = targetId;
    }
  }
}

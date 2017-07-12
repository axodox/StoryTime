using Newtonsoft.Json;
using System;

namespace ObjectVersioning
{
  public class VersionedReference : VersionedValue
  {
    public Guid TargetId { get; }

    private object _reference;
    [JsonIgnore]
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

    public VersionedReference(IHistoryStorage historyStorage, Guid targetId)
      : base(historyStorage)
    {
      TargetId = targetId;
    }

    public static VersionedReference FromValue(object value)
    {
      switch(value)
      {
        case VersionedReference versionedReference:
          return versionedReference;
        case VersionedValue versionedValue:
          return new VersionedReference(versionedValue.HistoryStorage, versionedValue.Id)
          {
            _reference = versionedValue
          };
        default:
          return null;
      }
    }
  }
}

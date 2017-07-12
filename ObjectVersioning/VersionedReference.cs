using Newtonsoft.Json;
using System;

namespace ObjectVersioning
{
  public class VersionedReference : VersionedValue
  {
    public Guid TargetId { get; }

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
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectVersioning
{
  public class VersionedReference : VersionedValue
  {
    public Guid TargetId { get; }

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

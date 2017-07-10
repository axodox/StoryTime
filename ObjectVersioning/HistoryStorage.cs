using ObjectVersioning.Actions;
using System;

namespace ObjectVersioning
{
  public abstract class HistoryStorage
  {
    public abstract void Add(EditAction editAction);

    public abstract VersionedValue ResolveReference(Guid targetId);
  }
}

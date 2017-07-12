using ObjectVersioning.Actions;
using System;

namespace ObjectVersioning
{
  public interface IHistoryStorage
  {
    void RecordAction(EditAction editAction);

    bool RegisterObject(VersionedValue value);

    object ResolveObject(Guid id);

    bool UnregisterObject(VersionedValue value);
  }
}

using ObjectVersioning.Actions;
using System;

namespace ObjectVersioning
{
  public interface IHistoryStorage
  {
    void RecordAction(EditAction editAction);
  }
}

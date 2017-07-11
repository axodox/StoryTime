using ObjectVersioning.Actions;
using System;
using System.Collections.Generic;

namespace ObjectVersioning
{
  public class VersionedObject : VersionedValue
  {
    public VersionedObject(Guid id)
      : base(id)
    {

    }

    public VersionedObject(IHistoryStorage historyStorage)
      : base(historyStorage)
    {

    }

    protected void RecordSetPropertyActionAction(string name, object value)
    {
      var action = new SetPropertyAction(Id, name, value);
      HistoryStorage.RecordAction(action);
    }
  }
}
using Newtonsoft.Json;
using ObjectVersioning.Actions;
using System;

namespace ObjectVersioning
{
  public class VersionedObject : VersionedValue
  {
    [JsonConstructor]
    public VersionedObject(Guid id)
      : base(id)
    {

    }

    public VersionedObject(IHistoryStorage historyStorage)
      : base(historyStorage)
    {

    }

    protected void RecordSetPropertyAction(string name, object value)
    {
      var action = new SetPropertyAction(Id, name, value);
      HistoryStorage.RecordAction(action);
    }
  }
}
using Newtonsoft.Json;
using ObjectVersioning.Actions;
using System;

namespace ObjectVersioning
{
  public abstract class VersionedValue
  {
    [JsonIgnore]
    public IHistoryStorage HistoryStorage { get; private set; }

    public Guid Id { get; }    

    [JsonConstructor]
    public VersionedValue(Guid id)
      : this(NullStorage.Instance, id) { }

    public VersionedValue(IHistoryStorage historyStorage) 
      : this(historyStorage, Guid.NewGuid()) { }

    public VersionedValue(IHistoryStorage historyStorage, Guid id)
    {
      HistoryStorage = historyStorage ?? throw new ArgumentNullException(nameof(historyStorage));
      Id = id;

      HistoryStorage.RegisterObject(this);
      RecordNewObjectAction();
    }

    private void RecordNewObjectAction()
    {
      var action = new NewObjectAction(Id, GetType().FullName);
      HistoryStorage.RecordAction(action);
    }
  }
}
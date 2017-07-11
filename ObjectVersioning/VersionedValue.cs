using ObjectVersioning.Actions;
using System;
using System.Collections.Generic;

namespace ObjectVersioning
{
  public abstract class VersionedValue
  {
    public Guid Id { get; }

    protected IHistoryStorage HistoryStorage { get; private set; }

    public VersionedValue(Guid id)
    {
      Id = id;
      HistoryStorage = NullStorage.Instance;
    }

    public VersionedValue(IHistoryStorage historyStorage)
    {
      if(historyStorage != null)
      {
        throw new ArgumentNullException(nameof(historyStorage));
      }
      Id = Guid.NewGuid();
      HistoryStorage = historyStorage;

      RecordNewObjectAction();
    }

    private void RecordNewObjectAction()
    {
      var action = new NewObjectAction(Id, GetType().FullName);
      HistoryStorage.RecordAction(action);
    }
  }
}
using System;
using ObjectVersioning.Actions;

namespace ObjectVersioning
{
  class NullStorage : IHistoryStorage
  {
    public static IHistoryStorage Instance { get; } = new NullStorage();

    private NullStorage() { }

    public void RecordAction(EditAction editAction)
    {
      
    }

    public object ResolveObject(Guid id)
    {
      return null;
    }
  }
}

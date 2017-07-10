using ObjectVersioning.Actions;
using System;
using System.Collections.Generic;

namespace ObjectVersioning
{
  public class VersionedObject : VersionedValue
  {
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

    public object this[string propertyName]
    {
      get
      {
        if(_properties.TryGetValue(propertyName, out var propertyValue))
        {
          if (propertyValue is VersionedReference versionedReference)
          {
            return _historyStorage.ResolveReference(versionedReference.TargetId);
          }
          else
          {
            return propertyValue;
          }
        }
        else
        {
          return null;
        }
      }
      set
      {
        if(_properties[propertyName] is VersionedReference versionedReference)
        {
          _historyStorage.ResolveReference(versionedReference.TargetId)?.RemoveReference(this);
        }

        var propertyValue = value is VersionedValue versionedValue ? versionedValue.AddReference(this) : value;
        _historyStorage.Add(new PropertySetAction(Id, propertyName, propertyValue));
        _properties[propertyName] = propertyValue;
      }
    }

    public VersionedObject(HistoryStorage historyStorage)
      : base(historyStorage)
    {

    }
  }
}
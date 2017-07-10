using System;

namespace ObjectVersioning.Actions
{
  public class PropertySetAction : EditAction
  {
    public string PropertyName { get; }

    public object PropertyValue { get; }

    public PropertySetAction(Guid targetId, string propertyName, object propertyValue)
      : base(targetId)
    {
      PropertyName = propertyName;
      PropertyValue = propertyValue;
    }
  }
}

using System;

namespace ObjectVersioning.Actions
{
  public class SetPropertyAction : EditAction
  {
    public string PropertyName { get; }

    public object PropertyValue { get; }

    public SetPropertyAction(Guid targetId, string propertyName, object propertyValue)
      : base(targetId)
    {
      PropertyName = propertyName;
      PropertyValue = propertyValue;
    }
  }
}

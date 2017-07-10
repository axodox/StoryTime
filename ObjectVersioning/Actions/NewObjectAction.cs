using System;

namespace ObjectVersioning.Actions
{
  public class NewObjectAction : EditAction
  {
    public string Type { get; }

    public NewObjectAction(Guid targetId, string type)
      : base(targetId)
    {
      Type = type;
    }
  }
}

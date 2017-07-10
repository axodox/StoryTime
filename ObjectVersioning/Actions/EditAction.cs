using System;

namespace ObjectVersioning.Actions
{
  public abstract class EditAction
  {
    public Guid TargetId { get; }

    public EditAction(Guid targetId)
    {
      TargetId = targetId;
    }
  }
}

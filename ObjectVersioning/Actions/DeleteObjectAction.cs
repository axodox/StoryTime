using System;

namespace ObjectVersioning.Actions
{
  public class DeleteObjectAction : EditAction
  {
    public DeleteObjectAction(Guid targetId)
      : base(targetId)
    {

    }
  }
}

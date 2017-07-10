using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectVersioning
{
  public class VersionedReference
  {
    public Guid TargetId { get; }

    public VersionedReference(Guid targetId)
    {
      TargetId = targetId;
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace StoryTime.Data
{
  public interface IStory
  {
    string Title { get; set; }

    IReferences References { get; }

    IList<IAct> Acts { get; }
  }
}

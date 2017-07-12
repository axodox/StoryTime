using System.Collections.Generic;

namespace StoryTime.Data
{
  public interface IStory
  {
    string Title { get; set; }

    IReferences References { get; }

    IList<IAct> Acts { get; }
  }
}

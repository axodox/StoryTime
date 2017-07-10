using System.Collections.Generic;

namespace StoryTime.Data
{
  public interface IAct
  {
    string Title { get; set; }

    IList<IChapter> Chapters { get; }
  }
}
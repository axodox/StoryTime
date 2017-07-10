using System.Collections.Generic;

namespace StoryTime.Data
{
  public interface IChapter
  {
    string Title { get; set; }

    IList<IScene> Scenes { get; set; }
  }
}
using System.Collections.Generic;

namespace StoryTime.Data
{
  public interface IScene
  {
    string Summary { get; set; }

    IPlace Place { get; set; }

    IList<ICharacter> Characters { get; }
  }
}
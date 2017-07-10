using System.Collections.Generic;

namespace StoryTime.Data
{
  public interface IReferences
  {
    IList<ICharacter> Characters { get; set; }

    IList<IPlace> Places { get; set; }
  }
}
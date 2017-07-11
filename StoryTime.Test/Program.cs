using Newtonsoft.Json;
using ObjectVersioning;
using StoryTime.Data;
using System;

namespace StoryTime.Test
{
  class Program
  {
    class Test
    {
      public string Name { get; }

      public Test(string name = null)
      {
        Name = name;
      }
    }

    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      var test = JsonConvert.DeserializeObject<Test>("{\"Name\" : \"test\"}");

      var type = VersionedType.Get<ITestEntity>();

      var value = Activator.CreateInstance(type, Guid.NewGuid());
    }
  }

  public interface ITestEntity
  {
    string Name { get; set; }

    int Number { get; set; }
  }

  class Character : VersionedObject, ITestEntity
  {
    public Character(Guid id) : base(id)
    {
    }

    public Character(IHistoryStorage historyStorage) : base(historyStorage)
    {
    }

    private string _name;
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        if (Equals(value, _name)) return;
        _name = value;
        RecordSetPropertyActionAction(nameof(Name), value);
      }
    }

    private int _number;
    public int Number
    {
      get
      {
        return _number;
      }
      set
      {
        if (Equals(value, _number)) return;
        _number = value;
        RecordSetPropertyActionAction(nameof(Number), value);
      }
    }
  }
}
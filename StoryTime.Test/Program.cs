using ObjectVersioning;
using System;
using System.Diagnostics;

namespace StoryTime.Test
{
  class Program
  {
    static void Main(string[] args)
    {
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      ITestEntity testEntity = CreateTestEntity();

      stopwatch.Stop();
      Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
      Console.ReadLine();

      var text = VersionedType.Serialize(testEntity);
      var entity = VersionedType.Deserialize<ITestEntity>(text);
    }

    private static ITestEntity CreateTestEntity()
    {
      var testEntity = VersionedType.New<ITestEntity>();
      testEntity.Name = "sad";
      testEntity.Number = 4;

      var childEntity = VersionedType.New<ITestEntity>();
      childEntity.Name = "child";
      childEntity.Number = 1;
      testEntity.Child = childEntity;
      return testEntity;
    }
  }

  public interface ITestEntity
  {
    string Name { get; set; }

    int Number { get; set; }

    ITestEntity Child { get; set; }
  }

  class TestEntity : VersionedObject, ITestEntity
  {
    public TestEntity(Guid id) : base(id)
    {
    }

    public TestEntity(IHistoryStorage historyStorage) : base(historyStorage)
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
        RecordSetPropertyAction(nameof(Name), value);
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
        RecordSetPropertyAction(nameof(Number), value);
      }
    }

    private TestEntity _child;
    public TestEntity Child
    {
      get
      {
        return _child;
      }
      set
      {
        if (Equals(value, _child)) return;
        _child = value;
        RecordSetPropertyAction(nameof(Child), value);
      }
    }

    ITestEntity ITestEntity.Child
    {
      get
      {
        return Child;
      }
      set
      {
        Child = value as TestEntity;
      }
    }
  }
}
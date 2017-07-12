using ObjectVersioning.Actions;

namespace ObjectVersioning
{
  public interface IHistoryStorage
  {
    void RecordAction(EditAction editAction);
  }
}

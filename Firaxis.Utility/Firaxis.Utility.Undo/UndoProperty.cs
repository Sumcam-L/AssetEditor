namespace Firaxis.Utility.Undo;

[UndoStyle(UndoStyle.Property)]
public class UndoProperty : IUndo
{
	private object obj;

	private string field;

	private string oldValue;

	private string newValue;

	public UndoProperty(object obj, string field, object oldValue)
	{
		this.obj = obj;
		this.field = field;
		this.oldValue = Transpose.ToString(oldValue);
		newValue = string.Empty;
	}

	public void PerformUndo()
	{
		UndoHelper.ApplyValue(obj, field, oldValue);
	}

	public void PerformRedo()
	{
		UndoHelper.ApplyValue(obj, field, newValue);
	}

	public void StoreUndo()
	{
	}

	public void StoreRedo()
	{
		newValue = UndoHelper.AcquireValue(obj, field);
	}
}

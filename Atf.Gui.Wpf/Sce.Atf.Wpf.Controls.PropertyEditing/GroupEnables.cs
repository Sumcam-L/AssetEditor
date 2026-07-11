namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class GroupEnables
{
	public string GroupName { get; private set; }

	public string[] StringValues { get; private set; }

	public object[] Values { get; set; }

	public GroupEnables(string groupName, string[] stringValues)
	{
		GroupName = groupName;
		StringValues = stringValues;
	}
}

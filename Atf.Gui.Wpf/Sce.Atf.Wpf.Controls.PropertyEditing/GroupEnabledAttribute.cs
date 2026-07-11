using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class GroupEnabledAttribute : Attribute
{
	public readonly GroupEnables[] GroupEnables;

	public GroupEnabledAttribute(GroupEnables[] enables)
	{
		GroupEnables = enables;
	}

	public GroupEnabledAttribute(string groupName, string stringValues)
	{
		string[] stringValues2 = stringValues.Split(',');
		GroupEnables = new GroupEnables[1]
		{
			new GroupEnables(groupName, stringValues2)
		};
	}
}

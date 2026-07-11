using System;

namespace Sce.Atf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property)]
public class IgnoreChildrenAttribute : Attribute
{
	public bool IgnoreChildren { get; }

	public IgnoreChildrenAttribute(bool ignoreChildren = true)
	{
		IgnoreChildren = ignoreChildren;
	}
}

using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property)]
public class ImageListAttribute : Attribute
{
	public object[] ImageKeys { get; set; }

	public ImageListAttribute(object[] imageKeys)
	{
		ImageKeys = imageKeys;
	}
}

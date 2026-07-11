using System;

namespace Sce.Atf.Wpf;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
[Obsolete("Please use Sce.Atf.ImageResourceAttribute instead.")]
public sealed class WpfImageResourceAttribute : Attribute
{
	public string ImageName { get; private set; }

	public WpfImageResourceAttribute(string imageName)
	{
		ImageName = imageName;
	}
}

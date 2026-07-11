using System;

namespace Sce.Atf;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ImageResourceAttribute : Attribute
{
	private readonly string m_imageName1;

	private readonly string m_imageName2;

	private readonly string m_imageName3;

	public string ImageName1 => m_imageName1;

	public string ImageName2 => m_imageName2;

	public string ImageName3 => m_imageName3;

	public ImageResourceAttribute(string imageName)
		: this(imageName, null, null)
	{
	}

	public ImageResourceAttribute(string imageName16, string imageName24, string imageName32)
	{
		m_imageName1 = imageName16;
		m_imageName2 = imageName24;
		m_imageName3 = imageName32;
	}
}

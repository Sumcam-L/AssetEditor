using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class EmbeddedImage : IDisposable
{
	private readonly string m_id;

	private readonly bool m_isOwner;

	public Image Image { get; private set; }

	public EmbeddedImage(string id, string assemblyName, string pathToImage)
	{
		m_isOwner = true;
		m_id = id;
		Assembly assembly = Assembly.LoadFrom(Application.StartupPath + Path.DirectorySeparatorChar + assemblyName);
		Image = GdiUtil.GetImage(assembly, pathToImage);
		ResourceUtil.RegisterImage(m_id, Image);
	}

	public EmbeddedImage(string id, ImageSizes size)
	{
		m_isOwner = false;
		m_id = id;
		switch (size)
		{
		case ImageSizes.e16x16:
			Image = ResourceUtil.GetImage16(id);
			break;
		case ImageSizes.e24x24:
			Image = ResourceUtil.GetImage24(id);
			break;
		case ImageSizes.e32x32:
			Image = ResourceUtil.GetImage32(id);
			break;
		default:
			throw new ArgumentException("Invalid size specified");
		}
		if (Image == null)
		{
			throw new ArgumentException("No image registered with this id and size");
		}
	}

	public virtual void Dispose()
	{
		if (m_isOwner && Image != null)
		{
			Image.Dispose();
		}
	}

	public void Reregister()
	{
		ResourceUtil.RegisterImage(m_id, Image);
	}
}

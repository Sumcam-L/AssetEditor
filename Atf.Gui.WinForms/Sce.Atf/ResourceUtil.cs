using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf;

public static class ResourceUtil
{
	private static readonly Dictionary<string, Image> s_images;

	private static readonly ImageList s_images13;

	private static readonly ImageList s_images16;

	private static readonly ImageList s_images24;

	private static readonly ImageList s_images32;

	private static readonly Dictionary<string, Icon> s_icons;

	private static readonly Dictionary<string, Cursor> s_cursors;

	public static bool RegistrationStarted { get; set; }

	public static void RegisterImage(string id, Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (image.Width == 13 && image.Height == 13)
		{
			int num = s_images13.Images.IndexOfKey(id);
			if (num == -1)
			{
				s_images13.Images.Add(id, image);
			}
			else
			{
				s_images13.Images[num] = image;
			}
		}
		else if (image.Width == 16 && image.Height == 16)
		{
			int num2 = s_images16.Images.IndexOfKey(id);
			if (num2 == -1)
			{
				s_images16.Images.Add(id, image);
			}
			else
			{
				s_images16.Images[num2] = image;
			}
		}
		else if (image.Width == 24 && image.Height == 24)
		{
			int num3 = s_images24.Images.IndexOfKey(id);
			if (num3 == -1)
			{
				s_images24.Images.Add(id, image);
			}
			else
			{
				s_images24.Images[num3] = image;
			}
		}
		else if (image.Width == 32 && image.Height == 32)
		{
			int num4 = s_images32.Images.IndexOfKey(id);
			if (num4 == -1)
			{
				s_images32.Images.Add(id, image);
			}
			else
			{
				s_images32.Images[num4] = image;
			}
		}
		s_images[id] = image;
	}

	public static void RegisterImage(string id, Image image16, Image image24, Image image32)
	{
		Image image33 = null;
		if (image16 != null)
		{
			image16 = GdiUtil.ResizeImage(image16, 16);
			s_images16.Images.Add(id, image16);
			image33 = image16;
		}
		if (image24 != null)
		{
			image24 = GdiUtil.ResizeImage(image24, 24);
			s_images24.Images.Add(id, image24);
			image33 = image24;
		}
		if (image32 != null)
		{
			image32 = GdiUtil.ResizeImage(image32, 32);
			s_images32.Images.Add(id, image32);
			image33 = image32;
		}
		if (image33 == null)
		{
			throw new ArgumentNullException("at least one image must be non-null");
		}
		s_images[id] = image33;
	}

	public static Image GetImage(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");
		}
		s_images.TryGetValue(id, out var value);
		return value;
	}

	public static Image GetImage13(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");
		}
		Image image = s_images13.Images[id];
		if (image == null)
		{
			image = GetImage(id);
			if (image != null)
			{
				image = GdiUtil.ResizeImage(image, 13);
			}
		}
		return image;
	}

	public static Image GetImage16(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");
		}
		Image image = s_images16.Images[id];
		if (image == null)
		{
			image = GetImage(id);
			if (image != null)
			{
				image = GdiUtil.ResizeImage(image, 16);
			}
		}
		return image;
	}

	public static Image GetImage24(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");
		}
		Image image = s_images24.Images[id];
		if (image == null)
		{
			image = GetImage(id);
			if (image != null)
			{
				image = GdiUtil.ResizeImage(image, 24);
			}
		}
		return image;
	}

	public static Image GetImage32(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");
		}
		Image image = s_images32.Images[id];
		if (image == null)
		{
			image = GetImage(id);
			if (image != null)
			{
				image = GdiUtil.ResizeImage(image, 32);
			}
		}
		return image;
	}

	public static ImageList GetImageList13()
	{
		return s_images13;
	}

	public static ImageList GetImageList16()
	{
		return s_images16;
	}

	public static ImageList GetImageList24()
	{
		return s_images24;
	}

	public static ImageList GetImageList32()
	{
		return s_images32;
	}

	public static Cursor GetCursor(string id)
	{
		s_cursors.TryGetValue(id, out var value);
		return value;
	}

	public static Icon GetIcon(string id)
	{
		s_icons.TryGetValue(id, out var value);
		return value;
	}

	public static void Register(Type type)
	{
		string resourcePath = type.FullName + ".";
		Register(type, resourcePath);
	}

	public static void Register(Type type, string resourcePath)
	{
		Assembly assembly = type.Assembly;
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(ImageResourceAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				ImageResourceAttribute imageResourceAttribute = customAttributes[0] as ImageResourceAttribute;
				string text = resourcePath + imageResourceAttribute.ImageName1;
				fieldInfo.SetValue(null, text);
				string name = ((imageResourceAttribute.ImageName2 != null) ? (resourcePath + imageResourceAttribute.ImageName2) : null);
				string name2 = ((imageResourceAttribute.ImageName3 != null) ? (resourcePath + imageResourceAttribute.ImageName3) : null);
				RegisterImage(assembly, text, name, name2);
				continue;
			}
			customAttributes = fieldInfo.GetCustomAttributes(typeof(IconResourceAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				IconResourceAttribute iconResourceAttribute = customAttributes[0] as IconResourceAttribute;
				string text2 = resourcePath + iconResourceAttribute.IconName;
				fieldInfo.SetValue(null, text2);
				RegisterIcon(assembly, text2);
				continue;
			}
			customAttributes = fieldInfo.GetCustomAttributes(typeof(CursorResourceAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				CursorResourceAttribute cursorResourceAttribute = customAttributes[0] as CursorResourceAttribute;
				string text3 = resourcePath + cursorResourceAttribute.CursorName;
				fieldInfo.SetValue(null, text3);
				RegisterCursor(assembly, text3);
			}
		}
	}

	private static void RegisterCursor(Assembly resourceAssembly, string name)
	{
		Cursor cursor = null;
		Stream stream = null;
		try
		{
			stream = resourceAssembly.GetManifestResourceStream(name);
			cursor = new Cursor(stream);
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
		}
		finally
		{
			stream?.Close();
		}
		if (cursor != null && !s_cursors.ContainsKey(name))
		{
			s_cursors.Add(name, cursor);
		}
	}

	private static void RegisterIcon(Assembly resourceAssembly, string name)
	{
		Icon icon = null;
		Stream stream = null;
		try
		{
			stream = resourceAssembly.GetManifestResourceStream(name);
			icon = new Icon(stream);
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
		}
		finally
		{
			stream?.Close();
		}
		if (icon == null)
		{
			throw new ArgumentNullException("Icon must be non-null");
		}
		if (!s_icons.ContainsKey(name))
		{
			s_icons.Add(name, icon);
			RegisterImage(name, icon.ToBitmap());
		}
	}

	private static void RegisterImage(Assembly resourceAssembly, string name1, string name2, string name3)
	{
		if (name1 != null)
		{
			if (name2 == null || name3 == null)
			{
				RegisterImage(name1, GdiUtil.GetImage(resourceAssembly, name1));
			}
			else
			{
				RegisterImage(name1, GdiUtil.GetImage(resourceAssembly, name1), GdiUtil.GetImage(resourceAssembly, name2), GdiUtil.GetImage(resourceAssembly, name3));
			}
		}
	}

	static ResourceUtil()
	{
		s_images = new Dictionary<string, Image>();
		s_images13 = new ImageList();
		s_images16 = new ImageList();
		s_images24 = new ImageList();
		s_images32 = new ImageList();
		s_icons = new Dictionary<string, Icon>();
		s_cursors = new Dictionary<string, Cursor>();
		s_images13.ColorDepth = ColorDepth.Depth32Bit;
		s_images13.TransparentColor = Color.Transparent;
		s_images13.ImageSize = new Size(13, 13);
		s_images16.ColorDepth = ColorDepth.Depth32Bit;
		s_images16.TransparentColor = Color.Transparent;
		s_images16.ImageSize = new Size(16, 16);
		s_images24.ColorDepth = ColorDepth.Depth32Bit;
		s_images24.TransparentColor = Color.Transparent;
		s_images24.ImageSize = new Size(24, 24);
		s_images32.ColorDepth = ColorDepth.Depth32Bit;
		s_images32.TransparentColor = Color.Transparent;
		s_images32.ImageSize = new Size(32, 32);
		Register(typeof(Resources));
	}
}

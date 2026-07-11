using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Sce.Atf.Wpf;

public static class ResourceUtil
{
	private static readonly Dictionary<string, ResourceKey> s_resourceKeys = new Dictionary<string, ResourceKey>();

	private static readonly Dictionary<Type, Type> s_registeredTypes = new Dictionary<Type, Type>();

	public static bool RegistrationStarted { get; set; }

	public static void Register(Type owningType)
	{
		Register(owningType, "Resources/");
	}

	public static void Register(Type owningType, string subPath)
	{
		if (s_registeredTypes.ContainsKey(owningType))
		{
			throw new Exception("Cannot register the resources of the same type twice");
		}
		s_registeredTypes[owningType] = owningType;
		if (string.IsNullOrEmpty(subPath))
		{
			throw new ArgumentNullException("subPath");
		}
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		FieldInfo[] fields = owningType.GetFields(BindingFlags.Static | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo field in array)
		{
			if (TryGetImageResource(owningType, field, resourceDictionary, subPath) || TryGetResourceDictionaryResource(owningType, field))
			{
			}
		}
		if (resourceDictionary.Count > 0)
		{
			Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
		}
	}

	public static object GetKeyFromImageName(string imageName)
	{
		if (s_resourceKeys.ContainsKey(imageName))
		{
			return s_resourceKeys[imageName];
		}
		return imageName;
	}

	public static string GetAssemblyAttribute<T>(this Assembly assembly, Func<T, string> value) where T : Attribute
	{
		T arg = (T)Attribute.GetCustomAttribute(assembly, typeof(T));
		return value(arg);
	}

	public static ImageSource GetImage(string name)
	{
		return string.IsNullOrEmpty(name) ? null : (Application.Current.Resources[name] as ImageSource);
	}

	public static BitmapImage ConvertWinFormsImage(Image winFormsImage)
	{
		if (winFormsImage == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		winFormsImage.Save(memoryStream, ImageFormat.Bmp);
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
		bitmapImage.EndInit();
		return bitmapImage;
	}

	private static bool TryGetImageResource(Type owningType, FieldInfo field, ResourceDictionary imageResources, string subPath)
	{
		if (string.IsNullOrEmpty(subPath))
		{
			throw new ArgumentNullException("subPath");
		}
		object[] customAttributes = field.GetCustomAttributes(typeof(ImageResourceAttribute), inherit: false);
		if (customAttributes.Length == 0)
		{
			return false;
		}
		ImageResourceAttribute imageResourceAttribute = (ImageResourceAttribute)customAttributes[0];
		Freezable freezable = null;
		if (string.IsNullOrEmpty(imageResourceAttribute.ImageName1))
		{
			throw new InvalidOperationException("Cannot have an image attribute whose first image name is null or empty");
		}
		string extension = Path.GetExtension(imageResourceAttribute.ImageName1);
		if (string.IsNullOrEmpty(extension))
		{
			throw new InvalidOperationException("Cannot have an image attribute whose first image name has no extension");
		}
		extension = extension.ToLower();
		AssemblyName name = owningType.Assembly.GetName();
		string text = string.Concat("/", name, ";component/", subPath);
		if (!text.EndsWith("/"))
		{
			text += "/";
		}
		switch (extension)
		{
		case ".xaml":
		{
			Uri resourceLocator = new Uri(text + imageResourceAttribute.ImageName1, UriKind.Relative);
			freezable = Application.LoadComponent(resourceLocator) as Freezable;
			if (freezable == null)
			{
				throw new InvalidOperationException("Invalid xaml image resource: " + imageResourceAttribute.ImageName1);
			}
			break;
		}
		case ".cur":
		{
			Uri uriResource = new Uri(text + imageResourceAttribute.ImageName1, UriKind.Relative);
			try
			{
				StreamResourceInfo resourceStream = Application.GetResourceStream(uriResource);
				freezable = new FreezableCursor
				{
					Cursor = new Cursor(resourceStream.Stream)
				};
			}
			catch (IOException)
			{
			}
			break;
		}
		default:
			if (!(extension == ".ico"))
			{
				throw new Exception("Unrecognized extension '" + extension + "' on image resource '" + field.Name + "'");
			}
			goto case ".bmp";
		case ".bmp":
		case ".png":
		{
			Stream manifestResourceStream = owningType.Assembly.GetManifestResourceStream(string.Concat(owningType, ".", imageResourceAttribute.ImageName1));
			if (manifestResourceStream != null)
			{
				freezable = BitmapFrame.Create(manifestResourceStream, BitmapCreateOptions.None, BitmapCacheOption.None);
				break;
			}
			string text2 = "pack://application:,,," + text;
			Uri uriSource = new Uri(text2 + imageResourceAttribute.ImageName1);
			try
			{
				freezable = new BitmapImage(uriSource);
			}
			catch (IOException)
			{
			}
			break;
		}
		}
		if (freezable == null)
		{
			throw new Exception("Failed to create image from image resource '" + field.Name + "'");
		}
		if (freezable is DrawingGroup)
		{
			DrawingBrush drawingBrush = new DrawingBrush(freezable as DrawingGroup);
			drawingBrush.Stretch = Stretch.Uniform;
			freezable = drawingBrush;
		}
		if (freezable.CanFreeze && !freezable.IsFrozen)
		{
			freezable.Freeze();
		}
		object obj = owningType.FullName + "." + imageResourceAttribute.ImageName1;
		if (field.FieldType.IsAssignableFrom(typeof(ResourceKey)))
		{
			ComponentResourceKey componentResourceKey = new ComponentResourceKey(owningType, obj);
			s_resourceKeys[imageResourceAttribute.ImageName1] = componentResourceKey;
			obj = componentResourceKey;
		}
		field.SetValue(owningType, obj);
		if (!imageResources.Contains(obj))
		{
			imageResources.Add(obj, freezable);
		}
		return true;
	}

	private static bool TryGetResourceDictionaryResource(Type owningType, FieldInfo field)
	{
		if (field.FieldType != typeof(string))
		{
			return false;
		}
		object[] customAttributes = field.GetCustomAttributes(typeof(ResourceDictionaryResourceAttribute), inherit: false);
		if (customAttributes.Length == 0)
		{
			return false;
		}
		ResourceDictionaryResourceAttribute resourceDictionaryResourceAttribute = customAttributes[0] as ResourceDictionaryResourceAttribute;
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		Assembly assembly = owningType.Assembly;
		Uri source = new Uri(string.Concat("pack://application:,,,/", assembly.GetName(), ";component/Resources/", resourceDictionaryResourceAttribute.Path));
		resourceDictionary.Source = source;
		Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Sce.Atf.Wpf;

[Obsolete("Please use Sce.Atf.Wpf.ResourceUtil instead.")]
public static class WpfResourceUtil
{
	private static readonly HashSet<Type> s_registeredTypes = new HashSet<Type>();

	public static string GetAssemblyAttribute<T>(this Assembly assembly, Func<T, string> value) where T : Attribute
	{
		T arg = (T)Attribute.GetCustomAttribute(assembly, typeof(T));
		return value(arg);
	}

	public static ImageSource GetImage(string name)
	{
		return string.IsNullOrEmpty(name) ? null : (Application.Current.Resources[name] as ImageSource);
	}

	public static void Register(Type type, string resourcePath)
	{
		if (s_registeredTypes.Contains(type))
		{
			return;
		}
		s_registeredTypes.Add(type);
		AssemblyName name = type.Assembly.GetName();
		string text = "/" + name.Name + ";component/" + resourcePath;
		string text2 = "pack://application:,,," + text;
		ResourceDictionary resourceDictionary = null;
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(WpfImageResourceAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				if (!fieldInfo.FieldType.IsAssignableFrom(typeof(ResourceKey)))
				{
				}
				WpfImageResourceAttribute wpfImageResourceAttribute = (WpfImageResourceAttribute)customAttributes[0];
				Freezable freezable = null;
				string text3 = Path.GetExtension(wpfImageResourceAttribute.ImageName).ToLower();
				if (text3 == ".bmp" || text3 == ".png" || text3 == ".ico")
				{
					Uri uriSource = new Uri(text2 + wpfImageResourceAttribute.ImageName);
					try
					{
						freezable = new BitmapImage(uriSource);
					}
					catch (IOException)
					{
					}
				}
				else if (text3 == ".xaml")
				{
					Uri resourceLocator = new Uri(text + wpfImageResourceAttribute.ImageName, UriKind.Relative);
					freezable = Application.LoadComponent(resourceLocator) as Freezable;
					if (freezable == null)
					{
						throw new InvalidOperationException("Invalid xaml image resource: " + wpfImageResourceAttribute.ImageName);
					}
				}
				else
				{
					if (!(text3 == ".cur"))
					{
						throw new InvalidOperationException("Unrecognized Wpf image resource file extension for file: " + wpfImageResourceAttribute.ImageName);
					}
					Uri uriResource = new Uri(text + wpfImageResourceAttribute.ImageName, UriKind.Relative);
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
				}
				if (freezable != null)
				{
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
					object obj = type.FullName + "." + wpfImageResourceAttribute.ImageName;
					if (fieldInfo.FieldType.IsAssignableFrom(typeof(ResourceKey)))
					{
						obj = new ComponentResourceKey(type, obj);
					}
					fieldInfo.SetValue(type, obj);
					if (resourceDictionary == null)
					{
						resourceDictionary = new ResourceDictionary();
					}
					resourceDictionary.Add(obj, freezable);
				}
			}
			if (fieldInfo.FieldType == typeof(string))
			{
				customAttributes = fieldInfo.GetCustomAttributes(typeof(ResourceDictionaryResourceAttribute), inherit: false);
				if (customAttributes.Length != 0)
				{
					ResourceDictionaryResourceAttribute resourceDictionaryResourceAttribute = customAttributes[0] as ResourceDictionaryResourceAttribute;
					ResourceDictionary resourceDictionary2 = new ResourceDictionary();
					string uriString = text2 + resourceDictionaryResourceAttribute.Path;
					resourceDictionary2.Source = new Uri(uriString, UriKind.RelativeOrAbsolute);
					Application.Current.Resources.MergedDictionaries.Add(resourceDictionary2);
				}
			}
		}
		if (resourceDictionary != null)
		{
			Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
		}
	}
}

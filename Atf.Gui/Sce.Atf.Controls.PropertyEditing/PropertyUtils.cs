using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Sce.Atf.Controls.PropertyEditing;

public static class PropertyUtils
{
	private static readonly Dictionary<DomNodeType, Dictionary<string, System.ComponentModel.PropertyDescriptor>> s_descriptorCache = new Dictionary<DomNodeType, Dictionary<string, System.ComponentModel.PropertyDescriptor>>();

	public static bool UseCustomTypeDescriptorsOnly { get; set; }

	public static event EventHandler<PropertyErrorEventArgs> PropertyError;

	public static event EventHandler<PropertyEditedEventArgs> PropertyEdited;

	public static PropertyDescriptorCollection GetDefaultProperties(object item)
	{
		ICustomTypeDescriptor customTypeDescriptor = item.As<ICustomTypeDescriptor>();
		if (customTypeDescriptor != null)
		{
			return customTypeDescriptor.GetProperties();
		}
		if (!UseCustomTypeDescriptorsOnly)
		{
			return TypeDescriptor.GetProperties(item);
		}
		return new PropertyDescriptorCollection(EmptyArray<System.ComponentModel.PropertyDescriptor>.Instance);
	}

	public static System.ComponentModel.PropertyDescriptor[] GetDefaultProperties2(object owner)
	{
		PropertyDescriptorCollection defaultProperties = GetDefaultProperties(owner);
		System.ComponentModel.PropertyDescriptor[] array = new System.ComponentModel.PropertyDescriptor[defaultProperties.Count];
		defaultProperties.CopyTo(array, 0);
		return array;
	}

	public static IEnumerable<System.ComponentModel.PropertyDescriptor> GetProperties(IEnumerable<object> items)
	{
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
		HashSet<System.ComponentModel.PropertyDescriptor> hashSet = new HashSet<System.ComponentModel.PropertyDescriptor>();
		foreach (object item2 in items)
		{
			PropertyDescriptorCollection defaultProperties = GetDefaultProperties(item2);
			foreach (System.ComponentModel.PropertyDescriptor item3 in defaultProperties)
			{
				if (!hashSet.Contains(item3))
				{
					hashSet.Add(item3);
					list.Add(item3);
				}
			}
		}
		return list;
	}

	public static IEnumerable<System.ComponentModel.PropertyDescriptor> GetSharedProperties(IEnumerable<object> items)
	{
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
		bool flag = true;
		foreach (object item in items)
		{
			IEnumerable<System.ComponentModel.PropertyDescriptor> collection = GetDefaultProperties(item).Cast<System.ComponentModel.PropertyDescriptor>();
			if (flag)
			{
				flag = false;
				list.AddRange(collection);
				continue;
			}
			int num = 0;
			while (num < list.Count)
			{
				string propertyDescriptorKey = list[num].GetPropertyDescriptorKey();
				System.ComponentModel.PropertyDescriptor propertyDescriptor = FindPropertyDescriptor(item, propertyDescriptorKey);
				if (propertyDescriptor != null)
				{
					MultiPropertyDescriptor multiPropertyDescriptor = list[num] as MultiPropertyDescriptor;
					if (multiPropertyDescriptor == null)
					{
						multiPropertyDescriptor = new MultiPropertyDescriptor(list[num]);
						list[num] = multiPropertyDescriptor;
					}
					num++;
				}
				else
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public static IEnumerable<System.ComponentModel.PropertyDescriptor> GetSharedPropertiesOriginal(IEnumerable<object> items)
	{
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
		bool flag = true;
		foreach (object item3 in items)
		{
			PropertyDescriptorCollection defaultProperties = GetDefaultProperties(item3);
			if (flag)
			{
				flag = false;
				foreach (System.ComponentModel.PropertyDescriptor item4 in defaultProperties)
				{
					list.Add(item4);
				}
				continue;
			}
			HashSet<System.ComponentModel.PropertyDescriptor> hashSet = new HashSet<System.ComponentModel.PropertyDescriptor>();
			foreach (System.ComponentModel.PropertyDescriptor item5 in defaultProperties)
			{
				hashSet.Add(item5);
			}
			int num = 0;
			while (num < list.Count)
			{
				if (!hashSet.Contains(list[num]))
				{
					list.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
		return list;
	}

	public static int GetPropertyDescriptorHash(this System.ComponentModel.PropertyDescriptor propertyDescriptor)
	{
		return propertyDescriptor.Name.GetHashCode() ^ (GetCategoryName(propertyDescriptor).GetHashCode() << 8) ^ propertyDescriptor.PropertyType.GetHashCode();
	}

	public static string GetPropertyDescriptorKey(this System.ComponentModel.PropertyDescriptor propertyDescriptor)
	{
		string name = propertyDescriptor.Name;
		string categoryName = GetCategoryName(propertyDescriptor);
		string fullName = propertyDescriptor.PropertyType.FullName;
		int capacity = name.Length + categoryName.Length + fullName.Length + 2;
		StringBuilder stringBuilder = new StringBuilder(name, 0, name.Length, capacity);
		stringBuilder.Append(',');
		stringBuilder.Append(categoryName);
		stringBuilder.Append(',');
		stringBuilder.Append(fullName);
		return stringBuilder.ToString();
	}

	public static bool PropertyDescriptorsEqual(System.ComponentModel.PropertyDescriptor a, System.ComponentModel.PropertyDescriptor b)
	{
		return a.Name == b.Name && GetCategoryName(a) == GetCategoryName(b) && a.PropertyType == b.PropertyType;
	}

	public static string GetCategoryName(System.ComponentModel.PropertyDescriptor descriptor)
	{
		string text = descriptor.Category;
		if (text == null)
		{
			text = "Misc".Localize("Miscellaneous category");
		}
		return text;
	}

	public static string GetCategoryName(System.ComponentModel.PropertyDescriptor descriptor, object propItem)
	{
		if (descriptor is ICustomPropertyCatergoryName customPropertyCatergoryName)
		{
			return customPropertyCatergoryName.GetCategoryName(propItem);
		}
		return GetCategoryName(descriptor);
	}

	public static string GetPropertyText(object owner, System.ComponentModel.PropertyDescriptor descriptor)
	{
		string text = string.Empty;
		object value = descriptor.GetValue(owner);
		if (value != null)
		{
			TypeConverter converter = descriptor.Converter;
			if (converter != null && converter.CanConvertTo(typeof(string)))
			{
				using TypeConversionContext context = new TypeConversionContext(owner, descriptor);
				text = converter.ConvertTo(context, CultureInfo.InvariantCulture, value, typeof(string)) as string;
			}
			if (text == null)
			{
				text = ((!(value is IFormattable formattable)) ? value.ToString() : formattable.ToString(null, CultureInfo.CurrentUICulture));
			}
		}
		return text;
	}

	public static bool IsEditKey(Keys keyData)
	{
		if (KeysUtil.IsPrintable(keyData))
		{
			return true;
		}
		keyData &= Keys.KeyCode;
		return keyData == Keys.Back || keyData == Keys.Delete;
	}

	public static void SetProperty(object owner, System.ComponentModel.PropertyDescriptor descriptor, object value)
	{
		try
		{
			TypeConverter converter = descriptor.Converter;
			if (converter != null && value != null && converter.CanConvertFrom(value.GetType()))
			{
				value = converter.ConvertFrom(value);
			}
			bool flag = false;
			object obj = null;
			EventHandler<PropertyEditedEventArgs> propertyEdited = PropertyUtils.PropertyEdited;
			if (propertyEdited != null)
			{
				obj = descriptor.GetValue(owner);
				flag = !AreEqual(obj, value);
			}
			descriptor.SetValue(owner, value);
			if (flag)
			{
				PropertyEditedEventArgs e = new PropertyEditedEventArgs(owner, descriptor, obj, value);
				propertyEdited(null, e);
			}
		}
		catch (InvalidTransactionException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			PropertyErrorEventArgs e2 = new PropertyErrorEventArgs(owner, descriptor, ex2);
			PropertyUtils.PropertyError.Raise(null, e2);
			if (!e2.Cancel)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex2.Message);
			}
		}
	}

	public static bool CanResetProperty(IEnumerable<object> owners, System.ComponentModel.PropertyDescriptor descriptor)
	{
		foreach (object owner in owners)
		{
			if (!descriptor.CanResetValue(owner))
			{
				return false;
			}
		}
		return true;
	}

	public static void ResetProperty(IEnumerable<object> owners, System.ComponentModel.PropertyDescriptor descriptor)
	{
		foreach (object owner in owners)
		{
			descriptor.ResetValue(owner);
		}
	}

	public static bool AreEqual(object value1, object value2)
	{
		if (value1 == null)
		{
			return value2 == null;
		}
		if (value1 is Array array && value2 is Array array2)
		{
			int rank = array.Rank;
			if (rank != array2.Rank)
			{
				return false;
			}
			for (int i = 0; i < rank; i++)
			{
				int length = array.GetLength(i);
				if (length != array2.GetLength(i))
				{
					return false;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (!AreEqual(array.GetValue(j), array2.GetValue(j)))
				{
					return false;
				}
			}
			return true;
		}
		if (value1 is float)
		{
			return value2 is float && MathUtil.AreApproxEqual((float)value1, (float)value2, 1E-06);
		}
		if (value1 is double)
		{
			return value2 is double && MathUtil.AreApproxEqual((double)value1, (double)value2, 1E-06);
		}
		return value1.Equals(value2);
	}

	internal static System.ComponentModel.PropertyDescriptor FindPropertyDescriptor(object item, string key)
	{
		IDynamicTypeDescriptor dynamicTypeDescriptor = item.As<IDynamicTypeDescriptor>();
		if (dynamicTypeDescriptor == null || dynamicTypeDescriptor.CacheableProperties)
		{
			DomNode domNode = item.As<DomNode>();
			if (domNode != null)
			{
				if (!s_descriptorCache.TryGetValue(domNode.Type, out var value))
				{
					value = new Dictionary<string, System.ComponentModel.PropertyDescriptor>();
					foreach (System.ComponentModel.PropertyDescriptor defaultProperty in GetDefaultProperties(item))
					{
						value.Add(defaultProperty.GetPropertyDescriptorKey(), defaultProperty);
					}
					s_descriptorCache.Add(domNode.Type, value);
				}
				value.TryGetValue(key, out var value2);
				return value2;
			}
		}
		foreach (System.ComponentModel.PropertyDescriptor defaultProperty2 in GetDefaultProperties(item))
		{
			if (defaultProperty2.GetPropertyDescriptorKey() == key)
			{
				return defaultProperty2;
			}
		}
		return null;
	}
}

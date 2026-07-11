using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Error;

namespace Firaxis.Controls;

public static class ControlExtension
{
	public static void SafeInvoke(this Control control, Action operationToInvoke)
	{
		if (control.IsHandleCreated && !control.IsDisposed)
		{
			control.Invoke(operationToInvoke);
		}
	}

	public static IAsyncResult SafeBeginInvoke(this Control control, Action operationToInvoke)
	{
		if (!control.IsHandleCreated)
		{
			return null;
		}
		if (control.IsDisposed)
		{
			return null;
		}
		return control.BeginInvoke(operationToInvoke);
	}

	public static bool IsMouseOver(this Control control)
	{
		return control.RectangleToScreen(control.ClientRectangle).Contains(Cursor.Position);
	}

	public static Control Find(this Control.ControlCollection collection, Predicate<Control> match)
	{
		foreach (Control item in collection)
		{
			if (match(item))
			{
				return item;
			}
		}
		return null;
	}

	public static void FillDropdownInOrder<T>(this ComboBox comboBox, Func<T, T> transform, T startingValue, T endValue) where T : IComparable<T>
	{
		T other = transform(startingValue);
		ref T reference = ref startingValue;
		T other2 = endValue;
		bool flag = reference.CompareTo(other2) < 0;
		if (flag)
		{
			PlatformAssert.If(startingValue.CompareTo(other) >= 0, "Starting value is less than ending value, but function doesn't increase values.");
		}
		else
		{
			PlatformAssert.If(startingValue.CompareTo(other) < 0, "Starting value is greater than ending value, but function doesn't decrease values.");
		}
		comboBox.SelectedIndex = -1;
		comboBox.Items.Clear();
		Func<bool> func = (flag ? ((Func<bool>)delegate
		{
			ref T reference2 = ref startingValue;
			T other3 = endValue;
			return reference2.CompareTo(other3) <= 0;
		}) : ((Func<bool>)delegate
		{
			ref T reference2 = ref startingValue;
			T other3 = endValue;
			return reference2.CompareTo(other3) > 0;
		}));
		while (func())
		{
			comboBox.Items.Add(startingValue);
			startingValue = transform(startingValue);
			PlatformAssert.If(string.IsNullOrEmpty(startingValue.ToString()), "Tried to add a value with an empty ToString to the combo Box.  Will cause too many items exception.  Ending value: {0}", endValue);
		}
	}

	public static bool HasSelection(this ListBox listBox)
	{
		return listBox != null && listBox.Items.Count > 0 && listBox.SelectedIndex >= 0;
	}

	public static bool ContainsCursor(this Control control)
	{
		Point pt = control.PointToClient(Control.MousePosition);
		return control.ClientRectangle.Contains(pt);
	}

	public static IEnumerable<string> GetItemStrings(this ListBox listBox)
	{
		List<string> list = new List<string>();
		foreach (object item in listBox.Items)
		{
			if (item != null)
			{
				list.Add(item.ToString());
			}
		}
		return list;
	}

	public static void SafeInsertAndSelect(this ListBox listBox, object item, int index)
	{
		if (index < listBox.Items.Count)
		{
			listBox.Items.Insert(index, item);
		}
		else
		{
			listBox.Items.Add(item);
		}
		listBox.SelectedItem = item;
	}

	public static void SafeRemove(this ListBox listBox, object item, int index)
	{
		if (listBox.SelectedIndex == index)
		{
			listBox.SelectedIndex = -1;
		}
		if (index < listBox.Items.Count)
		{
			listBox.Items.RemoveAt(index);
		}
		else
		{
			listBox.Items.Remove(item);
		}
	}

	public static void RefreshListBoxItem(this ListBox listBox, object item)
	{
		for (int i = 0; i < listBox.Items.Count; i++)
		{
			if (listBox.Items[i] == item)
			{
				listBox.Items[i] = item;
				listBox.Refresh();
			}
		}
	}

	public static Bitmap ScreenshotContents(this Control control)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		if (!control.IsHandleCreated)
		{
			throw new ArgumentException($"Control of type {control.GetType().Name} does not have a valid Handle!");
		}
		Size size = control.Size;
		Bitmap bitmap = new Bitmap(size.Width, size.Height, control.CreateGraphics());
		Point location = control.Location;
		Point screenPoint = new Point(0, 0);
		Action action = delegate
		{
			screenPoint = control.PointToScreen(control.Location);
		};
		if (control.InvokeRequired)
		{
			control.Invoke(action);
		}
		else
		{
			action();
		}
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.CopyFromScreen(screenPoint.X, screenPoint.Y, 0, 0, size);
		}
		return bitmap;
	}
}

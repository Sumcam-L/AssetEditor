using System.Collections.Generic;
using System.Text;

namespace Sce.Atf.Input;

public static class KeysUtil
{
	public static Keys NumPadToNum(Keys keys)
	{
		Keys keys2 = keys & Keys.KeyCode;
		if (keys2 >= Keys.NumPad0 && keys2 <= Keys.NumPad9)
		{
			keys2 -= 48;
			keys = (keys & Keys.Modifiers) | keys2;
		}
		return keys;
	}

	public static string KeysToString(Keys k, bool digitOnly)
	{
		string text = string.Empty;
		Keys keys = k & Keys.KeyCode;
		if (k == Keys.None || keys == Keys.Back)
		{
			return text;
		}
		if (keys == Keys.Menu || keys == Keys.ControlKey || keys == Keys.ShiftKey)
		{
			return text;
		}
		if ((k & Keys.Alt) == Keys.Alt)
		{
			text += "Alt";
		}
		if ((k & Keys.Control) == Keys.Control)
		{
			if (text.Length > 0)
			{
				text += "+";
			}
			text += "Ctrl";
		}
		if ((k & Keys.Shift) == Keys.Shift)
		{
			if (text.Length > 0)
			{
				text += "+";
			}
			text += "Shift";
		}
		if (text.Length > 0)
		{
			text += "+";
		}
		Keys keys2 = k & Keys.KeyCode;
		text = keys2 switch
		{
			Keys.Oemplus => text + "+", 
			Keys.OemMinus => text + "-", 
			_ => text + keys2, 
		};
		if (digitOnly)
		{
			if (keys >= Keys.NumPad0 && keys <= Keys.NumPad9)
			{
				text = text.Replace("NumPad", "");
			}
			else if (keys >= Keys.D0 && keys <= Keys.D9)
			{
				text = text.Replace("D", "");
			}
		}
		return text;
	}

	public static string KeysToString(IEnumerable<Keys> k, bool digitOnly)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (Keys item in k)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(KeysToString(item, digitOnly));
			flag = true;
		}
		return stringBuilder.ToString();
	}

	public static Keys KeyArgToKeys(KeyEventArgs ke)
	{
		Keys keys = Keys.None;
		if (ke.KeyCode == Keys.None || ke.KeyCode == Keys.Back)
		{
			return keys;
		}
		if (ke.KeyCode == Keys.Menu || ke.KeyCode == Keys.ControlKey || ke.KeyCode == Keys.ShiftKey)
		{
			return keys;
		}
		if (ke.Alt)
		{
			keys |= Keys.Alt;
		}
		if (ke.Control)
		{
			keys |= Keys.Control;
		}
		if (ke.Shift)
		{
			keys |= Keys.Shift;
		}
		return keys | ke.KeyCode;
	}

	public static string KeyArgToString(KeyEventArgs ke)
	{
		return KeysToString(KeyArgToKeys(ke), digitOnly: false);
	}

	public static bool IsPrintable(Keys k)
	{
		if ((k & (Keys.Control | Keys.Alt)) != Keys.None)
		{
			return false;
		}
		Keys keys = k & Keys.KeyCode;
		return (keys >= Keys.A && keys <= Keys.Z) || (keys >= Keys.D0 && keys <= Keys.D9) || keys == Keys.Tab || keys == Keys.LineFeed || keys == Keys.Space || (keys >= Keys.NumPad0 && keys <= Keys.Divide) || (keys >= Keys.Oem1 && keys <= Keys.Oem102);
	}

	public static bool IsTextBoxInput(bool isMultiline, Keys k)
	{
		switch (k)
		{
		case Keys.Back:
		case Keys.End:
		case Keys.Home:
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
		case Keys.Delete:
		case Keys.End | Keys.Shift:
		case Keys.Home | Keys.Shift:
		case Keys.Insert | Keys.Shift:
		case Keys.Delete | Keys.Shift:
		case Keys.Delete | Keys.Control:
		case Keys.A | Keys.Control:
		case Keys.C | Keys.Control:
		case Keys.V | Keys.Control:
		case Keys.X | Keys.Control:
			return true;
		case Keys.Enter:
			if (isMultiline)
			{
				return true;
			}
			break;
		}
		return IsPrintable(k);
	}

	public static void Select<T>(ICollection<T> collection, T item, Keys modifiers)
	{
		Select(collection, new T[1] { item }, modifiers);
	}

	public static void Select<T>(ICollection<T> collection, IEnumerable<T> items, Keys modifiers)
	{
		if (ClearsSelection(modifiers))
		{
			collection.Clear();
			{
				foreach (T item in items)
				{
					collection.Add(item);
				}
				return;
			}
		}
		foreach (T item2 in items)
		{
			if ((modifiers & Keys.Control) == Keys.Control)
			{
				if (!collection.Remove(item2))
				{
					collection.Add(item2);
				}
			}
			else
			{
				collection.Remove(item2);
				collection.Add(item2);
			}
		}
	}

	public static bool ClearsSelection(Keys modifiers)
	{
		return (modifiers & (Keys.Shift | Keys.Control)) == 0;
	}

	public static bool TogglesSelection(Keys modifiers)
	{
		return (modifiers & Keys.Control) == Keys.Control;
	}

	public static bool AddsSelection(Keys modifiers)
	{
		return (modifiers & Keys.Shift) == Keys.Shift;
	}
}

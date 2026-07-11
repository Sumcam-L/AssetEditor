using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

public static class KeysUtil
{
	public static Sce.Atf.Input.Keys NumPadToNum(Sce.Atf.Input.Keys keys)
	{
		return Sce.Atf.Input.KeysUtil.NumPadToNum(keys);
	}

	public static System.Windows.Forms.Keys NumPadToNum(System.Windows.Forms.Keys keys)
	{
		return KeysInterop.ToWf(Sce.Atf.Input.KeysUtil.NumPadToNum(KeysInterop.ToAtf(keys)));
	}

	public static string KeysToString(Sce.Atf.Input.Keys k, bool digitOnly)
	{
		return Sce.Atf.Input.KeysUtil.KeysToString(k, digitOnly);
	}

	public static string KeysToString(System.Windows.Forms.Keys k, bool digitOnly)
	{
		return Sce.Atf.Input.KeysUtil.KeysToString(KeysInterop.ToAtf(k), digitOnly);
	}

	public static string KeysToString(IEnumerable<Sce.Atf.Input.Keys> k, bool digitOnly)
	{
		return Sce.Atf.Input.KeysUtil.KeysToString(k, digitOnly);
	}

	public static string KeysToString(IEnumerable<System.Windows.Forms.Keys> k, bool digitOnly)
	{
		return Sce.Atf.Input.KeysUtil.KeysToString(KeysInterop.ToAtf(k), digitOnly);
	}

	public static Sce.Atf.Input.Keys KeyArgToKeys(System.Windows.Forms.KeyEventArgs ke)
	{
		return Sce.Atf.Input.KeysUtil.KeyArgToKeys(KeyEventArgsInterop.ToAtf(ke));
	}

	public static Sce.Atf.Input.Keys KeyArgToKeys(Sce.Atf.Input.KeyEventArgs ke)
	{
		return Sce.Atf.Input.KeysUtil.KeyArgToKeys(ke);
	}

	public static string KeyArgToString(Sce.Atf.Input.KeyEventArgs ke)
	{
		return Sce.Atf.Input.KeysUtil.KeyArgToString(ke);
	}

	public static bool IsPrintable(Sce.Atf.Input.Keys k)
	{
		return Sce.Atf.Input.KeysUtil.IsPrintable(k);
	}

	public static bool IsTextBoxInput(bool isMultiline, Sce.Atf.Input.Keys k)
	{
		return Sce.Atf.Input.KeysUtil.IsTextBoxInput(isMultiline, k);
	}

	public static bool IsTextBoxInput(Control control, System.Windows.Forms.Keys k)
	{
		bool isMultiline = control is TextBoxBase && ((TextBoxBase)control).Multiline;
		return Sce.Atf.Input.KeysUtil.IsTextBoxInput(isMultiline, KeysInterop.ToAtf(k));
	}

	public static void Select<T>(ICollection<T> collection, T item, Sce.Atf.Input.Keys modifiers)
	{
		Sce.Atf.Input.KeysUtil.Select(collection, item, modifiers);
	}

	public static void Select<T>(ICollection<T> collection, T item, System.Windows.Forms.Keys modifiers)
	{
		Sce.Atf.Input.KeysUtil.Select(collection, item, KeysInterop.ToAtf(modifiers));
	}

	public static void Select<T>(ICollection<T> collection, IEnumerable<T> items, Sce.Atf.Input.Keys modifiers)
	{
		Sce.Atf.Input.KeysUtil.Select(collection, items, modifiers);
	}

	public static void Select<T>(ICollection<T> collection, IEnumerable<T> items, System.Windows.Forms.Keys modifiers)
	{
		Sce.Atf.Input.KeysUtil.Select(collection, items, KeysInterop.ToAtf(modifiers));
	}

	public static bool ClearsSelection(Sce.Atf.Input.Keys modifiers)
	{
		return Sce.Atf.Input.KeysUtil.ClearsSelection(modifiers);
	}

	public static bool ClearsSelection(System.Windows.Forms.Keys modifiers)
	{
		return Sce.Atf.Input.KeysUtil.ClearsSelection(KeysInterop.ToAtf(modifiers));
	}

	public static bool TogglesSelection(Sce.Atf.Input.Keys modifiers)
	{
		return Sce.Atf.Input.KeysUtil.TogglesSelection(modifiers);
	}

	public static bool TogglesSelection(System.Windows.Forms.Keys modifiers)
	{
		return Sce.Atf.Input.KeysUtil.TogglesSelection(KeysInterop.ToAtf(modifiers));
	}

	public static bool AddsSelection(Sce.Atf.Input.Keys modifiers)
	{
		return Sce.Atf.Input.KeysUtil.AddsSelection(modifiers);
	}
}

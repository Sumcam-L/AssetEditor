using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Interop;

public static class KeysInterop
{
	public static Sce.Atf.Input.Keys ToAtf(System.Windows.Forms.Keys keys)
	{
		return (Sce.Atf.Input.Keys)keys;
	}

	public static IEnumerable<Sce.Atf.Input.Keys> ToAtf(IEnumerable<System.Windows.Forms.Keys> keys)
	{
		foreach (System.Windows.Forms.Keys key in keys)
		{
			yield return (Sce.Atf.Input.Keys)key;
		}
	}

	public static System.Windows.Forms.Keys ToWf(Sce.Atf.Input.Keys keys)
	{
		return (System.Windows.Forms.Keys)keys;
	}

	public static IEnumerable<System.Windows.Forms.Keys> ToWf(IEnumerable<Sce.Atf.Input.Keys> keys)
	{
		foreach (Sce.Atf.Input.Keys key in keys)
		{
			yield return (System.Windows.Forms.Keys)key;
		}
	}

	public static Sce.Atf.Input.Keys ToAtf(Key wpfKey)
	{
		return (Sce.Atf.Input.Keys)KeyInterop.VirtualKeyFromKey(wpfKey);
	}

	public static Sce.Atf.Input.Keys ToAtf(ModifierKeys wpfKey)
	{
		Sce.Atf.Input.Keys keys = Sce.Atf.Input.Keys.None;
		if ((wpfKey & ModifierKeys.Shift) != ModifierKeys.None)
		{
			keys |= Sce.Atf.Input.Keys.Shift;
		}
		if ((wpfKey & ModifierKeys.Control) != ModifierKeys.None)
		{
			keys |= Sce.Atf.Input.Keys.Control;
		}
		if ((wpfKey & ModifierKeys.Alt) != ModifierKeys.None)
		{
			keys |= Sce.Atf.Input.Keys.Alt;
		}
		return keys;
	}

	public static Key ToWpf(Sce.Atf.Input.Keys atfKey)
	{
		Sce.Atf.Input.Keys virtualKey = atfKey & Sce.Atf.Input.Keys.KeyCode;
		return KeyInterop.KeyFromVirtualKey((int)virtualKey);
	}

	public static ModifierKeys ToWpfModifiers(Sce.Atf.Input.Keys atfKeys)
	{
		Sce.Atf.Input.Keys keys = (atfKeys &= Sce.Atf.Input.Keys.Modifiers);
		ModifierKeys modifierKeys = ModifierKeys.None;
		if ((keys & Sce.Atf.Input.Keys.Alt) > Sce.Atf.Input.Keys.None)
		{
			modifierKeys |= ModifierKeys.Alt;
		}
		if ((keys & Sce.Atf.Input.Keys.Shift) > Sce.Atf.Input.Keys.None)
		{
			modifierKeys |= ModifierKeys.Shift;
		}
		if ((keys & Sce.Atf.Input.Keys.Control) > Sce.Atf.Input.Keys.None)
		{
			modifierKeys |= ModifierKeys.Control;
		}
		if ((atfKeys & Sce.Atf.Input.Keys.RWin) > Sce.Atf.Input.Keys.None || (atfKeys & Sce.Atf.Input.Keys.RWin) > Sce.Atf.Input.Keys.None)
		{
			modifierKeys |= ModifierKeys.Windows;
		}
		return modifierKeys;
	}
}

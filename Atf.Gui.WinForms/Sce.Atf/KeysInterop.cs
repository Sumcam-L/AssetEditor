using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

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
}

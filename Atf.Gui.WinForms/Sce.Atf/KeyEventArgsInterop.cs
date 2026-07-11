using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

public static class KeyEventArgsInterop
{
	public static Sce.Atf.Input.KeyEventArgs ToAtf(System.Windows.Forms.KeyEventArgs args)
	{
		return new Sce.Atf.Input.KeyEventArgs(KeysInterop.ToAtf(args.KeyData));
	}

	public static System.Windows.Forms.KeyEventArgs ToWf(Sce.Atf.Input.KeyEventArgs args)
	{
		return new System.Windows.Forms.KeyEventArgs(KeysInterop.ToWf(args.KeyData));
	}
}

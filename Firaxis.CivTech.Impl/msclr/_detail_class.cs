using System.Runtime.InteropServices;

namespace msclr;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct _detail_class
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct dummy_struct
	{
		public static readonly string dummy_string = "";
	}

	public static string _safe_true = dummy_struct.dummy_string;

	public static string _safe_false = null;
}

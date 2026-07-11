using SharpDX.WIC;

namespace SharpDX.Direct2D1;

internal class ModuleInit
{
	[Tag("SharpDX.ModuleInit")]
	internal static void Setup()
	{
		ResultDescriptor.RegisterProvider(typeof(ResultCode));
		ResultDescriptor.RegisterProvider(typeof(SharpDX.WIC.ResultCode));
	}
}

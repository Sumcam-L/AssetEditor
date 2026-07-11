namespace SharpDX.DXGI;

internal class ModuleInit
{
	[Tag("SharpDX.ModuleInit")]
	internal static void Setup()
	{
		ResultDescriptor.RegisterProvider(typeof(ResultCode));
	}
}

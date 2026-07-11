namespace SharpDX;

internal class ModuleInit
{
	[Tag("SharpDX.ModuleInit")]
	internal static void Setup()
	{
		ResultDescriptor.RegisterProvider(typeof(Result));
	}
}

using System.Runtime.InteropServices;

namespace SharpDX.Direct3D;

public static class PixHelper
{
	public static bool IsCurrentlyProfiled => D3DPERF_GetStatus() != 0;

	public static int BeginEvent(Color color, string name)
	{
		return D3DPERF_BeginEvent(color.ToBgra(), name);
	}

	public static int BeginEvent(Color color, string name, params object[] parameters)
	{
		return D3DPERF_BeginEvent(color.ToBgra(), string.Format(name, parameters));
	}

	public static int EndEvent()
	{
		return D3DPERF_EndEvent();
	}

	public static void SetMarker(Color color, string name)
	{
		D3DPERF_SetMarker(color.ToBgra(), name);
	}

	public static void SetMarker(Color color, string name, params object[] parameters)
	{
		D3DPERF_SetMarker(color.ToBgra(), string.Format(name, parameters));
	}

	public static void AllowProfiling(bool enableFlag)
	{
		D3DPERF_SetOptions((!enableFlag) ? 1 : 0);
	}

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern int D3DPERF_BeginEvent(int color, string name);

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern int D3DPERF_EndEvent();

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern void D3DPERF_SetMarker(int color, string wszName);

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern void D3DPERF_SetOptions(int options);

	[DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
	private static extern int D3DPERF_GetStatus();
}

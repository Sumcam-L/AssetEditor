using System.Runtime.InteropServices;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FrustumCameraParams
{
	public Vector3 Position;

	public Vector3 LookAtDir;

	public Vector3 UpDir;

	public float FOV;

	public float ZNear;

	public float ZFar;

	public float AspectRatio;
}

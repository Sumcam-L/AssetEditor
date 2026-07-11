using System;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX.Direct3D;

public struct ShaderMacro : IEquatable<ShaderMacro>, IDataSerializable
{
	internal struct __Native
	{
		public IntPtr Name;

		public IntPtr Definition;

		internal void __MarshalFree()
		{
			if (Name != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(Name);
			}
			if (Definition != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(Definition);
			}
		}
	}

	public string Name;

	public string Definition;

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Name = ((@ref.Name == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(@ref.Name));
		Definition = ((@ref.Definition == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(@ref.Definition));
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Name = ((Name == null) ? IntPtr.Zero : Utilities.StringToHGlobalAnsi(Name));
		@ref.Definition = ((Definition == null) ? IntPtr.Zero : Utilities.StringToHGlobalAnsi(Definition));
	}

	public ShaderMacro(string name, object definition)
	{
		Name = name;
		Definition = definition?.ToString();
	}

	public bool Equals(ShaderMacro other)
	{
		if (string.Equals(Name, other.Name))
		{
			return string.Equals(Definition, other.Definition);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is ShaderMacro)
		{
			return Equals((ShaderMacro)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((Name != null) ? Name.GetHashCode() : 0) * 397) ^ ((Definition != null) ? Definition.GetHashCode() : 0);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref Name);
		serializer.Serialize(ref Definition, SerializeFlags.Nullable);
	}

	public static bool operator ==(ShaderMacro left, ShaderMacro right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ShaderMacro left, ShaderMacro right)
	{
		return !left.Equals(right);
	}
}

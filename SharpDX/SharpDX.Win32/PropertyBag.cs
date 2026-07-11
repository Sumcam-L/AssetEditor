using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

public class PropertyBag : ComObject
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct PROPBAG2 : IDisposable
	{
		internal uint type;

		internal ushort vt;

		internal ushort cfType;

		internal IntPtr dwHint;

		internal IntPtr pstrName;

		internal Guid clsid;

		public string Name
		{
			get
			{
				return Marshal.PtrToStringUni(pstrName);
			}
			set
			{
				pstrName = Marshal.StringToCoTaskMemUni(value);
			}
		}

		public void Dispose()
		{
			if (pstrName != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(pstrName);
				pstrName = IntPtr.Zero;
			}
		}
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("22F55882-280B-11D0-A8A9-00A0C90C2004")]
	private interface IPropertyBag2
	{
		[PreserveSig]
		Result Read([In] int cProperties, [In] ref PROPBAG2 pPropBag, IntPtr pErrLog, out object pvarValue, out Result phrError);

		[PreserveSig]
		Result Write([In] int cProperties, [In] ref PROPBAG2 pPropBag, ref object value);

		[PreserveSig]
		Result CountProperties(out int pcProperties);

		[PreserveSig]
		Result GetPropertyInfo([In] int iProperty, [In] int cProperties, out PROPBAG2 pPropBag, out int pcProperties);

		[PreserveSig]
		Result LoadObject([In][MarshalAs(UnmanagedType.LPWStr)] string pstrName, [In] uint dwHint, [In][MarshalAs(UnmanagedType.IUnknown)] object pUnkObject, IntPtr pErrLog);
	}

	private IPropertyBag2 nativePropertyBag;

	public int Count
	{
		get
		{
			CheckIfInitialized();
			nativePropertyBag.CountProperties(out var pcProperties);
			return pcProperties;
		}
	}

	public string[] Keys
	{
		get
		{
			CheckIfInitialized();
			List<string> list = new List<string>();
			for (int i = 0; i < Count; i++)
			{
				nativePropertyBag.GetPropertyInfo(i, 1, out var pPropBag, out var _);
				list.Add(pPropBag.Name);
			}
			return list.ToArray();
		}
	}

	public PropertyBag(IntPtr propertyBagPointer)
		: base(propertyBagPointer)
	{
	}

	protected override void NativePointerUpdated(IntPtr oldNativePointer)
	{
		base.NativePointerUpdated(oldNativePointer);
		if (base.NativePointer != IntPtr.Zero)
		{
			nativePropertyBag = (IPropertyBag2)Marshal.GetObjectForIUnknown(base.NativePointer);
		}
		else
		{
			nativePropertyBag = null;
		}
	}

	private void CheckIfInitialized()
	{
		if (nativePropertyBag == null)
		{
			throw new InvalidOperationException("This instance is not bound to an unmanaged IPropertyBag2");
		}
	}

	public object Get(string name)
	{
		CheckIfInitialized();
		PROPBAG2 pPropBag = new PROPBAG2
		{
			Name = name
		};
		if (nativePropertyBag.Read(1, ref pPropBag, IntPtr.Zero, out var pvarValue, out var phrError).Failure || phrError.Failure)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Property with name [{0}] is not valid for this instance", new object[1] { name }));
		}
		pPropBag.Dispose();
		return pvarValue;
	}

	public T1 Get<T1, T2>(PropertyBagKey<T1, T2> propertyKey)
	{
		object value = Get(propertyKey.Name);
		return (T1)Convert.ChangeType(value, typeof(T1));
	}

	public void Set(string name, object value)
	{
		CheckIfInitialized();
		object obj = Get(name);
		value = Convert.ChangeType(value, (obj == null) ? value.GetType() : obj.GetType());
		PROPBAG2 pPropBag = new PROPBAG2
		{
			Name = name
		};
		IPropertyBag2 propertyBag = nativePropertyBag;
		object value2 = value;
		propertyBag.Write(1, ref pPropBag, ref value2).CheckError();
		pPropBag.Dispose();
	}

	public void Set<T1, T2>(PropertyBagKey<T1, T2> propertyKey, T1 value)
	{
		Set(propertyKey.Name, value);
	}
}

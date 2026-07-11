using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpDX;

internal class ShadowContainer : DisposeBase
{
	private readonly Dictionary<Guid, CppObjectShadow> guidToShadow = new Dictionary<Guid, CppObjectShadow>();

	private static readonly Dictionary<Type, List<Type>> typeToShadowTypes = new Dictionary<Type, List<Type>>();

	private IntPtr guidPtr;

	public IntPtr[] Guids { get; private set; }

	public void Initialize(ICallbackable callbackable)
	{
		callbackable.Shadow = this;
		Type type = callbackable.GetType();
		List<Type> value;
		lock (typeToShadowTypes)
		{
			if (!typeToShadowTypes.TryGetValue(type, out value))
			{
				Type[] interfaces = type.GetInterfaces();
				value = new List<Type>();
				value.AddRange(interfaces);
				typeToShadowTypes.Add(type, value);
				Type[] array = interfaces;
				foreach (Type type2 in array)
				{
					ShadowAttribute shadowAttribute = ShadowAttribute.Get(type2);
					if (shadowAttribute == null)
					{
						value.Remove(type2);
						continue;
					}
					Type[] interfaces2 = type2.GetInterfaces();
					Type[] array2 = interfaces2;
					foreach (Type item in array2)
					{
						value.Remove(item);
					}
				}
			}
		}
		CppObjectShadow cppObjectShadow = null;
		foreach (Type item2 in value)
		{
			ShadowAttribute shadowAttribute2 = ShadowAttribute.Get(item2);
			CppObjectShadow cppObjectShadow2 = (CppObjectShadow)Activator.CreateInstance(shadowAttribute2.Type);
			cppObjectShadow2.Initialize(callbackable);
			if (cppObjectShadow == null)
			{
				cppObjectShadow = cppObjectShadow2;
				guidToShadow.Add(ComObjectShadow.IID_IUnknown, cppObjectShadow);
			}
			guidToShadow.Add(Utilities.GetGuidFromType(item2), cppObjectShadow2);
			Type[] interfaces3 = item2.GetInterfaces();
			Type[] array3 = interfaces3;
			foreach (Type type3 in array3)
			{
				ShadowAttribute shadowAttribute3 = ShadowAttribute.Get(type3);
				if (shadowAttribute3 != null)
				{
					guidToShadow.Add(Utilities.GetGuidFromType(type3), cppObjectShadow2);
				}
			}
		}
	}

	internal IntPtr Find(Type type)
	{
		return Find(Utilities.GetGuidFromType(type));
	}

	internal IntPtr Find(Guid guidType)
	{
		return FindShadow(guidType)?.NativePointer ?? IntPtr.Zero;
	}

	internal CppObjectShadow FindShadow(Guid guidType)
	{
		guidToShadow.TryGetValue(guidType, out var value);
		return value;
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		foreach (CppObjectShadow value in guidToShadow.Values)
		{
			value.Dispose();
		}
		guidToShadow.Clear();
		if (guidPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(guidPtr);
			guidPtr = IntPtr.Zero;
		}
	}
}

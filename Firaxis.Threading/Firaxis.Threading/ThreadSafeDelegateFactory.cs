using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Firaxis.TypeGeneration;

namespace Firaxis.Threading;

public static class ThreadSafeDelegateFactory
{
	private class ThreadSafeInvokeData
	{
		public Delegate Delegate;

		public IThreadSafeInvoker Invoker;
	}

	private static object Lock = new object();

	private static Type SafeDelegateType;

	private static MethodInfo InitMethod;

	private static Dictionary<Type, DynamicMethod> CachedInvokeMethods;

	public static Delegate CreateDelegate(Delegate method, IThreadSafeInvoker invoker)
	{
		if (InitMethod == null)
		{
			lock (Lock)
			{
				if (InitMethod == null)
				{
					CreateSafeDelegateType();
				}
			}
		}
		Delegate obj = (Delegate)Activator.CreateInstance(SafeDelegateType, typeof(ThreadSafeDelegateFactory), InitMethod.MethodHandle.GetFunctionPointer());
		InitMethod.Invoke(obj, new object[2] { method, invoker });
		return obj;
	}

	public static DelegateType CreateDelegate<DelegateType>(DelegateType method, IThreadSafeInvoker invoker) where DelegateType : class
	{
		Type typeFromHandle = typeof(DelegateType);
		DynamicMethod value;
		lock (Lock)
		{
			if (CachedInvokeMethods == null)
			{
				CachedInvokeMethods = new Dictionary<Type, DynamicMethod>();
			}
			if (!CachedInvokeMethods.TryGetValue(typeFromHandle, out value))
			{
				value = CreateInvokeMethod(typeFromHandle);
				CachedInvokeMethods[typeFromHandle] = value;
			}
		}
		ThreadSafeInvokeData threadSafeInvokeData = new ThreadSafeInvokeData();
		threadSafeInvokeData.Delegate = method as Delegate;
		threadSafeInvokeData.Invoker = invoker;
		object obj = value.CreateDelegate(typeFromHandle, threadSafeInvokeData);
		return (DelegateType)obj;
	}

	private static DynamicMethod CreateInvokeMethod(Type delegateType)
	{
		if (delegateType.BaseType != typeof(MulticastDelegate))
		{
			throw new ArgumentException("Not a delegate.");
		}
		MethodInfo method = delegateType.GetMethod("Invoke");
		if (method == null)
		{
			throw new ArgumentException("Not a delegate.");
		}
		ParameterInfo[] parameters = method.GetParameters();
		Type[] array = new Type[parameters.Length + 1];
		array[0] = typeof(ThreadSafeInvokeData);
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i + 1] = parameters[i].ParameterType;
		}
		DynamicMethod dynamicMethod = new DynamicMethod("ThreadSafeDelegate", null, array, typeof(ThreadSafeInvokeData));
		FieldInfo field = typeof(ThreadSafeInvokeData).GetField("Delegate");
		FieldInfo field2 = typeof(ThreadSafeInvokeData).GetField("Invoker");
		MethodInfo meth = ((array.Length != 1) ? typeof(IThreadSafeInvoker).GetMethod("Invoke", new Type[2]
		{
			typeof(Delegate),
			typeof(object[])
		}) : typeof(IThreadSafeInvoker).GetMethod("Invoke", new Type[1] { typeof(Delegate) }));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, field2);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, field);
		if (array.Length > 1)
		{
			iLGenerator.Emit(OpCodes.Ldc_I4_S, array.Length - 1);
			iLGenerator.Emit(OpCodes.Newarr, typeof(object));
			LocalBuilder local = iLGenerator.DeclareLocal(typeof(object[]));
			iLGenerator.Emit(OpCodes.Stloc, local);
			for (int j = 1; j < array.Length; j++)
			{
				iLGenerator.Emit(OpCodes.Ldloc, local);
				iLGenerator.Emit(OpCodes.Ldc_I4, j - 1);
				iLGenerator.Emit(OpCodes.Ldarg, j);
				if (array[j].IsValueType)
				{
					iLGenerator.Emit(OpCodes.Box, array[j]);
				}
				iLGenerator.Emit(OpCodes.Stelem_Ref);
			}
			iLGenerator.Emit(OpCodes.Ldloc, local);
		}
		iLGenerator.Emit(OpCodes.Call, meth);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ret);
		return dynamicMethod;
	}

	private static void CreateSafeDelegateType()
	{
		TypeAttributes attr = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass;
		TypeBuilder typeBuilder = AssemblyGenerator.DefaultGenerator.ModuleBuilder.DefineType("SafeDelegate", attr, typeof(MulticastDelegate));
		FieldBuilder field = typeBuilder.DefineField("Delegate", typeof(Delegate), FieldAttributes.Family);
		FieldBuilder field2 = typeBuilder.DefineField("Invoker", typeof(IThreadSafeInvoker), FieldAttributes.Family);
		MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName;
		ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(attributes, CallingConventions.Standard, new Type[2]
		{
			typeof(object),
			typeof(IntPtr)
		});
		constructorBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		MethodAttributes attributes2 = MethodAttributes.Family | MethodAttributes.Virtual;
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("DynamicInvokeImpl", attributes2, typeof(object), new Type[1] { typeof(object[]) });
		MethodInfo method = typeof(IThreadSafeInvoker).GetMethod("Invoke", new Type[1] { typeof(Delegate) });
		MethodInfo method2 = typeof(IThreadSafeInvoker).GetMethod("Invoke", new Type[2]
		{
			typeof(Delegate),
			typeof(object[])
		});
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		Label label = iLGenerator.DefineLabel();
		Label label2 = iLGenerator.DefineLabel();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, field2);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, field);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Brfalse, label);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Call, method2);
		iLGenerator.Emit(OpCodes.Br_S, label2);
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Call, method);
		iLGenerator.MarkLabel(label2);
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ldnull);
		iLGenerator.Emit(OpCodes.Ret);
		attributes2 = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask;
		MethodBuilder methodBuilder2 = typeBuilder.DefineMethod("Invoke", attributes2, typeof(object), new Type[1] { typeof(object[]) });
		methodBuilder2.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		MethodBuilder methodBuilder3 = typeBuilder.DefineMethod("Init", MethodAttributes.Public, null, new Type[2]
		{
			typeof(Delegate),
			typeof(IThreadSafeInvoker)
		});
		iLGenerator = methodBuilder3.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Stfld, field);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_2);
		iLGenerator.Emit(OpCodes.Stfld, field2);
		iLGenerator.Emit(OpCodes.Ret);
		SafeDelegateType = typeBuilder.CreateType();
		InitMethod = typeBuilder.GetMethod("Init");
	}
}

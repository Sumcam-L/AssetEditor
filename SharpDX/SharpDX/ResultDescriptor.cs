using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpDX;

public sealed class ResultDescriptor
{
	private const string UnknownText = "Unknown";

	private static readonly object LockDescriptor = new object();

	private static readonly List<Type> RegisteredDescriptorProvider = new List<Type>();

	private static readonly Dictionary<Result, ResultDescriptor> Descriptors = new Dictionary<Result, ResultDescriptor>();

	public Result Result { get; private set; }

	public int Code => Result.Code;

	public string Module { get; private set; }

	public string NativeApiCode { get; private set; }

	public string ApiCode { get; private set; }

	public string Description { get; set; }

	public ResultDescriptor(Result code, string module, string nativeApiCode, string apiCode, string description = null)
	{
		Result = code;
		Module = module;
		NativeApiCode = nativeApiCode;
		ApiCode = apiCode;
		Description = description;
	}

	public bool Equals(ResultDescriptor other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		return other.Result.Equals(Result);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj.GetType() != typeof(ResultDescriptor))
		{
			return false;
		}
		return Equals((ResultDescriptor)obj);
	}

	public override int GetHashCode()
	{
		return Result.GetHashCode();
	}

	public override string ToString()
	{
		return $"HRESULT: [0x{Result.Code:X}], Module: [{Module}], ApiCode: [{NativeApiCode}/{ApiCode}], Message: {Description}";
	}

	public static implicit operator Result(ResultDescriptor result)
	{
		return result.Result;
	}

	public static explicit operator int(ResultDescriptor result)
	{
		return result.Result.Code;
	}

	public static explicit operator uint(ResultDescriptor result)
	{
		return (uint)result.Result.Code;
	}

	public static bool operator ==(ResultDescriptor left, Result right)
	{
		if (left == null)
		{
			return false;
		}
		return left.Result.Code == right.Code;
	}

	public static bool operator !=(ResultDescriptor left, Result right)
	{
		if (left == null)
		{
			return false;
		}
		return left.Result.Code != right.Code;
	}

	public static void RegisterProvider(Type descriptorsProviderType)
	{
		lock (LockDescriptor)
		{
			if (!RegisteredDescriptorProvider.Contains(descriptorsProviderType))
			{
				RegisteredDescriptorProvider.Add(descriptorsProviderType);
			}
		}
	}

	public static ResultDescriptor Find(Result result)
	{
		ResultDescriptor value;
		lock (LockDescriptor)
		{
			if (RegisteredDescriptorProvider.Count > 0)
			{
				foreach (Type item in RegisteredDescriptorProvider)
				{
					AddDescriptorsFromType(item);
				}
				RegisteredDescriptorProvider.Clear();
			}
			if (!Descriptors.TryGetValue(result, out value))
			{
				value = new ResultDescriptor(result, "Unknown", "Unknown", "Unknown");
			}
			if (value.Description == null)
			{
				string descriptionFromResultCode = GetDescriptionFromResultCode(result.Code);
				value.Description = descriptionFromResultCode ?? "Unknown";
			}
		}
		return value;
	}

	private static void AddDescriptorsFromType(Type type)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeof(ResultDescriptor))
			{
				ResultDescriptor resultDescriptor = (ResultDescriptor)fieldInfo.GetValue(null);
				if (!Descriptors.ContainsKey(resultDescriptor.Result))
				{
					Descriptors.Add(resultDescriptor.Result, resultDescriptor);
				}
			}
		}
	}

	private static string GetDescriptionFromResultCode(int resultCode)
	{
		IntPtr lpBuffer = IntPtr.Zero;
		FormatMessageW(4864, IntPtr.Zero, resultCode, 0, ref lpBuffer, 0, IntPtr.Zero);
		string result = Marshal.PtrToStringUni(lpBuffer);
		Marshal.FreeHGlobal(lpBuffer);
		return result;
	}

	[DllImport("kernel32.dll")]
	private static extern uint FormatMessageW(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, ref IntPtr lpBuffer, int nSize, IntPtr Arguments);
}

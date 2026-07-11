using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using SharpDX.Collections;

namespace SharpDX.Diagnostics;

public static class ObjectTracker
{
	private static Dictionary<IntPtr, List<ObjectReference>> processGlobalObjectReferences;

	[ThreadStatic]
	private static Dictionary<IntPtr, List<ObjectReference>> threadStaticObjectReferences;

	private static Dictionary<IntPtr, List<ObjectReference>> ObjectReferences
	{
		get
		{
			if (Configuration.UseThreadStaticObjectTracking)
			{
				if (threadStaticObjectReferences == null)
				{
					threadStaticObjectReferences = new Dictionary<IntPtr, List<ObjectReference>>(EqualityComparer.DefaultIntPtr);
				}
				return threadStaticObjectReferences;
			}
			if (processGlobalObjectReferences == null)
			{
				processGlobalObjectReferences = new Dictionary<IntPtr, List<ObjectReference>>(EqualityComparer.DefaultIntPtr);
			}
			return processGlobalObjectReferences;
		}
	}

	public static event EventHandler<ComObjectEventArgs> Tracked;

	public static event EventHandler<ComObjectEventArgs> UnTracked;

	static ObjectTracker()
	{
		AppDomain.CurrentDomain.DomainUnload += OnProcessExit;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
	}

	private static void OnProcessExit(object sender, EventArgs e)
	{
		if (Configuration.EnableObjectTracking)
		{
			string value = ReportActiveObjects();
			if (!string.IsNullOrEmpty(value))
			{
				Console.WriteLine(value);
			}
		}
	}

	public static void Track(ComObject comObject)
	{
		if (comObject == null || comObject.NativePointer == IntPtr.Zero)
		{
			return;
		}
		lock (ObjectReferences)
		{
			if (!ObjectReferences.TryGetValue(comObject.NativePointer, out var value))
			{
				value = new List<ObjectReference>();
				ObjectReferences.Add(comObject.NativePointer, value);
			}
			StringBuilder stringBuilder = new StringBuilder();
			StackTrace stackTrace = new StackTrace(3, fNeedFileInfo: true);
			StackFrame[] frames = stackTrace.GetFrames();
			foreach (StackFrame stackFrame in frames)
			{
				if (stackFrame.GetFileLineNumber() != 0)
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\t{0}({1},{2}) : {3}", stackFrame.GetFileName(), stackFrame.GetFileLineNumber(), stackFrame.GetFileColumnNumber(), stackFrame.GetMethod()).AppendLine();
				}
			}
			value.Add(new ObjectReference(DateTime.Now, comObject, stringBuilder.ToString()));
			OnTracked(comObject);
		}
	}

	public static List<ObjectReference> Find(IntPtr comObjectPtr)
	{
		lock (ObjectReferences)
		{
			if (ObjectReferences.TryGetValue(comObjectPtr, out var value))
			{
				return new List<ObjectReference>(value);
			}
		}
		return new List<ObjectReference>();
	}

	public static ObjectReference Find(ComObject comObject)
	{
		lock (ObjectReferences)
		{
			if (ObjectReferences.TryGetValue(comObject.NativePointer, out var value))
			{
				foreach (ObjectReference item in value)
				{
					if (object.ReferenceEquals(item.Object.Target, comObject))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	public static void UnTrack(ComObject comObject)
	{
		if (comObject == null || comObject.NativePointer == IntPtr.Zero)
		{
			return;
		}
		lock (ObjectReferences)
		{
			if (!ObjectReferences.TryGetValue(comObject.NativePointer, out var value))
			{
				return;
			}
			for (int num = value.Count - 1; num >= 0; num--)
			{
				ObjectReference objectReference = value[num];
				if (object.ReferenceEquals(objectReference.Object.Target, comObject))
				{
					value.RemoveAt(num);
				}
				else if (!objectReference.IsAlive)
				{
					value.RemoveAt(num);
				}
			}
			if (value.Count == 0)
			{
				ObjectReferences.Remove(comObject.NativePointer);
			}
			OnUnTracked(comObject);
		}
	}

	public static List<ObjectReference> FindActiveObjects()
	{
		List<ObjectReference> list = new List<ObjectReference>();
		lock (ObjectReferences)
		{
			foreach (List<ObjectReference> value in ObjectReferences.Values)
			{
				foreach (ObjectReference item in value)
				{
					if (item.IsAlive)
					{
						list.Add(item);
					}
				}
			}
			return list;
		}
	}

	public static string ReportActiveObjects()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (ObjectReference item in FindActiveObjects())
		{
			string text = item.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.AppendFormat("[{0}]: {1}", num, text);
				object target = item.Object.Target;
				if (target != null)
				{
					string name = target.GetType().Name;
					if (!dictionary.TryGetValue(name, out var value))
					{
						dictionary[name] = 0;
					}
					dictionary[name] = value + 1;
				}
			}
			num++;
		}
		List<string> list = new List<string>(dictionary.Keys);
		list.Sort();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Count per Type:");
		foreach (string item2 in list)
		{
			stringBuilder.AppendFormat("{0} : {1}", item2, dictionary[item2]);
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	private static void OnTracked(ComObject obj)
	{
		ObjectTracker.Tracked?.Invoke(null, new ComObjectEventArgs(obj));
	}

	private static void OnUnTracked(ComObject obj)
	{
		ObjectTracker.UnTracked?.Invoke(null, new ComObjectEventArgs(obj));
	}
}

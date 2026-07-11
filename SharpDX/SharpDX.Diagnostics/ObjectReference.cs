using System;
using System.Globalization;
using System.Text;

namespace SharpDX.Diagnostics;

public class ObjectReference
{
	public DateTime CreationTime { get; private set; }

	public WeakReference Object { get; private set; }

	public string StackTrace { get; private set; }

	public bool IsAlive => Object.IsAlive;

	public ObjectReference(DateTime creationTime, ComObject comObject, string stackTrace)
	{
		CreationTime = creationTime;
		Object = new WeakReference(comObject, trackResurrection: true);
		StackTrace = stackTrace;
	}

	public override string ToString()
	{
		if (!(Object.Target is ComObject comObject))
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Active COM Object: [0x{0:X}] Class: [{1}] Time [{2}] Stack:\r\n{3}", comObject.NativePointer.ToInt64(), comObject.GetType().FullName, CreationTime, StackTrace).AppendLine();
		return stringBuilder.ToString();
	}
}

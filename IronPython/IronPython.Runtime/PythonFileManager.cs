using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

internal class PythonFileManager
{
	private HybridMapping<PythonFile> fileMapping = new HybridMapping<PythonFile>(3);

	private HybridMapping<object> objMapping = new HybridMapping<object>(3);

	public int AddToStrongMapping(PythonFile pf)
	{
		return fileMapping.StrongAdd(pf);
	}

	public int AddToStrongMapping(object o)
	{
		return objMapping.StrongAdd(o);
	}

	public void Remove(PythonFile pf)
	{
		fileMapping.RemoveOnObject(pf);
	}

	public void Remove(object o)
	{
		objMapping.RemoveOnObject(o);
	}

	public PythonFile GetFileFromId(PythonContext context, int id)
	{
		if (!TryGetFileFromId(context, id, out var pf))
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, 9, "Bad file descriptor");
		}
		return pf;
	}

	public bool TryGetFileFromId(PythonContext context, int id, out PythonFile pf)
	{
		switch (id)
		{
		case 0:
			pf = context.GetSystemStateValue("__stdin__") as PythonFile;
			break;
		case 1:
			pf = context.GetSystemStateValue("__stdout__") as PythonFile;
			break;
		case 2:
			pf = context.GetSystemStateValue("__stderr__") as PythonFile;
			break;
		default:
			pf = fileMapping.GetObjectFromId(id);
			break;
		}
		return pf != null;
	}

	public object GetObjectFromId(int id)
	{
		object objectFromId = objMapping.GetObjectFromId(id);
		if (objectFromId == null)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, 9, "Bad file descriptor");
		}
		return objectFromId;
	}

	public int GetIdFromFile(PythonFile pf)
	{
		if (pf.IsConsole)
		{
			for (int i = 0; i < 3; i++)
			{
				if (pf == GetFileFromId(pf.Context, i))
				{
					return i;
				}
			}
		}
		int num = fileMapping.GetIdFromObject(pf);
		if (num == -1)
		{
			num = fileMapping.WeakAdd(pf);
		}
		return num;
	}

	public int GetIdFromObject(object o)
	{
		int num = objMapping.GetIdFromObject(o);
		if (num == -1)
		{
			num = objMapping.WeakAdd(o);
		}
		return num;
	}
}

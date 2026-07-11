using System;
using System.Collections.Generic;
using System.Numerics;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Modules;

[PythonType("_subprocess_handle")]
public class PythonSubprocessHandle
{
	private readonly IntPtr _internalHandle;

	internal bool _closed;

	internal bool _duplicated;

	internal bool _isProcess;

	internal int _exitCode;

	private static List<PythonSubprocessHandle> _active = new List<PythonSubprocessHandle>();

	internal PythonSubprocessHandle(IntPtr handle)
	{
		_internalHandle = handle;
	}

	internal PythonSubprocessHandle(IntPtr handle, bool isProcess)
	{
		_internalHandle = handle;
		_isProcess = isProcess;
	}

	~PythonSubprocessHandle()
	{
		if (_isProcess)
		{
			lock (_active)
			{
				int num = -1;
				int i = 0;
				for (; i < _active.Count; i++)
				{
					if (_active[i] == null)
					{
						num = i;
					}
					else
					{
						if (_active[i].PollForExit())
						{
							_active[i] = null;
							num = i;
							if (_active[i] == this)
							{
								Close();
								return;
							}
						}
						else
						{
							if (_active[i] == this)
							{
								return;
							}
						}
					}
				}
				if (!PollForExit())
				{
					if (num != -1)
					{
						_active[num] = this;
					}
					else
					{
						_active.Add(this);
					}
					return;
				}
				Close();
			}
		}
		Close();
	}

	private bool PollForExit()
	{
		if (PythonSubprocess.WaitForSingleObjectPI(_internalHandle, 0) == 0)
		{
			PythonSubprocess.GetExitCodeProcessPI(_internalHandle, out _exitCode);
			return true;
		}
		return false;
	}

	public void Close()
	{
		lock (this)
		{
			if (!_closed)
			{
				PythonSubprocess.CloseHandle(_internalHandle);
				_closed = true;
				GC.SuppressFinalize(this);
			}
		}
	}

	public object Detach(CodeContext context)
	{
		lock (this)
		{
			if (!_closed)
			{
				_closed = true;
				GC.SuppressFinalize(this);
				return _internalHandle.ToPython();
			}
		}
		return -1;
	}

	public static implicit operator int(PythonSubprocessHandle type)
	{
		return type._internalHandle.ToInt32();
	}

	public static implicit operator BigInteger(PythonSubprocessHandle type)
	{
		return type._internalHandle.ToInt32();
	}

	public static implicit operator IntPtr(PythonSubprocessHandle type)
	{
		return type._internalHandle;
	}
}

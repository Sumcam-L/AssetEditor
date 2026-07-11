using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonSelect
{
	public const string __doc__ = "Provides support for asynchronous socket operations.";

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException("selecterror", dict, "error", "select");
	}

	[Documentation("select(iwtd, owtd, ewtd[, timeout]) -> readlist, writelist, errlist\n\nBlock until sockets are available for reading or writing, until an error\noccurs, or until a the timeout expires. The first three parameters are\nsequences of socket objects (opened using the socket module). The last is a\ntimeout value, given in seconds as a float. If timeout is omitted, select()\nblocks until at least one socket is ready. A timeout of zero never blocks, but\ncan be used for polling.\n\nThe return value is a tuple of lists of sockets that are ready (subsets of\niwtd, owtd, and ewtd). If the timeout occurs before any sockets are ready, a\ntuple of three empty lists is returned.\n\nNote that select() on IronPython works only with sockets; it will not work with\nfiles or other objects.")]
	public static PythonTuple select(CodeContext context, object iwtd, object owtd, object ewtd, [DefaultParameterValue(null)] object timeout)
	{
		ProcessSocketSequence(context, iwtd, out var socketList, out var socketToOriginal);
		ProcessSocketSequence(context, owtd, out var socketList2, out var socketToOriginal2);
		ProcessSocketSequence(context, ewtd, out var socketList3, out var socketToOriginal3);
		int microSeconds;
		if (timeout == null)
		{
			microSeconds = -2;
		}
		else
		{
			if (!Converter.TryConvertToDouble(timeout, out var result))
			{
				throw PythonOps.TypeErrorForTypeMismatch("float or None", timeout);
			}
			microSeconds = (int)(1000000.0 * result);
		}
		try
		{
			Socket.Select(socketList, socketList2, socketList3, microSeconds);
		}
		catch (ArgumentNullException)
		{
			throw MakeException(context, SocketExceptionToTuple(new SocketException(10022)));
		}
		catch (SocketException e)
		{
			throw MakeException(context, SocketExceptionToTuple(e));
		}
		for (int i = 0; i < socketList.__len__(); i++)
		{
			socketList[i] = socketToOriginal[(Socket)socketList[i]];
		}
		for (int j = 0; j < socketList2.__len__(); j++)
		{
			socketList2[j] = socketToOriginal2[(Socket)socketList2[j]];
		}
		for (int k = 0; k < socketList3.__len__(); k++)
		{
			socketList3[k] = socketToOriginal3[(Socket)socketList3[k]];
		}
		return PythonTuple.MakeTuple(socketList, socketList2, socketList3);
	}

	private static PythonTuple SocketExceptionToTuple(SocketException e)
	{
		return PythonTuple.MakeTuple(e.ErrorCode, e.Message);
	}

	private static Exception MakeException(CodeContext context, object value)
	{
		return PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState("selecterror"), value);
	}

	private static void ProcessSocketSequence(CodeContext context, object sequence, out List socketList, out Dictionary<Socket, object> socketToOriginal)
	{
		socketToOriginal = new Dictionary<Socket, object>();
		socketList = new List();
		IEnumerator enumerator = PythonOps.GetEnumerator(sequence);
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			Socket socket = ObjectToSocket(context, current);
			socketList.append(socket);
			socketToOriginal[socket] = current;
		}
	}

	private static Socket ObjectToSocket(CodeContext context, object obj)
	{
		if (obj is PythonSocket.socket socket)
		{
			return socket._socket;
		}
		if (!Converter.TryConvertToInt64(obj, out var result))
		{
			object boundAttr = PythonOps.GetBoundAttr(context, obj, "fileno");
			object value = PythonCalls.Call(context, boundAttr);
			result = Converter.ConvertToInt64(value);
		}
		if (result < 0)
		{
			throw PythonOps.ValueError("file descriptor cannot be a negative number ({0})", result);
		}
		Socket socket2 = PythonSocket.socket.HandleToSocket(result);
		if (socket2 == null)
		{
			SocketException ex = new SocketException(10038);
			throw PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState("selecterror"), PythonTuple.MakeTuple(ex.ErrorCode, ex.Message));
		}
		return socket2;
	}
}

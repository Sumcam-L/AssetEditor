using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonSocket
{
	[PythonType]
	[Documentation("socket([family[, type[, proto]]]) -> socket object\n\nCreate a socket (a network connection endpoint) of the given family, type,\nand protocol. socket() accepts keyword arguments.\n - family (address family) defaults to AF_INET\n - type (socket type) defaults to SOCK_STREAM\n - proto (protocol type) defaults to 0, which specifies the default protocol\n\nThis module supports only IP sockets. It does not support raw or Unix sockets.\nBoth IPv4 and IPv6 are supported.")]
	public class socket : IWeakReferenceable
	{
		private const int DefaultAddressFamily = 2;

		private const int DefaultSocketType = 1;

		private const int DefaultProtocolType = 0;

		public const string __module__ = "socket";

		private static readonly Dictionary<IntPtr, WeakReference> _handleToSocket = new Dictionary<IntPtr, WeakReference>();

		internal Socket _socket;

		internal string _hostName;

		private WeakRefTracker _weakRefTracker;

		private int _referenceCount = 1;

		internal CodeContext _context;

		private int _timeout;

		private IAsyncResult _acceptResult;

		public socket _sock => this;

		public int family => (int)_socket.AddressFamily;

		public int type => (int)_socket.SocketType;

		public int proto => (int)_socket.ProtocolType;

		public socket()
		{
		}

		public void __init__(CodeContext context, [DefaultParameterValue(2)] int addressFamily, [DefaultParameterValue(1)] int socketType, [DefaultParameterValue(0)] int protocolType, [DefaultParameterValue(null)] socket _sock)
		{
			SocketType socketType2 = (SocketType)Enum.ToObject(typeof(SocketType), socketType);
			if (!Enum.IsDefined(typeof(SocketType), socketType2))
			{
				throw MakeException(context, new SocketException(10044));
			}
			AddressFamily addressFamily2 = (AddressFamily)Enum.ToObject(typeof(AddressFamily), addressFamily);
			if (!Enum.IsDefined(typeof(AddressFamily), addressFamily2))
			{
				throw MakeException(context, new SocketException(10047));
			}
			ProtocolType protocolType2 = (ProtocolType)Enum.ToObject(typeof(ProtocolType), protocolType);
			if (!Enum.IsDefined(typeof(ProtocolType), protocolType2))
			{
				throw MakeException(context, new SocketException(10043));
			}
			if (_sock == null)
			{
				Socket socket2;
				try
				{
					socket2 = new Socket(addressFamily2, socketType2, protocolType2);
				}
				catch (SocketException exception)
				{
					throw MakeException(context, exception);
				}
				Initialize(context, socket2);
			}
			else
			{
				_socket = _sock._socket;
				_hostName = _sock._hostName;
				GC.SuppressFinalize(_sock);
				Initialize(context, _socket);
			}
		}

		public void __del__()
		{
			_close();
		}

		~socket()
		{
			_close();
		}

		[Documentation("accept() -> (conn, address)\n\nAccept a connection. The socket must be bound and listening before calling\naccept(). conn is a new socket object connected to the remote host, and\naddress is the remote host's address (e.g. a (host, port) tuple for IPv4).\n\n")]
		public PythonTuple accept()
		{
			Socket socket2;
			try
			{
				if (_acceptResult != null && _acceptResult.IsCompleted)
				{
					socket2 = _socket.EndAccept(_acceptResult);
				}
				else
				{
					int timeout = _timeout;
					if (timeout != 0)
					{
						IAsyncResult asyncResult = _acceptResult ?? _socket.BeginAccept(delegate
						{
						}, null);
						if (!asyncResult.AsyncWaitHandle.WaitOne(timeout))
						{
							_acceptResult = asyncResult;
							throw PythonExceptions.CreateThrowable(PythonSocket.timeout(_context), 0, "timeout");
						}
						socket2 = _socket.EndAccept(asyncResult);
						_acceptResult = null;
					}
					else
					{
						socket2 = _socket.Accept();
					}
				}
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			socket socket3 = new socket(_context, socket2);
			return PythonTuple.MakeTuple(socket3, socket3.getpeername());
		}

		[Documentation("bind(address) -> None\n\nBind to an address. If the socket is already bound, socket.error is raised.\nFor IP sockets, address is a (host, port) tuple. Raw sockets are not\nsupported.\n\nIf you do not care which local address is assigned, set host to INADDR_ANY and\nthe system will assign the most appropriate network address. Similarly, if you\nset port to 0, the system will assign an available port number between 1024\nand 5000.")]
		public void bind(PythonTuple address)
		{
			IPEndPoint localEP = TupleToEndPoint(_context, address, _socket.AddressFamily, out _hostName);
			try
			{
				_socket.Bind(localEP);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("close() -> None\n\nClose the socket. It cannot be used after being closed.")]
		public void close()
		{
			if (_referenceCount < 1)
			{
				_close();
			}
			else
			{
				Interlocked.Decrement(ref _referenceCount);
			}
		}

		internal void _close()
		{
			if (_socket == null)
			{
				return;
			}
			lock (_handleToSocket)
			{
				if (_handleToSocket.TryGetValue(_socket.Handle, out var value))
				{
					Socket socket2 = value.Target as Socket;
					if (socket2 == _socket || socket2 == null)
					{
						_handleToSocket.Remove(_socket.Handle);
					}
				}
			}
			_socket.Close();
			_referenceCount = 0;
		}

		[Documentation("connect(address) -> None\n\nConnect to a remote socket at the given address. IP addresses are expressed\nas (host, port).\n\nRaises socket.error if the socket has been closed, the socket is listening, or\nanother connection error occurred.\nDifference from CPython: connect() does not support timeouts in blocking mode.\nIf a timeout is set and the socket is in blocking mode, connect() will block\nindefinitely until a connection is made or an error occurs.")]
		public void connect(PythonTuple address)
		{
			IPEndPoint remoteEP = TupleToEndPoint(_context, address, _socket.AddressFamily, out _hostName);
			try
			{
				_socket.Connect(remoteEP);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("connect_ex(address) -> error_code\n\nLike connect(), but return an error code insted of raising an exception for\nsocket exceptions raised by the underlying system Connect() call. Note that\nexceptions other than SocketException generated by the system Connect() call\nwill still be raised.\n\nA return value of 0 indicates that the connect call was successful.\nDifference from CPython: connect_ex() does not support timeouts in blocking\nmode. If a timeout is set and the socket is in blocking mode, connect_ex() will\nblock indefinitely until a connection is made or an error occurs.")]
		public int connect_ex(PythonTuple address)
		{
			IPEndPoint remoteEP = TupleToEndPoint(_context, address, _socket.AddressFamily, out _hostName);
			try
			{
				_socket.Connect(remoteEP);
			}
			catch (SocketException ex)
			{
				return ex.ErrorCode;
			}
			return 0;
		}

		[Documentation("fileno() -> file_handle\n\nReturn the underlying system handle for this socket (a 64-bit integer).")]
		public long fileno()
		{
			try
			{
				return _socket.Handle.ToInt64();
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("getpeername() -> address\n\nReturn the address of the remote end of this socket. The address format is\nfamily-dependent (e.g. a (host, port) tuple for IPv4).")]
		public PythonTuple getpeername()
		{
			try
			{
				if (!(_socket.RemoteEndPoint is IPEndPoint endPoint))
				{
					throw MakeException(_context, new SocketException(10047));
				}
				return EndPointToTuple(endPoint);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("getsockname() -> address\n\nReturn the address of the local end of this socket. The address format is\nfamily-dependent (e.g. a (host, port) tuple for IPv4).")]
		public PythonTuple getsockname()
		{
			try
			{
				if (!(_socket.LocalEndPoint is IPEndPoint endPoint))
				{
					throw MakeException(_context, new SocketException(10022));
				}
				return EndPointToTuple(endPoint);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("getsockopt(level, optname[, buflen]) -> value\n\nReturn the value of a socket option. level is one of the SOL_* constants\ndefined in this module, and optname is one of the SO_* constants. If buflen is\nomitted or zero, an integer value is returned. If it is present, a byte string\nwhose maximum length is buflen bytes) is returned. The caller must the decode\nthe resulting byte string.")]
		public object getsockopt(int optionLevel, int optionName, [DefaultParameterValue(0)] int optionLength)
		{
			SocketOptionLevel socketOptionLevel = (SocketOptionLevel)Enum.ToObject(typeof(SocketOptionLevel), optionLevel);
			if (!Enum.IsDefined(typeof(SocketOptionLevel), socketOptionLevel))
			{
				throw MakeException(_context, new SocketException(10022));
			}
			SocketOptionName socketOptionName = (SocketOptionName)Enum.ToObject(typeof(SocketOptionName), optionName);
			if (!Enum.IsDefined(typeof(SocketOptionName), socketOptionName))
			{
				throw MakeException(_context, new SocketException(10042));
			}
			try
			{
				if (optionLength == 0)
				{
					return (int)_socket.GetSocketOption(socketOptionLevel, socketOptionName);
				}
				return _socket.GetSocketOption(socketOptionLevel, socketOptionName, optionLength).MakeString();
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("listen(backlog) -> None\n\nListen for connections on the socket. Backlog is the maximum length of the\npending connections queue. The maximum value is system-dependent.")]
		public void listen(int backlog)
		{
			try
			{
				_socket.Listen(backlog);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("makefile([mode[, bufsize]]) -> file object\n\nReturn a regular file object corresponding to the socket.  The mode\nand bufsize arguments are as for the built-in open() function.")]
		public PythonFile makefile([DefaultParameterValue("r")] string mode, [DefaultParameterValue(8192)] int bufSize)
		{
			Interlocked.Increment(ref _referenceCount);
			return new _fileobject(_context, this, mode, bufSize, close: false);
		}

		[Documentation("recv(bufsize[, flags]) -> string\n\nReceive data from the socket, up to bufsize bytes. For connection-oriented\nprotocols (e.g. SOCK_STREAM), you must first call either connect() or\naccept(). Connectionless protocols (e.g. SOCK_DGRAM) may also use recvfrom().\n\nrecv() blocks until data is available, unless a timeout was set using\nsettimeout(). If the timeout was exceeded, socket.timeout is raised.recv() returns immediately with zero bytes when the connection is closed.")]
		public string recv(int maxBytes, [DefaultParameterValue(0)] int flags)
		{
			byte[] array = new byte[maxBytes];
			int maxBytes2;
			try
			{
				maxBytes2 = _socket.Receive(array, (SocketFlags)flags);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			return array.MakeString(maxBytes2);
		}

		[Documentation("recv_into(buffer, [nbytes[, flags]]) -> nbytes_read\n\nA version of recv() that stores its data into a buffer rather than creating\na new string.  Receive up to buffersize bytes from the socket.  If buffersize\nis not specified (or 0), receive up to the size available in the given buffer.\n\nSee recv() for documentation about the flags.\n")]
		public int recv_into(PythonBuffer buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			if (nbytes < 0)
			{
				throw PythonOps.ValueError("negative buffersize in recv_into");
			}
			throw PythonOps.TypeError("buffer is read-only");
		}

		[Documentation("recv_into(buffer, [nbytes[, flags]]) -> nbytes_read\n\nA version of recv() that stores its data into a buffer rather than creating\na new string.  Receive up to buffersize bytes from the socket.  If buffersize\nis not specified (or 0), receive up to the size available in the given buffer.\n\nSee recv() for documentation about the flags.\n")]
		public int recv_into(string buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			throw PythonOps.TypeError("Cannot use string as modifiable buffer");
		}

		[Documentation("recv_into(buffer, [nbytes[, flags]]) -> nbytes_read\n\nA version of recv() that stores its data into a buffer rather than creating\na new string.  Receive up to buffersize bytes from the socket.  If buffersize\nis not specified (or 0), receive up to the size available in the given buffer.\n\nSee recv() for documentation about the flags.\n")]
		public int recv_into(ArrayModule.array buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			byte[] buffer2 = new byte[byteBufferSize("recv_into", nbytes, buffer.__len__(), buffer.itemsize)];
			int result;
			try
			{
				result = _socket.Receive(buffer2, (SocketFlags)flags);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			buffer.FromStream(new MemoryStream(buffer2), 0);
			return result;
		}

		[Documentation("recv_into(bytearray, [nbytes[, flags]]) -> nbytes_read\n\nA version of recv() that stores its data into a bytearray rather than creating\na new string.  Receive up to buffersize bytes from the socket.  If buffersize\nis not specified (or 0), receive up to the size available in the given buffer.\n\nSee recv() for documentation about the flags.\n")]
		public int recv_into(ByteArray buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			byte[] array = new byte[byteBufferSize("recv_into", nbytes, buffer.Count, 1)];
			int num;
			try
			{
				num = _socket.Receive(array, (SocketFlags)flags);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			for (int i = 0; i < num; i++)
			{
				buffer[i] = array[i];
			}
			return num;
		}

		[Documentation("recvfrom(bufsize[, flags]) -> (string, address)\n\nReceive data from the socket, up to bufsize bytes. string is the data\nreceived, and address (whose format is protocol-dependent) is the address of\nthe socket from which the data was received.")]
		public PythonTuple recvfrom(int maxBytes, [DefaultParameterValue(0)] int flags)
		{
			if (maxBytes < 0)
			{
				throw PythonOps.ValueError("negative buffersize in recvfrom");
			}
			byte[] array = new byte[maxBytes];
			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
			EndPoint remoteEP = iPEndPoint;
			int maxBytes2;
			try
			{
				maxBytes2 = _socket.ReceiveFrom(array, (SocketFlags)flags, ref remoteEP);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			string text = array.MakeString(maxBytes2);
			PythonTuple pythonTuple = EndPointToTuple((IPEndPoint)remoteEP);
			return PythonTuple.MakeTuple(text, pythonTuple);
		}

		[Documentation("recvfrom_into(buffer[, nbytes[, flags]]) -> (nbytes, address info)\n\nLike recv_into(buffer[, nbytes[, flags]]) but also return the sender's address info.\n")]
		public PythonTuple recvfrom_into(PythonBuffer buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			if (nbytes < 0)
			{
				throw PythonOps.ValueError("negative buffersize in recvfrom_into");
			}
			throw PythonOps.TypeError("buffer is read-only");
		}

		[Documentation("recvfrom_into(buffer[, nbytes[, flags]]) -> (nbytes, address info)\n\nLike recv_into(buffer[, nbytes[, flags]]) but also return the sender's address info.\n")]
		public PythonTuple recvfrom_into(string buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			throw PythonOps.TypeError("Cannot use string as modifiable buffer");
		}

		[Documentation("recvfrom_into(buffer[, nbytes[, flags]]) -> (nbytes, address info)\n\nLike recv_into(buffer[, nbytes[, flags]]) but also return the sender's address info.\n")]
		public PythonTuple recvfrom_into(ArrayModule.array buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			byte[] buffer2 = new byte[byteBufferSize("recvfrom_into", nbytes, buffer.__len__(), buffer.itemsize)];
			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
			EndPoint remoteEP = iPEndPoint;
			int num;
			try
			{
				num = _socket.ReceiveFrom(buffer2, (SocketFlags)flags, ref remoteEP);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			buffer.FromStream(new MemoryStream(buffer2), 0);
			PythonTuple pythonTuple = EndPointToTuple((IPEndPoint)remoteEP);
			return PythonTuple.MakeTuple(num, pythonTuple);
		}

		[Documentation("recvfrom_into(buffer[, nbytes[, flags]]) -> (nbytes, address info)\n\nLike recv_into(buffer[, nbytes[, flags]]) but also return the sender's address info.\n")]
		public PythonTuple recvfrom_into(IList<byte> buffer, [DefaultParameterValue(0)] int nbytes, [DefaultParameterValue(0)] int flags)
		{
			byte[] array = new byte[byteBufferSize("recvfrom_into", nbytes, buffer.Count, 1)];
			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
			EndPoint remoteEP = iPEndPoint;
			int num;
			try
			{
				num = _socket.ReceiveFrom(array, (SocketFlags)flags, ref remoteEP);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			for (int i = 0; i < array.Length; i++)
			{
				buffer[i] = array[i];
			}
			PythonTuple pythonTuple = EndPointToTuple((IPEndPoint)remoteEP);
			return PythonTuple.MakeTuple(num, pythonTuple);
		}

		private static int byteBufferSize(string funcName, int nbytes, int bufLength, int itemSize)
		{
			if (nbytes < 0)
			{
				throw PythonOps.ValueError("negative buffersize in " + funcName);
			}
			if (nbytes == 0)
			{
				return bufLength * itemSize;
			}
			int num = nbytes % itemSize;
			return Math.Min((num == 0) ? nbytes : (nbytes + itemSize - num), bufLength * itemSize);
		}

		[Documentation("send(string[, flags]) -> bytes_sent\n\nSend data to the remote socket. The socket must be connected to a remote\nsocket (by calling either connect() or accept(). Returns the number of bytes\nsent to the remote socket.\n\nNote that the successful completion of a send() call does not mean that all of\nthe data was sent. The caller must keep track of the number of bytes sent and\nretry the operation until all of the data has been sent.\n\nAlso note that there is no guarantee that the data you send will appear on the\nnetwork immediately. To increase network efficiency, the underlying system may\ndelay transmission until a significant amount of outgoing data is collected. A\nsuccessful completion of the Send method means that the underlying system has\nhad room to buffer your data for a network send")]
		public int send(string data, [DefaultParameterValue(0)] int flags)
		{
			byte[] buffer = data.MakeByteArray();
			try
			{
				return _socket.Send(buffer, (SocketFlags)flags);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("send(string[, flags]) -> bytes_sent\n\nSend data to the remote socket. The socket must be connected to a remote\nsocket (by calling either connect() or accept(). Returns the number of bytes\nsent to the remote socket.\n\nNote that the successful completion of a send() call does not mean that all of\nthe data was sent. The caller must keep track of the number of bytes sent and\nretry the operation until all of the data has been sent.\n\nAlso note that there is no guarantee that the data you send will appear on the\nnetwork immediately. To increase network efficiency, the underlying system may\ndelay transmission until a significant amount of outgoing data is collected. A\nsuccessful completion of the Send method means that the underlying system has\nhad room to buffer your data for a network send")]
		public int send(PythonBuffer data, [DefaultParameterValue(0)] int flags)
		{
			byte[] byteCache = data.byteCache;
			try
			{
				return _socket.Send(byteCache, (SocketFlags)flags);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("sendall(string[, flags]) -> None\n\nSend data to the remote socket. The socket must be connected to a remote\nsocket (by calling either connect() or accept().\n\nUnlike send(), sendall() blocks until all of the data has been sent or until a\ntimeout or an error occurs. None is returned on success. If an error occurs,\nthere is no way to tell how much data, if any, was sent.\n\nDifference from CPython: timeouts do not function as you would expect. The\nfunction is implemented using multiple calls to send(), so the timeout timer\nis reset after each of those calls. That means that the upper bound on the\ntime that it will take for sendall() to return is the number of bytes in\nstring times the timeout interval.\n\nAlso note that there is no guarantee that the data you send will appear on the\nnetwork immediately. To increase network efficiency, the underlying system may\ndelay transmission until a significant amount of outgoing data is collected. A\nsuccessful completion of the Send method means that the underlying system has\nhad room to buffer your data for a network send")]
		public void sendall(string data, [DefaultParameterValue(0)] int flags)
		{
			byte[] array = data.MakeByteArray();
			try
			{
				int num = array.Length;
				for (int num2 = num; num2 > 0; num2 -= _socket.Send(array, num - num2, num2, (SocketFlags)flags))
				{
				}
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("sendall(string[, flags]) -> None\n\nSend data to the remote socket. The socket must be connected to a remote\nsocket (by calling either connect() or accept().\n\nUnlike send(), sendall() blocks until all of the data has been sent or until a\ntimeout or an error occurs. None is returned on success. If an error occurs,\nthere is no way to tell how much data, if any, was sent.\n\nDifference from CPython: timeouts do not function as you would expect. The\nfunction is implemented using multiple calls to send(), so the timeout timer\nis reset after each of those calls. That means that the upper bound on the\ntime that it will take for sendall() to return is the number of bytes in\nstring times the timeout interval.\n\nAlso note that there is no guarantee that the data you send will appear on the\nnetwork immediately. To increase network efficiency, the underlying system may\ndelay transmission until a significant amount of outgoing data is collected. A\nsuccessful completion of the Send method means that the underlying system has\nhad room to buffer your data for a network send")]
		public void sendall(PythonBuffer data, [DefaultParameterValue(0)] int flags)
		{
			byte[] byteCache = data.byteCache;
			try
			{
				int num = byteCache.Length;
				for (int num2 = num; num2 > 0; num2 -= _socket.Send(byteCache, num - num2, num2, (SocketFlags)flags))
				{
				}
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("sendto(string[, flags], address) -> bytes_sent\n\nSend data to the remote socket. The socket does not need to be connected to a\nremote socket since the address is specified in the call to sendto(). Returns\nthe number of bytes sent to the remote socket.\n\nBlocking sockets will block until the all of the bytes in the buffer are sent.\nSince a nonblocking Socket completes immediately, it might not send all of the\nbytes in the buffer. It is your application's responsibility to keep track of\nthe number of bytes sent and to retry the operation until the application sends\nall of the bytes in the buffer.\n\nNote that there is no guarantee that the data you send will appear on the\nnetwork immediately. To increase network efficiency, the underlying system may\ndelay transmission until a significant amount of outgoing data is collected. A\nsuccessful completion of the Send method means that the underlying system has\nhad room to buffer your data for a network send")]
		public int sendto(string data, int flags, PythonTuple address)
		{
			byte[] buffer = data.MakeByteArray();
			EndPoint remoteEP = TupleToEndPoint(_context, address, _socket.AddressFamily, out _hostName);
			try
			{
				return _socket.SendTo(buffer, (SocketFlags)flags, remoteEP);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("")]
		public int sendto(string data, PythonTuple address)
		{
			return sendto(data, 0, address);
		}

		[Documentation("setblocking(flag) -> None\n\nSet the blocking mode of the socket. If flag is 0, the socket will be set to\nnon-blocking mode; otherwise, it will be set to blocking mode. If the socket is\nin blocking mode, and a method is called (such as send() or recv() which does\nnot complete immediately, the caller will block execution until the requested\noperation completes. In non-blocking mode, a socket.timeout exception would\nwould be raised in this case.\n\nNote that changing blocking mode also affects the timeout setting:\nsetblocking(0) is equivalent to settimeout(0), and setblocking(1) is equivalent\nto settimeout(None).")]
		public void setblocking(int shouldBlock)
		{
			if (shouldBlock == 0)
			{
				settimeout(0);
			}
			else
			{
				settimeout(null);
			}
		}

		[Documentation("settimeout(value) -> None\n\nSet a timeout on blocking socket methods. value may be either None or a\nnon-negative float, with one of the following meanings:\n - None: disable timeouts and block indefinitely - 0.0: don't block at all (return immediately if the operation can be\n   completed; raise socket.error otherwise)\n - float > 0.0: block for up to the specified number of seconds; raise\n   socket.timeout if the operation cannot be completed in time\n\nsettimeout(None) is equivalent to setblocking(1), and settimeout(0.0) is\nequivalent to setblocking(0).\nIf the timeout is non-zero and is less than 0.5, it will be set to 0.5. This\nlimitation is specific to IronPython.\n")]
		public void settimeout(object timeout)
		{
			try
			{
				if (timeout == null)
				{
					_socket.Blocking = true;
					_socket.SendTimeout = 0;
					return;
				}
				double num = Converter.ConvertToDouble(timeout);
				if (num < 0.0)
				{
					throw PythonOps.TypeError("a non-negative float is required");
				}
				_socket.Blocking = num > 0.0;
				_socket.SendTimeout = (int)(num * 1000.0);
				_timeout = (int)(num * 1000.0);
			}
			finally
			{
				_socket.ReceiveTimeout = _socket.SendTimeout;
			}
		}

		[Documentation("gettimeout() -> value\n\nReturn the timeout duration in seconds for this socket as a float. If no\ntimeout is set, return None. For more details on timeouts and blocking, see the\nPython socket module documentation.")]
		public object gettimeout()
		{
			try
			{
				if (_socket.Blocking && _socket.SendTimeout == 0)
				{
					return null;
				}
				return (double)_socket.SendTimeout / 1000.0;
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		[Documentation("setsockopt(level, optname[, value]) -> None\n\nSet the value of a socket option. level is one of the SOL_* constants defined\nin this module, and optname is one of the SO_* constants. value may be either\nan integer or a string containing a binary structure. The caller is responsible\nfor properly encoding the byte string.")]
		public void setsockopt(int optionLevel, int optionName, object value)
		{
			SocketOptionLevel socketOptionLevel = (SocketOptionLevel)Enum.ToObject(typeof(SocketOptionLevel), optionLevel);
			if (!Enum.IsDefined(typeof(SocketOptionLevel), socketOptionLevel))
			{
				throw MakeException(_context, new SocketException(10022));
			}
			SocketOptionName socketOptionName = (SocketOptionName)Enum.ToObject(typeof(SocketOptionName), optionName);
			if (!Enum.IsDefined(typeof(SocketOptionName), socketOptionName))
			{
				throw MakeException(_context, new SocketException(10042));
			}
			try
			{
				if (Converter.TryConvertToInt32(value, out var result))
				{
					_socket.SetSocketOption(socketOptionLevel, socketOptionName, result);
					return;
				}
				if (Converter.TryConvertToString(value, out var result2))
				{
					_socket.SetSocketOption(socketOptionLevel, socketOptionName, result2.MakeByteArray());
					return;
				}
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
			throw PythonOps.TypeError("setsockopt() argument 3 must be int or string");
		}

		[Documentation("shutdown() -> None\n\nReturn the timeout duration in seconds for this socket as a float. If no\ntimeout is set, return None. For more details on timeouts and blocking, see the\nPython socket module documentation.")]
		public void shutdown(int how)
		{
			SocketShutdown socketShutdown = (SocketShutdown)Enum.ToObject(typeof(SocketShutdown), how);
			if (!Enum.IsDefined(typeof(SocketShutdown), socketShutdown))
			{
				throw MakeException(_context, new SocketException(10022));
			}
			try
			{
				_socket.Shutdown(socketShutdown);
			}
			catch (Exception exception)
			{
				throw MakeException(_context, exception);
			}
		}

		public int ioctl(BigInteger cmd, int option)
		{
			return _socket.IOControl((IOControlCode)(long)cmd, BitConverter.GetBytes(option), null);
		}

		public override string ToString()
		{
			try
			{
				return $"<socket object, fd={fileno()}, family={family}, type={type}, protocol={proto}>";
			}
			catch
			{
				return "<socket object, fd=?, family=?, type=, protocol=>";
			}
		}

		internal static Socket HandleToSocket(long handle)
		{
			lock (_handleToSocket)
			{
				if (_handleToSocket.TryGetValue((IntPtr)handle, out var value))
				{
					return value.Target as Socket;
				}
			}
			return null;
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _weakRefTracker;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			_weakRefTracker = value;
			return true;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			_weakRefTracker = value;
		}

		private socket(CodeContext context, Socket socket)
		{
			Initialize(context, socket);
		}

		private void Initialize(CodeContext context, Socket socket)
		{
			_socket = socket;
			_context = context;
			int? defaultTimeout = GetDefaultTimeout(context);
			if (!defaultTimeout.HasValue)
			{
				settimeout(null);
			}
			else
			{
				settimeout((double)defaultTimeout.Value / 1000.0);
			}
			lock (_handleToSocket)
			{
				_handleToSocket[socket.Handle] = new WeakReference(socket);
			}
		}
	}

	private class PythonUserSocketStream : Stream
	{
		private readonly object _userSocket;

		private List<string> _data = new List<string>();

		private int _dataSize;

		private readonly int _bufSize;

		private readonly bool _close;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public PythonUserSocketStream(object userSocket, int bufferSize, bool close)
		{
			_userSocket = userSocket;
			_bufSize = bufferSize;
			_close = close;
		}

		public override void Close()
		{
			Dispose(disposing: false);
		}

		public override void Flush()
		{
			if (_data.Count <= 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string datum in _data)
			{
				stringBuilder.Append(datum);
			}
			DefaultContext.DefaultPythonContext.CallSplat(PythonOps.GetBoundAttr(DefaultContext.Default, _userSocket, "sendall"), stringBuilder.ToString());
			_data.Clear();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			object value = DefaultContext.DefaultPythonContext.CallSplat(PythonOps.GetBoundAttr(DefaultContext.Default, _userSocket, "recv"), count);
			string text = Converter.ConvertToString(value);
			return PythonAsciiEncoding.Instance.GetBytes(text, 0, text.Length, buffer, offset);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			string text = new string(PythonAsciiEncoding.Instance.GetChars(buffer, offset, count));
			_data.Add(text);
			_dataSize += text.Length;
			if (_dataSize > _bufSize)
			{
				Flush();
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}

	[PythonType]
	public class _fileobject : PythonFile
	{
		public new const string name = "<socket>";

		public const string __module__ = "socket";

		private readonly socket _socket;

		private bool _close;

		public object bufsize = 8192;

		public _fileobject(CodeContext context, object socket, [DefaultParameterValue("rb")] string mode, [DefaultParameterValue(-1)] int bufsize, [DefaultParameterValue(false)] bool close)
			: base(PythonContext.GetContext(context))
		{
			_close = close;
			Stream stream = ((socket == null || !(socket.GetType() == typeof(socket)) || !((socket)socket)._socket.Connected) ? ((Stream)new PythonUserSocketStream(socket, GetBufferSize(context, bufsize), close)) : ((Stream)new NetworkStream((_socket = socket as socket)._socket, ownsSocket: false)));
			_isOpen = true;
			base.__init__(stream, Encoding.Default, mode);
		}

		public void __init__(params object[] args)
		{
		}

		public void __init__([ParamDictionary] IDictionary<object, object> kwargs, params object[] args)
		{
		}

		public void __del__()
		{
			if (_socket != null && _isOpen)
			{
				if (_close)
				{
					_socket.close();
				}
				_isOpen = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_socket != null && _isOpen)
			{
				if (_close)
				{
					_socket.close();
				}
				_isOpen = false;
			}
			base.Dispose(disposing);
		}

		public override object close()
		{
			if (!_isOpen)
			{
				return null;
			}
			if (_socket != null && _close)
			{
				_socket.close();
			}
			_isOpen = false;
			return base.close();
		}

		private static int GetBufferSize(CodeContext context, int size)
		{
			if (size == -1)
			{
				return Converter.ConvertToInt32(Getdefault_bufsize(context));
			}
			return size;
		}

		[SpecialName]
		[PropertyMethod]
		[StaticExtensionMethod]
		public static object Getdefault_bufsize(CodeContext context)
		{
			return PythonContext.GetContext(context).GetModuleState(_defaultBufsizeKey);
		}

		[SpecialName]
		[PropertyMethod]
		[StaticExtensionMethod]
		public static void Setdefault_bufsize(CodeContext context, object value)
		{
			PythonContext.GetContext(context).SetModuleState(_defaultBufsizeKey, value);
		}
	}

	public class ssl
	{
		private SslStream _sslStream;

		private socket _socket;

		private readonly X509Certificate2Collection _certCollection;

		private readonly X509Certificate _cert;

		private readonly int _protocol;

		private readonly int _certsMode;

		private readonly bool _validate;

		private readonly bool _serverSide;

		private readonly CodeContext _context;

		private readonly RemoteCertificateValidationCallback _callback;

		private Exception _validationFailure;

		public ssl(CodeContext context, socket sock, [DefaultParameterValue(null)] string keyfile, [DefaultParameterValue(null)] string certfile)
		{
			_context = context;
			_sslStream = new SslStream(new NetworkStream(sock._socket, ownsSocket: false), leaveInnerStreamOpen: true, CertValidationCallback);
			_socket = sock;
			_certCollection = new X509Certificate2Collection();
			_protocol = -1;
			_validate = false;
		}

		internal ssl(CodeContext context, socket sock, bool server_side, [DefaultParameterValue(null)] string keyfile, [DefaultParameterValue(null)] string certfile, [DefaultParameterValue(0)] int certs_mode, [DefaultParameterValue(-1)] int protocol, string cacertsfile)
		{
			if (sock == null)
			{
				throw PythonOps.TypeError("expected socket object, got None");
			}
			if (keyfile == null != (certfile == null))
			{
				throw PythonExceptions.CreateThrowable(PythonSsl.SSLError(context), "When key or certificate is provided both must be provided");
			}
			_serverSide = server_side;
			_certsMode = certs_mode;
			bool validate;
			RemoteCertificateValidationCallback callback;
			switch (certs_mode)
			{
			case 0:
				validate = false;
				callback = CertValidationCallback;
				break;
			case 1:
				validate = true;
				callback = CertValidationCallbackOptional;
				break;
			case 2:
				validate = true;
				callback = CertValidationCallbackRequired;
				break;
			default:
				throw new InvalidOperationException($"bad certs_mode: {certs_mode}");
			}
			_callback = callback;
			if (certfile != null)
			{
				_cert = PythonSsl.ReadCertificate(context, certfile);
			}
			_socket = sock;
			EnsureSslStream(throwWhenNotConnected: false);
			_certCollection = ((cacertsfile != null) ? new X509Certificate2Collection(new X509Certificate2[1] { PythonSsl.ReadCertificate(context, cacertsfile) }) : new X509Certificate2Collection());
			_protocol = protocol;
			_validate = validate;
			_context = context;
		}

		private void EnsureSslStream(bool throwWhenNotConnected)
		{
			if (_sslStream == null && _socket._socket.Connected)
			{
				if (_serverSide)
				{
					_sslStream = new SslStream(new NetworkStream(_socket._socket, ownsSocket: false), leaveInnerStreamOpen: true, _callback);
				}
				else
				{
					_sslStream = new SslStream(new NetworkStream(_socket._socket, ownsSocket: false), leaveInnerStreamOpen: true, _callback, CertSelectLocal);
				}
			}
			if (throwWhenNotConnected && _sslStream == null)
			{
				PythonModule builtinModule = _context.LanguageContext.GetBuiltinModule("socket");
				PythonType socketError = GetSocketError(_context.LanguageContext, builtinModule.__dict__);
				throw PythonExceptions.CreateThrowable(socketError, 10057, "A request to send or receive data was disallowed because the socket is not connected.");
			}
		}

		internal bool CertValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		internal bool CertValidationCallbackOptional(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (!_serverSide && certificate != null && sslPolicyErrors != SslPolicyErrors.None)
			{
				ValidateCertificate(certificate, chain, sslPolicyErrors);
			}
			return true;
		}

		internal X509Certificate CertSelectLocal(object sender, string targetHost, X509CertificateCollection collection, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			if (collection.Count > 0)
			{
				return collection[0];
			}
			return null;
		}

		internal bool CertValidationCallbackRequired(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (!_serverSide)
			{
				if (certificate == null)
				{
					ValidationError(SslPolicyErrors.None);
				}
				else if (sslPolicyErrors != SslPolicyErrors.None)
				{
					ValidateCertificate(certificate, chain, sslPolicyErrors);
				}
			}
			return true;
		}

		private void ValidateCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			chain = new X509Chain();
			chain.ChainPolicy.ExtraStore.AddRange(_certCollection);
			chain.Build((X509Certificate2)certificate);
			if (chain.ChainStatus.Length <= 0)
			{
				return;
			}
			X509ChainStatus[] chainStatus = chain.ChainStatus;
			int num = 0;
			while (num < chainStatus.Length)
			{
				X509ChainStatus x509ChainStatus = chainStatus[num];
				if (x509ChainStatus.Status == X509ChainStatusFlags.UntrustedRoot)
				{
					bool flag = false;
					X509Certificate2Enumerator enumerator = _certCollection.GetEnumerator();
					while (enumerator.MoveNext())
					{
						X509Certificate2 current = enumerator.Current;
						if (certificate.Issuer == current.Subject)
						{
							flag = true;
						}
					}
					if (flag)
					{
						num++;
						continue;
					}
				}
				ValidationError(sslPolicyErrors);
				break;
			}
		}

		private void ValidationError(object reason)
		{
			_validationFailure = PythonExceptions.CreateThrowable(PythonSsl.SSLError(_context), "errors while validating certificate chain: ", reason.ToString());
		}

		public void do_handshake()
		{
			try
			{
				_ = _socket._socket.Available;
			}
			catch (SocketException)
			{
				throw PythonExceptions.CreateThrowable(PythonExceptions.IOError, "socket closed before handshake");
			}
			EnsureSslStream(throwWhenNotConnected: true);
			try
			{
				if (_serverSide)
				{
					_sslStream.AuthenticateAsServer(_cert, _certsMode == 2, GetProtocolTypeServer(_protocol), checkCertificateRevocation: false);
				}
				else
				{
					X509CertificateCollection x509CertificateCollection = new X509CertificateCollection();
					if (_cert != null)
					{
						x509CertificateCollection.Add(_cert);
					}
					_sslStream.AuthenticateAsClient(_socket._hostName, x509CertificateCollection, GetProtocolTypeClient(_protocol), checkCertificateRevocation: false);
				}
			}
			catch (AuthenticationException ex2)
			{
				_socket._socket.Close();
				throw PythonExceptions.CreateThrowable(PythonSsl.SSLError(_context), "errors while performing handshake: ", ex2.ToString());
			}
			if (_validationFailure != null)
			{
				throw _validationFailure;
			}
		}

		public socket shutdown()
		{
			_sslStream.Close();
			return _socket;
		}

		private static SslProtocols GetProtocolTypeServer(int type)
		{
			switch (type)
			{
			case 0:
				return SslProtocols.Ssl2;
			case 1:
				return SslProtocols.Ssl3;
			case -1:
			case 2:
				return SslProtocols.Default | SslProtocols.Ssl2;
			case 3:
				return SslProtocols.Tls;
			default:
				throw new InvalidOperationException("bad ssl protocol type: " + type);
			}
		}

		private static SslProtocols GetProtocolTypeClient(int type)
		{
			switch (type)
			{
			case 0:
				return SslProtocols.Ssl2;
			case -1:
			case 1:
				return SslProtocols.Ssl3;
			case 2:
				return SslProtocols.Ssl2 | SslProtocols.Ssl3;
			case 3:
				return SslProtocols.Tls;
			default:
				throw new InvalidOperationException("bad ssl protocol type: " + type);
			}
		}

		public PythonTuple cipher()
		{
			if (_sslStream != null && _sslStream.IsAuthenticated)
			{
				return PythonTuple.MakeTuple(_sslStream.CipherAlgorithm.ToString(), ProtocolToPython(), _sslStream.CipherStrength);
			}
			return null;
		}

		private string ProtocolToPython()
		{
			return _sslStream.SslProtocol switch
			{
				SslProtocols.Ssl2 => "SSLv2", 
				SslProtocols.Ssl3 => "TLSv1/SSLv3", 
				SslProtocols.Tls => "TLSv1", 
				_ => _sslStream.SslProtocol.ToString(), 
			};
		}

		public object peer_certificate(bool binary_form)
		{
			if (_sslStream != null)
			{
				X509Certificate remoteCertificate = _sslStream.RemoteCertificate;
				if (remoteCertificate != null)
				{
					if (binary_form)
					{
						return remoteCertificate.GetRawCertData().MakeString();
					}
					if (_validate)
					{
						return PythonSsl.CertificateToPython(_context, remoteCertificate, complete: true);
					}
				}
			}
			return null;
		}

		public int pending()
		{
			return _socket._socket.Available;
		}

		[Documentation("issuer() -> issuer_certificate\n\nReturns a string that describes the issuer of the server's certificate. Only useful for debugging purposes.")]
		public string issuer()
		{
			if (_sslStream != null && _sslStream.IsAuthenticated)
			{
				X509Certificate remoteCertificate = _sslStream.RemoteCertificate;
				if (remoteCertificate != null)
				{
					return remoteCertificate.Issuer;
				}
				return string.Empty;
			}
			return string.Empty;
		}

		[Documentation("read([n]) -> buffer_read\n\nIf n is present, reads up to n bytes from the SSL connection. Otherwise, reads to EOF.")]
		public string read(CodeContext context, [DefaultParameterValue(int.MaxValue)] int n)
		{
			EnsureSslStream(throwWhenNotConnected: true);
			try
			{
				byte[] array = new byte[2048];
				MemoryStream memoryStream = new MemoryStream(n);
				int num;
				int num2;
				do
				{
					num = ((n < array.Length) ? n : array.Length);
					num2 = _sslStream.Read(array, 0, num);
					if (num2 > 0)
					{
						memoryStream.Write(array, 0, num2);
						n -= num2;
					}
				}
				while (num2 != 0 && n != 0 && num2 >= num);
				return memoryStream.ToArray().MakeString();
			}
			catch (Exception exception)
			{
				throw MakeException(context, exception);
			}
		}

		[Documentation("server() -> server_certificate\n\nReturns a string that describes the server's certificate. Only useful for debugging purposes.")]
		public string server()
		{
			if (_sslStream != null && _sslStream.IsAuthenticated)
			{
				X509Certificate remoteCertificate = _sslStream.RemoteCertificate;
				if (remoteCertificate != null)
				{
					return remoteCertificate.Subject;
				}
			}
			return string.Empty;
		}

		[Documentation("write(s) -> bytes_sent\n\nWrites the string s through the SSL connection.")]
		public int write(CodeContext context, string data)
		{
			EnsureSslStream(throwWhenNotConnected: true);
			byte[] array = data.MakeByteArray();
			try
			{
				_sslStream.Write(array);
				return array.Length;
			}
			catch (Exception exception)
			{
				throw MakeException(context, exception);
			}
		}
	}

	private const int DefaultBufferSize = 8192;

	public const string __doc__ = "Implementation module for socket operations.\n\nThis module is a loose wrapper around the .NET System.Net.Sockets API, so you\nmay find the corresponding MSDN documentation helpful in decoding error\nmessages and understanding corner cases.\n\nThis implementation of socket differs slightly from the standard CPython\nsocket module. Many of these differences are due to the implementation of the\n.NET socket libraries. These differences are summarized below. For full\ndetails, check the docstrings of the functions mentioned.\n - s.accept(), s.connect(), and s.connect_ex() do not support timeouts.\n - Timeouts in s.sendall() don't work correctly.\n - s.dup() is not implemented.\n - getservbyname() and getservbyport() are not implemented.\n - SSL support is not implemented.\nAn Extra IronPython-specific function is exposed only if the clr module is\nimported:\n - s.HandleToSocket() returns the System.Net.Sockets.Socket object associated\n   with a particular \"file descriptor number\" (as returned by s.fileno()).\n";

	private const string AnyAddrToken = "";

	private const string BroadcastAddrToken = "<broadcast>";

	private const string LocalhostAddrToken = "";

	private const int IPv4AddrBytes = 4;

	private const int IPv6AddrBytes = 16;

	private const double MillisecondsPerSecond = 1000.0;

	public const int AF_APPLETALK = 16;

	public const int AF_DECnet = 12;

	public const int AF_INET = 2;

	public const int AF_INET6 = 23;

	public const int AF_IPX = 6;

	public const int AF_IRDA = 26;

	public const int AF_SNA = 11;

	public const int AF_UNSPEC = 0;

	public const int AI_CANONNAME = 2;

	public const int AI_NUMERICHOST = 4;

	public const int AI_PASSIVE = 1;

	public const int EAI_AGAIN = 11002;

	public const int EAI_BADFLAGS = 10022;

	public const int EAI_FAIL = 11003;

	public const int EAI_FAMILY = 10047;

	public const int EAI_MEMORY = 10055;

	public const int EAI_NODATA = 11001;

	public const int EAI_NONAME = 11001;

	public const int EAI_SERVICE = 10109;

	public const int EAI_SOCKTYPE = 10044;

	public const int EAI_SYSTEM = -1;

	public const int EBADF = 9;

	public const int INADDR_ALLHOSTS_GROUP = -536870911;

	public const int INADDR_ANY = 0;

	public const int INADDR_BROADCAST = -1;

	public const int INADDR_LOOPBACK = 2130706433;

	public const int INADDR_MAX_LOCAL_GROUP = -536870657;

	public const int INADDR_NONE = -1;

	public const int INADDR_UNSPEC_GROUP = -536870912;

	public const int IPPORT_RESERVED = 1024;

	public const int IPPORT_USERRESERVED = 5000;

	public const int IPPROTO_AH = 51;

	public const int IPPROTO_DSTOPTS = 60;

	public const int IPPROTO_ESP = 50;

	public const int IPPROTO_FRAGMENT = 44;

	public const int IPPROTO_GGP = 3;

	public const int IPPROTO_HOPOPTS = 0;

	public const int IPPROTO_ICMP = 1;

	public const int IPPROTO_ICMPV6 = 58;

	public const int IPPROTO_IDP = 22;

	public const int IPPROTO_IGMP = 2;

	public const int IPPROTO_IP = 0;

	public const int IPPROTO_IPV4 = 4;

	public const int IPPROTO_IPV6 = 41;

	public const int IPPROTO_MAX = 256;

	public const int IPPROTO_ND = 77;

	public const int IPPROTO_NONE = 59;

	public const int IPPROTO_PUP = 12;

	public const int IPPROTO_RAW = 255;

	public const int IPPROTO_ROUTING = 43;

	public const int IPPROTO_TCP = 6;

	public const int IPPROTO_UDP = 17;

	public const int IPV6_HOPLIMIT = 21;

	public const int IPV6_JOIN_GROUP = 12;

	public const int IPV6_LEAVE_GROUP = 13;

	public const int IPV6_MULTICAST_HOPS = 10;

	public const int IPV6_MULTICAST_IF = 9;

	public const int IPV6_MULTICAST_LOOP = 11;

	public const int IPV6_PKTINFO = 19;

	public const int IPV6_UNICAST_HOPS = 4;

	public const int IP_ADD_MEMBERSHIP = 12;

	public const int IP_DROP_MEMBERSHIP = 13;

	public const int IP_HDRINCL = 2;

	public const int IP_MULTICAST_IF = 9;

	public const int IP_MULTICAST_LOOP = 11;

	public const int IP_MULTICAST_TTL = 10;

	public const int IP_OPTIONS = 1;

	public const int IP_TOS = 3;

	public const int IP_TTL = 4;

	public const int MSG_DONTROUTE = 4;

	public const int MSG_OOB = 1;

	public const int MSG_PEEK = 2;

	public const int NI_DGRAM = 16;

	public const int NI_MAXHOST = 1025;

	public const int NI_MAXSERV = 32;

	public const int NI_NAMEREQD = 4;

	public const int NI_NOFQDN = 1;

	public const int NI_NUMERICHOST = 2;

	public const int NI_NUMERICSERV = 8;

	public const int SHUT_RD = 0;

	public const int SHUT_RDWR = 2;

	public const int SHUT_WR = 1;

	public const int SOCK_DGRAM = 2;

	public const int SOCK_RAW = 3;

	public const int SOCK_RDM = 4;

	public const int SOCK_SEQPACKET = 5;

	public const int SOCK_STREAM = 1;

	public const int SOL_IP = 0;

	public const int SOL_IPV6 = 41;

	public const int SOL_SOCKET = 65535;

	public const int SOL_TCP = 6;

	public const int SOL_UDP = 17;

	public const int SOMAXCONN = int.MaxValue;

	public const int SO_ACCEPTCONN = 2;

	public const int SO_BROADCAST = 32;

	public const int SO_DEBUG = 1;

	public const int SO_DONTROUTE = 16;

	public const int SO_ERROR = 4103;

	public const int SO_EXCLUSIVEADDRUSE = -5;

	public const int SO_KEEPALIVE = 8;

	public const int SO_LINGER = 128;

	public const int SO_OOBINLINE = 256;

	public const int SO_RCVBUF = 4098;

	public const int SO_RCVLOWAT = 4100;

	public const int SO_RCVTIMEO = 4102;

	public const int SO_REUSEADDR = 4;

	public const int SO_SNDBUF = 4097;

	public const int SO_SNDLOWAT = 4099;

	public const int SO_SNDTIMEO = 4101;

	public const int SO_TYPE = 4104;

	public const int SO_USELOOPBACK = 64;

	public const int TCP_NODELAY = 1;

	public const int RCVALL_ON = 1;

	public const int RCVALL_OFF = 0;

	public const int RCVALL_SOCKETLEVELONLY = 2;

	public const int RCVALL_MAX = 3;

	public const int has_ipv6 = 1;

	private static readonly object _defaultTimeoutKey = new object();

	private static readonly object _defaultBufsizeKey = new object();

	public static PythonTuple _delegate_methods = PythonTuple.MakeTuple("recv", "recvfrom", "recv_into", "recvfrom_into", "send", "sendto");

	public static PythonType SocketType = DynamicHelpers.GetPythonTypeFromType(typeof(socket));

	public static object _GLOBAL_DEFAULT_TIMEOUT = new object();

	public static readonly BigInteger SIO_RCVALL = 2550136833L;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		if (!context.HasModuleState(_defaultTimeoutKey))
		{
			context.SetModuleState(_defaultTimeoutKey, null);
		}
		context.SetModuleState(_defaultBufsizeKey, 8192);
		PythonType socketError = GetSocketError(context, dict);
		context.EnsureModuleException("socketherror", socketError, dict, "herror", "socket");
		context.EnsureModuleException("socketgaierror", socketError, dict, "gaierror", "socket");
		context.EnsureModuleException("sockettimeout", socketError, dict, "timeout", "socket");
	}

	internal static PythonType GetSocketError(PythonContext context, PythonDictionary dict)
	{
		return context.EnsureModuleException("socketerror", PythonExceptions.IOError, dict, "error", "socket");
	}

	[Documentation("Connect to *address* and return the socket object.\n\nConvenience function.  Connect to *address* (a 2-tuple ``(host,\nport)``) and return the socket object.  Passing the optional\n*timeout* parameter will set the timeout on the socket instance\nbefore attempting to connect.  If no *timeout* is supplied, the\nglobal default timeout setting returned by :func:`getdefaulttimeout`\nis used.\n")]
	public static socket create_connection(CodeContext context, PythonTuple address)
	{
		return create_connection(context, address, _GLOBAL_DEFAULT_TIMEOUT);
	}

	[Documentation("Connect to *address* and return the socket object.\n\nConvenience function.  Connect to *address* (a 2-tuple ``(host,\nport)``) and return the socket object.  Passing the optional\n*timeout* parameter will set the timeout on the socket instance\nbefore attempting to connect.  If no *timeout* is supplied, the\nglobal default timeout setting returned by :func:`getdefaulttimeout`\nis used.\n")]
	public static socket create_connection(CodeContext context, PythonTuple address, object timeout)
	{
		return create_connection(context, address, timeout, null);
	}

	public static socket create_connection(CodeContext context, PythonTuple address, object timeout, PythonTuple source_address)
	{
		string value = "getaddrinfo returns an empty list";
		string host = Converter.ConvertToString(address[0]);
		object port = address[1];
		IEnumerator enumerator = getaddrinfo(context, host, port, 0, 1, 0, 0).GetEnumerator();
		while (enumerator.MoveNext())
		{
			PythonTuple pythonTuple = (PythonTuple)enumerator.Current;
			int addressFamily = Converter.ConvertToInt32(pythonTuple[0]);
			int socketType = Converter.ConvertToInt32(pythonTuple[1]);
			int protocolType = Converter.ConvertToInt32(pythonTuple[2]);
			PythonTuple address2 = (PythonTuple)pythonTuple[4];
			socket socket2 = null;
			try
			{
				socket2 = new socket();
				socket2.__init__(context, addressFamily, socketType, protocolType, null);
				if (timeout != _GLOBAL_DEFAULT_TIMEOUT)
				{
					socket2.settimeout(timeout);
				}
				if (source_address != null)
				{
					socket2.bind(source_address);
				}
				socket2.connect(address2);
				return socket2;
			}
			catch (Exception ex)
			{
				if (PythonOps.CheckException(context, ex, error(context)) != null)
				{
					socket2?.close();
					value = ex.Message;
				}
			}
		}
		throw PythonExceptions.CreateThrowableForRaise(context, error(context), value);
	}

	[Documentation("")]
	public static List getaddrinfo(CodeContext context, string host, object port, [DefaultParameterValue(0)] int family, [DefaultParameterValue(0)] int socktype, [DefaultParameterValue(0)] int proto, [DefaultParameterValue(0)] int flags)
	{
		int result;
		if (port == null)
		{
			result = 0;
		}
		else if (port is int)
		{
			result = (int)port;
		}
		else if (port is Extensible<int>)
		{
			result = ((Extensible<int>)port).Value;
		}
		else if (port is string)
		{
			if (!int.TryParse((string)port, out result))
			{
				throw PythonExceptions.CreateThrowable(gaierror(context), "getaddrinfo failed");
			}
		}
		else
		{
			if (!(port is ExtensibleString))
			{
				throw PythonExceptions.CreateThrowable(gaierror(context), "getaddrinfo failed");
			}
			if (!int.TryParse(((ExtensibleString)port).Value, out result))
			{
				throw PythonExceptions.CreateThrowable(gaierror(context), "getaddrinfo failed");
			}
		}
		if (socktype != 0)
		{
			SocketType socketType = (SocketType)Enum.ToObject(typeof(SocketType), socktype);
			if (socketType == System.Net.Sockets.SocketType.Unknown || !Enum.IsDefined(typeof(SocketType), socketType))
			{
				throw PythonExceptions.CreateThrowable(gaierror(context), PythonTuple.MakeTuple(10044, "getaddrinfo failed"));
			}
		}
		AddressFamily addressFamily = (AddressFamily)Enum.ToObject(typeof(AddressFamily), family);
		if (!Enum.IsDefined(typeof(AddressFamily), addressFamily))
		{
			throw PythonExceptions.CreateThrowable(gaierror(context), PythonTuple.MakeTuple(10047, "getaddrinfo failed"));
		}
		Enum.ToObject(typeof(ProtocolType), proto);
		IPAddress[] array = HostToAddresses(context, host, addressFamily);
		List list = new List();
		IPAddress[] array2 = array;
		foreach (IPAddress iPAddress in array2)
		{
			list.append(PythonTuple.MakeTuple((int)iPAddress.AddressFamily, socktype, proto, "", EndPointToTuple(new IPEndPoint(iPAddress, result))));
		}
		return list;
	}

	private static PythonType gaierror(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("socketgaierror");
	}

	[Documentation("getfqdn([hostname_or_ip]) -> hostname\n\nReturn the fully-qualified domain name for the specified hostname or IP\naddress. An unspecified or empty name is interpreted as the local host. If the\nname lookup fails, the passed-in name is returned as-is.")]
	public static string getfqdn(string host)
	{
		host = host.Trim();
		if (host == "<broadcast>")
		{
			return host;
		}
		try
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(host);
			if (hostEntry.HostName.Contains("."))
			{
				return hostEntry.HostName;
			}
			string[] aliases = hostEntry.Aliases;
			foreach (string text in aliases)
			{
				if (text.Contains("."))
				{
					return text;
				}
			}
		}
		catch (SocketException)
		{
		}
		return host;
	}

	[Documentation("")]
	public static string getfqdn()
	{
		return getfqdn("");
	}

	[Documentation("gethostbyname(hostname) -> ip address\n\nReturn the string IPv4 address associated with the given hostname (e.g.\n'10.10.0.1'). The hostname is returned as-is if it an IPv4 address. The empty\nstring is treated as the local host.\n\ngethostbyname() doesn't support IPv6; for IPv4/IPv6 support, use getaddrinfo().")]
	public static string gethostbyname(CodeContext context, string host)
	{
		return HostToAddress(context, host, AddressFamily.InterNetwork).ToString();
	}

	[Documentation("gethostbyname_ex(hostname) -> (hostname, aliases, ip_addresses)\n\nReturn the real host name, a list of aliases, and a list of IP addresses\nassociated with the given hostname. If the hostname is an IPv4 address, the\ntuple ([hostname, [], [hostname]) is returned without doing a DNS lookup.\n\ngethostbyname_ex() doesn't support IPv6; for IPv4/IPv6 support, use\ngetaddrinfo().")]
	public static PythonTuple gethostbyname_ex(CodeContext context, string host)
	{
		List list = PythonOps.MakeList();
		string text;
		List list2;
		if (IPAddress.TryParse(host, out var address))
		{
			if (AddressFamily.InterNetwork != address.AddressFamily)
			{
				throw PythonExceptions.CreateThrowable(gaierror(context), 11001, "no IPv4 addresses associated with host");
			}
			text = host;
			list2 = PythonOps.MakeEmptyList(0);
			list.append(host);
		}
		else
		{
			IPHostEntry hostEntry;
			try
			{
				hostEntry = Dns.GetHostEntry(host);
			}
			catch (SocketException ex)
			{
				throw PythonExceptions.CreateThrowable(gaierror(context), ex.ErrorCode, "no IPv4 addresses associated with host");
			}
			text = hostEntry.HostName;
			list2 = PythonOps.MakeList(hostEntry.Aliases);
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (AddressFamily.InterNetwork == iPAddress.AddressFamily)
				{
					list.append(iPAddress.ToString());
				}
			}
		}
		return PythonTuple.MakeTuple(text, list2, list);
	}

	[Documentation("gethostname() -> hostname\nReturn this machine's hostname")]
	public static string gethostname()
	{
		return Dns.GetHostName();
	}

	[Documentation("gethostbyaddr(host) -> (hostname, aliases, ipaddrs)\n\nReturn a tuple of (primary hostname, alias hostnames, ip addresses). host may\nbe either a hostname or an IP address.")]
	public static object gethostbyaddr(CodeContext context, string host)
	{
		if (host == "")
		{
			host = gethostname();
		}
		host = gethostbyname(context, host);
		IPAddress[] array = null;
		IPHostEntry iPHostEntry = null;
		try
		{
			array = Dns.GetHostAddresses(host);
			iPHostEntry = Dns.GetHostEntry(host);
		}
		catch (Exception exception)
		{
			throw MakeException(context, exception);
		}
		List list = PythonOps.MakeList();
		IPAddress[] array2 = array;
		foreach (IPAddress iPAddress in array2)
		{
			list.append(iPAddress.ToString());
		}
		return PythonTuple.MakeTuple(iPHostEntry.HostName, PythonOps.MakeList(iPHostEntry.Aliases), list);
	}

	[Documentation("getnameinfo(socketaddr, flags) -> (host, port)\nGiven a socket address, the return a tuple of the corresponding hostname and\nport. Available flags:\n - NI_NOFQDN: Return only the hostname part of the domain name for hosts on the\n   same domain as the executing machine.\n - NI_NUMERICHOST: return the numeric form of the host (e.g. '127.0.0.1' or\n   '::1' rather than 'localhost').\n - NI_NAMEREQD: Raise an error if the hostname cannot be looked up.\n - NI_NUMERICSERV: Return string containing the numeric form of the port (e.g.\n   '80' rather than 'http'). This flag is required (see below).\n - NI_DGRAM: Silently ignored (see below).\n\nDifference from CPython: the following flag behavior differs from CPython\nbecause the .NET framework libraries offer no name-to-port conversion APIs:\n - NI_NUMERICSERV: This flag is required because the .NET framework libraries\n   offer no port-to-name mapping APIs. If it is omitted, getnameinfo() will\n   raise a NotImplementedError.\n - The NI_DGRAM flag is ignored because it only applies when NI_NUMERICSERV is\n   omitted. It it were supported, it would return the UDP-based port name\n   rather than the TCP-based port name.\n")]
	public static object getnameinfo(CodeContext context, PythonTuple socketAddr, int flags)
	{
		if (socketAddr.__len__() < 2 || socketAddr.__len__() > 4)
		{
			throw PythonOps.TypeError("socket address must be a 2-tuple (IPv4 or IPv6) or 4-tuple (IPv6)");
		}
		if ((flags & 8) == 0)
		{
			throw PythonOps.NotImplementedError("getnameinfo() required the NI_NUMERICSERV flag (see docstring)");
		}
		string text = Converter.ConvertToString(socketAddr[0]);
		if (text == null)
		{
			throw PythonOps.TypeError("argument 1 must be string");
		}
		int num = 0;
		try
		{
			num = (int)socketAddr[1];
		}
		catch (InvalidCastException)
		{
			throw PythonOps.TypeError("an integer is required");
		}
		string text2 = null;
		string text3 = null;
		IList<IPAddress> list = null;
		try
		{
			list = HostToAddresses(context, text, AddressFamily.InterNetwork);
			if (list.Count < 1)
			{
				throw PythonExceptions.CreateThrowable(error(context), "sockaddr resolved to zero addresses");
			}
		}
		catch (SocketException ex2)
		{
			throw PythonExceptions.CreateThrowable(gaierror(context), ex2.ErrorCode, ex2.Message);
		}
		catch (IndexOutOfRangeException)
		{
			throw PythonExceptions.CreateThrowable(gaierror(context), "sockaddr resolved to zero addresses");
		}
		if (list.Count > 1)
		{
			List<IPAddress> list2 = new List<IPAddress>(list.Count);
			foreach (IPAddress item in list)
			{
				if (item.AddressFamily == AddressFamily.InterNetwork)
				{
					list2.Add(item);
				}
			}
			if (list2.Count > 1)
			{
				throw PythonExceptions.CreateThrowable(error(context), "sockaddr resolved to multiple addresses");
			}
			list = list2;
		}
		if (list.Count < 1)
		{
			throw PythonExceptions.CreateThrowable(error(context), "sockaddr resolved to zero addresses");
		}
		IPHostEntry iPHostEntry = null;
		try
		{
			iPHostEntry = Dns.GetHostEntry(list[0]);
		}
		catch (SocketException ex4)
		{
			throw PythonExceptions.CreateThrowable(gaierror(context), ex4.ErrorCode, ex4.Message);
		}
		text2 = (((flags & 2) != 0) ? list[0].ToString() : (((flags & 1) == 0) ? iPHostEntry.HostName : RemoveLocalDomain(iPHostEntry.HostName)));
		text3 = num.ToString();
		return PythonTuple.MakeTuple(text2, text3);
	}

	[Documentation("getprotobyname(protoname) -> integer proto\n\nGiven a string protocol name (e.g. \"udp\"), return the associated integer\nprotocol number, suitable for passing to socket(). The name is case\ninsensitive.\n\nRaises socket.error if no protocol number can be found.")]
	public static object getprotobyname(CodeContext context, string protocolName)
	{
		return protocolName.ToLower() switch
		{
			"ah" => 51, 
			"esp" => 50, 
			"dstopts" => 60, 
			"fragment" => 44, 
			"ggp" => 3, 
			"icmp" => 1, 
			"icmpv6" => 58, 
			"ip" => 0, 
			"ipv4" => 4, 
			"ipv6" => 41, 
			"nd" => 77, 
			"none" => 59, 
			"pup" => 12, 
			"raw" => 255, 
			"routing" => 43, 
			"tcp" => 6, 
			"udp" => 17, 
			_ => throw PythonExceptions.CreateThrowable(error(context), "protocol not found"), 
		};
	}

	[Documentation("getservbyname(service_name[, protocol_name]) -> port\n\nNot implemented.")]
	public static int getservbyname(string serviceName, [DefaultParameterValue(null)] string protocolName)
	{
		throw PythonOps.NotImplementedError("name to service conversion not supported");
	}

	[Documentation("getservbyport(port[, protocol_name]) -> service_name\n\nNot implemented.")]
	public static string getservbyport(int port, [DefaultParameterValue(null)] string protocolName)
	{
		throw PythonOps.NotImplementedError("service to name conversion not supported");
	}

	[Documentation("ntohl(x) -> integer\n\nConvert a 32-bit integer from network byte order to host byte order.")]
	public static object ntohl(object x)
	{
		int num = IPAddress.NetworkToHostOrder(SignInsensitiveToInt32(x));
		if (num < 0)
		{
			return (BigInteger)(uint)num;
		}
		return num;
	}

	[Documentation("ntohs(x) -> integer\n\nConvert a 16-bit integer from network byte order to host byte order.")]
	public static int ntohs(object x)
	{
		return (ushort)IPAddress.NetworkToHostOrder(SignInsensitiveToInt16(x));
	}

	[Documentation("htonl(x) -> integer\n\nConvert a 32bit integer from host byte order to network byte order.")]
	public static object htonl(object x)
	{
		int num = IPAddress.HostToNetworkOrder(SignInsensitiveToInt32(x));
		if (num < 0)
		{
			return (BigInteger)(uint)num;
		}
		return num;
	}

	[Documentation("htons(x) -> integer\n\nConvert a 16-bit integer from host byte order to network byte order.")]
	public static int htons(object x)
	{
		return (ushort)IPAddress.HostToNetworkOrder(SignInsensitiveToInt16(x));
	}

	private static int SignInsensitiveToInt32(object x)
	{
		BigInteger bigInteger = Converter.ConvertToBigInteger(x);
		if (bigInteger < 0L)
		{
			throw PythonOps.OverflowError("can't convert negative number to unsigned long");
		}
		if (bigInteger <= 2147483647L)
		{
			return (int)bigInteger;
		}
		return (int)(uint)bigInteger;
	}

	private static short SignInsensitiveToInt16(object x)
	{
		BigInteger bigInteger = Converter.ConvertToBigInteger(x);
		if (bigInteger < 0L)
		{
			throw PythonOps.OverflowError("can't convert negative number to unsigned long");
		}
		if (bigInteger <= 32767L)
		{
			return (short)bigInteger;
		}
		return (short)(ushort)bigInteger;
	}

	[Documentation("inet_pton(addr_family, ip_string) -> packed_ip\n\nConvert an IP address (in string format, e.g. '127.0.0.1' or '::1') to a 32-bit\npacked binary format, as 4-byte (IPv4) or 16-byte (IPv6) string. The return\nformat matches the format of the standard C library's in_addr or in6_addr\nstruct.\n\nIf the address format is invalid, socket.error will be raised. Validity is\ndetermined by the .NET System.Net.IPAddress.Parse() method.\n\ninet_pton() supports IPv4 and IPv6.")]
	public static string inet_pton(CodeContext context, int addressFamily, string ipString)
	{
		if (addressFamily != 2 && addressFamily != 23)
		{
			throw MakeException(context, new SocketException(10047));
		}
		IPAddress iPAddress;
		try
		{
			iPAddress = IPAddress.Parse(ipString);
			if (addressFamily != (int)iPAddress.AddressFamily)
			{
				throw MakeException(context, new SocketException(10047));
			}
		}
		catch (FormatException)
		{
			throw PythonExceptions.CreateThrowable(error(context), "illegal IP address passed to inet_pton");
		}
		return iPAddress.GetAddressBytes().MakeString();
	}

	[Documentation("inet_ntop(address_family, packed_ip) -> ip_string\n\nConvert a packed IP address (a 4-byte [IPv4] or 16-byte [IPv6] string) to a\nstring IP address (e.g. '127.0.0.1' or '::1').\n\nThe input format matches the format of the standard C library's in_addr or\nin6_addr struct. If the input string is not exactly 4 bytes or 16 bytes,\nsocket.error will be raised.\n\ninet_ntop() supports IPv4 and IPv6.")]
	public static string inet_ntop(CodeContext context, int addressFamily, string packedIP)
	{
		if ((packedIP.Length != 4 || addressFamily != 2) && (packedIP.Length != 16 || addressFamily != 23))
		{
			throw PythonExceptions.CreateThrowable(error(context), "invalid length of packed IP address string");
		}
		byte[] array = packedIP.MakeByteArray();
		if (addressFamily == 23)
		{
			return IPv6BytesToColonHex(array);
		}
		return new IPAddress(array).ToString();
	}

	[Documentation("inet_aton(ip_string) -> packed_ip\nConvert an IP address (in string dotted quad format, e.g. '127.0.0.1') to a\n32-bit packed binary format, as four-character string. The return format\nmatches the format of the standard C library's in_addr struct.\n\nIf the address format is invalid, socket.error will be raised. Validity is\ndetermined by the .NET System.Net.IPAddress.Parse() method.\n\ninet_aton() supports only IPv4.")]
	public static string inet_aton(CodeContext context, string ipString)
	{
		return inet_pton(context, 2, ipString);
	}

	[Documentation("inet_ntoa(packed_ip) -> ip_string\n\nConvert a packed IP address (a 4-byte string) to a string IP address (in dotted\nquad format, e.g. '127.0.0.1'). The input format matches the format of the\nstandard C library's in_addr struct.\n\nIf the input string is not exactly 4 bytes, socket.error will be raised.\n\ninet_ntoa() supports only IPv4.")]
	public static string inet_ntoa(CodeContext context, string packedIP)
	{
		return inet_ntop(context, 2, packedIP);
	}

	[Documentation("getdefaulttimeout() -> timeout\n\nReturn the default timeout for new socket objects in seconds as a float. A\nvalue of None means that sockets have no timeout and begin in blocking mode.\nThe default value when the module is imported is None.")]
	public static object getdefaulttimeout(CodeContext context)
	{
		int? defaultTimeout = GetDefaultTimeout(context);
		if (!defaultTimeout.HasValue)
		{
			return null;
		}
		return (double)defaultTimeout.Value / 1000.0;
	}

	[Documentation("setdefaulttimeout(timeout) -> None\n\nSet the default timeout for new socket objects. timeout must be either None,\nmeaning that sockets have no timeout and start in blocking mode, or a\nnon-negative float that specifies the default timeout in seconds.")]
	public static void setdefaulttimeout(CodeContext context, object timeout)
	{
		if (timeout == null)
		{
			SetDefaultTimeout(context, null);
			return;
		}
		double num = Converter.ConvertToDouble(timeout);
		if (num < 0.0)
		{
			throw PythonOps.ValueError("a non-negative float is required");
		}
		SetDefaultTimeout(context, (int)(num * 1000.0));
	}

	internal static Exception MakeException(CodeContext context, Exception exception)
	{
		if (exception is SocketException)
		{
			SocketException ex = (SocketException)exception;
			SocketError socketErrorCode = ex.SocketErrorCode;
			if (socketErrorCode == SocketError.NotConnected || socketErrorCode == SocketError.TimedOut)
			{
				return PythonExceptions.CreateThrowable(timeout(context), ex.ErrorCode, ex.Message);
			}
			return PythonExceptions.CreateThrowable(error(context), ex.ErrorCode, ex.Message);
		}
		if (exception is ObjectDisposedException)
		{
			return PythonExceptions.CreateThrowable(error(context), 9, "the socket is closed");
		}
		if (exception is InvalidOperationException)
		{
			return MakeException(context, new SocketException(10022));
		}
		return exception;
	}

	private static string IPv6BytesToColonHex(byte[] ipBytes)
	{
		int[] array = new int[8];
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				array[i] <<= 8;
				array[i] += ipBytes[i * 2 + j];
			}
		}
		int num = 0;
		int num2 = 0;
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] != 0)
			{
				continue;
			}
			for (int l = k; l < array.Length; l++)
			{
				if (array[l] != 0)
				{
					k += num2;
					break;
				}
				if (l - k + 1 > num2)
				{
					num = k;
					num2 = l - k + 1;
				}
			}
		}
		StringBuilder stringBuilder = new StringBuilder(48);
		for (int m = 0; m < array.Length; m++)
		{
			if (m != 0)
			{
				stringBuilder.Append(':');
			}
			if (num2 > 0 && m == num)
			{
				if (num == 0)
				{
					stringBuilder.Append(':');
				}
				if (num + num2 == array.Length)
				{
					stringBuilder.Append(':');
				}
				m += num2 - 1;
			}
			else
			{
				stringBuilder.Append(array[m].ToString("x"));
			}
		}
		return stringBuilder.ToString();
	}

	private static string ConvertSpecialAddresses(string host)
	{
		return host switch
		{
			"" => IPAddress.Any.ToString(), 
			"<broadcast>" => IPAddress.Broadcast.ToString(), 
			_ => host, 
		};
	}

	private static IPAddress HostToAddress(CodeContext context, string host, AddressFamily family)
	{
		return HostToAddresses(context, host, family)[0];
	}

	private static IPAddress[] HostToAddresses(CodeContext context, string host, AddressFamily family)
	{
		host = ConvertSpecialAddresses(host);
		try
		{
			bool flag = true;
			int num = 0;
			string text = host;
			foreach (char c in text)
			{
				if (!char.IsNumber(c) && c != '.')
				{
					flag = false;
				}
				else if (c == '.')
				{
					num++;
				}
			}
			if (flag)
			{
				if (num == 3 && IPAddress.TryParse(host, out var address) && (family == AddressFamily.Unspecified || family == address.AddressFamily))
				{
					return new IPAddress[1] { address };
				}
			}
			else
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(host);
				List<IPAddress> list = new List<IPAddress>();
				IPAddress[] addressList = hostEntry.AddressList;
				foreach (IPAddress iPAddress in addressList)
				{
					if (family == AddressFamily.Unspecified || family == iPAddress.AddressFamily)
					{
						list.Add(iPAddress);
					}
				}
				if (list.Count > 0)
				{
					return list.ToArray();
				}
			}
			throw new SocketException(11001);
		}
		catch (SocketException ex)
		{
			throw PythonExceptions.CreateThrowable(gaierror(context), ex.ErrorCode, "no addresses of the specified family associated with host");
		}
	}

	private static string RemoveLocalDomain(string fqdn)
	{
		char[] separator = new char[1] { '.' };
		string[] array = getfqdn().Split(separator, 2);
		string[] array2 = fqdn.Split(separator, 2);
		if (array.Length < 2 || array2.Length < 2)
		{
			return fqdn;
		}
		if (array[1] == array2[1])
		{
			return array2[0];
		}
		return fqdn;
	}

	private static IPEndPoint TupleToEndPoint(CodeContext context, PythonTuple address, AddressFamily family, out string host)
	{
		if (address.__len__() != 2 && address.__len__() != 4)
		{
			throw PythonOps.TypeError("address tuple must have exactly 2 (IPv4) or exactly 4 (IPv6) elements");
		}
		try
		{
			host = Converter.ConvertToString(address[0]);
		}
		catch (ArgumentTypeException)
		{
			throw PythonOps.TypeError("host must be string");
		}
		int num;
		try
		{
			num = PythonContext.GetContext(context).ConvertToInt32(address[1]);
		}
		catch (ArgumentTypeException)
		{
			throw PythonOps.TypeError("port must be integer");
		}
		if (num < 0 || num > 65535)
		{
			throw PythonOps.OverflowError("getsockaddrarg: port must be 0-65535");
		}
		IPAddress address2 = HostToAddress(context, host, family);
		if (address.__len__() == 2)
		{
			return new IPEndPoint(address2, num);
		}
		try
		{
			Converter.ConvertToInt64(address[2]);
		}
		catch (ArgumentTypeException)
		{
			throw PythonOps.TypeError("flowinfo must be integer");
		}
		long scopeId;
		try
		{
			scopeId = Converter.ConvertToInt64(address[3]);
		}
		catch (ArgumentTypeException)
		{
			throw PythonOps.TypeError("scopeid must be integer");
		}
		IPEndPoint iPEndPoint = new IPEndPoint(address2, num);
		iPEndPoint.Address.ScopeId = scopeId;
		return iPEndPoint;
	}

	private static PythonTuple EndPointToTuple(IPEndPoint endPoint)
	{
		string text = endPoint.Address.ToString();
		int port = endPoint.Port;
		switch (endPoint.Address.AddressFamily)
		{
		case AddressFamily.InterNetwork:
			return PythonTuple.MakeTuple(text, port);
		case AddressFamily.InterNetworkV6:
		{
			long num = 0L;
			long scopeId = endPoint.Address.ScopeId;
			return PythonTuple.MakeTuple(text, port, num, scopeId);
		}
		default:
			throw new SocketException(10047);
		}
	}

	private static int? GetDefaultTimeout(CodeContext context)
	{
		return (int?)PythonContext.GetContext(context).GetModuleState(_defaultTimeoutKey);
	}

	private static void SetDefaultTimeout(CodeContext context, int? timeout)
	{
		PythonContext.GetContext(context).SetModuleState(_defaultTimeoutKey, timeout);
	}

	private static PythonType error(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("socketerror");
	}

	private static PythonType herror(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("socketherror");
	}

	private static PythonType timeout(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("sockettimeout");
	}
}

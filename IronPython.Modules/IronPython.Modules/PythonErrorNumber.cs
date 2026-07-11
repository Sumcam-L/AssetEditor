using System.Runtime.CompilerServices;
using IronPython.Runtime;

namespace IronPython.Modules;

public static class PythonErrorNumber
{
	public const string __doc__ = "Provides a list of common error numbers.  These numbers are frequently reported in various exceptions.";

	public const int E2BIG = 7;

	public const int EACCES = 13;

	public const int EADDRINUSE = 10048;

	public const int EADDRNOTAVAIL = 10049;

	public const int EAFNOSUPPORT = 10047;

	public const int EAGAIN = 11;

	public const int EALREADY = 10037;

	public const int EBADF = 9;

	public const int EBUSY = 16;

	public const int ECHILD = 10;

	public const int ECONNABORTED = 10053;

	public const int ECONNREFUSED = 10061;

	public const int ECONNRESET = 10054;

	public const int EDEADLK = 36;

	public const int EDEADLOCK = 36;

	public const int EDESTADDRREQ = 10039;

	public const int EDOM = 33;

	public const int EDQUOT = 10069;

	public const int EEXIST = 17;

	public const int EFAULT = 14;

	public const int EFBIG = 27;

	public const int EHOSTDOWN = 10064;

	public const int EHOSTUNREACH = 10065;

	public const int EILSEQ = 42;

	public const int EINPROGRESS = 10036;

	public const int EINTR = 4;

	public const int EINVAL = 22;

	public const int EIO = 5;

	public const int EISCONN = 10056;

	public const int EISDIR = 21;

	public const int ELOOP = 10062;

	public const int EMFILE = 24;

	public const int EMLINK = 31;

	public const int EMSGSIZE = 10040;

	public const int ENAMETOOLONG = 38;

	public const int ENETDOWN = 10050;

	public const int ENETRESET = 10052;

	public const int ENETUNREACH = 10051;

	public const int ENFILE = 23;

	public const int ENOBUFS = 10055;

	public const int ENODEV = 19;

	public const int ENOENT = 2;

	public const int ENOEXEC = 8;

	public const int ENOLCK = 39;

	public const int ENOMEM = 12;

	public const int ENOPROTOOPT = 10042;

	public const int ENOSPC = 28;

	public const int ENOSYS = 40;

	public const int ENOTCONN = 10057;

	public const int ENOTDIR = 20;

	public const int ENOTEMPTY = 41;

	public const int ENOTSOCK = 10038;

	public const int ENOTTY = 25;

	public const int ENXIO = 6;

	public const int EOPNOTSUPP = 10045;

	public const int EPERM = 1;

	public const int EPFNOSUPPORT = 10046;

	public const int EPIPE = 32;

	public const int EPROTONOSUPPORT = 10043;

	public const int EPROTOTYPE = 10041;

	public const int ERANGE = 34;

	public const int EREMOTE = 10071;

	public const int EROFS = 30;

	public const int ESHUTDOWN = 10058;

	public const int ESOCKTNOSUPPORT = 10044;

	public const int ESPIPE = 29;

	public const int ESRCH = 3;

	public const int ESTALE = 10070;

	public const int ETIMEDOUT = 10060;

	public const int ETOOMANYREFS = 10059;

	public const int EUSERS = 10068;

	public const int EWOULDBLOCK = 10035;

	public const int EXDEV = 18;

	public const int WSABASEERR = 10000;

	public const int WSAEACCES = 10013;

	public const int WSAEADDRINUSE = 10048;

	public const int WSAEADDRNOTAVAIL = 10049;

	public const int WSAEAFNOSUPPORT = 10047;

	public const int WSAEALREADY = 10037;

	public const int WSAEBADF = 10009;

	public const int WSAECONNABORTED = 10053;

	public const int WSAECONNREFUSED = 10061;

	public const int WSAECONNRESET = 10054;

	public const int WSAEDESTADDRREQ = 10039;

	public const int WSAEDISCON = 10101;

	public const int WSAEDQUOT = 10069;

	public const int WSAEFAULT = 10014;

	public const int WSAEHOSTDOWN = 10064;

	public const int WSAEHOSTUNREACH = 10065;

	public const int WSAEINPROGRESS = 10036;

	public const int WSAEINTR = 10004;

	public const int WSAEINVAL = 10022;

	public const int WSAEISCONN = 10056;

	public const int WSAELOOP = 10062;

	public const int WSAEMFILE = 10024;

	public const int WSAEMSGSIZE = 10040;

	public const int WSAENAMETOOLONG = 10063;

	public const int WSAENETDOWN = 10050;

	public const int WSAENETRESET = 10052;

	public const int WSAENETUNREACH = 10051;

	public const int WSAENOBUFS = 10055;

	public const int WSAENOPROTOOPT = 10042;

	public const int WSAENOTCONN = 10057;

	public const int WSAENOTEMPTY = 10066;

	public const int WSAENOTSOCK = 10038;

	public const int WSAEOPNOTSUPP = 10045;

	public const int WSAEPFNOSUPPORT = 10046;

	public const int WSAEPROCLIM = 10067;

	public const int WSAEPROTONOSUPPORT = 10043;

	public const int WSAEPROTOTYPE = 10041;

	public const int WSAEREMOTE = 10071;

	public const int WSAESHUTDOWN = 10058;

	public const int WSAESOCKTNOSUPPORT = 10044;

	public const int WSAESTALE = 10070;

	public const int WSAETIMEDOUT = 10060;

	public const int WSAETOOMANYREFS = 10059;

	public const int WSAEUSERS = 10068;

	public const int WSAEWOULDBLOCK = 10035;

	public const int WSANOTINITIALISED = 10093;

	public const int WSASYSNOTREADY = 10091;

	public const int WSAVERNOTSUPPORTED = 10092;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		PythonDictionary pythonDictionary = new PythonDictionary();
		pythonDictionary[7] = "E2BIG";
		pythonDictionary[13] = "EACCES";
		pythonDictionary[10048] = "EADDRINUSE";
		pythonDictionary[10049] = "EADDRNOTAVAIL";
		pythonDictionary[10047] = "EAFNOSUPPORT";
		pythonDictionary[11] = "EAGAIN";
		pythonDictionary[10037] = "EALREADY";
		pythonDictionary[9] = "EBADF";
		pythonDictionary[16] = "EBUSY";
		pythonDictionary[10] = "ECHILD";
		pythonDictionary[10053] = "ECONNABORTED";
		pythonDictionary[10061] = "ECONNREFUSED";
		pythonDictionary[10054] = "ECONNRESET";
		pythonDictionary[36] = "EDEADLK";
		pythonDictionary[36] = "EDEADLOCK";
		pythonDictionary[10039] = "EDESTADDRREQ";
		pythonDictionary[33] = "EDOM";
		pythonDictionary[10069] = "EDQUOT";
		pythonDictionary[17] = "EEXIST";
		pythonDictionary[14] = "EFAULT";
		pythonDictionary[27] = "EFBIG";
		pythonDictionary[10064] = "EHOSTDOWN";
		pythonDictionary[10065] = "EHOSTUNREACH";
		pythonDictionary[42] = "EILSEQ";
		pythonDictionary[10036] = "EINPROGRESS";
		pythonDictionary[4] = "EINTR";
		pythonDictionary[22] = "EINVAL";
		pythonDictionary[5] = "EIO";
		pythonDictionary[10056] = "EISCONN";
		pythonDictionary[21] = "EISDIR";
		pythonDictionary[10062] = "ELOOP";
		pythonDictionary[24] = "EMFILE";
		pythonDictionary[31] = "EMLINK";
		pythonDictionary[10040] = "EMSGSIZE";
		pythonDictionary[38] = "ENAMETOOLONG";
		pythonDictionary[10050] = "ENETDOWN";
		pythonDictionary[10052] = "ENETRESET";
		pythonDictionary[10051] = "ENETUNREACH";
		pythonDictionary[23] = "ENFILE";
		pythonDictionary[10055] = "ENOBUFS";
		pythonDictionary[19] = "ENODEV";
		pythonDictionary[2] = "ENOENT";
		pythonDictionary[8] = "ENOEXEC";
		pythonDictionary[39] = "ENOLCK";
		pythonDictionary[12] = "ENOMEM";
		pythonDictionary[10042] = "ENOPROTOOPT";
		pythonDictionary[28] = "ENOSPC";
		pythonDictionary[40] = "ENOSYS";
		pythonDictionary[10057] = "ENOTCONN";
		pythonDictionary[20] = "ENOTDIR";
		pythonDictionary[41] = "ENOTEMPTY";
		pythonDictionary[10038] = "ENOTSOCK";
		pythonDictionary[25] = "ENOTTY";
		pythonDictionary[6] = "ENXIO";
		pythonDictionary[10045] = "EOPNOTSUPP";
		pythonDictionary[1] = "EPERM";
		pythonDictionary[10046] = "EPFNOSUPPORT";
		pythonDictionary[32] = "EPIPE";
		pythonDictionary[10043] = "EPROTONOSUPPORT";
		pythonDictionary[10041] = "EPROTOTYPE";
		pythonDictionary[34] = "ERANGE";
		pythonDictionary[10071] = "EREMOTE";
		pythonDictionary[30] = "EROFS";
		pythonDictionary[10058] = "ESHUTDOWN";
		pythonDictionary[10044] = "ESOCKTNOSUPPORT";
		pythonDictionary[29] = "ESPIPE";
		pythonDictionary[3] = "ESRCH";
		pythonDictionary[10070] = "ESTALE";
		pythonDictionary[10060] = "ETIMEDOUT";
		pythonDictionary[10059] = "ETOOMANYREFS";
		pythonDictionary[10068] = "EUSERS";
		pythonDictionary[10035] = "EWOULDBLOCK";
		pythonDictionary[18] = "EXDEV";
		pythonDictionary[10000] = "WSABASEERR";
		pythonDictionary[10013] = "WSAEACCES";
		pythonDictionary[10048] = "WSAEADDRINUSE";
		pythonDictionary[10049] = "WSAEADDRNOTAVAIL";
		pythonDictionary[10047] = "WSAEAFNOSUPPORT";
		pythonDictionary[10037] = "WSAEALREADY";
		pythonDictionary[10009] = "WSAEBADF";
		pythonDictionary[10053] = "WSAECONNABORTED";
		pythonDictionary[10061] = "WSAECONNREFUSED";
		pythonDictionary[10054] = "WSAECONNRESET";
		pythonDictionary[10039] = "WSAEDESTADDRREQ";
		pythonDictionary[10101] = "WSAEDISCON";
		pythonDictionary[10069] = "WSAEDQUOT";
		pythonDictionary[10014] = "WSAEFAULT";
		pythonDictionary[10064] = "WSAEHOSTDOWN";
		pythonDictionary[10065] = "WSAEHOSTUNREACH";
		pythonDictionary[10036] = "WSAEINPROGRESS";
		pythonDictionary[10004] = "WSAEINTR";
		pythonDictionary[10022] = "WSAEINVAL";
		pythonDictionary[10056] = "WSAEISCONN";
		pythonDictionary[10062] = "WSAELOOP";
		pythonDictionary[10024] = "WSAEMFILE";
		pythonDictionary[10040] = "WSAEMSGSIZE";
		pythonDictionary[10063] = "WSAENAMETOOLONG";
		pythonDictionary[10050] = "WSAENETDOWN";
		pythonDictionary[10052] = "WSAENETRESET";
		pythonDictionary[10051] = "WSAENETUNREACH";
		pythonDictionary[10055] = "WSAENOBUFS";
		pythonDictionary[10042] = "WSAENOPROTOOPT";
		pythonDictionary[10057] = "WSAENOTCONN";
		pythonDictionary[10066] = "WSAENOTEMPTY";
		pythonDictionary[10038] = "WSAENOTSOCK";
		pythonDictionary[10045] = "WSAEOPNOTSUPP";
		pythonDictionary[10046] = "WSAEPFNOSUPPORT";
		pythonDictionary[10067] = "WSAEPROCLIM";
		pythonDictionary[10043] = "WSAEPROTONOSUPPORT";
		pythonDictionary[10041] = "WSAEPROTOTYPE";
		pythonDictionary[10071] = "WSAEREMOTE";
		pythonDictionary[10058] = "WSAESHUTDOWN";
		pythonDictionary[10044] = "WSAESOCKTNOSUPPORT";
		pythonDictionary[10070] = "WSAESTALE";
		pythonDictionary[10060] = "WSAETIMEDOUT";
		pythonDictionary[10059] = "WSAETOOMANYREFS";
		pythonDictionary[10068] = "WSAEUSERS";
		pythonDictionary[10035] = "WSAEWOULDBLOCK";
		pythonDictionary[10093] = "WSANOTINITIALISED";
		pythonDictionary[10091] = "WSASYSNOTREADY";
		pythonDictionary[10092] = "WSAVERNOTSUPPORTED";
		dict["errorcode"] = pythonDictionary;
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Windows.Forms;

namespace Firaxis.Error;

public static class ErrorHandling
{
	private static string sm_sAppName = "Unknown App";

	private static string sm_sAppVersion = string.Empty;

	private static IErrorReportForm sm_kErrorForm = null;

	private static bool sm_bSendErrorsToConsole = false;

	public static bool ShowErrorMessages = true;

	private static List<string> sm_ErrorReportRecipients = new List<string>();

	private static List<string> sm_ErrorReportAttachments = new List<string>();

	public static string AppName
	{
		get
		{
			return sm_sAppName;
		}
		set
		{
			sm_sAppName = value;
		}
	}

	public static string AppVersion
	{
		get
		{
			return sm_sAppVersion;
		}
		set
		{
			sm_sAppVersion = value;
		}
	}

	public static IErrorReportForm ErrorReportForm
	{
		get
		{
			return sm_kErrorForm;
		}
		set
		{
			sm_kErrorForm = value;
		}
	}

	public static bool SendErrorsToConsole
	{
		get
		{
			return sm_bSendErrorsToConsole;
		}
		set
		{
			sm_bSendErrorsToConsole = value;
		}
	}

	public static List<string> ErrorReportRecipients => sm_ErrorReportRecipients;

	public static List<string> ErrorReportAttachments => sm_ErrorReportAttachments;

	public static void CatchUnhandledExceptions()
	{
		try
		{
			Application.ThreadException += ThreadExceptionHandler;
			AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		}
		catch (Exception exception)
		{
			Error(exception, "Exception throw while setting unhandled exception handlers!", ErrorLevel.SendReport);
		}
	}

	public static void Error(Exception exception, ErrorLevel level)
	{
		Error(exception, string.Empty, level);
	}

	public static void Error(Exception exception, string sNote, ErrorLevel level)
	{
		try
		{
			LogException(exception, sNote, OperationResultLevel.Error);
			if (SendErrorsToConsole)
			{
				if (level == ErrorLevel.ShowMessage || level == ErrorLevel.SendReport)
				{
					ReportErrorToConsole(exception, sNote);
				}
				return;
			}
			switch (level)
			{
			case ErrorLevel.ShowMessage:
				if (string.IsNullOrEmpty(sNote))
				{
					sNote = ((exception == null) ? "null exception" : exception.Message);
				}
				ShowErrorMessage(exception, sNote);
				break;
			case ErrorLevel.SendReport:
				ErrorReport(exception, sNote);
				break;
			}
		}
		catch (Exception exception2)
		{
			try
			{
				LogException(exception2);
				ErrorReport(exception2, "Occurred during error handling.\nOriginal error:\n" + GetErrorReportText(exception, string.Empty));
			}
			catch
			{
			}
		}
	}

	public static string GetErrorReportText(Exception exception, string sNote)
	{
		return GetErrorReportText(exception, sNote, string.Empty);
	}

	private static bool IsInHouseUser()
	{
		return string.Equals(Environment.UserDomainName, "2KGAMES");
	}

	private static void GetCredentials(out string sUserName, out string sFullName)
	{
		try
		{
			sUserName = Environment.UserName;
			string displayName = UserPrincipal.Current.DisplayName;
			sFullName = displayName.Replace(" (Firaxis)", "");
		}
		catch
		{
			sUserName = string.Empty;
			sFullName = "Unknown User";
		}
	}

	public static string GetErrorReportHeader()
	{
		string sUserName;
		string sFullName;
		if (IsInHouseUser())
		{
			GetCredentials(out sUserName, out sFullName);
		}
		else
		{
			sUserName = "Unknown";
			sFullName = "Not an in-house user.";
		}
		string text;
		string text2;
		try
		{
			text = Environment.MachineName.ToString();
			text2 = string.Empty;
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform == PlatformID.Win32S)
			{
				text2 = "Windows 3.1";
			}
			else if (oSVersion.Platform == PlatformID.WinCE)
			{
				text2 = "Windows CE";
			}
			else if (oSVersion.Platform == PlatformID.Xbox)
			{
				text2 = "XBox";
			}
			else if (oSVersion.Platform == PlatformID.MacOSX)
			{
				text2 = "Mac OSX";
			}
			else if (oSVersion.Platform == PlatformID.Unix)
			{
				text2 = "Unix";
			}
			else if (oSVersion.Platform == PlatformID.Win32Windows)
			{
				switch (oSVersion.Version.Minor)
				{
				case 0:
					text2 = "Windows 95";
					break;
				case 10:
					text2 = "Windows 98";
					break;
				case 90:
					text2 = "Windows ME";
					break;
				}
			}
			else if (oSVersion.Platform == PlatformID.Win32NT)
			{
				if (oSVersion.Version.Major < 5)
				{
					text2 = "Windows NT";
				}
				else if (oSVersion.Version.Major == 5)
				{
					switch (oSVersion.Version.Minor)
					{
					case 0:
						text2 = "Windows 2000";
						break;
					case 1:
						text2 = "Windows XP";
						break;
					case 2:
						text2 = "Windows XP 64-Bit Edition";
						break;
					}
				}
				else if (oSVersion.Version.Major == 6)
				{
					switch (oSVersion.Version.Minor)
					{
					case 0:
						text2 = "Windows Vista";
						break;
					case 1:
						text2 = "Windows 7";
						break;
					case 2:
						text2 = "Windows 8";
						break;
					case 3:
						text2 = "Windows 8.1";
						break;
					}
				}
			}
			text2 = ((!(text2 == string.Empty)) ? (text2 + " (" + oSVersion.ServicePack + ")") : oSVersion.VersionString);
		}
		catch
		{
			text2 = "Unknown";
			text = "Unknown";
		}
		string text3 = ((IntPtr.Size == 8) ? "64 bit" : "32 bit");
		return string.Concat("Application: ", AppName, "\nUser: ", sUserName, " (", sFullName, ")\nTime: ", DateTime.Today.DayOfWeek, " ", DateTime.Now.ToString(), "\n\nRuntime: ", text3, "\nOS: ", text2, "\nMachine: ", text);
	}

	public static string GetErrorReportText(Exception exception, string sNote, string sUserComments)
	{
		try
		{
			string text = GetErrorReportHeader() + "\n\n";
			if (!string.IsNullOrEmpty(sNote))
			{
				text = text + "Note:\n" + sNote + "\n\n";
			}
			if (!string.IsNullOrEmpty(sUserComments))
			{
				text = text + "User Comments:\n" + sUserComments + "\n\n";
			}
			return text + GetExceptionReportText(exception);
		}
		catch (Exception exception2)
		{
			LogException(exception2);
			return string.Empty;
		}
	}

	public static string GetExceptionReportText(Exception exception)
	{
		try
		{
			if (exception == null)
			{
				return string.Empty;
			}
			string text = "Exception: " + exception.GetType().ToString() + "\nSource: " + exception.Source + "\nThread: " + Thread.CurrentThread.Name + "\nDescription: " + exception.Message + "\n\nStack Trace:\n" + exception.StackTrace;
			if (exception.InnerException != null)
			{
				text = text + "\n\nInner Exception:\n\n" + GetExceptionReportText(exception.InnerException);
			}
			return text;
		}
		catch (Exception exception2)
		{
			LogException(exception2);
			return string.Empty;
		}
	}

	private static void ThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
	{
		if (e != null)
		{
			UnhandledException(e.Exception);
		}
	}

	private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
	{
		if (e != null)
		{
			UnhandledException(e.ExceptionObject as Exception);
		}
	}

	private static void UnhandledException(Exception ex)
	{
		if (ex == null)
		{
			Error(ex, "Null exception sent to unhandled exception handler", ErrorLevel.SendReport);
		}
		else if (typeof(OutOfMemoryException).IsAssignableFrom(ex.GetType()))
		{
			Error(ex, "The application failed due to an \"Out of Memory\" exception.", ErrorLevel.ShowMessage);
		}
		else
		{
			Error(ex, string.Empty, ErrorLevel.SendReport);
		}
		try
		{
			Application.Exit();
		}
		catch
		{
			Application.ExitThread();
		}
	}

	private static void LogException(Exception exception)
	{
		LogException(exception, string.Empty, OperationResultLevel.Error);
	}

	private static void LogException(Exception exception, string sNote, OperationResultLevel level)
	{
		string text = sNote;
		if (exception != null)
		{
			text = exception.Message;
			ExceptionLogger.Log(exception, exception.GetType().ToString(), sNote, level);
		}
		else
		{
			ExceptionLogger.Log(sNote, string.Empty, level);
		}
	}

	public static void ReportErrorToConsole(Exception exception, string sMessage)
	{
		Console.Write("Error:\n" + GetErrorReportText(exception, sMessage) + "\n\n");
	}

	private static void ShowErrorMessage(Exception exception, string sMessage)
	{
		if (ShowErrorMessages)
		{
			ErrorMessage errorMessage = new ErrorMessage();
			errorMessage.Error = exception;
			errorMessage.Message = sMessage;
			errorMessage.ShowDialog();
		}
	}

	private static void ErrorReport(Exception exception, string sNote)
	{
		string errorReportText = GetErrorReportText(exception, sNote);
		try
		{
			if (ErrorReportForm == null)
			{
				ErrorReportForm = new ErrorReportWnd();
			}
			ErrorReportForm.ErrorReport = errorReportText;
			ErrorReportForm.ShowDialog();
			errorReportText = ErrorReportForm.ErrorReport;
			if (!string.IsNullOrEmpty(ErrorReportForm.Comments))
			{
				errorReportText = GetErrorReportText(exception, sNote, ErrorReportForm.Comments);
			}
		}
		catch (Exception exception2)
		{
			LogException(exception2);
		}
		if (Debugger.IsAttached)
		{
			DialogResult dialogResult = MessageBox.Show("A debugger is attached.  Would you still like to send an error message?", "Debugger Attached", MessageBoxButtons.YesNo);
			if (dialogResult != DialogResult.No)
			{
			}
		}
	}
}

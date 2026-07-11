using System;
using System.Reflection;
using System.Text;
using Firaxis.CivTech;
using Firaxis.Error;
using Firaxis.IO;

namespace Firaxis.MaxInterface;

public class Autodesk_3DSMaxInterface
{
	private object _maxComObject;

	private object MaxComObject
	{
		get
		{
			if (_maxComObject == null)
			{
				EstablishConnection();
			}
			return _maxComObject;
		}
		set
		{
			_maxComObject = value;
		}
	}

	private void SetProperty(object obj, string sProperty, object oValue)
	{
		object[] args = new object[1] { oValue };
		obj.GetType().InvokeMember(sProperty, BindingFlags.SetProperty, null, obj, args);
	}

	private object GetProperty(object obj, string sProperty, object oValue)
	{
		object[] args = new object[1] { oValue };
		return obj.GetType().InvokeMember(sProperty, BindingFlags.GetProperty, null, obj, args);
	}

	private object GetProperty(object obj, string sProperty, object oValue1, object oValue2)
	{
		object[] args = new object[2] { oValue1, oValue2 };
		return obj.GetType().InvokeMember(sProperty, BindingFlags.GetProperty, null, obj, args);
	}

	private object GetProperty(object obj, string sProperty)
	{
		return obj.GetType().InvokeMember(sProperty, BindingFlags.GetProperty, null, obj, null);
	}

	private object InvokeMethod(object target, string methodName, object[] methodParams)
	{
		BindingFlags invokeAttr = BindingFlags.InvokeMethod;
		Binder binder = null;
		return target.GetType().InvokeMember(methodName, invokeAttr, binder, target, methodParams);
	}

	private object InvokeMethod(object target, string methodName, object methodParam)
	{
		BindingFlags invokeAttr = BindingFlags.InvokeMethod;
		Binder binder = null;
		object[] args = new object[1] { methodParam };
		return target.GetType().InvokeMember(methodName, invokeAttr, binder, target, args);
	}

	private bool SaveFile(WindowsPath fileToSave)
	{
		string text = fileToSave.ToString();
		string text2 = "saveMaxFile @\"" + text + "\"";
		Console.WriteLine(text2);
		return ExecuteMAXScriptSafe(text2);
	}

	private bool HoldMaxFile()
	{
		return ExecuteMAXScriptSafe("holdMaxFile()");
	}

	private bool FetchMaxFile()
	{
		return ExecuteMAXScriptSafe("fetchMaxFile quiet:true");
	}

	private bool TempSaveAndExport(WindowsPath filePathToExport)
	{
		OpenFile(filePathToExport);
		return ExecuteMAXScriptSafe("ExportGeometry()");
	}

	public bool EstablishConnection()
	{
		try
		{
			Type maxCOMType = GetMaxCOMType();
			MaxComObject = Activator.CreateInstance(maxCOMType);
		}
		catch (Exception ex)
		{
			Console.WriteLine("3dsMax is probably not open: " + ex.Message);
			BugSubmitter.SilentException(new Exception("Unable to establish connection to 3dsmax @assign agould @summary Cannot connect to Max", ex));
			return false;
		}
		double value = 20.0;
		TimeSpan timeSpan = TimeSpan.FromSeconds(value);
		DateTime dateTime = DateTime.Now + timeSpan;
		bool flag = IsConnected();
		while (!flag && DateTime.Now < dateTime)
		{
			flag = IsConnected();
		}
		return flag;
	}

	private Type GetMaxCOMType()
	{
		Type type = null;
		type = Type.GetTypeFromProgID("MAX.Application.20");
		if (type == null)
		{
			type = Type.GetTypeFromProgID("MAX.Application.19");
		}
		if (type == null)
		{
			type = Type.GetTypeFromProgID("MAX.Application.18");
		}
		if (type == null)
		{
			type = Type.GetTypeFromProgID("MAX.Application.17");
		}
		if (type == null)
		{
			type = Type.GetTypeFromProgID("MAX.Application.15");
		}
		if (type == null)
		{
			type = Type.GetTypeFromProgID("MAX.Application.14");
		}
		BugSubmitter.SilentAssert(type != null, "No valid version of 3dsMax was found on this machine @assign agould");
		return type;
	}

	public object ExecuteMAXScript(string maxScriptCommand)
	{
		return InvokeMethod(methodParams: new object[1] { maxScriptCommand }, target: MaxComObject, methodName: "command_from_nexus");
	}

	private ResultCode ExecuteMAXScriptSafe(string maxScriptCommand)
	{
		try
		{
			ExecuteMAXScript(maxScriptCommand);
		}
		catch
		{
			BugSubmitter.SilentException(new Exception("Failed to execute maxscript command: " + maxScriptCommand + " @assign agould"));
		}
		return ResultCode.Success;
	}

	public WindowsPath GetTempFolderPath()
	{
		return new WindowsPath((string)ExecuteMAXScript("GetDir #temp"));
	}

	private bool IsConnected()
	{
		try
		{
			Console.WriteLine(GetTempFolderPath());
			return true;
		}
		catch
		{
			Console.WriteLine("Establishing Connection >>>");
			return false;
		}
	}

	public ResultCode OpenFile(WindowsPath imageFilename)
	{
		string text = imageFilename.ToString();
		string maxScriptCommand = "LoadMaxFile @\"" + text + "\" quiet:true useFileUnits:true";
		return ExecuteMAXScriptSafe(maxScriptCommand);
	}

	public ResultCode ExportAnimation(string projectPrefix)
	{
		return ExecuteMAXScriptSafe(projectPrefix + "ExportAnimation()");
	}

	public ResultCode ExportAnimation(string projectPrefix, WindowsPath filePathToExport)
	{
		return ExportAnimation(projectPrefix, filePathToExport, null, null, null, null);
	}

	public ResultCode ExportAnimation(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath)
	{
		return ExportAnimation(projectPrefix, filePathToExport, outputFolderPath, null, null, null);
	}

	public ResultCode ExportAnimation(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfAnimations)
	{
		return ExportAnimation(projectPrefix, filePathToExport, outputFolderPath, listOfAnimations, null, null);
	}

	public ResultCode ExportAnimation(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfAnimations, WindowsPath grannySettingsFilePath)
	{
		return ExportAnimation(projectPrefix, filePathToExport, outputFolderPath, listOfAnimations, null, grannySettingsFilePath);
	}

	public ResultCode ExportAnimation(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfAnimations, string listOfOutputFilenames)
	{
		return ExportAnimation(projectPrefix, filePathToExport, outputFolderPath, listOfAnimations, listOfOutputFilenames, null);
	}

	public ResultCode ExportAnimation(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfAnimations, string listOfOutputFilenames, WindowsPath grannySettingsFilePath)
	{
		StringBuilder stringBuilder = new StringBuilder(projectPrefix);
		stringBuilder.Append("ExportAnimation");
		if (outputFolderPath != null)
		{
			string value = CreateMAXParameter(" szExportPath: @", outputFolderPath.ToString());
			stringBuilder.Append(value);
		}
		if (listOfAnimations != null)
		{
			string value2 = CreateMAXParameter(" ListOfAnimations: ", listOfAnimations);
			stringBuilder.Append(value2);
		}
		if (listOfOutputFilenames != null)
		{
			string value3 = CreateMAXParameter(" ListOfFilenames: ", listOfOutputFilenames);
			stringBuilder.Append(value3);
		}
		if (grannySettingsFilePath != null)
		{
			string value4 = CreateMAXParameter(" sExportSettingsFile: @", grannySettingsFilePath.ToString());
			stringBuilder.Append(value4);
		}
		if (outputFolderPath == null && listOfAnimations == null && listOfOutputFilenames == null && grannySettingsFilePath == null)
		{
			stringBuilder.Append("()");
		}
		string commandToExecute = stringBuilder.ToString();
		string text = ExecuteMAXScriptOnFileSafe(filePathToExport, commandToExecute);
		if (text.Equals("OK"))
		{
			return ResultCode.Success;
		}
		return new ResultCode(text);
	}

	private string CreateMAXParameter(string commandName, string arguments)
	{
		return $" {commandName}\"{arguments}\"";
	}

	public string GetAnimationNames(string projectPrefix, WindowsPath filePath)
	{
		try
		{
			return ExecuteMAXScriptOnFile(filePath, projectPrefix + "GetAnimations()");
		}
		catch
		{
			return string.Empty;
		}
	}

	public ResultCode ExportGeometry(string projectPrefix)
	{
		return ExecuteMAXScriptSafe(projectPrefix + "ExportGeometry()");
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport)
	{
		return ExportGeometry(projectPrefix, filePathToExport, null, null, null, null);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, null, null, null);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, listOfModels, null, null);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels, WindowsPath grannySettingsFilePath)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, listOfModels, null, grannySettingsFilePath);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels, string listOfOutputFilenames)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, listOfModels, listOfOutputFilenames, null);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels, string listOfOutputFilenames, WindowsPath grannySettingsFilePath)
	{
		StringBuilder stringBuilder = new StringBuilder(projectPrefix);
		stringBuilder.Append("ExportGeometry");
		if (outputFolderPath != null)
		{
			string value = CreateMAXParameter(" szExportPath: @", outputFolderPath.ToString());
			stringBuilder.Append(value);
		}
		if (listOfModels != null)
		{
			string value2 = CreateMAXParameter(" ListOfModels: ", listOfModels);
			stringBuilder.Append(value2);
		}
		if (listOfOutputFilenames != null)
		{
			string value3 = CreateMAXParameter(" ListOfFilenames: ", listOfOutputFilenames);
			stringBuilder.Append(value3);
		}
		if (grannySettingsFilePath != null)
		{
			string value4 = CreateMAXParameter(" sExportSettingsFile: @", grannySettingsFilePath.ToString());
			stringBuilder.Append(value4);
		}
		if (outputFolderPath == null && listOfModels == null && listOfOutputFilenames == null && grannySettingsFilePath == null)
		{
			stringBuilder.Append("()");
		}
		string commandToExecute = stringBuilder.ToString();
		string text = ExecuteMAXScriptOnFileSafe(filePathToExport, commandToExecute);
		if (text.Equals("OK"))
		{
			return ResultCode.Success;
		}
		return new ResultCode(text);
	}

	public string GetModelNames(string projectPrefix, WindowsPath filePath)
	{
		try
		{
			return ExecuteMAXScriptOnFile(filePath, projectPrefix + "GetModels()");
		}
		catch
		{
			return string.Empty;
		}
	}

	public string ExecuteMAXScriptOnFile(WindowsPath filePathToExport, string CommandToExecute)
	{
		bool flag = false;
		string strA = (string)ExecuteMAXScript("maxFilePath + maxFileName");
		string result;
		if (string.Compare(strA, string.Empty) == 0)
		{
			if (DoesFileNeedSaving())
			{
				HoldMaxFile();
				flag = true;
			}
			OpenFile(filePathToExport);
			result = ExecuteMAXScript(CommandToExecute).ToString();
			ExecuteMAXScript("NewMaxScene()");
			if (flag)
			{
				FetchMaxFile();
			}
			return result;
		}
		WindowsPath windowsPath = new WindowsPath((string)ExecuteMAXScript("maxFilePath + maxFileName"));
		if (WindowsPath.Equals(windowsPath, filePathToExport))
		{
			return ExecuteMAXScript(CommandToExecute).ToString();
		}
		if (DoesFileNeedSaving())
		{
			HoldMaxFile();
			flag = true;
		}
		OpenFile(filePathToExport);
		result = ExecuteMAXScript(CommandToExecute).ToString();
		OpenFile(windowsPath);
		if (flag)
		{
			FetchMaxFile();
		}
		return result;
	}

	private string ExecuteMAXScriptOnFileSafe(WindowsPath filePathToExport, string CommandToExecute)
	{
		try
		{
			return ExecuteMAXScriptOnFile(filePathToExport, CommandToExecute);
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public bool DoesFileNeedSaving()
	{
		try
		{
			bool flag = (bool)ExecuteMAXScript("getSaveRequired()");
			string text = (string)ExecuteMAXScript("maxFilename");
			return flag || text.Length == 0;
		}
		catch
		{
			return false;
		}
	}

	public WindowsPath SaveTempFile()
	{
		WindowsPath windowsPath;
		try
		{
			WindowsPath tempFolderPath = GetTempFolderPath();
			string path = string.Concat(tempFolderPath, "\\temporaryExportMaxFile");
			windowsPath = new WindowsPath(path);
			SaveFile(windowsPath);
		}
		catch
		{
			windowsPath = null;
		}
		return windowsPath;
	}

	public bool ForceFileSave()
	{
		try
		{
			return (bool)ExecuteMAXScript("savemaxfile maxfilename quiet:true");
		}
		catch
		{
			return false;
		}
	}
}

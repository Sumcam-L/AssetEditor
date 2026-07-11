using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;
using Firaxis.CivTech;
using Firaxis.Error;
using Firaxis.IO;
using Firaxis.MayaInterface.Utilities;
using Microsoft.Win32;

namespace Firaxis.MayaInterface;

public class AutodeskMayaInterface
{
	public enum MAYA_VERSIONS
	{
		maya_2011 = 2011,
		maya_2012,
		maya_2013,
		maya_2014,
		maya_2015,
		maya_2016,
		maya_2017,
		maya_2018
	}

	private int MAYA_INTERFACE_PORT = 6292;

	private int MAYA_MANUAL_INTERFACE_PORT = 6293;

	public MAYA_VERSIONS SUPPORTED_MAYA_VERSION = MAYA_VERSIONS.maya_2018;

	private bool hasValidMayaVersion = false;

	private string[] EMPTY_ARRAY = new string[0];

	public AutodeskMayaInterface()
	{
		if (GetMayaInstallPath(MAYA_VERSIONS.maya_2018) != null)
		{
			SUPPORTED_MAYA_VERSION = MAYA_VERSIONS.maya_2018;
			hasValidMayaVersion = true;
		}
		else if (GetMayaInstallPath(MAYA_VERSIONS.maya_2017) != null)
		{
			SUPPORTED_MAYA_VERSION = MAYA_VERSIONS.maya_2017;
			hasValidMayaVersion = true;
		}
		else if (GetMayaInstallPath(MAYA_VERSIONS.maya_2016) != null)
		{
			SUPPORTED_MAYA_VERSION = MAYA_VERSIONS.maya_2016;
			hasValidMayaVersion = true;
		}
		else if (GetMayaInstallPath(MAYA_VERSIONS.maya_2015) != null)
		{
			SUPPORTED_MAYA_VERSION = MAYA_VERSIONS.maya_2015;
			hasValidMayaVersion = true;
		}
		else
		{
			hasValidMayaVersion = false;
		}
	}

	private string GetMayaInstallPath(MAYA_VERSIONS mayaVersion)
	{
		string text = Convert.ChangeType(mayaVersion, mayaVersion.GetTypeCode()).ToString();
		RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
		registryKey = registryKey.OpenSubKey("SOFTWARE\\Autodesk\\Maya\\");
		if (registryKey != null && registryKey.GetSubKeyNames().Contains(text))
		{
			registryKey = registryKey.OpenSubKey(text + "\\Setup\\InstallPath");
			if (registryKey != null)
			{
				string.IsNullOrEmpty((string)registryKey.GetValue("MAYA_INSTALL_LOCATION"));
			}
		}
		return "E:\\Program Files\\Autodesk\\Maya2020\\";
	}

	private string GetMayaServerBatchFileLocation(MAYA_VERSIONS mayaVersion)
	{
		string mayaInstallPath = GetMayaInstallPath(mayaVersion);
		if (mayaInstallPath == null)
		{
			return null;
		}
		return mayaInstallPath + "bin\\MayaPy_Civ6_External.bat";
	}

	public bool EstablishConnection()
	{
		if (!hasValidMayaVersion)
		{
			BugSubmitter.SilentReport("No valid version of Maya was found on this machine @assign agould");
			ExceptionLogger.Log("Unable to get any Maya Install Path.  Please ensure that Maya is installed.", "Unable to find Maya.");
			return false;
		}
		string text = SendMelCommand("about -version", 100);
		if (text == null)
		{
			try
			{
				string mayaServerBatchFileLocation = GetMayaServerBatchFileLocation(SUPPORTED_MAYA_VERSION);
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.CreateNoWindow = true;
				processStartInfo.WorkingDirectory = GetMayaInstallPath(SUPPORTED_MAYA_VERSION) + "bin";
				processStartInfo.FileName = mayaServerBatchFileLocation;
				if (File.Exists(mayaServerBatchFileLocation))
				{
					int timeoutInMiliseconds = 35000;
					try
					{
						Process process = Process.Start(processStartInfo);
						text = SendMelCommand("about -version", timeoutInMiliseconds);
					}
					catch (Exception ex)
					{
						BugSubmitter.SilentException(new Exception("Failed to send Mel command @assign agould", ex));
						ExceptionLogger.Log($"Failed to start Maya from path \"{mayaServerBatchFileLocation}\"\n\nError: \"{ex.Message}\"", "Unable to launch Maya.");
					}
				}
				else
				{
					ErrorHandling.Error(new FileNotFoundException("Maya Batch server file not found", mayaServerBatchFileLocation), "It Appears that either Autodesk Maya, or the Maya Civtech Scripts are not installed on this computer. Please install those and try again.", ErrorLevel.ShowMessage);
				}
			}
			catch (NullReferenceException)
			{
				ExceptionLogger.Log("Unable to get the Maya Install Path.  Please ensure that Maya is installed.", "Unable to launch Maya.");
			}
		}
		return text != null;
	}

	public string SendMelCommand(string melCommand)
	{
		int timeoutInMiliseconds = (int)TimeSpan.FromHours(2.0).TotalMilliseconds;
		return SendMelCommand(melCommand, timeoutInMiliseconds);
	}

	public string SendMelCommand(string melCommand, int timeoutInMiliseconds)
	{
		string text = null;
		bool flag = true;
		try
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress address = IPAddress.Parse("127.0.0.1");
			IPEndPoint remoteEP = new IPEndPoint(address, MAYA_MANUAL_INTERFACE_PORT);
			socket.Connect(remoteEP);
			byte[] bytes = Encoding.ASCII.GetBytes(melCommand);
			socket.Send(bytes);
			byte[] array = new byte[1024];
			int num = socket.Receive(array);
			char[] array2 = new char[num];
			Decoder decoder = Encoding.UTF8.GetDecoder();
			int chars = decoder.GetChars(array, 0, num, array2, 0);
			text = new string(array2);
			socket.Close();
		}
		catch
		{
			flag = false;
		}
		if (!flag)
		{
			string commandURL = GetCommandURL(melCommand);
			DateTime dateTime = DateTime.Now + TimeSpan.FromMilliseconds(timeoutInMiliseconds);
			DateTime now = DateTime.Now;
			while (text == null && now < dateTime)
			{
				text = SendHTTPRequest(commandURL);
				now = DateTime.Now;
			}
			if (text == null)
			{
				Console.WriteLine("Timed out");
			}
		}
		return text;
	}

	private string GetCommandURL(string melCommand)
	{
		return $"http://127.0.0.1:{MAYA_INTERFACE_PORT}/?command={melCommand}";
	}

	private string SendHTTPRequest(string url)
	{
		try
		{
			TimeSpan timeSpan = TimeSpan.FromHours(2.0);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Timeout = (int)timeSpan.TotalMilliseconds;
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = httpWebResponse.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream);
			return streamReader.ReadLine();
		}
		catch (Exception ex)
		{
			string value = ex.ToString();
			Console.WriteLine(value);
			return null;
		}
	}

	public string[] GetAnimationNames(WindowsPath filePath)
	{
		string[] source = null;
		if (EstablishConnection())
		{
			string universalFilePath = GetUniversalFilePath(filePath);
			if (OpenMayaFile(universalFilePath))
			{
				string text = SendMelCommand("Civ6_GetAnimationNames()");
				if (!string.IsNullOrEmpty(text))
				{
					source = GetStringArrayResult(text);
				}
			}
		}
		return source.Where((string x) => !string.IsNullOrEmpty(x)).ToArray();
	}

	public string[] GetModelNames(WindowsPath filePath)
	{
		string[] result = null;
		if (EstablishConnection())
		{
			string universalFilePath = GetUniversalFilePath(filePath);
			if (OpenMayaFile(universalFilePath))
			{
				string text = SendMelCommand("Civ6_GetModelNames()");
				if (!string.IsNullOrEmpty(text))
				{
					result = GetStringArrayResult(text);
				}
			}
		}
		return result;
	}

	private string GetUniversalFilePath(WindowsPath path)
	{
		string text = path.ToString();
		return text.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	private string[] GetStringArrayResult(string commandResultString)
	{
		string[] result = EMPTY_ARRAY;
		Json json = new Json();
		Hashtable hashtable = (Hashtable)json.Decode(commandResultString);
		if (hashtable != null)
		{
			ArrayList arrayList = (ArrayList)hashtable["result"];
			if (arrayList != null)
			{
				result = (string[])arrayList.ToArray(typeof(string));
			}
		}
		else
		{
			char[] separator = new char[3] { '\n', '\t', '\0' };
			result = commandResultString.Split(separator);
		}
		return result;
	}

	private string GetExportResultMessage(string exportCommandResult)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 20971520;
		MayaResult mayaResult = new MayaResult();
		try
		{
			mayaResult = javaScriptSerializer.Deserialize<MayaResult>(exportCommandResult);
		}
		catch
		{
			mayaResult.success = GetStringArrayResult(exportCommandResult).ToList()[0].Equals("success", StringComparison.OrdinalIgnoreCase);
		}
		if (mayaResult.success)
		{
			return "success";
		}
		string text = string.Join("\n", mayaResult.result.ToArray());
		if (text.Contains("ai_translator"))
		{
			text = "There was an error loading your Maya file because it contains Arnold\nnodes which are not currently supported. Please deactivate the \n'mtoa' plugin in Maya and reopen and resave your file and try again.";
		}
		return text;
	}

	private bool OpenMayaFile(string filePath)
	{
		string melCommand = string.Format("about -b", filePath);
		string json = SendMelCommand(melCommand);
		Json json2 = new Json();
		Hashtable hashtable = null;
		try
		{
			hashtable = (Hashtable)json2.Decode(json);
		}
		catch
		{
			if (hashtable == null)
			{
				melCommand = string.Format("file -q -sn", filePath);
				json = SendMelCommand(melCommand);
				char[] separator = new char[3] { '\n', '\t', '\0' };
				string[] array = json.Split(separator);
				if (array[0].Equals(filePath, StringComparison.OrdinalIgnoreCase))
				{
					return json != null;
				}
			}
		}
		melCommand = $"file -o -f \" {filePath}\"";
		json = SendMelCommand(melCommand);
		return json != null;
	}

	public ResultCode ExportAnimation(string projectPrefix)
	{
		return ExportAnimation(projectPrefix, null, null, null, null, null);
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
		if (!EstablishConnection())
		{
			return new ResultCode("Unable to connect to Maya");
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (filePathToExport != null)
		{
			string parameterValue = Path.GetFullPath(filePathToExport.FullPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string scriptingParameter = GetScriptingParameter("sourceFilename", parameterValue);
			stringBuilder.Append(scriptingParameter);
		}
		if (outputFolderPath != null)
		{
			string parameterValue2 = Path.GetFullPath(outputFolderPath.FullPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string scriptingParameter2 = GetScriptingParameter("outputDirectory", parameterValue2);
			stringBuilder.Append(scriptingParameter2);
		}
		if (listOfAnimations != null)
		{
			string scriptingParameter3 = GetScriptingParameter("animationNames", listOfAnimations);
			stringBuilder.Append(scriptingParameter3);
		}
		if (listOfOutputFilenames != null)
		{
			string scriptingParameter4 = GetScriptingParameter("outputFilenames", listOfOutputFilenames);
			stringBuilder.Append(scriptingParameter4);
		}
		if (grannySettingsFilePath != null)
		{
			string value = $"exportSettings = @\"{grannySettingsFilePath.ToString()}\", ";
			stringBuilder.Append(value);
		}
		char[] charactersToRemove = new char[2] { ' ', ',' };
		TrimFromEnd(stringBuilder, charactersToRemove);
		string text = stringBuilder.ToString();
		Console.WriteLine(text);
		string melCommand = $" Civ6_ExportAnimations(\"{text}\") ";
		string exportCommandResult = SendMelCommand(melCommand);
		ResultCode result = ResultCode.Success;
		if (!GetExportResultMessage(exportCommandResult).Equals("success"))
		{
			result = new ResultCode(GetExportResultMessage(exportCommandResult));
		}
		return result;
	}

	public ResultCode ExportGeometry(string projectPrefix)
	{
		return ExportGeometry(projectPrefix, null, null, null, null, null, exportWigFile: false);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport)
	{
		return ExportGeometry(projectPrefix, filePathToExport, null, null, null, null, exportWigFile: false);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, null, null, null, exportWigFile: false);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, listOfModels, null, null, exportWigFile: false);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels, WindowsPath grannySettingsFilePath)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, listOfModels, null, grannySettingsFilePath, exportWigFile: false);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels, string listOfOutputFilenames)
	{
		return ExportGeometry(projectPrefix, filePathToExport, outputFolderPath, listOfModels, listOfOutputFilenames, null, exportWigFile: false);
	}

	public ResultCode ExportGeometry(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfModels, string listOfOutputFilenames, WindowsPath grannySettingsFilePath, bool exportWigFile)
	{
		if (!EstablishConnection())
		{
			return new ResultCode("Unable to connect to Maya");
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (filePathToExport != null)
		{
			string parameterValue = Path.GetFullPath(filePathToExport.FullPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string scriptingParameter = GetScriptingParameter("sourceFilename", parameterValue);
			stringBuilder.Append(scriptingParameter);
		}
		if (outputFolderPath != null)
		{
			string parameterValue2 = Path.GetFullPath(outputFolderPath.FullPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string scriptingParameter2 = GetScriptingParameter("outputDirectory", parameterValue2);
			stringBuilder.Append(scriptingParameter2);
		}
		if (listOfModels != null)
		{
			string scriptingParameter3 = GetScriptingParameter("modelNames", listOfModels);
			stringBuilder.Append(scriptingParameter3);
		}
		if (listOfOutputFilenames != null)
		{
			string scriptingParameter4 = GetScriptingParameter("outputFilenames", listOfOutputFilenames);
			stringBuilder.Append(scriptingParameter4);
		}
		if (grannySettingsFilePath != null)
		{
			string value = $"exportSettings = @\"{grannySettingsFilePath.ToString()}\", ";
			stringBuilder.Append(value);
		}
		if (exportWigFile)
		{
			string scriptingParameter5 = GetScriptingParameter("exportWigFile", "True");
			stringBuilder.Append(scriptingParameter5);
		}
		char[] charactersToRemove = new char[2] { ' ', ',' };
		TrimFromEnd(stringBuilder, charactersToRemove);
		string text = stringBuilder.ToString();
		Console.WriteLine(text);
		string melCommand = $" Civ6_ExportModels(\"{text}\") ";
		string exportCommandResult = SendMelCommand(melCommand);
		ResultCode result = ResultCode.Success;
		if (!GetExportResultMessage(exportCommandResult).Equals("success"))
		{
			result = new ResultCode(GetExportResultMessage(exportCommandResult));
		}
		return result;
	}

	public bool ExportAnimationPlaybasts(string projectPrefix, WindowsPath filePathToExport, WindowsPath outputFolderPath, string listOfAnimations)
	{
		if (!EstablishConnection())
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (filePathToExport != null)
		{
			string parameterValue = Path.GetFullPath(filePathToExport.FullPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string scriptingParameter = GetScriptingParameter("sourceFilename", parameterValue);
			stringBuilder.Append(scriptingParameter);
		}
		if (outputFolderPath != null)
		{
			string parameterValue2 = Path.GetFullPath(outputFolderPath.FullPath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string scriptingParameter2 = GetScriptingParameter("outputDirectory", parameterValue2);
			stringBuilder.Append(scriptingParameter2);
		}
		if (listOfAnimations != null)
		{
			string scriptingParameter3 = GetScriptingParameter("animationNames", listOfAnimations);
			stringBuilder.Append(scriptingParameter3);
		}
		char[] charactersToRemove = new char[2] { ' ', ',' };
		TrimFromEnd(stringBuilder, charactersToRemove);
		string text = stringBuilder.ToString();
		Console.WriteLine(text);
		string melCommand = $" Civ6_ExportAnimationPlayblasts(\"{text}\") ";
		return SendMelCommand(melCommand) != null;
	}

	private void TrimFromEnd(StringBuilder builder, IEnumerable<char> charactersToRemove)
	{
		int num = builder.Length - 1;
		if (num > 0)
		{
			char value = builder[num];
			while (num > 0 && charactersToRemove.Contains(value))
			{
				builder.Remove(num, 1);
				value = builder[--num];
			}
		}
	}

	private string GetScriptingParameter(string parameterName, string parameterValue)
	{
		return $"{parameterName} = \\\"{parameterValue}\\\", ";
	}

	public void CloseConnection()
	{
		SendMelCommand("quit");
	}

	public void OpenFile(string filePath)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		string text = GetMayaInstallPath(SUPPORTED_MAYA_VERSION) + "bin";
		if (text != null)
		{
			processStartInfo.WorkingDirectory = text;
			processStartInfo.FileName = "maya.exe";
			processStartInfo.Arguments = "-file \"" + filePath + "\"";
			string path = Path.Combine(processStartInfo.WorkingDirectory, processStartInfo.FileName);
			if (File.Exists(path))
			{
				try
				{
					Process process = Process.Start(processStartInfo);
					return;
				}
				catch (Exception ex)
				{
					BugSubmitter.SilentException(ex);
					ExceptionLogger.Log($"Failed to open file \"{text}\" with Maya in folder \"{filePath}\"\n\nError: \"{ex.Message}\"", "Unable to launch Maya.");
					return;
				}
			}
			ErrorHandling.Error(new FileNotFoundException("Maya application executable not found", text), "It Appears that Maya is not installed on this computer. Please install Maya try again.", ErrorLevel.ShowMessage);
		}
		else
		{
			ExceptionLogger.Log("Unable to get the Maya Install Path.  Please ensure that Maya is installed.", "Unable to launch Maya.");
		}
	}
}

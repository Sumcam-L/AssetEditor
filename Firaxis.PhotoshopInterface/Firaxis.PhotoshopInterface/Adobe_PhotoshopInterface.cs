using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.Error;
using Firaxis.IO;
using Microsoft.Win32;

namespace Firaxis.PhotoshopInterface;

public class Adobe_PhotoshopInterface
{
	private class PhotoshopRegistryEntry
	{
		public string ApplicationID { get; set; }

		public string ActionDescriptorID { get; set; }

		public PhotoshopRegistryEntry()
		{
			ApplicationID = string.Empty;
			ActionDescriptorID = string.Empty;
		}

		public PhotoshopRegistryEntry(string appID, string descID)
		{
			ApplicationID = appID;
			ActionDescriptorID = descID;
		}
	}

	public const int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

	private Type PhotoshopType { get; set; }

	private object Photoshop { get; set; }

	private string ActionDescriptorType { get; set; }

	private Dictionary<string, PhotoshopRegistryEntry> PhotoshopVersionLookupDictionary { get; set; }

	public Adobe_PhotoshopInterface()
	{
		PhotoshopVersionLookupDictionary = CreateVersionLookupDictionary();
	}

	private Dictionary<string, PhotoshopRegistryEntry> CreateVersionLookupDictionary()
	{
		Dictionary<string, PhotoshopRegistryEntry> dictionary = new Dictionary<string, PhotoshopRegistryEntry>();
		string text = "Photoshop.Application.";
		string text2 = "Photoshop.ActionDescriptor.";
		string key = "CS3";
		PhotoshopRegistryEntry value = new PhotoshopRegistryEntry(text + "10", text2 + "10");
		dictionary.Add(key, value);
		key = "CS4";
		value = new PhotoshopRegistryEntry(text + "11", text2 + "11");
		dictionary.Add(key, value);
		key = "CS5";
		value = new PhotoshopRegistryEntry(text + "12", text2 + "12");
		dictionary.Add(key, value);
		key = "CS5.1";
		value = new PhotoshopRegistryEntry(text + "55", text2 + "55");
		dictionary.Add(key, value);
		key = "CS6";
		value = new PhotoshopRegistryEntry(text + "60", text2 + "60");
		dictionary.Add(key, value);
		key = "CC";
		value = new PhotoshopRegistryEntry(text + "70", text2 + "70");
		dictionary.Add(key, value);
		key = "15.2";
		value = new PhotoshopRegistryEntry(text + "80", text2 + "80");
		dictionary.Add(key, value);
		key = "16.0";
		value = new PhotoshopRegistryEntry(text + "90", text2 + "90");
		dictionary.Add(key, value);
		key = "19.0";
		value = new PhotoshopRegistryEntry(text + "120", text2 + "120");
		dictionary.Add(key, value);
		key = "21.0";
		value = new PhotoshopRegistryEntry(text + "140", text2 + "140");
		dictionary.Add(key, value);
		key = "21.0.1";
		value = new PhotoshopRegistryEntry(text + "140.1", text2 + "140.1");
		dictionary.Add(key, value);
		return dictionary;
	}

	public static IEnumerable<RegistryKey> GetPhotoshopApplicationSubkeys()
	{
		List<RegistryKey> list = new List<RegistryKey>();
		string text = "SOFTWARE\\Classes";
		RegistryKey localMachine = Registry.LocalMachine;
		localMachine = localMachine.OpenSubKey(text);
		string[] subKeyNames = localMachine.GetSubKeyNames();
		List<string> list2 = subKeyNames.Where((string subkey) => subkey.StartsWith("Photoshop.Application.") && !subkey.EndsWith(".1")).ToList();
		foreach (string item in list2)
		{
			string name = text + "\\" + item;
			RegistryKey localMachine2 = Registry.LocalMachine;
			localMachine2 = localMachine2.OpenSubKey(name);
			list.Add(localMachine2);
		}
		return list;
	}

	public static IEnumerable<string> GetPhotoshopActionDescriptorNames()
	{
		string name = "SOFTWARE\\Classes";
		RegistryKey localMachine = Registry.LocalMachine;
		localMachine = localMachine.OpenSubKey(name);
		string[] subKeyNames = localMachine.GetSubKeyNames();
		return subKeyNames.Where((string subkey) => subkey.StartsWith("Photoshop.ActionDescriptor.")).ToList();
	}

	public static string GetLastPhotoshopGUID()
	{
		string name = "SOFTWARE\\Classes\\Photoshop.Application\\CLSID";
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
		if (registryKey == null)
		{
			string text = "Cannot find Photoshop GUID Registry Key.  Please ensure that Adobe Photoshop is installed.";
			InvalidOperationException exception = new InvalidOperationException(text);
			ErrorHandling.Error(exception, text, ErrorLevel.ShowMessage);
			return string.Empty;
		}
		return registryKey.GetValue(string.Empty).ToString();
	}

	public static string GetLastUsedPhotoshopVersion()
	{
		IEnumerable<RegistryKey> photoshopApplicationSubkeys = GetPhotoshopApplicationSubkeys();
		string lastPhotoshopGUID = GetLastPhotoshopGUID();
		string result = string.Empty;
		foreach (RegistryKey item in photoshopApplicationSubkeys)
		{
			RegistryKey registryKey = item.OpenSubKey("CLSID");
			string text = registryKey.GetValue(string.Empty).ToString();
			if (text.Equals(lastPhotoshopGUID))
			{
				result = item.Name.Substring(item.Name.Length - 2);
			}
		}
		return result;
	}

	public static string GetFriendlyPhotoshopVersionName(string unfriendlyName)
	{
		string empty = string.Empty;
		return unfriendlyName.Substring(unfriendlyName.Length - 2) switch
		{
			"10" => "Photoshop CS3", 
			"11" => "Photoshop CS4", 
			"12" => "Photoshop CS5", 
			"55" => "Photoshop CS5.1", 
			"60" => "Photoshop CS6", 
			"70" => "Photoshop CC", 
			_ => unfriendlyName, 
		};
	}

	public static string GetRunningPhotoshopVersion()
	{
		string result = string.Empty;
		try
		{
			Process[] processesByName = Process.GetProcessesByName("Photoshop");
			if (processesByName.Length != 0)
			{
				Process process = processesByName[0];
				result = process.MainModule.FileVersionInfo.ProductVersion;
			}
		}
		catch (Exception)
		{
		}
		return result;
	}

	public static bool SetDefaultPhotoshopVersion(string applicationKeyName, string GUID)
	{
		bool result = false;
		if (!applicationKeyName.StartsWith("Photoshop.Application."))
		{
			return result;
		}
		IEnumerable<RegistryKey> photoshopApplicationSubkeys = GetPhotoshopApplicationSubkeys();
		foreach (RegistryKey item in photoshopApplicationSubkeys)
		{
			if (item.Name.EndsWith(applicationKeyName) && item.OpenSubKey("CLSID").GetValue(string.Empty).ToString() == GUID)
			{
				result = SetLastUsedPhotoshopKey(applicationKeyName, GUID);
				break;
			}
		}
		return result;
	}

	private static bool SetLastUsedPhotoshopKey(string applicationName, string GUID)
	{
		bool result = true;
		string name = "SOFTWARE\\Classes\\Photoshop.Application";
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(name);
		try
		{
			registryKey.OpenSubKey("CLSID").SetValue(string.Empty, GUID);
			registryKey.OpenSubKey("CurVer").SetValue(string.Empty, applicationName);
		}
		catch
		{
			result = false;
		}
		return result;
	}

	private object GetProperty(object obj, string sProperty)
	{
		try
		{
			return obj.GetType().InvokeMember(sProperty, BindingFlags.GetProperty, null, obj, null);
		}
		catch (COMException ex)
		{
			BugSubmitter.SilentReport($"Failed to GetProperty({obj}, \"{sProperty}\") from PhotoshopInterface with HRESULT of {ex.HResult}\nMessage:\n{ex.Message} @summary Failed to GetProperty from PhotoshopInterface @assign bwhitman");
		}
		catch (TargetInvocationException ex2)
		{
			BugSubmitter.SilentReport($"Failed to GetProperty({obj}, \"{sProperty}\") from PhotoshopInterface\nSource: \"{ex2.Source}\" \nMessage:\n{ex2.Message} @summary Failed to GetProperty from PhotoshopInterface @assign bwhitman");
		}
		catch (Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
		}
		return null;
	}

	private object InvokeMethod(object obj, string sProperty, object[] oParam)
	{
		try
		{
			return obj.GetType().InvokeMember(sProperty, BindingFlags.InvokeMethod, null, obj, oParam);
		}
		catch (COMException ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to InvokeMethod({0}, \"{1}\"", obj.ToString(), sProperty);
			foreach (object arg in oParam)
			{
				stringBuilder.AppendFormat(", {0}", arg);
			}
			stringBuilder.AppendFormat(") from PhotoshopInterface with HRESULT of {0}\nMessage:\n{1} @summary Failed to InvokeMethod from PhotoshopInterface @assign bwhitman", ex.HResult, ex.Message);
			BugSubmitter.SilentReport(stringBuilder.ToString());
		}
		catch (TargetInvocationException ex2)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("Failed to InvokeMethod({0}, \"{1}\"", obj.ToString(), sProperty);
			foreach (object arg2 in oParam)
			{
				stringBuilder2.AppendFormat(", {0}", arg2);
			}
			stringBuilder2.AppendFormat(") from PhotoshopInterface\nSource: \"{0}\"\nMessage:\n{1} @summary Failed to InvokeMethod from PhotoshopInterface @assign bwhitman", ex2.Source, ex2.Message);
			BugSubmitter.SilentReport(stringBuilder2.ToString());
		}
		catch (Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
		}
		return null;
	}

	private object InvokeMethod(object obj, string sProperty, object oValue)
	{
		return InvokeMethod(obj, sProperty, new object[1] { oValue });
	}

	private T CallPhotoshopFunction<T>(Func<T> function, int retryCount = 0, int maxRetryCount = 5) where T : class
	{
		while (retryCount < maxRetryCount)
		{
			try
			{
				return function();
			}
			catch (COMException ex)
			{
				if (ex.HResult != -2147417846)
				{
					throw;
				}
				Thread.Sleep(10);
				retryCount++;
			}
		}
		return null;
	}

	private bool CallPhotoshopFunction(Action action, int retryCount = 0, int maxRetryCount = 5)
	{
		while (retryCount < maxRetryCount)
		{
			try
			{
				action();
				return true;
			}
			catch (COMException ex)
			{
				if (ex.HResult != -2147417846)
				{
					throw;
				}
				Thread.Sleep(10);
				retryCount++;
			}
		}
		return false;
	}

	private string FindActionDescriptorVersion()
	{
		string lastUsedPhotoshopVersion = GetLastUsedPhotoshopVersion();
		if (string.IsNullOrEmpty(lastUsedPhotoshopVersion))
		{
			string text = "Cannot initialize connection to Photoshop - the last used Photoshop version is no longer installed.";
			InvalidOperationException exception = new InvalidOperationException(text);
			ErrorHandling.Error(exception, text, ErrorLevel.ShowMessage);
			return string.Empty;
		}
		IEnumerable<string> photoshopActionDescriptorNames = GetPhotoshopActionDescriptorNames();
		foreach (string item in photoshopActionDescriptorNames)
		{
			if (item.EndsWith(lastUsedPhotoshopVersion))
			{
				return item;
			}
		}
		return string.Empty;
	}

	public object OpenImage(WindowsPath imageFilename)
	{
		object[] functionArgs = new object[1];
		if (IsFileOpen(imageFilename))
		{
			string canonicalFileName = GetCanonicalFileName(imageFilename);
			if (!string.IsNullOrEmpty(canonicalFileName))
			{
				functionArgs[0] = canonicalFileName;
			}
			else
			{
				string message = $"Can't get canonical file name: \n{imageFilename.ToString()}\nin Photoshop. Please make sure there are no open dialog windows in Photoshop.";
				ErrorHandling.Error(new ApplicationException(message), ErrorLevel.ShowMessage);
			}
		}
		else
		{
			functionArgs[0] = imageFilename.ToString();
		}
		Func<object> function = () => InvokeMethod(Photoshop, "Open", functionArgs);
		object obj = CallPhotoshopFunction(function);
		if (obj == null)
		{
			string message2 = $"Can't open up the file: \n{imageFilename.ToString()}\nin Photoshop. Please make sure there are no open dialog windows in Photoshop.";
			ErrorHandling.Error(new ApplicationException(message2), ErrorLevel.ShowMessage);
		}
		return obj;
	}

	private void CloseImage(object imageObjectToClose)
	{
		object[] functionArgs = new object[1];
		functionArgs[0] = 2;
		Action action = delegate
		{
			InvokeMethod(imageObjectToClose, "Close", functionArgs);
		};
		if (!CallPhotoshopFunction(action))
		{
			string message = "Can't communicate with Photoshop. Please make sure there are no dialog windows open and export again. If you continue to see this error, please try closing Photoshop. Thanks";
			ErrorHandling.Error(new ApplicationException(message), ErrorLevel.ShowMessage);
		}
	}

	public bool EstablishConnection()
	{
		return EstablishConnection("Photoshop.Application", null);
	}

	private bool EstablishConnection(string applicationKey, string actionDescriptor)
	{
		if (string.IsNullOrEmpty(applicationKey))
		{
			applicationKey = "Photoshop.Application";
		}
		PhotoshopType = Type.GetTypeFromProgID(applicationKey);
		if (PhotoshopType == null)
		{
			string message = $"The exporter is having trouble connecting to Photoshop with the key \"{applicationKey}\", please make sure you are running the 64-bit version of Photoshop. If you get this error again, please try closing Photoshop before exporting. Thanks.";
			ErrorHandling.Error(new ApplicationException(message), ErrorLevel.ShowMessage);
		}
		Func<object> function = () => Activator.CreateInstance(PhotoshopType);
		Photoshop = CallPhotoshopFunction(function);
		if (Photoshop == null)
		{
			string message2 = "The exporter is having trouble connecting to Photoshop, please make sure you are running the 64-bit version of Photoshop. If you get this error again, please try closing Photoshop before exporting. Thanks.";
			ErrorHandling.Error(new ApplicationException(message2), ErrorLevel.ShowMessage);
		}
		if (Photoshop != null)
		{
			if (string.IsNullOrEmpty(actionDescriptor))
			{
				ActionDescriptorType = FindActionDescriptorVersion();
				if (string.IsNullOrEmpty(ActionDescriptorType))
				{
					string text = "Cannot initialize connection to Photoshop - the last used Photoshop version does not have a matching action descriptor.  This requires Photoshop to be reinstalled.";
					InvalidOperationException exception = new InvalidOperationException(text);
					ErrorHandling.Error(exception, text, ErrorLevel.ShowMessage);
				}
			}
			else
			{
				ActionDescriptorType = actionDescriptor;
			}
		}
		return Photoshop != null && !string.IsNullOrEmpty(ActionDescriptorType);
	}

	public bool EstablishConnectionToRunningPhotoshop()
	{
		string runningPhotoshopVersion = GetRunningPhotoshopVersion();
		if (string.IsNullOrEmpty(runningPhotoshopVersion))
		{
			return EstablishConnection();
		}
		if (!PhotoshopVersionLookupDictionary.TryGetValue(runningPhotoshopVersion, out var value))
		{
			return EstablishConnection();
		}
		return EstablishConnection(value.ApplicationID, value.ActionDescriptorID);
	}

	public void ExportLayerGroup(string filename_to_export, string destination_folder_path, IEnumerable<string> normal_layer_groups, IEnumerable<string> gradient_map_layer_groups, IEnumerable<string> normal_layer_group_names, IEnumerable<string> gradient_map_layer_group_names, IEnumerable<string> normal_export_params, IEnumerable<string> gradient_export_params)
	{
		WindowsPath windowsPath = new WindowsPath(filename_to_export);
		WindowsPath destinationPath = new WindowsPath(destination_folder_path);
		Func<object> function = () => GetProperty(Photoshop, "documents");
		object obj = CallPhotoshopFunction(function);
		if (obj == null)
		{
			string message = "Can't communicate with Photoshop. Please make sure there are no dialog windows open and export again. If you continue to see this error, please try closing Photoshop. Thanks";
			ErrorHandling.Error(new ApplicationException(message), ErrorLevel.ShowMessage);
			return;
		}
		if (!destinationPath.FullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
		{
			destinationPath.SetPath(destinationPath.FullPath + Path.DirectorySeparatorChar);
		}
		WindowsPath windowsPath2 = new WindowsPath(filename_to_export);
		bool flag = IsFileOpen(windowsPath2);
		object obj2 = OpenImage(windowsPath2);
		if (obj2 != null)
		{
			Action action = delegate
			{
				CallAutomationPlugin(destinationPath.FullPath, Path.GetFileName(filename_to_export), normal_layer_groups, gradient_map_layer_groups, normal_layer_group_names, gradient_map_layer_group_names, normal_export_params, gradient_export_params);
			};
			if (!CallPhotoshopFunction(action))
			{
				string message2 = "Failed to call the Photoshop Automation Plug-in.  Ensure that the Photoshop - Civ6 tools are installed and try again.";
				ErrorHandling.Error(new ApplicationException(message2), ErrorLevel.ShowMessage);
			}
			if (!flag)
			{
				CloseImage(obj2);
			}
		}
		else
		{
			string message3 = $"Couldn't open the file: {filename_to_export}.  Please make sure that it exists and that it's a valid PSD file. Thanks";
			ErrorHandling.Error(new ApplicationException(message3), ErrorLevel.ShowMessage);
		}
	}

	public string ExportLooseLayers(WindowsPath filename_to_export, WindowsPath destination_folder_path, string export_parameters, string output_file_name)
	{
		Func<object> function = () => GetProperty(Photoshop, "documents");
		object obj = CallPhotoshopFunction(function);
		if (obj == null)
		{
			string message = "Can't communicate with Photoshop. Please make sure there are no dialog windows open and export again. If you continue to see this error, please try closing Photoshop. Thanks";
			ErrorHandling.Error(new ApplicationException(message), ErrorLevel.ShowMessage);
			return string.Empty;
		}
		if (!destination_folder_path.FullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
		{
			destination_folder_path.SetPath(destination_folder_path.FullPath + Path.DirectorySeparatorChar);
		}
		bool flag = IsFileOpen(filename_to_export);
		object obj2 = OpenImage(filename_to_export);
		if (obj2 != null)
		{
			Func<string> function2 = () => CallDDSExporterPlugin(destination_folder_path.FullPath, filename_to_export.FileName, export_parameters, output_file_name);
			string text = CallPhotoshopFunction(function2);
			if (string.IsNullOrEmpty(text))
			{
				string message2 = "Failed to call the Photoshop DDS Export Plug-in.  Ensure that the Photoshop - Civ6 tools are installed and try again.";
				ErrorHandling.Error(new ApplicationException(message2), ErrorLevel.ShowMessage);
			}
			if (!flag)
			{
				CloseImage(obj2);
			}
			return text;
		}
		string message3 = $"Couldn't open the file: {filename_to_export}.  Please make sure that it exists and that it's a valid PSD file. Thanks";
		ErrorHandling.Error(new ApplicationException(message3), ErrorLevel.ShowMessage);
		return string.Empty;
	}

	private void CallAutomationPlugin(string destination_path, string source_document_name, IEnumerable<string> normal_layer_groups, IEnumerable<string> gradient_map_layer_groups, IEnumerable<string> normal_layer_group_names, IEnumerable<string> gradient_map_layer_group_names, IEnumerable<string> normal_export_params, IEnumerable<string> gradient_export_params)
	{
		string text = "$FROMXML";
		Type typeFromProgID = Type.GetTypeFromProgID(ActionDescriptorType);
		object obj = Activator.CreateInstance(typeFromProgID);
		object[] array = new object[2]
		{
			InvokeMethod(Photoshop, "CharIDToTypeID", "docN"),
			source_document_name
		};
		InvokeMethod(obj, "PutString", array);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string normal_layer_group in normal_layer_groups)
		{
			stringBuilder.Append(normal_layer_group + ",");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "lyGN");
			array[1] = stringBuilder.ToString();
			InvokeMethod(obj, "PutString", array);
		}
		stringBuilder.Clear();
		foreach (string gradient_map_layer_group in gradient_map_layer_groups)
		{
			stringBuilder.Append(gradient_map_layer_group + ",");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "lyGG");
			array[1] = stringBuilder.ToString();
			InvokeMethod(obj, "PutString", array);
		}
		stringBuilder.Clear();
		foreach (string normal_layer_group_name in normal_layer_group_names)
		{
			stringBuilder.Append(Path.Combine(destination_path, normal_layer_group_name + ".dds") + ",");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "outN");
			array[1] = stringBuilder.ToString();
			InvokeMethod(obj, "PutString", array);
		}
		stringBuilder.Clear();
		foreach (string gradient_map_layer_group_name in gradient_map_layer_group_names)
		{
			stringBuilder.Append(Path.Combine(destination_path, gradient_map_layer_group_name + ".dds") + ",");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "outG");
			array[1] = stringBuilder.ToString();
			InvokeMethod(obj, "PutString", array);
		}
		stringBuilder.Clear();
		foreach (string normal_export_param in normal_export_params)
		{
			stringBuilder.Append(text + normal_export_param + "###");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 3, 3);
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "norP");
			array[1] = stringBuilder.ToString();
			InvokeMethod(obj, "PutString", array);
		}
		stringBuilder.Clear();
		foreach (string gradient_export_param in gradient_export_params)
		{
			stringBuilder.Append(text + gradient_export_param + "###");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 3, 3);
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "graP");
			array[1] = stringBuilder.ToString();
			InvokeMethod(obj, "PutString", array);
		}
		InvokeMethod(oParam: new object[3]
		{
			InvokeMethod(Photoshop, "StringIDToTypeID", "ExportLayerGroups"),
			obj,
			3
		}, obj: Photoshop, sProperty: "ExecuteAction");
	}

	private string CallDDSExporterPlugin(string destination_path, string source_document_name, string export_settings, string output_file_name)
	{
		Type typeFromProgID = Type.GetTypeFromProgID(ActionDescriptorType);
		object obj = Activator.CreateInstance(typeFromProgID);
		object obj2 = Activator.CreateInstance(typeFromProgID);
		int num = 240;
		object[] array = new object[2];
		output_file_name = Path.Combine(destination_path, output_file_name);
		array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "ddsF");
		array[1] = output_file_name;
		InvokeMethod(obj, "PutString", array);
		array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "ddsX");
		array[1] = true;
		InvokeMethod(obj, "PutBoolean", array);
		int num2 = export_settings.Length / num + 1;
		array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "numS");
		array[1] = num2;
		InvokeMethod(obj, "PutInteger", array);
		for (int i = 1; i <= num2; i++)
		{
			int startIndex = (i - 1) * num;
			int length = ((i * num < export_settings.Length) ? num : (export_settings.Length - (i - 1) * num));
			array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "zz0" + i);
			array[1] = export_settings.Substring(startIndex, length);
			InvokeMethod(obj, "PutString", array);
		}
		array = new object[3]
		{
			InvokeMethod(Photoshop, "CharIDToTypeID", "Usng"),
			InvokeMethod(Photoshop, "CharIDToTypeID", "ddsE"),
			obj
		};
		InvokeMethod(obj2, "PutObject", array);
		array[0] = InvokeMethod(Photoshop, "CharIDToTypeID", "Expr");
		array[1] = obj2;
		array[2] = 3;
		InvokeMethod(Photoshop, "ExecuteAction", array);
		return output_file_name;
	}

	public bool IsFileOpen(WindowsPath fullFilePath)
	{
		Func<object> function = () => GetProperty(Photoshop, "Documents");
		object openDocuments = CallPhotoshopFunction(function);
		if (openDocuments == null)
		{
			return false;
		}
		Func<object> function2 = () => GetProperty(openDocuments, "Count");
		object obj = CallPhotoshopFunction(function2);
		if (obj == null)
		{
			return false;
		}
		object[] functionArgs = new object[1];
		Func<object> function3 = () => InvokeMethod(openDocuments, "Item", functionArgs);
		object document = null;
		Func<string> function4 = () => (string)GetProperty(document, "FullName");
		bool flag = false;
		int num = (int)obj;
		for (int num2 = 0; num2 < num; num2++)
		{
			if (flag)
			{
				break;
			}
			functionArgs[0] = num2 + 1;
			try
			{
				document = CallPhotoshopFunction(function3);
				if (document != null)
				{
					string text = CallPhotoshopFunction(function4);
					if (!string.IsNullOrEmpty(text))
					{
						flag = fullFilePath.Equals(text);
					}
				}
			}
			catch (COMException comException)
			{
				if (!IsUnsavedDocumentException(comException))
				{
					throw;
				}
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException is COMException comException2)
				{
					if (!IsUnsavedDocumentException(comException2))
					{
						throw;
					}
					continue;
				}
				throw;
			}
		}
		return flag;
	}

	public string GetCanonicalFileName(WindowsPath fullFilePath)
	{
		Func<object> function = () => GetProperty(Photoshop, "Documents");
		object openDocuments = CallPhotoshopFunction(function);
		if (openDocuments == null)
		{
			return null;
		}
		Func<object> function2 = () => GetProperty(openDocuments, "Count");
		object obj = CallPhotoshopFunction(function2);
		if (obj == null)
		{
			return null;
		}
		object[] functionArgs = new object[1];
		Func<object> function3 = () => InvokeMethod(openDocuments, "Item", functionArgs);
		object document = null;
		Func<string> function4 = () => (string)GetProperty(document, "FullName");
		string text = null;
		int num = (int)obj;
		for (int num2 = 0; num2 < num; num2++)
		{
			if (text != null)
			{
				break;
			}
			functionArgs[0] = num2 + 1;
			try
			{
				document = CallPhotoshopFunction(function3);
				if (document != null)
				{
					string text2 = CallPhotoshopFunction(function4);
					if (!string.IsNullOrEmpty(text2) && fullFilePath.Equals(text2))
					{
						text = text2;
						return text;
					}
				}
			}
			catch (COMException comException)
			{
				if (!IsUnsavedDocumentException(comException))
				{
					throw;
				}
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException is COMException comException2)
				{
					if (!IsUnsavedDocumentException(comException2))
					{
						throw;
					}
					continue;
				}
				throw;
			}
		}
		return text;
	}

	private bool IsUnsavedDocumentException(COMException comException)
	{
		return comException.Message.StartsWith("The document has not yet been saved", StringComparison.CurrentCultureIgnoreCase);
	}
}

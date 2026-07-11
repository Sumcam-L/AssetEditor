using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;

namespace Sce.Atf.Applications;

public abstract class SettingsServiceBase : ISettingsService, ISettingsPathsProvider, ICommandClient
{
	protected class SettingsInfo
	{
		public class Setting
		{
			public string Name;

			public string ValueString;

			public PropertyDescriptor PropertyDescriptor;

			public Setting(PropertyDescriptor descriptor)
			{
				PropertyDescriptor = descriptor;
			}

			public Setting(string name, string valueString)
			{
				Name = name;
				ValueString = valueString;
			}

			public void Set(string name, string valueString)
			{
				Name = name;
				ValueString = valueString;
				if (PropertyDescriptor != null)
				{
					SetValue();
				}
			}

			public void Set(PropertyDescriptor descriptor)
			{
				PropertyDescriptor = descriptor;
				if (Name != null && ValueString != null)
				{
					SetValue();
				}
			}

			private void SetValue()
			{
				if (CanMakeChanges)
				{
					object value = GetValue(PropertyDescriptor.PropertyType, ValueString);
					PropertyDescriptor.SetValue(null, value);
				}
			}
		}

		public readonly string Name;

		public readonly SortedDictionary<string, Setting> Settings = new SortedDictionary<string, Setting>();

		public static bool CanMakeChanges { private get; set; }

		public SettingsInfo(string name)
		{
			Name = name;
		}

		public void Add(string name, string valueString)
		{
			if (Settings.TryGetValue(name, out var value))
			{
				value.Set(name, valueString);
			}
			else
			{
				Settings.Add(name, new Setting(name, valueString));
			}
		}

		public void Add(PropertyDescriptor descriptor)
		{
			if (Settings.TryGetValue(descriptor.Name, out var value))
			{
				value.Set(descriptor);
			}
			else
			{
				Settings.Add(descriptor.Name, new Setting(descriptor));
			}
		}
	}

	protected class UserSettingsInfo
	{
		public readonly string Name;

		public readonly List<PropertyDescriptor> Settings;

		public UserSettingsInfo(string name, PropertyDescriptor[] settings)
		{
			Name = name;
			Settings = new List<PropertyDescriptor>(settings);
		}
	}

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	private readonly string m_applicationName;

	private readonly string m_versionString;

	private string m_settingsPath;

	private string m_defaultSettingsPath;

	private readonly SortedDictionary<string, SettingsInfo> m_settings = new SortedDictionary<string, SettingsInfo>();

	private bool m_allowUserLoadSave = true;

	private bool m_allowUserEdits = true;

	private static readonly char[] s_delimiters = new char[3] { '/', '.', '\\' };

	public string SettingsPath
	{
		get
		{
			return m_settingsPath;
		}
		set
		{
			m_settingsPath = value;
		}
	}

	public string DefaultSettingsPath
	{
		get
		{
			return m_defaultSettingsPath;
		}
		set
		{
			m_defaultSettingsPath = value;
		}
	}

	public bool AllowUserEdits
	{
		get
		{
			return m_allowUserEdits;
		}
		set
		{
			m_allowUserEdits = value;
		}
	}

	public bool AllowUserLoadSave
	{
		get
		{
			return m_allowUserLoadSave;
		}
		set
		{
			m_allowUserLoadSave = value;
		}
	}

	public object State
	{
		get
		{
			MemoryStream memoryStream = new MemoryStream();
			Serialize(memoryStream);
			return memoryStream;
		}
		set
		{
			if (!(value is MemoryStream memoryStream))
			{
				throw new ArgumentException("Not a valid memento");
			}
			memoryStream.Position = 0L;
			Deserialize(memoryStream);
		}
	}

	public object UserState
	{
		get
		{
			List<Pair<PropertyDescriptor, object>> list = new List<Pair<PropertyDescriptor, object>>();
			foreach (PropertyDescriptor userPropertyDescriptor in UserPropertyDescriptors)
			{
				list.Add(new Pair<PropertyDescriptor, object>(userPropertyDescriptor, userPropertyDescriptor.GetValue(null)));
			}
			return list;
		}
		set
		{
			List<Pair<PropertyDescriptor, object>> list = (List<Pair<PropertyDescriptor, object>>)value;
			foreach (Pair<PropertyDescriptor, object> item in list)
			{
				item.First.SetValue(null, item.Second);
			}
		}
	}

	protected virtual ITreeView UserSettings => null;

	private IEnumerable<PropertyDescriptor> UserPropertyDescriptors
	{
		get
		{
			if (!(UserSettings is Tree<object> userSettings))
			{
				throw new InvalidOperationException("userSettings");
			}
			foreach (Tree<object> node in userSettings.Children)
			{
				if (!(node.Value is UserSettingsInfo info))
				{
					continue;
				}
				foreach (PropertyDescriptor setting in info.Settings)
				{
					yield return setting;
				}
			}
		}
	}

	public event EventHandler Saving;

	public event EventHandler Loading;

	public event EventHandler Reloaded;

	protected SettingsServiceBase()
	{
		Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		AssemblyName name = assembly.GetName();
		m_applicationName = name.Name;
		Version version = name.Version;
		m_versionString = version.Major + "." + version.Minor;
		string directoryName = Path.GetDirectoryName(new Uri(name.CodeBase).LocalPath);
		m_defaultSettingsPath = Path.Combine(directoryName, "DefaultSettings.xml");
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		m_settingsPath = $"{folderPath}\\{m_applicationName}\\{m_versionString}\\AppSettings.xml";
	}

	public void SetDefaults()
	{
		foreach (SettingsInfo value in m_settings.Values)
		{
			foreach (SettingsInfo.Setting value2 in value.Settings.Values)
			{
				if (value2.PropertyDescriptor != null && value2.PropertyDescriptor.CanResetValue(null))
				{
					value2.PropertyDescriptor.ResetValue(null);
				}
			}
		}
	}

	public virtual bool CanDoCommand(object tag)
	{
		bool result = false;
		if (tag is CommandId commandId && (commandId == CommandId.EditPreferences || commandId == CommandId.EditImportExportSettings))
		{
			result = true;
		}
		return result;
	}

	public virtual void DoCommand(object tag)
	{
		if (tag is CommandId commandId)
		{
			switch (commandId)
			{
			case CommandId.EditPreferences:
				PresentUserSettings(null);
				break;
			case CommandId.EditImportExportSettings:
				PresentLoadSaveSettings();
				break;
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public void RegisterSettings(string uid, params PropertyDescriptor[] settings)
	{
		if (!m_settings.TryGetValue(uid, out var value))
		{
			value = new SettingsInfo(uid);
			m_settings.Add(uid, value);
		}
		foreach (PropertyDescriptor descriptor in settings)
		{
			value.Add(descriptor);
		}
	}

	public void RegisterUserSettings(string pathName, params PropertyDescriptor[] settings)
	{
		if (string.IsNullOrEmpty(pathName))
		{
			throw new ArgumentException("pathName");
		}
		string[] array = pathName.Split(s_delimiters, 16);
		Tree<object> tree = UserSettings as Tree<object>;
		if (tree == null)
		{
			throw new InvalidOperationException("userSettings");
		}
		for (int i = 0; i < array.Length - 1; i++)
		{
			tree = GetOrCreateFolder(array[i], tree);
		}
		string text = array[array.Length - 1];
		UserSettingsInfo userSettingsInfo = null;
		int num = 0;
		foreach (Tree<object> child in tree.Children)
		{
			if (child.Value is UserSettingsInfo userSettingsInfo2)
			{
				if (userSettingsInfo2.Name == text)
				{
					userSettingsInfo = userSettingsInfo2;
					break;
				}
				if (userSettingsInfo2.Name.CompareTo(text) < 0)
				{
					num++;
				}
			}
		}
		if (userSettingsInfo != null)
		{
			foreach (PropertyDescriptor item in settings)
			{
				userSettingsInfo.Settings.Add(item);
			}
		}
		else
		{
			Tree<object> item2 = new Tree<object>(new UserSettingsInfo(text, settings));
			tree.Children.Insert(num, item2);
		}
	}

	public abstract void PresentUserSettings(string pathName);

	protected virtual void Initialize()
	{
		if (m_commandService != null)
		{
			if (m_allowUserEdits)
			{
				m_commandService.RegisterCommand(CommandId.EditPreferences, StandardMenu.Edit, StandardCommandGroup.EditPreferences, "Preferences...".Localize("Edit user preferences"), "Edit user preferences".Localize(), this);
			}
			if (m_allowUserLoadSave)
			{
				m_commandService.RegisterCommand(CommandId.EditImportExportSettings, StandardMenu.Edit, StandardCommandGroup.EditPreferences, "Load or Save Settings...".Localize(), "User can save or load application settings from files".Localize(), this);
			}
		}
		LoadSettings();
	}

	protected abstract void PresentLoadSaveSettings();

	public void SaveSettings()
	{
		string text = string.Empty;
		string mutexName = GetMutexName(m_settingsPath);
		using Mutex mutex = new Mutex(initiallyOwned: false, mutexName);
		try
		{
			mutex.WaitOne();
			text = Path.GetTempFileName();
			using (Stream stream = File.Create(text))
			{
				Serialize(stream);
			}
			string directoryName = Path.GetDirectoryName(m_settingsPath);
			Directory.CreateDirectory(directoryName);
			string text2 = Path.Combine(directoryName, "~Settings.xml");
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			if (File.Exists(m_settingsPath))
			{
				File.Move(m_settingsPath, text2);
			}
			File.Move(text, m_settingsPath);
			File.Delete(text2);
		}
		catch (TargetInvocationException)
		{
		}
		catch (Exception ex2)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex2.Message);
		}
		finally
		{
			File.Delete(text);
			mutex.ReleaseMutex();
		}
	}

	public void LoadSettings()
	{
		this.Loading.Raise(this, EventArgs.Empty);
		try
		{
			bool flag = File.Exists(m_defaultSettingsPath);
			bool flag2 = File.Exists(m_settingsPath);
			SettingsInfo.CanMakeChanges = flag && !flag2;
			if (flag)
			{
				using Stream stream = File.OpenRead(m_defaultSettingsPath);
				Deserialize(stream);
			}
			SettingsInfo.CanMakeChanges = true;
			if (flag2)
			{
				using (Stream stream2 = File.OpenRead(m_settingsPath))
				{
					Deserialize(stream2);
					return;
				}
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
		finally
		{
			SettingsInfo.CanMakeChanges = true;
		}
	}

	protected void Serialize(Stream stream)
	{
		this.Saving.Raise(this, EventArgs.Empty);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
		XmlElement xmlElement = xmlDocument.CreateElement("settings");
		xmlDocument.AppendChild(xmlElement);
		xmlElement.SetAttribute("appName", m_applicationName);
		xmlElement.SetAttribute("appVersion", m_versionString);
		foreach (SettingsInfo value2 in m_settings.Values)
		{
			XmlElement xmlElement2 = xmlDocument.CreateElement("block");
			xmlElement2.SetAttribute("id", value2.Name);
			foreach (SettingsInfo.Setting value3 in value2.Settings.Values)
			{
				PropertyDescriptor propertyDescriptor = value3.PropertyDescriptor;
				if (propertyDescriptor != null)
				{
					object value = propertyDescriptor.GetValue(null);
					if (CanWriteValue(value))
					{
						WriteValue(propertyDescriptor.Name, value, xmlElement2);
					}
				}
				else
				{
					WriteValue(value3.Name, value3.ValueString, xmlElement2);
				}
			}
			if (xmlElement2.ChildNodes.Count > 0)
			{
				xmlElement.AppendChild(xmlElement2);
			}
		}
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.CloseOutput = false;
		xmlWriterSettings.Indent = true;
		using XmlWriter w = XmlWriter.Create(stream, xmlWriterSettings);
		xmlDocument.WriteTo(w);
	}

	protected bool Deserialize(Stream stream)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(stream);
			XmlElement documentElement = xmlDocument.DocumentElement;
			XmlNodeList xmlNodeList = documentElement.SelectNodes("block");
			if (xmlNodeList == null || xmlNodeList.Count == 0)
			{
				throw new Exception("The setting file is empty");
			}
			foreach (XmlElement item in xmlNodeList)
			{
				try
				{
					string attribute = item.GetAttribute("id");
					if (!m_settings.TryGetValue(attribute, out var value))
					{
						value = new SettingsInfo(attribute);
						m_settings.Add(attribute, value);
					}
					XmlNodeList xmlNodeList2 = item.SelectNodes("value");
					if (xmlNodeList2 == null || xmlNodeList2.Count == 0)
					{
						continue;
					}
					foreach (XmlElement item2 in xmlNodeList2)
					{
						string attribute2 = item2.GetAttribute("name");
						string elementValueString = GetElementValueString(item2);
						value.Add(attribute2, elementValueString);
					}
				}
				catch (Exception ex)
				{
					Outputs.WriteLine(OutputMessageType.Error, ex.Message);
				}
			}
			OnReloaded();
		}
		catch (Exception ex2)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Can't load settings".Localize() + ": {0}", ex2.Message);
			return false;
		}
		return true;
	}

	protected virtual void OnReloaded()
	{
		this.Reloaded.Raise(this, EventArgs.Empty);
	}

	protected List<PropertyDescriptor> GetProperties(Tree<object> tree)
	{
		if (!(tree.Value is UserSettingsInfo { Settings: var settings }))
		{
			return null;
		}
		return settings;
	}

	protected Path<object> GetSettingsPath(string pathName)
	{
		string[] array = pathName.Split(s_delimiters, 16);
		object[] array2 = new object[array.Length + 1];
		if (!(UserSettings is Tree<object> tree))
		{
			throw new InvalidOperationException("userSettings");
		}
		Tree<object> tree2 = tree;
		array2[0] = tree.Value;
		for (int i = 1; i < array2.Length - 1; i++)
		{
			tree2 = GetOrCreateFolder(array[i - 1], tree2);
			array2[i] = tree2.Value;
		}
		foreach (Tree<object> child in tree2.Children)
		{
			if (tree2.Value is UserSettingsInfo userSettingsInfo && userSettingsInfo.Name == array[array.Length - 1])
			{
				array2[array2.Length - 1] = child.Value;
				break;
			}
		}
		return new Path<object>(array2);
	}

	private Tree<object> GetOrCreateFolder(string name, Tree<object> tree)
	{
		Tree<object> tree2 = null;
		int num = 0;
		foreach (Tree<object> child in tree.Children)
		{
			if (child.Value is string text)
			{
				if (text == name)
				{
					tree2 = child;
					break;
				}
				if (text.CompareTo(name) < 0)
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		if (tree2 == null)
		{
			tree2 = new Tree<object>(name);
			tree.Children.Insert(num, tree2);
		}
		return tree2;
	}

	private bool CanWriteValue(object value)
	{
		if (value == null)
		{
			return false;
		}
		TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
		return CanConvertToAndFromString(converter) || value.GetType().IsSerializable;
	}

	private void WriteValue(string name, object value, XmlElement block)
	{
		if (value == null)
		{
			return;
		}
		string text = null;
		Type type = value.GetType();
		TypeConverter converter = TypeDescriptor.GetConverter(type);
		if (CanConvertToAndFromString(converter))
		{
			text = converter.ConvertToInvariantString(value);
		}
		else if (type.IsSerializable)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, value);
			text = Convert.ToBase64String(memoryStream.GetBuffer());
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		XmlDocument ownerDocument = block.OwnerDocument;
		XmlElement xmlElement = ownerDocument.CreateElement("value");
		xmlElement.SetAttribute("name", name);
		xmlElement.SetAttribute("type", type.Name);
		XmlDocument xmlDocument = StringToXmlDoc(text);
		if (xmlDocument != null)
		{
			if (xmlDocument.FirstChild is XmlDeclaration oldChild)
			{
				xmlDocument.RemoveChild(oldChild);
			}
			xmlElement.InnerXml = xmlDocument.DocumentElement.OuterXml;
		}
		else
		{
			xmlElement.InnerText = text;
		}
		block.AppendChild(xmlElement);
	}

	private static string GetElementValueString(XmlElement element)
	{
		string text = element.InnerText;
		if (string.IsNullOrEmpty(text))
		{
			text = element.InnerXml;
		}
		return text;
	}

	private static object GetValue(Type type, string valueString)
	{
		object result = null;
		try
		{
			TypeConverter converter = TypeDescriptor.GetConverter(type);
			if (CanConvertToAndFromString(converter))
			{
				result = converter.ConvertFromInvariantString(valueString);
			}
			else
			{
				byte[] buffer = Convert.FromBase64String(valueString);
				using MemoryStream serializationStream = new MemoryStream(buffer);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				result = binaryFormatter.Deserialize(serializationStream);
			}
		}
		catch
		{
			result = null;
		}
		return result;
	}

	private static bool CanConvertToAndFromString(TypeConverter converter)
	{
		return converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string));
	}

	private XmlDocument StringToXmlDoc(string strXml)
	{
		XmlDocument xmlDocument = null;
		try
		{
			int length = ((strXml.Length > 20) ? 20 : strXml.Length);
			string text = StringUtil.RemoveAllWhiteSpace(strXml.Substring(0, length)).ToLower();
			if (text.Contains("<?xmlversion="))
			{
				xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(strXml);
			}
		}
		catch
		{
			xmlDocument = null;
		}
		return xmlDocument;
	}

	private static string GetMutexName(string pathName)
	{
		string text = pathName;
		if (text.Length > 250)
		{
			text = text.Substring(text.Length - 250);
		}
		text = text.Replace('/', '-');
		return text.Replace('\\', '-');
	}
}

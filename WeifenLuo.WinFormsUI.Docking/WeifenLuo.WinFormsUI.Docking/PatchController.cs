using System;
using System.Configuration;
using Microsoft.Win32;
using WeifenLuo.WinFormsUI.Docking.Configuration;

namespace WeifenLuo.WinFormsUI.Docking;

public static class PatchController
{
	private static bool? _highDpi;

	private static bool? _memoryLeakFix;

	private static bool? _focusLostFix;

	private static bool? _nestedDisposalFix;

	private static bool? _fontInheritanceFix;

	private static bool? _contentOrderFix;

	private static bool? _activeXFix;

	private static bool? _displayingPaneFix;

	public static bool? EnableAll { private get; set; }

	public static bool? EnableHighDpi
	{
		get
		{
			if (_highDpi.HasValue)
			{
				return _highDpi;
			}
			if (EnableAll.HasValue)
			{
				return _highDpi = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _highDpi = patchSection.EnableAll;
				}
				return _highDpi = patchSection.EnableHighDpi;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableHighDpi");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _highDpi = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableHighDpi");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _highDpi = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableHighDpi");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _highDpi = result3;
					}
				}
			}
			return _highDpi = true;
		}
		set
		{
			_highDpi = value;
		}
	}

	public static bool? EnableMemoryLeakFix
	{
		get
		{
			if (_memoryLeakFix.HasValue)
			{
				return _memoryLeakFix;
			}
			if (EnableAll.HasValue)
			{
				return _memoryLeakFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _memoryLeakFix = patchSection.EnableAll;
				}
				return _memoryLeakFix = patchSection.EnableMemoryLeakFix;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableMemoryLeakFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _memoryLeakFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableMemoryLeakFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _memoryLeakFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableMemoryLeakFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _memoryLeakFix = result3;
					}
				}
			}
			return _memoryLeakFix = true;
		}
		set
		{
			_memoryLeakFix = value;
		}
	}

	public static bool? EnableMainWindowFocusLostFix
	{
		get
		{
			if (_focusLostFix.HasValue)
			{
				return _focusLostFix;
			}
			if (EnableAll.HasValue)
			{
				return _focusLostFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _focusLostFix = patchSection.EnableAll;
				}
				return _focusLostFix = patchSection.EnableMainWindowFocusLostFix;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableMainWindowFocusLostFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _focusLostFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableMainWindowFocusLostFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _focusLostFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableMainWindowFocusLostFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _focusLostFix = result3;
					}
				}
			}
			return _focusLostFix = true;
		}
		set
		{
			_focusLostFix = value;
		}
	}

	public static bool? EnableNestedDisposalFix
	{
		get
		{
			if (_nestedDisposalFix.HasValue)
			{
				return _nestedDisposalFix;
			}
			if (EnableAll.HasValue)
			{
				return _nestedDisposalFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _nestedDisposalFix = patchSection.EnableAll;
				}
				return _nestedDisposalFix = patchSection.EnableNestedDisposalFix;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableNestedDisposalFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _nestedDisposalFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableNestedDisposalFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _nestedDisposalFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableNestedDisposalFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _nestedDisposalFix = result3;
					}
				}
			}
			return _nestedDisposalFix = true;
		}
		set
		{
			_focusLostFix = value;
		}
	}

	public static bool? EnableFontInheritanceFix
	{
		get
		{
			if (_fontInheritanceFix.HasValue)
			{
				return _fontInheritanceFix;
			}
			if (EnableAll.HasValue)
			{
				return _fontInheritanceFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _fontInheritanceFix = patchSection.EnableAll;
				}
				return _fontInheritanceFix = patchSection.EnableFontInheritanceFix;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableFontInheritanceFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _fontInheritanceFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableFontInheritanceFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _fontInheritanceFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableFontInheritanceFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _fontInheritanceFix = result3;
					}
				}
			}
			return _fontInheritanceFix = true;
		}
		set
		{
			_fontInheritanceFix = value;
		}
	}

	public static bool? EnableContentOrderFix
	{
		get
		{
			if (_contentOrderFix.HasValue)
			{
				return _contentOrderFix;
			}
			if (EnableAll.HasValue)
			{
				return _contentOrderFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _contentOrderFix = patchSection.EnableAll;
				}
				return _contentOrderFix = patchSection.EnableContentOrderFix;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableContentOrderFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _contentOrderFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableContentOrderFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _contentOrderFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableContentOrderFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _contentOrderFix = result3;
					}
				}
			}
			return _contentOrderFix = true;
		}
		set
		{
			_contentOrderFix = value;
		}
	}

	public static bool? EnableActiveXFix
	{
		get
		{
			if (_activeXFix.HasValue)
			{
				return _activeXFix;
			}
			if (EnableAll.HasValue)
			{
				return _activeXFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _activeXFix = patchSection.EnableAll;
				}
				return _activeXFix = patchSection.EnableContentOrderFix;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableActiveXFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _activeXFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableActiveXFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _activeXFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableActiveXFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _activeXFix = result3;
					}
				}
			}
			return _activeXFix = false;
		}
		set
		{
			_activeXFix = value;
		}
	}

	public static bool? EnableDisplayingPaneFix
	{
		get
		{
			if (_displayingPaneFix.HasValue)
			{
				return _displayingPaneFix;
			}
			if (EnableAll.HasValue)
			{
				return _displayingPaneFix = EnableAll;
			}
			if (ConfigurationManager.GetSection("dockPanelSuite") is PatchSection { EnableAll: var enableAll } patchSection)
			{
				if (enableAll.HasValue)
				{
					return _displayingPaneFix = patchSection.EnableAll;
				}
				return _displayingPaneFix = patchSection.EnableHighDpi;
			}
			string environmentVariable = Environment.GetEnvironmentVariable("DPS_EnableDisplayingPaneFix");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				bool result = false;
				if (bool.TryParse(environmentVariable, out result))
				{
					return _displayingPaneFix = result;
				}
			}
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("EnableDisplayingPaneFix");
				if (value != null)
				{
					bool result2 = false;
					if (bool.TryParse(value.ToString(), out result2))
					{
						return _displayingPaneFix = result2;
					}
				}
			}
			RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\DockPanelSuite");
			if (registryKey2 != null)
			{
				object value2 = registryKey2.GetValue("EnableDisplayingPaneFix");
				if (value2 != null)
				{
					bool result3 = false;
					if (bool.TryParse(value2.ToString(), out result3))
					{
						return _displayingPaneFix = result3;
					}
				}
			}
			return _displayingPaneFix = true;
		}
		set
		{
			_displayingPaneFix = value;
		}
	}

	public static void Reset()
	{
		EnableAll = (_highDpi = (_memoryLeakFix = (_nestedDisposalFix = (_focusLostFix = (_contentOrderFix = (_fontInheritanceFix = (_activeXFix = (_displayingPaneFix = null))))))));
	}
}

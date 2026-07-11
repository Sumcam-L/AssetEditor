using System.Configuration;

namespace WeifenLuo.WinFormsUI.Docking.Configuration;

public class PatchSection : ConfigurationSection
{
	[ConfigurationProperty("enableAll", DefaultValue = null)]
	public bool? EnableAll => (bool?)base["enableAll"];

	[ConfigurationProperty("enableHighDpi", DefaultValue = true)]
	public bool EnableHighDpi => (bool)base["enableHighDpi"];

	[ConfigurationProperty("enableMemoryLeakFix", DefaultValue = true)]
	public bool EnableMemoryLeakFix => (bool)base["enableMemoryLeakFix"];

	[ConfigurationProperty("enableMainWindowFocusLostFix", DefaultValue = true)]
	public bool EnableMainWindowFocusLostFix => (bool)base["enableMainWindowFocusLostFix"];

	[ConfigurationProperty("enableNestedDisposalFix", DefaultValue = true)]
	public bool EnableNestedDisposalFix => (bool)base["enableNestedDisposalFix"];

	[ConfigurationProperty("enableFontInheritanceFix", DefaultValue = true)]
	public bool EnableFontInheritanceFix => (bool)base["enableFontInheritanceFix"];

	[ConfigurationProperty("enableContentOrderFix", DefaultValue = true)]
	public bool EnableContentOrderFix => (bool)base["enableContentOrderFix"];

	[ConfigurationProperty("enableActiveXFix", DefaultValue = false)]
	public bool EnableActiveXFix => (bool)base["enableActiveXFix"];

	[ConfigurationProperty("enableDisplayingPaneFix", DefaultValue = true)]
	public bool EnableDisplayingPaneFix => (bool)base["enableDisplayingPaneFix"];
}

using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IVersionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class VersionService : IVersionService
{
	private Version m_applicationVersion;

	private Version m_localToolVersion;

	public Version ApplicationVersion
	{
		get
		{
			if (m_applicationVersion == null)
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				string productVersion = Application.ProductVersion;
				m_applicationVersion = ToolVersionConverter.GetVersion(productVersion, version);
				string name = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name;
				Outputs.WriteLine(OutputMessageType.Info, "{0}: {1} ", name, m_applicationVersion.ToString());
			}
			return m_applicationVersion;
		}
	}

	public Version LocalToolVersion
	{
		get
		{
			if (m_localToolVersion == null)
			{
				m_localToolVersion = new Version(1, 0, 0, 0);
			}
			return m_localToolVersion;
		}
	}

	public bool IsLocalBuild()
	{
		return ApplicationVersion == LocalToolVersion;
	}
}

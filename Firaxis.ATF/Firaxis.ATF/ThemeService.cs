using System;
using System.ComponentModel.Composition;
using Firaxis.Theme;
using Sce.Atf;
using Sce.Atf.Applications;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.ATF;

[Export(typeof(IThemeService))]
[Export(typeof(ThemeService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ThemeService : IThemeService
{
	private ThemeBase m_currentTheme = new FiraxisTheme();

	private IControlHostService ControlHostService { get; set; }

	public ThemeBase ActiveTheme
	{
		get
		{
			return m_currentTheme;
		}
		set
		{
			if (m_currentTheme != value && (m_currentTheme == null || value == null || m_currentTheme.GetType() != value.GetType()))
			{
				ControlHostService.Theme = value;
				m_currentTheme = value;
				SkinService.ApplyActiveSkin(m_currentTheme);
				this.ThemeChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler ThemeChanged;

	[ImportingConstructor]
	public ThemeService(IControlHostService ctlHostSvc)
	{
		ControlHostService = ctlHostSvc;
		ControlHostService.Theme = m_currentTheme;
	}
}

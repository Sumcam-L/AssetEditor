using System.Drawing;
using System.Drawing.Drawing2D;

namespace WeifenLuo.WinFormsUI.Docking;

public class VS2005Theme : ThemeBase
{
	public VS2005Theme()
	{
		base.Skin = CreateVisualStudio2005();
		base.Measures.SplitterSize = 4;
	}

	internal static DockPanelSkin CreateVisualStudio2005()
	{
		return new DockPanelSkin
		{
			AutoHideStripSkin = 
			{
				DockStripGradient = 
				{
					StartColor = SystemColors.ControlLight,
					EndColor = SystemColors.ControlLight
				},
				TabGradient = 
				{
					TextColor = SystemColors.ControlDarkDark
				}
			},
			DockPaneStripSkin = 
			{
				DocumentGradient = 
				{
					DockStripGradient = 
					{
						StartColor = SystemColors.Control,
						EndColor = SystemColors.Control
					},
					ActiveTabGradient = 
					{
						StartColor = SystemColors.ControlLightLight,
						EndColor = SystemColors.ControlLightLight
					},
					InactiveTabGradient = 
					{
						StartColor = SystemColors.ControlLight,
						EndColor = SystemColors.ControlLight
					}
				},
				ToolWindowGradient = 
				{
					DockStripGradient = 
					{
						StartColor = SystemColors.ControlLight,
						EndColor = SystemColors.ControlLight
					},
					ActiveTabGradient = 
					{
						StartColor = SystemColors.Control,
						EndColor = SystemColors.Control
					},
					InactiveTabGradient = 
					{
						StartColor = Color.Transparent,
						EndColor = Color.Transparent,
						TextColor = SystemColors.ControlDarkDark
					},
					ActiveCaptionGradient = 
					{
						StartColor = SystemColors.GradientActiveCaption,
						EndColor = SystemColors.ActiveCaption,
						LinearGradientMode = LinearGradientMode.Vertical,
						TextColor = SystemColors.ActiveCaptionText
					},
					InactiveCaptionGradient = 
					{
						StartColor = SystemColors.GradientInactiveCaption,
						EndColor = SystemColors.InactiveCaption,
						LinearGradientMode = LinearGradientMode.Vertical,
						TextColor = SystemColors.InactiveCaptionText
					}
				}
			}
		};
	}
}

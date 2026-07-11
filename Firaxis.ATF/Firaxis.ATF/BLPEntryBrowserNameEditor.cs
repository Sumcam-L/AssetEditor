using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using Firaxis.CivTech;

namespace Firaxis.ATF;

public class BLPEntryBrowserNameEditor : UITypeEditor
{
	private readonly BLPEntryBrowser m_blpEntryBrowser;

	private readonly ICivTechService m_civTechService;

	private readonly AssetBrowserFileCommands m_fileCommands;

	private readonly IXLPRegistry m_xlpRegistry;

	public BLPEntryBrowserNameEditor(IXLPRegistry xlpReg, ICivTechService civTechService, AssetBrowserFileCommands fileCommands)
	{
		m_blpEntryBrowser = new BLPEntryBrowser(xlpReg, civTechService);
		m_xlpRegistry = xlpReg;
		m_civTechService = civTechService;
		m_fileCommands = fileCommands;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			using BLPEntryBrowserLauncher bLPEntryBrowserLauncher = new BLPEntryBrowserLauncher(context, value, m_blpEntryBrowser, m_civTechService, m_fileCommands, m_xlpRegistry);
			windowsFormsEditorService.DropDownControl(bLPEntryBrowserLauncher);
			if (bLPEntryBrowserLauncher.UserPressedOK)
			{
				value = bLPEntryBrowserLauncher.SelectedData;
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}
}

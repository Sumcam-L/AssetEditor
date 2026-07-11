using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetBrowser;

public class AssetBrowserNameEditor : UITypeEditor
{
	public IEnumerable<InstanceType> Filter { get; set; }

	private ICivTechService CivTechService { get; set; }

	private IEntityFilteringService EntityFilterService { get; set; }

	public AssetBrowserNameEditor(ICivTechService civTechSvc)
	{
		CivTechService = civTechSvc;
		Filter = new List<InstanceType> { InstanceType.IT_ASSET };
	}

	public AssetBrowserNameEditor(ICivTechService civTechSvc, IEntityFilteringService filterSvc, IEnumerable<InstanceType> filter)
	{
		CivTechService = civTechSvc;
		EntityFilterService = filterSvc;
		Filter = filter;
	}

	private InstanceType GetTypeFromExtension(string absolutePath)
	{
		string extension = Path.GetExtension(absolutePath);
		return EnumToStringConverter.GetTypeFromExtension(extension);
	}

	private string GetNameFromPath(string absolutePath)
	{
		InstanceType typeFromExtension = GetTypeFromExtension(absolutePath);
		string text = absolutePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		foreach (string pantryRoot in CivTechService.ProjectMapService.LayeredPantry.PantryRoots)
		{
			string text2 = StaticMethods.PantryRootForInstanceType(pantryRoot, typeFromExtension).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.AltDirectorySeparatorChar;
			if (text.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
			{
				string text3 = Path.ChangeExtension(text, null);
				return text3.Replace(text2, "");
			}
		}
		return absolutePath;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		object obj = context?.Instance;
		IEntityFilteringContext filteringContext = EntityFilterService.GetFilteringContext(Filter, (obj as IAssetBrowserAllowedClassProvider)?.AllowedClasses);
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = ((System.Windows.Application.Current == null) ? AssetBrowserDialog.CreateDialog(out dialogViewModel, filteringContext, System.Windows.Forms.Application.OpenForms.OfType<Form>().FirstOrDefault((Form fod) => fod.Visible)) : AssetBrowserDialog.CreateDialog(out dialogViewModel, filteringContext, System.Windows.Application.Current.MainWindow));
		if (flag.HasValue && flag.Value && dialogViewModel.HasSelection)
		{
			value = GetNameFromPath(dialogViewModel.SelectedPath);
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}
}

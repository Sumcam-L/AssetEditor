using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Firaxis.AssetBrowser;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace Firaxis.ATF;

[Export(typeof(IAssetBrowserDialogService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetBrowserFileService : IAssetBrowserDialogService
{
	private ICivTechService CivTechService { get; set; }

	private IEntityFilteringService EntityFilteringService { get; set; }

	[Import(AllowDefault = true)]
	private WpfSkinService SkinService { get; set; }

	public string InitialDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(FirstRunInitialDirectory) || !Directory.Exists(FirstRunInitialDirectory))
			{
				FirstRunInitialDirectory = CivTechService.PrimaryProject.Paths.GamePantry;
			}
			return FirstRunInitialDirectory;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
			{
				FirstRunInitialDirectory = value;
			}
		}
	}

	public string ForcedInitialDirectory { get; set; }

	[Import(AllowDefault = true)]
	public IMainWindow MainWindow { get; set; }

	[Import(AllowDefault = true)]
	public Form MainForm { get; set; }

	public string FirstRunInitialDirectory { get; protected set; }

	[ImportingConstructor]
	public AssetBrowserFileService(ICivTechService civTechSvc, IEntityFilteringService entityFilteringService)
	{
		CivTechService = civTechSvc;
		EntityFilteringService = entityFilteringService;
	}

	private void SkinDialog(Window dialog)
	{
		if (SkinService != null)
		{
			RoutedEventHandler loadedHandler = null;
			loadedHandler = delegate
			{
				SkinService.ApplySkin(dialog);
				dialog.Loaded -= loadedHandler;
			};
			dialog.Loaded += loadedHandler;
			CancelEventHandler closingHandler = null;
			closingHandler = delegate
			{
				SkinService.RemoveSkin(dialog);
				dialog.Closing -= closingHandler;
			};
			dialog.Closing += closingHandler;
		}
	}

	public DialogResult OpenFileName(ref string pathName, IEntityFilteringContext entityFilteringContext)
	{
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, entityFilteringContext, GetDialogOwner());
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return DialogResult.Cancel;
		}
		pathName = dialogViewModel.SelectedPath;
		return DialogResult.OK;
	}

	public DialogResult OpenFileName(ref string pathName, IEnumerable<InstanceType> filter)
	{
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, filter, Enumerable.Empty<string>(), GetDialogOwner());
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return DialogResult.Cancel;
		}
		pathName = dialogViewModel.SelectedPath;
		return DialogResult.OK;
	}

	public DialogResult OpenFileName(ref string pathName, IEnumerable<InstanceType> filter, IEnumerable<string> allowedClasses)
	{
		allowedClasses = allowedClasses.ToArray();
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, filter, allowedClasses, GetDialogOwner());
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return DialogResult.Cancel;
		}
		if (IsSelectionValid(dialogViewModel.SelectedEntities, allowedClasses))
		{
			pathName = dialogViewModel.SelectedPath;
			return DialogResult.OK;
		}
		Outputs.WriteLine(OutputMessageType.Warning, "The selected entity is not of the appropriate class.  Only the following classes are valid for this selection: \n\t{0}", string.Join("\n\t* ", allowedClasses));
		return DialogResult.Cancel;
	}

	public DialogResult OpenFileNames(ref string[] pathNames, IEnumerable<InstanceType> filter)
	{
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, filter, null, GetDialogOwner(), allowMultipleSelection: true);
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return DialogResult.Cancel;
		}
		pathNames = dialogViewModel.SelectedPaths.ToArray();
		return DialogResult.OK;
	}

	private bool IsSelectionValid(IEnumerable<KeyValuePair<string, InstanceType>> selectedEntities, IEnumerable<string> allowedClasses)
	{
		allowedClasses = allowedClasses?.ToArray();
		bool flag = allowedClasses == null || !allowedClasses.Any();
		if (flag)
		{
			return true;
		}
		using (IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() }))
		{
			using IEnumerator<KeyValuePair<string, InstanceType>> enumerator = selectedEntities.GetEnumerator();
			flag = true;
			while (flag && enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				InstanceType value = enumerator.Current.Value;
				IInstanceEntity instanceEntity = instanceSet.LoadEntityIfUnique(key, value);
				flag = instanceEntity != null && allowedClasses.Contains(instanceEntity.ClassName);
			}
		}
		return flag;
	}

	public DialogResult OpenFileNames(ref string[] pathNames, IEnumerable<InstanceType> filter, IEnumerable<string> allowedClasses)
	{
		allowedClasses = allowedClasses.ToArray();
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, filter, allowedClasses, GetDialogOwner(), allowMultipleSelection: true);
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return DialogResult.Cancel;
		}
		if (IsSelectionValid(dialogViewModel.SelectedEntities, allowedClasses))
		{
			pathNames = dialogViewModel.SelectedPaths.ToArray();
			return DialogResult.OK;
		}
		Outputs.WriteLine(OutputMessageType.Warning, "The selected entity is not of the appropriate class. Only the following classes are valid for this selection: \n\t{0}", string.Join("\n\t* ", allowedClasses));
		return DialogResult.Cancel;
	}

	public DialogResult OpenEntities(IDictionary<string, InstanceType> entities, IEnumerable<InstanceType> filter, IEnumerable<string> allowedClasses)
	{
		if (entities == null)
		{
			throw new ArgumentNullException("entities", "Cannot store entities in a null dictionary.");
		}
		allowedClasses = allowedClasses.ToArray();
		AssetBrowserDialogViewModel dialogViewModel;
		bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, filter, allowedClasses, GetDialogOwner(), allowMultipleSelection: true);
		if (!flag.HasValue || !flag.Value || !dialogViewModel.HasSelection)
		{
			return DialogResult.Cancel;
		}
		foreach (KeyValuePair<string, InstanceType> selectedEntity in dialogViewModel.SelectedEntities)
		{
			entities.Add(selectedEntity.Key, selectedEntity.Value);
		}
		return DialogResult.OK;
	}

	public DialogResult SaveFileName(out string pathName, IInstanceEntity filter)
	{
		string path = StaticMethods.PantryRootForInstanceType(CivTechService.PrimaryProject.Paths.GamePantry, filter.Type);
		pathName = Path.Combine(path, filter.Name + filter.XMLExtension).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		return DialogResult.OK;
	}

	public DialogResult ConfirmFileClose(string message)
	{
		ConfirmationDialog obj = new ConfirmationDialog("Close".Localize("Close file"), message)
		{
			YesButtonText = "&Save".Localize("The '&' is optional and means that Alt+S is the keyboard shortcut on this button"),
			NoButtonText = "&Discard".Localize("The '&' is optional and means that Alt+D is the keyboard shortcut on this button")
		};
		DialogResult result = obj.ShowDialog(GetDialogOwner());
		obj.Dispose();
		return result;
	}

	public bool PathExists(string pathName)
	{
		return false;
	}

	private IWin32Window GetDialogOwner()
	{
		if (MainWindow != null)
		{
			return MainWindow.DialogOwner;
		}
		if (MainForm != null)
		{
			return MainForm;
		}
		return Form.ActiveForm;
	}
}

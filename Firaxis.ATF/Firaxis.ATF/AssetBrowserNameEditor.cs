using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class AssetBrowserNameEditor : UITypeEditor
{
	private ICivTechService m_civTechService;

	private IEntityFilteringService m_filteringService;

	private IAssetBrowserDialogService m_fileDialogService;

	private IInstanceSet m_instanceSet;

	public IEnumerable<InstanceType> Filter { get; set; }

	private IEnumerable<string> ValidClassNames { get; set; } = Enumerable.Empty<string>();

	private IEntityFilteringContext FilteringContext { get; set; }

	[ImportingConstructor]
	public AssetBrowserNameEditor(ICivTechService civTechSvc, IEntityFilteringService filteringService, IAssetBrowserDialogService fileDialogService)
	{
		m_civTechService = civTechSvc;
		m_filteringService = filteringService;
		m_fileDialogService = fileDialogService;
		m_instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { m_civTechService.GetActivePantryPaths() });
		Filter = new List<InstanceType> { InstanceType.IT_ASSET };
	}

	public AssetBrowserNameEditor(ICivTechService civTechSvc, IAssetBrowserDialogService fileDialogService, IEnumerable<InstanceType> filter)
	{
		m_civTechService = civTechSvc;
		m_fileDialogService = fileDialogService;
		m_instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { m_civTechService.GetActivePantryPaths() });
		Filter = filter;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		string pathName = "";
		if (value is string)
		{
			pathName = (string)value;
		}
		GetContextInformation(context);
		m_fileDialogService.ForcedInitialDirectory = "";
		if (m_fileDialogService.OpenFileName(ref pathName, FilteringContext) == DialogResult.OK)
		{
			InstanceType typeFromExtension = GetTypeFromExtension(pathName);
			string entityNameFromFilePath = StaticMethods.GetEntityNameFromFilePath(m_civTechService.ProjectMapService, pathName, typeFromExtension);
			if (IsValidEntity(typeFromExtension, entityNameFromFilePath))
			{
				value = entityNameFromFilePath;
			}
			else
			{
				ReportFailure();
			}
		}
		return value;
	}

	private void GetContextInformation(ITypeDescriptorContext context)
	{
		IAssetBrowserTypeProvider typeProvider = GetTypeProvider(context.Instance);
		ValidClassNames = typeProvider?.ValidClassNames ?? Enumerable.Empty<string>();
		FilteringContext = typeProvider?.EntityFilteringContext ?? m_filteringService.GetFilteringContext(Filter, null);
	}

	private bool IsValidEntity(InstanceType entType, string name)
	{
		bool result = true;
		if (ValidClassNames.Any())
		{
			IInstanceEntity instanceEntity = m_instanceSet.LoadEntityByName(name, entType);
			result = ValidClassNames.Contains(instanceEntity.ClassName);
			m_instanceSet.Clear();
		}
		return result;
	}

	private void ReportFailure()
	{
		MessageBox.Show(string.Format("The entity selected had an invalid class.  Allowed classes are:\n{0}", string.Join("\n\t", ValidClassNames)), "Invalid selection");
		BugSubmitter.SilentReport("EntityCacheService had an invalid class name when a user selected an entity.  @assign dgurley @summary NameBrowser_CacheError");
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	private string GetNameFromPath(string absolutePath)
	{
		InstanceType typeFromExtension = GetTypeFromExtension(absolutePath);
		string oldValue = StaticMethods.PantryRootForInstanceType(m_civTechService.PrimaryProject.Paths.GamePantry, typeFromExtension).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.AltDirectorySeparatorChar;
		return Path.ChangeExtension(absolutePath, null).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(oldValue, "");
	}

	private string GetProjectFromPath(string filePath)
	{
		try
		{
			return m_civTechService.GetProjectName(new Uri(filePath));
		}
		catch (System.Exception)
		{
		}
		return m_civTechService.PrimaryProject.Name;
	}

	private InstanceType GetTypeFromExtension(string absolutePath)
	{
		return EnumToStringConverter.GetTypeFromExtension(Path.GetExtension(absolutePath));
	}

	private IAssetBrowserTypeProvider GetTypeProvider(object thing)
	{
		IAssetBrowserTypeProvider assetBrowserTypeProvider = thing as IAssetBrowserTypeProvider;
		if (assetBrowserTypeProvider == null && thing is DomNode adaptable)
		{
			assetBrowserTypeProvider = adaptable.As<IAssetBrowserTypeProvider>();
		}
		return assetBrowserTypeProvider;
	}
}

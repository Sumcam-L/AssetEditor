using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Firaxis.Collections;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.CivTech.AssetObjects;

public static class StaticMethods
{
	private static IDictionary<string, InstanceType> _extensionTypeLookup;

	private static readonly object _extensionTypeLookupLock = new object();

	private static IDictionary<string, InstanceType> ExtensionTypeLookup
	{
		get
		{
			if (_extensionTypeLookup == null)
			{
				lock (_extensionTypeLookupLock)
				{
					if (_extensionTypeLookup != null)
					{
						return _extensionTypeLookup;
					}
					_extensionTypeLookup = new Dictionary<string, InstanceType>();
					foreach (InstanceType value in Enum.GetValues(typeof(InstanceType)))
					{
						if (value != InstanceType.IT_COUNT && value != InstanceType.IT_INVALID)
						{
							string text = ExtensionForInstanceType(value);
							if (!string.IsNullOrEmpty(text))
							{
								_extensionTypeLookup[text] = value;
							}
						}
					}
					return _extensionTypeLookup;
				}
			}
			return _extensionTypeLookup;
		}
	}

	[DllImport("Firaxis.CivTech.Impl.dll")]
	[return: MarshalAs(UnmanagedType.LPWStr)]
	public static extern string ExtensionForInstanceType([MarshalAs(UnmanagedType.U4)] InstanceType eType);

	[DllImport("Firaxis.CivTech.Impl.dll")]
	[return: MarshalAs(UnmanagedType.LPWStr)]
	public static extern string PantryRootForInstanceType([MarshalAs(UnmanagedType.LPWStr)] string PantryRoot, [MarshalAs(UnmanagedType.U4)] InstanceType eType);

	public static string SanitizeEntityName(string entityName)
	{
		string newEntityName = entityName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		for (int num = 10; num > 1; num--)
		{
			newEntityName = newEntityName.Replace(new string(Path.AltDirectorySeparatorChar, num), new string(Path.AltDirectorySeparatorChar, 1));
		}
		Path.GetInvalidPathChars().ForEach(delegate(char ch)
		{
			newEntityName = newEntityName.Replace(ch, '-');
		});
		char[] list = new char[3] { '@', '#', '%' };
		list.ForEach(delegate(char ch)
		{
			newEntityName = newEntityName.Replace(ch.ToString(), "");
		});
		return newEntityName;
	}

	public static string GetEntityNameFromFilePath(IProjectMapService projMapSvc, string EntityPath, InstanceType eType)
	{
		string projectNameFromUri = projMapSvc.GetProjectNameFromUri(new Uri(EntityPath));
		string gamePantry = projMapSvc.AllProjectsMap[projectNameFromUri].Paths.GamePantry;
		string text = ExtensionForInstanceType(eType).ToLower();
		string text2 = PantryRootForInstanceType(gamePantry, eType).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLower();
		if (text2.Substring(text2.Length - 1) != "\\")
		{
			text2 += "\\";
		}
		string text3 = EntityPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		string text4 = text3.ToLower();
		text4 = text4.Replace(text2, "");
		text3 = text3.Substring(text3.Length - text4.Length, text4.Length);
		if (text3.Substring(text3.Length - text.Length).ToLower() == text)
		{
			text3 = text3.Substring(0, text3.Length - text.Length);
		}
		return text3;
	}

	public static string RemapPathToProject(IProjectMapService projMapSvc, string originalPath, string projectName)
	{
		string gamePantry = projMapSvc.AllProjectsMap[projectName].Paths.GamePantry;
		string extension = Path.GetExtension(originalPath);
		if (extension.Equals(".artdef", StringComparison.CurrentCultureIgnoreCase))
		{
			string path = Path.Combine(gamePantry, "Artdefs");
			return Path.Combine(path, Path.GetFileName(originalPath));
		}
		if (extension.Equals(".xlp", StringComparison.CurrentCultureIgnoreCase))
		{
			string path2 = Path.Combine(gamePantry, "XLPs");
			return Path.Combine(path2, Path.GetFileName(originalPath));
		}
		InstanceType type = InstanceType.IT_INVALID;
		GetInstanceType(originalPath, out type);
		string entityNameFromFilePath = GetEntityNameFromFilePath(projMapSvc, originalPath, type);
		string path3 = PantryRootForInstanceType(gamePantry, type);
		string text = ExtensionForInstanceType(type);
		return Path.Combine(path3, entityNameFromFilePath + text);
	}

	public static bool IsImportableType(InstanceType type)
	{
		return type == InstanceType.IT_ANIMATION || type == InstanceType.IT_TEXTURE || type == InstanceType.IT_GEOMETRY || type == InstanceType.IT_ENVIRONMENT_LIGHT || type == InstanceType.IT_ANALYTIC_LIGHT || type == InstanceType.IT_PARTICLE_EFFECT;
	}

	public static bool IsBehaviorProviderType(InstanceType type)
	{
		return type == InstanceType.IT_ASSET || type == InstanceType.IT_BEHAVIOR;
	}

	public static bool GetInstanceNameAndType(IProjectMapService projMapSvc, string entityPath, out string instanceName, out InstanceType type)
	{
		instanceName = string.Empty;
		type = InstanceType.IT_COUNT;
		string extension = Path.GetExtension(entityPath);
		if (!ExtensionTypeLookup.TryGetValue(extension, out type))
		{
			return false;
		}
		instanceName = GetEntityNameFromFilePath(projMapSvc, entityPath, type);
		return !string.IsNullOrEmpty(instanceName);
	}

	public static EntityID GetEntityIDFromPath(IProjectMapService projMapSvc, string entityPath)
	{
		string instanceName = string.Empty;
		InstanceType type = InstanceType.IT_COUNT;
		GetInstanceNameAndType(projMapSvc, entityPath, out instanceName, out type);
		return new EntityID(instanceName, type);
	}

	public static ISet<EntityID> GetUniqueEntityIDs(IProjectMapService projMapSvc, IEnumerable<Uri> entityURIs)
	{
		ISet<EntityID> set = new HashSet<EntityID>();
		foreach (Uri entityURI in entityURIs)
		{
			string localPath = entityURI.LocalPath;
			EntityID entityIDFromPath = GetEntityIDFromPath(projMapSvc, localPath);
			if (entityIDFromPath.Type != InstanceType.IT_COUNT && entityIDFromPath.Type != InstanceType.IT_INVALID)
			{
				set.Add(entityIDFromPath);
			}
		}
		return set;
	}

	public static bool GetInstanceType(string entityPath, out InstanceType type)
	{
		type = InstanceType.IT_COUNT;
		string extension = Path.GetExtension(entityPath);
		return ExtensionTypeLookup.TryGetValue(extension, out type);
	}

	public static void RepopulateInstanceCookParameters(IInstanceEntity instanceEntity, IClassEntity classEntity)
	{
		if (instanceEntity == null)
		{
			throw new ArgumentNullException("Instance Entity cannot be null.");
		}
		if (classEntity == null)
		{
			throw new ArgumentNullException("Class Entity cannot be null.");
		}
		if (instanceEntity.ClassName != classEntity.Name)
		{
			throw new ArgumentException($"The instance entity class name and class's name must match.  Current params: Entity.ClassName: {instanceEntity.ClassName}; class.Name: {classEntity.Name}");
		}
		instanceEntity.CookParameters.RemoveAllValues();
		instanceEntity.CookParameters.AddDefaultValuesAsNecessary(classEntity.CookParameters);
	}

	public static IEnumerable<string> GetEntitySubdirectories(IInstanceEntity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("Entity cannot be null.");
		}
		return GetEntitySubdirectories(entity.Name);
	}

	public static IEnumerable<string> GetEntitySubdirectories(string entityName)
	{
		if (entityName == null)
		{
			throw new ArgumentNullException("Entity Name cannot be null.");
		}
		List<string> list = new List<string>();
		if (entityName.Contains(Path.DirectorySeparatorChar) || entityName.Contains(Path.AltDirectorySeparatorChar))
		{
			string text = entityName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string[] array = text.Split(Path.DirectorySeparatorChar);
			int num = array.Length - 1;
			for (int i = 0; i < num; i++)
			{
				list.Add(array[i]);
			}
		}
		return list;
	}

	public static bool IsEntityNameValid(IInstanceEntity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("Entity cannot be null.");
		}
		return IsEntityNameValid(entity.Name);
	}

	public static bool IsEntityNameValid(string entityName)
	{
		if (entityName == null)
		{
			throw new ArgumentNullException("Entity cannot be null.");
		}
		IEnumerable<string> entitySubdirectories = GetEntitySubdirectories(entityName);
		bool flag = false;
		bool flag2 = false;
		foreach (string item in entitySubdirectories)
		{
			if (!PathHelper.IsValidPath(item))
			{
				flag = true;
			}
		}
		flag2 = !PathHelper.IsValidPath(entityName);
		return !(flag || flag2);
	}

	public static IEnumerable<T> ItemsByType<T>(this IParameterSet parameters) where T : class, IParameter
	{
		return from param in parameters.Items
			where param is T
			select param as T;
	}

	public static IEnumerable<InstanceType> GetObjectParameterTypes(this IParameterSet parameters)
	{
		IEnumerable<IObjectParameter> enumerable = parameters.ItemsByType<IObjectParameter>();
		List<InstanceType> list = new List<InstanceType>();
		foreach (IObjectParameter item in enumerable)
		{
			if (!list.Contains(item.ObjectType))
			{
				list.Add(item.ObjectType);
			}
		}
		return list;
	}

	public static IEnumerable<T> ItemsOfType<T>(this IValueSet valueSet) where T : class, IValue
	{
		List<T> list = new List<T>();
		if (valueSet == null || valueSet.Items == null)
		{
			return list;
		}
		IEnumerable<ICollectionValue> source = valueSet.Items.OfType<ICollectionValue>();
		IEnumerable<IEnumerable<IValue>> source2 = from col in source
			where col != null
			select col.Items;
		list.AddRange(valueSet.Items.OfType<T>().Union(source2.OfType<T>()));
		return list;
	}

	public static InstanceType InstanceTypeFromEntityType<T>()
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(ITextureInstance))
		{
			return InstanceType.IT_TEXTURE;
		}
		if (typeFromHandle == typeof(IGeometryInstanceBuildable) || typeFromHandle == typeof(IGeometryInstance))
		{
			return InstanceType.IT_GEOMETRY;
		}
		if (typeFromHandle == typeof(IAnimationInstance))
		{
			return InstanceType.IT_ANIMATION;
		}
		if (typeFromHandle == typeof(IMaterialInstance))
		{
			return InstanceType.IT_MATERIAL;
		}
		if (typeFromHandle == typeof(IDSGInstance))
		{
			return InstanceType.IT_DSG;
		}
		if (typeFromHandle == typeof(IEnvironmentLightInstance))
		{
			return InstanceType.IT_ENVIRONMENT_LIGHT;
		}
		if (typeFromHandle == typeof(IAnalyticLightInstance))
		{
			return InstanceType.IT_ANALYTIC_LIGHT;
		}
		if (typeFromHandle == typeof(ILightRigInstance))
		{
			return InstanceType.IT_LIGHT_RIG;
		}
		if (typeFromHandle == typeof(IParticleEffectInstance))
		{
			return InstanceType.IT_PARTICLE_EFFECT;
		}
		if (typeFromHandle == typeof(IAssetInstance))
		{
			return InstanceType.IT_ASSET;
		}
		if (typeFromHandle == typeof(IBehaviorInstance))
		{
			return InstanceType.IT_BEHAVIOR;
		}
		if (typeFromHandle == typeof(IFireFXInstance))
		{
			return InstanceType.IT_FIREFX;
		}
		return InstanceType.IT_INVALID;
	}

	public static IEnumerable<IModelInstance> ReconcileAssetGeometrySet(IAssetInstance asset, IGeometryInstance changedGeometry, CivTechContext civTechContext, ICivTechService civTechSvc)
	{
		List<IModelInstance> list = new List<IModelInstance>();
		using (IInstanceSet instanceSet = civTechContext.CreateInstance<IInstanceSet>(new object[1] { civTechSvc.GetActivePantryPaths() }))
		{
			IEnumerable<IModelInstance> enumerable = asset.GeometrySet.ModelInstances.Where((IModelInstance model) => model.GeoName == changedGeometry.Name).ToArray();
			foreach (IModelInstance item in enumerable)
			{
				IEnumerable<IPrimGroupStateInformation> primGroupStateInformation = GetPrimGroupStateInformation(item, civTechContext);
				IEnumerable<IAttachmentPoint> affectedAttachmentPoint = GetAffectedAttachmentPoint(item.Name, asset.AttachmentPointSet.Items);
				asset.RemoveModelInstance(item.Name);
				IModelInstance modelInstance = asset.AddModelToAsset(changedGeometry, civTechSvc.PrimaryProject.Config.Classes);
				if (modelInstance != null)
				{
					modelInstance.AssignMaterialsToModel(civTechSvc.ProjectMapService.LayeredPantry, changedGeometry, civTechSvc.PrimaryProject.Config.Classes, instanceSet, null, civTechSvc.PrimaryProject.Paths.GamePantry);
					modelInstance.RestorePrimGroupState(primGroupStateInformation);
					RemoveNonExistentAttachmentPoints(affectedAttachmentPoint, asset.AttachmentPointSet, changedGeometry, modelInstance.Name);
					list.Add(modelInstance);
				}
			}
		}
		return list;
	}

	public static IEnumerable<IPrimGroupStateInformation> GetPrimGroupStateInformation(IModelInstance model, CivTechContext civTechContext)
	{
		if (model == null)
		{
			return Enumerable.Empty<IPrimGroupStateInformation>();
		}
		IList<IPrimGroupStateInformation> list = new List<IPrimGroupStateInformation>(model.PrimGroups.Count());
		foreach (IPrimGroupState primGroup in model.PrimGroups)
		{
			IPrimGroupStateInformation primGroupStateInformation = civTechContext.CreateInstance<IPrimGroupStateInformation>();
			primGroupStateInformation.AssignFromPrimGroupState(primGroup);
			list.Add(primGroupStateInformation);
		}
		return list;
	}

	private static IEnumerable<IAttachmentPoint> GetAffectedAttachmentPoint(string modelName, IEnumerable<IAttachmentPoint> attachmentPointSetData)
	{
		ICollection<IAttachmentPoint> collection = new List<IAttachmentPoint>();
		foreach (IAttachmentPoint item in attachmentPointSetData.Where((IAttachmentPoint atPt) => atPt.ModelInstanceName == modelName))
		{
			collection.Add(item);
		}
		return collection;
	}

	private static void RemoveAffectedAttachmentPoints(IEnumerable<IAttachmentPoint> attachmentPoints, IAttachmentPointSet attachmentPointSet)
	{
		foreach (IAttachmentPoint attachmentPoint in attachmentPoints)
		{
			attachmentPointSet.RemoveAttachmentPoint(attachmentPoint.Name);
		}
	}

	private static void RemoveNonExistentAttachmentPoints(IEnumerable<IAttachmentPoint> attachmentPoints, IAttachmentPointSet attachmentPointSet, IGeometryInstance geometry, string modelName)
	{
		foreach (IAttachmentPoint attachmentPoint in attachmentPoints)
		{
			if (!geometry.HasBone(attachmentPoint.BoneName))
			{
				attachmentPointSet.RemoveAttachmentPoint(attachmentPoint.Name);
			}
		}
	}

	public static string AssignDefaultMaterial(this IObjectValue value, IVirtualPantry virtPan, IObjectParameter objParam, string triangleGroupName, string stateName, IEnumerable<string> materialNames, IInstanceSet instances, string gamePantry)
	{
		if (objParam.ObjectType != InstanceType.IT_MATERIAL)
		{
			return null;
		}
		PlatformAssert.If(value.ParameterName != objParam.Name, "Value parameter and parameter passed in do not match.");
		string text = value.GetBoundObjectName();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (objParam.Name.Equals("BurnMaterial"))
		{
			text = ((!stateName.Equals("Ruined") && !stateName.Equals("Pillaged")) ? string.Empty : "DefaultBurnMaterial");
		}
		else
		{
			bool flag = materialNames?.Any() ?? false;
			IObjectValue objectValue = objParam.DefaultValue as IObjectValue;
			IMaterialInstance materialInstance = null;
			string text2 = (string.IsNullOrEmpty(stateName) ? triangleGroupName : $"{triangleGroupName}_{stateName}");
			if (!string.IsNullOrEmpty(objectValue.GetBoundObjectName()))
			{
				text = objectValue.GetBoundObjectName();
			}
			else if (flag)
			{
				if (materialNames.Contains(text2))
				{
					text = text2;
				}
				else if (materialNames.Contains(triangleGroupName))
				{
					text = triangleGroupName;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(text2);
				if (materialInstance == null)
				{
					materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(triangleGroupName);
				}
				if (materialInstance != null)
				{
					if (objParam.AllowedClasses.Contains(materialInstance.ClassName))
					{
						text = materialInstance.Name;
					}
				}
				else if (flag)
				{
					foreach (string materialName in materialNames)
					{
						materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(materialName);
						if (materialInstance != null && objParam.AllowedClasses.Contains(materialInstance.ClassName))
						{
							text = materialInstance.Name;
							break;
						}
					}
				}
			}
			if (materialInstance == null && !string.IsNullOrEmpty(text))
			{
				materialInstance = instances.LoadEntityIfUnique<IMaterialInstance>(text);
				if (materialInstance == null || !objParam.AllowedClasses.Contains(materialInstance.ClassName))
				{
					text = string.Empty;
				}
			}
		}
		value.BindObject(text, objParam.ObjectType);
		return text;
	}

	public static string GetPlatformDirectory(Platforms platform)
	{
		string result = string.Empty;
		switch (platform)
		{
		case Platforms.PLATFORM_WINDOWS:
			result = "Windows";
			break;
		case Platforms.PLATFORM_MACOS:
			result = "MacOS";
			break;
		case Platforms.PLATFORM_IOS:
			result = "iOS";
			break;
		case Platforms.PLATFORM_LINUX:
			result = "Linux";
			break;
		}
		return result;
	}

	public static bool IsEntityReadOnly(ICivTechService civTechSvc, IImportedEntity entity)
	{
		bool flag = false;
		string xMLPath = entity.GetXMLPath();
		if (File.Exists(xMLPath))
		{
			FileInfo fileInfo = new FileInfo(xMLPath);
			if (fileInfo.IsReadOnly)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return flag;
		}
		IClassEntity classEntity = civTechSvc.PrimaryProject.Config.Classes.FindForInstance(entity);
		if (classEntity == null)
		{
			return false;
		}
		foreach (IInstanceDataFile df in entity.DataFiles)
		{
			IClassDataFile classDataFile = classEntity.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == df.ID);
			if (classDataFile == null || classDataFile.IsGenerated)
			{
				continue;
			}
			string dataFilePath = entity.GetDataFilePath(df.RelativePath);
			if (File.Exists(dataFilePath))
			{
				FileInfo fileInfo2 = new FileInfo(dataFilePath);
				if (fileInfo2.IsReadOnly)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public static IEnumerable<EntityID> ShallowCrawlCooktimeDependencies(this IInstanceEntity entity)
	{
		ICollection<EntityID> dependencies = new List<EntityID>();
		entity?.CrawlCooktimeDependencies(delegate(InstanceType type, string name)
		{
			dependencies.Add(new EntityID(name, type));
		});
		return dependencies;
	}

	public static void CreateWorkspaceDirectories(IEnumerable<ProjectEnvironment> projects)
	{
		foreach (ProjectEnvironment project in projects)
		{
			string workspaceRoot = project.VersionControl.WorkspaceRoot;
			CreateDirectory(workspaceRoot);
			string gamePantry = project.Paths.GamePantry;
			CreateDirectory(gamePantry);
			foreach (InstanceType value in Enum.GetValues(typeof(InstanceType)))
			{
				if (value != InstanceType.IT_COUNT && value != InstanceType.IT_INVALID)
				{
					string directory = PantryRootForInstanceType(gamePantry, value);
					CreateDirectory(directory);
				}
			}
			CreateDirectory(project.Paths.ArtDefRoot);
			CreateDirectory(project.Paths.XLPRoot);
			CreateDirectory(project.Paths.ArtDefOutputRoot);
			foreach (string outputPlatform in GetOutputPlatforms())
			{
				string directory2 = project.Paths.XLPOutputRoot.Replace("{PLATFORM}", outputPlatform);
				CreateDirectory(directory2);
			}
		}
	}

	private static IEnumerable<string> GetPantrySubdirectories()
	{
		List<string> list = new List<string>();
		list.Add("Animations");
		list.Add("Assets");
		list.Add("Behaviors");
		list.Add("DSGs");
		list.Add("EnvironmentLights");
		list.Add("Geometries");
		list.Add("LightRigs");
		list.Add("Lights");
		list.Add("Materials");
		list.Add("ParticleEffects");
		list.Add("Textures");
		return list;
	}

	private static IEnumerable<string> GetOutputPlatforms()
	{
		List<string> list = new List<string>();
		list.Add("iOS");
		list.Add("Linux");
		list.Add("MacOS");
		list.Add("Windows");
		return list;
	}

	private static void CreateDirectory(string directory)
	{
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
	}

	public static IEnumerable<InstanceType> GetValidInstanceTypes()
	{
		foreach (InstanceType type in Enum.GetValues(typeof(InstanceType)))
		{
			if (type != InstanceType.IT_INVALID && type != InstanceType.IT_COUNT)
			{
				yield return type;
			}
		}
	}

	public static IEnumerable<Uri> GetEntityURIs(ICivTechService civTechService, IEnumerable<EntityID> entityIDs)
	{
		foreach (EntityID entity in entityIDs)
		{
			if ((bool)civTechService.GetEntityPath(entity.Name, entity.Type, out var entityPath) && Uri.TryCreate(entityPath, UriKind.Absolute, out var entityUri) && entityUri != null)
			{
				yield return entityUri;
			}
			entityUri = null;
			entityPath = null;
		}
	}
}

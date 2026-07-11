using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.Packages;

namespace Firaxis.CivTech.AssetObjects;

public static class IAssetInstanceExtensions
{
	public static void CrawlSplineDependencies(this IAssetInstance asset, IProjectConfig projCfg, Action<InstanceType, string> entityCrawler)
	{
		IEnumerable<ISpline> enumerable = asset?.SplineSet?.Splines ?? Enumerable.Empty<ISpline>();
		BugSubmitter.SilentAssert(entityCrawler != null, "Invalid entity crawler functor! @assign bwhitman");
		foreach (ISpline item in enumerable)
		{
			foreach (IBLPEntryValue cookParam in item.CookParameters.Items.OfType<IBLPEntryValue>())
			{
				if (!string.IsNullOrEmpty(cookParam.EntryName))
				{
					IXLPClass iXLPClass = projCfg.XLPClasses.Items.FirstOrDefault((IXLPClass xlpCls) => xlpCls.Name == cookParam.XLPClass);
					if (iXLPClass != null)
					{
						entityCrawler?.Invoke(iXLPClass.InstanceType, cookParam.EntryName);
					}
				}
			}
		}
	}

	public static IModelInstance AddModelToAsset(this IAssetInstance asset, IGeometryInstance geo, IClassSet classes)
	{
		string text = (string.IsNullOrEmpty(geo.ModelName) ? geo.Name : geo.ModelName);
		foreach (IModelInstance modelInstance2 in asset.GeometrySet.ModelInstances)
		{
			if (string.Compare(modelInstance2.Name, text) == 0)
			{
				return null;
			}
		}
		IModelInstance modelInstance = asset.AddModelInstance(text, geo);
		if (modelInstance != null)
		{
			IAssetClass assetClass = classes.FindForInstance(asset) as IAssetClass;
			foreach (IGeoMesh geometryMesh in geo.GeometryMeshes)
			{
				foreach (IGeoPrimGroup geoPrimGroup in geometryMesh.GeoPrimGroups)
				{
					foreach (IAssetClassState state in assetClass.States)
					{
						modelInstance.AddPrimGroupState(geometryMesh.Name, geoPrimGroup.Name, state.Name);
					}
				}
			}
		}
		return modelInstance;
	}

	public static void AssignMaterialsToModel(this IModelInstance modelInstance, IVirtualPantry virtPan, IGeometryInstance geo, IClassSet classes, IInstanceSet instanceSet, IEnumerable<string> materialNames, string gamePantry)
	{
		IGeometryClass geometryClass = classes.FindForInstance(geo) as IGeometryClass;
		HashSet<string> hashSet = null;
		hashSet = ((materialNames == null) ? new HashSet<string>() : new HashSet<string>(materialNames));
		foreach (IPrimGroupState primGroup in modelInstance.PrimGroups)
		{
			IParameterSet groupParameters = geometryClass.GroupParameters;
			primGroup.Values.AddDefaultValuesAsNecessary(groupParameters);
			IEnumerable<IObjectValue> enumerable = from val in primGroup.Values.Items.OfType<IObjectValue>()
				where val.GetBoundObjectType() == InstanceType.IT_MATERIAL
				select val;
			foreach (IObjectValue item in enumerable)
			{
				IObjectParameter objParam = groupParameters.FindByName(item.ParameterName) as IObjectParameter;
				string text = item.AssignDefaultMaterial(virtPan, objParam, primGroup.GroupName, primGroup.StateName, hashSet, instanceSet, gamePantry);
				if (!string.IsNullOrEmpty(text))
				{
					hashSet.Add(text);
				}
			}
		}
	}

	public static IEnumerable<IPrimGroupStateInformation> GetPrimGroupInformation(this IModelInstance model, Func<IPrimGroupStateInformation> factoryFunc)
	{
		IList<IPrimGroupStateInformation> list = new List<IPrimGroupStateInformation>();
		foreach (IPrimGroupState primGroup in model.PrimGroups)
		{
			IPrimGroupStateInformation primGroupStateInformation = factoryFunc();
			primGroupStateInformation.AssignFromPrimGroupState(primGroup);
			list.Add(primGroupStateInformation);
		}
		return list;
	}

	public static void RestorePrimGroupState(this IModelInstance model, IEnumerable<IPrimGroupStateInformation> stateInformation)
	{
		foreach (IPrimGroupStateInformation item in stateInformation)
		{
			model.FindPrimGroupState(item.MeshName, item.GroupName, item.StateName)?.Values.CopyFrom(item.Values);
		}
	}
}

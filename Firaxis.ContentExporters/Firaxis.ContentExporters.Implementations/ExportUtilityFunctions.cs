using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Texture;
using Firaxis.CivTech.TextureExport;
using Firaxis.Error;
using Firaxis.Granny;
using Firaxis.Utility;
using Firaxis.Wig;

namespace Firaxis.ContentExporters.Implementations;

public static class ExportUtilityFunctions
{
	static ExportUtilityFunctions()
	{
		Context.EnsureCreated<GrannyContext>();
		Context.EnsureCreated<CivTechContext>();
	}

	public static ResultCode Validate(IImportedEntity entity, IClassEntity entityClass, string localPantry)
	{
		if (entity == null)
		{
			return new ResultCode("Entity passed in for validation is null.");
		}
		if (entityClass == null || string.IsNullOrEmpty(entity.Name))
		{
			return new ResultCode("Entity ({0}) passed in for validation has a null class.", entity.Name);
		}
		if (!entity.DataFiles.Any())
		{
			return new ResultCode("Entity ({0}) does not have any data files.", entity.Name);
		}
		foreach (IInstanceDataFile dataFile in entity.DataFiles)
		{
			string dataFilePath = entity.GetDataFilePath(dataFile.RelativePath);
			if (!File.Exists(dataFilePath))
			{
				return new ResultCode("Entity ({0}) data file ({1}) does not exist on disk.", entity.Name, dataFilePath);
			}
		}
		return entity.Type switch
		{
			InstanceType.IT_TEXTURE => Validate(entity as ITextureInstance, entityClass as ITextureClass), 
			InstanceType.IT_ANIMATION => Validate(entity as IAnimationInstance, entityClass as IAnimationClass), 
			InstanceType.IT_GEOMETRY => Validate(entity as IGeometryInstance, entityClass as IGeometryClass, localPantry), 
			InstanceType.IT_PARTICLE_EFFECT => Validate(entity as IParticleEffectInstance, entityClass as IParticleEffectClass), 
			InstanceType.IT_ENVIRONMENT_LIGHT => Validate(entity as IEnvironmentLightInstance, entityClass as IEnvironmentLightClass, localPantry), 
			InstanceType.IT_ANALYTIC_LIGHT => new ResultCode("Entity {0} has an InstanceType of {1}, which is invalid for validation.", entity.Name, entity.Type.ToString()), 
			_ => new ResultCode("Entity {0} has an InstanceType of {1}, which is invalid for validation.", entity.Name, entity.Type.ToString()), 
		};
	}

	public static ResultCode ValidateClass(IImportedEntity entity, IClassEntity classEntity)
	{
		if (string.IsNullOrEmpty(entity.ClassName))
		{
			return new ResultCode("The class name is not set.");
		}
		if (classEntity == null)
		{
			return new ResultCode("The class does not exist in the current project.");
		}
		if (entity.ClassName != classEntity.Name)
		{
			return new ResultCode("The entity is expecting class {0}, while it is being tested against class {1}.", entity.ClassName, classEntity.Name);
		}
		ITextureInstance textureInstance = entity as ITextureInstance;
		ITextureClass textureClass = classEntity as ITextureClass;
		if (textureInstance != null && textureClass != null)
		{
			textureInstance.ExportSettings.AssignFromTextureClass(textureClass);
		}
		return ResultCode.Success;
	}

	private static ResultCode Validate(IEnvironmentLightInstance environmentLight, IEnvironmentLightClass environmentLightClass, string localPantry)
	{
		IEnvironmentMapImportOptions importOptions = environmentLightClass.ImportOptions;
		IDDSDataExtractor iDDSDataExtractor = Context.Get<CivTechContext>().CreateInstance<IDDSDataExtractor>();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IInstanceDataFile dataFile in environmentLight.DataFiles)
		{
			string dataFilePath = environmentLight.GetDataFilePath(dataFile.RelativePath);
			iDDSDataExtractor.LoadDDSFile(dataFilePath);
			if (!importOptions.AllowArtistMips && iDDSDataExtractor.GetTextureMipMapCount() != 0)
			{
				stringBuilder.AppendFormat("Environment light class ({0}) does not allow mips, but the environment light has ({1}) mip maps.", environmentLightClass.Name, iDDSDataExtractor.GetTextureMipMapCount());
				stringBuilder.AppendLine();
			}
			if (!ValueInRange(iDDSDataExtractor.GetTextureWidth(), importOptions.MinWidth, importOptions.MaxWidth))
			{
				stringBuilder.AppendFormat("Environment light class ({0}) allows widths between {1} and {2}.  Environment light width: {3}.", environmentLightClass.Name, importOptions.MinWidth, importOptions.MaxWidth, iDDSDataExtractor.GetTextureWidth());
				stringBuilder.AppendLine();
			}
			if (importOptions.RequirePow2)
			{
				if (!IsPow2(iDDSDataExtractor.GetTextureHeight()))
				{
					stringBuilder.AppendFormat("This environment light's height must be a power of 2.  Height is currently: {0}", iDDSDataExtractor.GetTextureHeight());
					stringBuilder.AppendLine();
				}
				if (!IsPow2(iDDSDataExtractor.GetTextureWidth()))
				{
					stringBuilder.AppendFormat("This environment light's width must be a power of 2.  Width is currently: {0}", iDDSDataExtractor.GetTextureWidth());
					stringBuilder.AppendLine();
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			return new ResultCode(stringBuilder.ToString());
		}
		return ResultCode.Success;
	}

	private static ResultCode Validate(ITextureInstance texture, ITextureClass textureClass)
	{
		if (textureClass == null)
		{
			return new ResultCode("Texture has been assigned an invalid texture class.");
		}
		ITextureExportOptions exportOptions = textureClass.ExportOptions;
		if (!ValueInRange(texture.Height, exportOptions.MinHeight, exportOptions.MaxHeight))
		{
			return new ResultCode("The texture's height of {0} falls outside the acceptable range of {1} to {2}", texture.Height, exportOptions.MinHeight, exportOptions.MaxHeight);
		}
		if (!ValueInRange(texture.Width, exportOptions.MinWidth, exportOptions.MaxWidth))
		{
			return new ResultCode("The texture's width of {0} falls outside the acceptable range of {1} to {2}", texture.Width, exportOptions.MinWidth, exportOptions.MaxWidth);
		}
		if ((exportOptions.ExportTextureType == TextureType.TEX_3D || exportOptions.ExportTextureType == TextureType.TEX_3D_COLORKEY) && !ValueInRange(texture.Depth, exportOptions.MinDepth, exportOptions.MaxDepth))
		{
			return new ResultCode("The texture's depth of {0} falls outside the acceptable range of {1} to {2}", texture.Depth, exportOptions.MinDepth, exportOptions.MaxDepth);
		}
		if (exportOptions.RequireSquare && texture.Width != texture.Height)
		{
			return new ResultCode("This texture's width and height must be equal.  Width is {0} and height is {1}", texture.Width, texture.Height);
		}
		if (exportOptions.RequirePow2)
		{
			if (!IsPow2(texture.Height))
			{
				return new ResultCode("This texture's height must be a power of 2.  Height is currently: {0}", texture.Height);
			}
			if (!IsPow2(texture.Width))
			{
				return new ResultCode("This texture's width must be a power of 2.  Width is currently: {0}", texture.Width);
			}
		}
		if (!exportOptions.AllowArtistMips && texture.ExportSettings.UseMips)
		{
			return new ResultCode("This texture does not allow mip maps to be generated for it.");
		}
		if (exportOptions.ExportPixelFormat != texture.ExportSettings.PixelFormat)
		{
			return new ResultCode("This texture has the wrong pixel format.  Expected format: {0}, Current format: {1}", exportOptions.ExportPixelFormat.ToString(), texture.ExportSettings.PixelFormat.ToString());
		}
		switch (exportOptions.ExportTextureType)
		{
		case TextureType.TEX_2D:
			if (texture.ExportSettings.ExportMode != ExportMode.Texture2D && texture.ExportSettings.ExportMode != ExportMode.ManualMipMaps)
			{
				return new ResultCode("This texture has the wrong type.  Adjust the export mode.  Expected type: {0}; Current type: {1} ", TextureType.TEX_2D.ToString(), texture.ExportSettings.ExportMode.ToString());
			}
			break;
		case TextureType.TEX_3D:
			if (texture.ExportSettings.ExportMode != ExportMode.Texture3D)
			{
				return new ResultCode("This texture has the wrong type.  Adjust the export mode.  Expected type: {0}; Current type: {1} ", TextureType.TEX_3D.ToString(), texture.ExportSettings.ExportMode.ToString());
			}
			break;
		case TextureType.TEX_3D_COLORKEY:
			if (texture.ExportSettings.ExportMode != ExportMode.ColorKey)
			{
				return new ResultCode("This texture has the wrong type.  Adjust the export mode.  Expected type: {0}; Current type: {1} ", TextureType.TEX_3D_COLORKEY.ToString(), texture.ExportSettings.ExportMode.ToString());
			}
			break;
		case TextureType.TEX_CUBE:
			if (texture.ExportSettings.ExportMode != ExportMode.CubeMap)
			{
				return new ResultCode("This texture has the wrong type.  Adjust the export mode.  Expected type: {0}; Current type: {1} ", TextureType.TEX_CUBE.ToString(), texture.ExportSettings.ExportMode.ToString());
			}
			break;
		default:
			return new ResultCode("This texture has an unknown type.  Type: {0}", exportOptions.ExportTextureType.ToString());
		}
		if (texture.ExportSettings.GammaIn != exportOptions.ExportGammaIn)
		{
			return new ResultCode("The texture's GammaIn of {0} is incorrect.  It should be {1}.", texture.ExportSettings.GammaIn, exportOptions.ExportGammaIn);
		}
		if (texture.ExportSettings.GammaOut != exportOptions.ExportGammaOut)
		{
			return new ResultCode("The texture's GammaOut of {0} is incorrect.  It should be {1}.", texture.ExportSettings.GammaOut, exportOptions.ExportGammaOut);
		}
		if (texture.ExportSettings.ValueClampMin != exportOptions.ExportClampMin)
		{
			return new ResultCode("The texture's Value Clamp Min of {0} is incorrect.  It should be {1}.", texture.ExportSettings.ValueClampMin, exportOptions.ExportClampMin);
		}
		if (texture.ExportSettings.ValueClampMax != exportOptions.ExportClampMax)
		{
			return new ResultCode("The texture's Value Clamp max of {0} is incorrect.  It should be {1}.", texture.ExportSettings.ValueClampMax, exportOptions.ExportClampMax);
		}
		return ResultCode.Success;
	}

	private static bool ValueInRange<T>(T value, T minValue, T maxValue) where T : IComparable<T>
	{
		return value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;
	}

	private static bool IsPow2(int test)
	{
		return (test & (test - 1)) == 0;
	}

	private static bool IsPow2(uint test)
	{
		return (test & (test - 1)) == 0;
	}

	private static ResultCode Validate(IGeometryInstance geo, IGeometryClass geoClass, string localPantry)
	{
		if (geoClass.ExportType != DCCExportType.EXPORT_CAMERA && geoClass.ExportType != DCCExportType.EXPORT_LIGHT)
		{
			if (string.IsNullOrEmpty(geo.ModelName))
			{
				return new ResultCode("Geometry ({0}) is invalid because it does not have a model.", geo.Name);
			}
			if (geoClass.ExportType == DCCExportType.EXPORT_DEFAULT)
			{
				foreach (IGeoMesh geometryMesh in geo.GeometryMeshes)
				{
					if (!geometryMesh.GeoPrimGroups.Any())
					{
						return new ResultCode("Geometry ({0}) is invalid because its mesh ({1}) does not contain any geometry.", geo.Name, geometryMesh.Name);
					}
					if (geometryMesh.PrimitiveCount == 0 || geometryMesh.VertexCount == 0)
					{
						return new ResultCode("Geometry ({0}) is invalid because its mesh ({1}) has no primitives or vertices.", geo.Name, geometryMesh.Name);
					}
					if (geoClass.MaxSkinnedBones != 0 && geometryMesh.BoundBoneCount > geoClass.MaxSkinnedBones)
					{
						string msg = $"Geometry ({geo.Name}) is invalid because its Mesh ({geometryMesh.Name}) exceeds the maximum bone count.  Bound Bones: {geometryMesh.BoundBoneCount}; Maximum: {geoClass.MaxSkinnedBones}";
						return new ResultCode(msg);
					}
					ISet<string> set = new HashSet<string>();
					foreach (IGeoPrimGroup geoPrimGroup in geometryMesh.GeoPrimGroups)
					{
						if (!set.Add(geoPrimGroup.Name))
						{
							return new ResultCode("Geometry ({0}) is invalid because its mesh ({1}) has multiple instances of the material/triangle group named ({2}).  These names are unique per mesh.", geo.Name, geometryMesh.Name, geoPrimGroup.Name);
						}
						if (geoPrimGroup.NumPrims == 0)
						{
							return new ResultCode("Geometry ({0}) is invalid because its Prim Group ({1}) has no primitives.", geo.Name, geoPrimGroup.Name);
						}
					}
				}
				if (geoClass.DataFiles.Any((IClassDataFile df) => df.ID == "GR2"))
				{
					return ValidateGeometryAgainstGrannyFile(geo, localPantry);
				}
			}
		}
		return ResultCode.Success;
	}

	private static ResultCode ValidateGeometryAgainstGrannyFile(IGeometryInstance geo, string localPantry)
	{
		IInstanceDataFile instanceDataFile = geo.DataFiles.FirstOrDefault((IInstanceDataFile df) => df.ID == "GR2");
		if (instanceDataFile == null)
		{
			return new ResultCode("Geometry ({0}) does not have any FGX files associated with it.", geo.Name);
		}
		string dataFilePath = geo.GetDataFilePath(instanceDataFile.RelativePath);
		if (!File.Exists(dataFilePath))
		{
			return new ResultCode("Could not parse the granny file ({0}) at path ({1}).  The granny file do not exist on disk.", geo.Name, dataFilePath);
		}
		GrannyContext grannyContext = Context.Get<GrannyContext>();
		IGrannyFile grannyFile = null;
		try
		{
			grannyFile = grannyContext.LoadGrannyFile(dataFilePath);
		}
		catch (Exception ex)
		{
			return new ResultCode("Failed to load the granny file ({0}) for Geometry ({1}).  Reason: {2}.", dataFilePath, geo.Name, ex.Message);
		}
		if (grannyFile == null)
		{
			return new ResultCode("Could not parse the granny file ({0}) for Geometry ({1}).  The granny file failed to load.", dataFilePath, geo.Name);
		}
		if (!grannyFile.Models.Any())
		{
			return new ResultCode("The granny file ({0}) for Geometry ({1}) does not contain any models.  This is an export bug.", dataFilePath, geo.Name);
		}
		if (grannyFile.Models.Count > 1)
		{
			return new ResultCode("The granny file ({0}) for Geometry ({1}) contains more than one model.  This is an export bug.", dataFilePath, geo.Name);
		}
		IGrannyModel grannyModel = grannyFile.Models[0];
		if (grannyModel.Name != geo.ModelName)
		{
			return new ResultCode("The Granny File for the Geometry ({0}) has a different model name than is set in the Geometry.  This is a tool chain bug.\nGeometry's Model Name: {1}; Granny Model Name: {2}", geo.Name, geo.ModelName, grannyModel.Name);
		}
		if (geo.GeometryMeshes.Count() != grannyModel.MeshBindings.Count)
		{
			return new ResultCode("The Geometry and its Granny File define an inconsistent set of Meshes.  This is a tool chain bug.");
		}
		if (grannyModel.MeshBindings.Any((IGrannyMesh grannyMesh2) => string.IsNullOrEmpty(grannyMesh2.Name)))
		{
			return new ResultCode("The Granny File for \"{0}\" contains a mesh that has not been assigned a name.  This is an exporter bug.", geo.Name);
		}
		foreach (IGeoMesh mesh in geo.GeometryMeshes)
		{
			IGrannyMesh grannyMesh = grannyModel.MeshBindings.FirstOrDefault((IGrannyMesh meshBinding) => meshBinding.Name == mesh.Name);
			if (grannyMesh == null)
			{
				return new ResultCode("The Geometry defines the mesh \"{0}\" which does not exist in its Granny File.  This is a tool chain bug.", mesh.Name);
			}
			if (mesh.VertexCount != (uint)grannyMesh.VertexCount)
			{
				return new ResultCode("Invalid vertex count in the Geometry Mesh \"{0}\".  This is a tool chain bug.", mesh.Name);
			}
			if (mesh.BoundBoneCount != (uint)grannyMesh.BoneBindings.Count)
			{
				return new ResultCode("Invalid bound bone in the Geometry Mesh \"{0}\".  This is a tool chain bug.", mesh.Name);
			}
			IList<IGrannyMaterial> materialBindings = grannyMesh.MaterialBindings;
			uint num = 0u;
			foreach (IGrannyTriMaterialGroup triangleMaterialGroup in grannyMesh.TriangleMaterialGroups)
			{
				int materialIndex = triangleMaterialGroup.MaterialIndex;
				if (materialIndex < materialBindings.Count)
				{
					num += (uint)triangleMaterialGroup.TriCount;
					IGrannyMaterial grannyMaterial = materialBindings[materialIndex];
					if (string.IsNullOrEmpty(grannyMaterial.Name))
					{
						return new ResultCode("The mesh \"{0}\" for the model \"{1}\" has not been assigned a name.  This is an exporter bug.", mesh.Name, geo.ModelName);
					}
					continue;
				}
				return new ResultCode("The mesh \"{0}\" for the model \"{1}\" has an out-of-bounds material.  This is an exporter bug.", mesh.Name, geo.ModelName);
			}
			if (mesh.PrimitiveCount != num)
			{
				return new ResultCode("Invalid primitive count in the Geometry Mesh \"{0}\".  This is a tool chain bug.", mesh.Name);
			}
			foreach (IGeoPrimGroup primGroup in mesh.GeoPrimGroups)
			{
				IGrannyTriMaterialGroup grannyTriMaterialGroup = grannyMesh.TriangleMaterialGroups.FirstOrDefault((IGrannyTriMaterialGroup group) => group.TriFirst == primGroup.NumFirstPrim);
				if (grannyTriMaterialGroup == null)
				{
					return new ResultCode("The Geometry Prim Group \"{0}\" does not have a matching material group in the Granny File.  This is a tool chain bug.", primGroup.Name);
				}
				if (grannyTriMaterialGroup.TriCount != (int)primGroup.NumPrims)
				{
					return new ResultCode("Inconsistent primitive count in the Geometry Prim Group \"{0}\".  This is a tool chain bug.", primGroup.Name);
				}
				IGrannyMaterial[] array = grannyMesh.MaterialBindings.Where((IGrannyMaterial mat) => mat.Name == primGroup.Name).ToArray();
				if (array.Length == 0)
				{
					return new ResultCode("The Geometry Prim Group \"{0}\" does not have a matching material in the Granny File.  This is a tool chain bug.", primGroup.Name);
				}
				if (array.Length > 1)
				{
					return new ResultCode("The Geometry Prim Group \"{0}\" has more than one matching material in the Granny File.  THIS IS NOT ALLOWED!!", primGroup.Name);
				}
				IGrannyMaterial grannyMaterial2 = array[0];
				if (grannyTriMaterialGroup.MaterialIndex < grannyMesh.MaterialBindings.Count)
				{
					IGrannyMaterial grannyMaterial3 = grannyMesh.MaterialBindings[grannyTriMaterialGroup.MaterialIndex];
					if (grannyMaterial2 != grannyMaterial3)
					{
						return new ResultCode("Inconsistent Granny File.  This is an export bug.");
					}
				}
			}
		}
		if (geo.BoneNames.Count() != grannyModel.Skeleton.Bones.Count)
		{
			return new ResultCode("The Geometry and its Granny File define an inconsistent set of Bones.  This is a tool chain bug.");
		}
		return ResultCode.Success;
	}

	private static ResultCode Validate(IAnimationInstance anim, IAnimationClass animClass)
	{
		return ResultCode.Success;
	}

	private static ResultCode Validate(IParticleEffectInstance particle, IParticleEffectClass particleClass)
	{
		return ResultCode.Success;
	}

	public static ResultCode ParseGrannyFiles(IInstanceEntity entity, string localPantry)
	{
		IInstanceDataFile instanceDataFile = entity.FindDataFileByID("GR2");
		if (instanceDataFile == null)
		{
			return new ResultCode("Entity {0} does not have a FGX file.", entity.Name);
		}
		string dataFilePath = entity.GetDataFilePath(instanceDataFile.RelativePath);
		if (File.Exists(dataFilePath))
		{
			ResultCode resultCode = new ResultCode("Unknown Entity Type");
			if (entity.Type == InstanceType.IT_GEOMETRY)
			{
				resultCode = ParseGrannyFile(entity as IGeometryInstanceBuildable, dataFilePath);
			}
			else if (entity.Type == InstanceType.IT_ANIMATION)
			{
				resultCode = ParseGrannyFile(entity as IAnimationInstance, dataFilePath);
			}
			if (!resultCode)
			{
				return resultCode;
			}
			return ResultCode.Success;
		}
		return new ResultCode("Could not parse FGX file at path ({0}) because it does not exist.", dataFilePath);
	}

	public static ResultCode ParseGrannyFile(IAnimationInstance animData, string grannyFilePath)
	{
		if (animData == null || !File.Exists(grannyFilePath))
		{
			return new ResultCode("Could not parse the granny file ({0}) at path ({1}).  The granny file or animation do not exist.", (animData == null) ? "<null>" : animData.Name, grannyFilePath);
		}
		GrannyContext grannyContext = Context.Get<GrannyContext>();
		IGrannyFile grannyFile = null;
		try
		{
			grannyFile = grannyContext.LoadGrannyFile(grannyFilePath);
		}
		catch (Exception ex)
		{
			return new ResultCode("Could not load the granny file ({0}) at path ({1}).  Reason: {0}.", animData.Name, grannyFilePath, ex.Message);
		}
		if (grannyFile == null)
		{
			return new ResultCode("Could not parse the granny file ({0}) at path ({1}).  The granny file could not be loaded by the granny loader.", animData.Name, grannyFilePath);
		}
		float num = 0f;
		foreach (IGrannyAnimation animation in grannyFile.Animations)
		{
			num += animation.Duration;
		}
		animData.Duration = num;
		return ResultCode.Success;
	}

	public static ResultCode ParseGrannyFile(IGeometryInstanceBuildable geoData, string grannyFilePath)
	{
		if (geoData == null || !File.Exists(grannyFilePath))
		{
			return new ResultCode("Could not parse the granny file ({0}) at path ({1}).  The granny file or geometry do not exist.", (geoData == null) ? "<null>" : geoData.Name, grannyFilePath);
		}
		GrannyContext grannyContext = Context.Get<GrannyContext>();
		IGrannyFile grannyFile = null;
		try
		{
			grannyFile = grannyContext.LoadGrannyFile(grannyFilePath);
		}
		catch (Exception ex)
		{
			return new ResultCode("Failed to load the granny file ({0}) for Geometry ({1}).  Reason: {2}.", grannyFilePath, geoData.Name, ex.Message);
		}
		if (grannyFile == null)
		{
			return new ResultCode("Could not parse the granny file ({0}) for Geometry ({1}).  The granny file failed to load.", grannyFilePath, geoData.Name);
		}
		if (!grannyFile.Models.Any())
		{
			return new ResultCode("The granny file ({0}) for Geometry ({1}) does not contain any models.  This is an export bug.", grannyFilePath, geoData.Name);
		}
		if (grannyFile.Models.Count > 1)
		{
			return new ResultCode("The granny file ({0}) for Geometry ({1}) contains more than one model.  This is an export bug.", grannyFilePath, geoData.Name);
		}
		IGrannyModel grannyModel = grannyFile.Models[0];
		geoData.SetModelName(grannyModel.Name);
		geoData.ClearGeometryMeshes();
		foreach (IGrannyMesh meshBinding in grannyModel.MeshBindings)
		{
			ResultCode resultCode = AddGrannyMeshToGeometry(meshBinding, geoData);
			if (!resultCode)
			{
				return resultCode;
			}
		}
		geoData.ClearBones();
		if (grannyModel.Skeleton != null && grannyModel.Skeleton.Bones != null)
		{
			foreach (IGrannyBone bone in grannyModel.Skeleton.Bones)
			{
				geoData.AddBone(bone.Name);
			}
		}
		AddLodsToGeometry(grannyModel, geoData);
		return ResultCode.Success;
	}

	private static void AddLodsToGeometry(IGrannyModel model, IGeometryInstanceBuildable geoData)
	{
		foreach (IGrannyLod lod2 in model.Lods)
		{
			Lod lod = new Lod();
			lod.TargetIndexCount = lod2.TargetIndexCount;
			lod.TransitionArea = lod2.TransitionArea;
			lod.Reduction = lod2.Reduction;
			geoData.AddLod(lod);
		}
	}

	private static ResultCode AddGrannyMeshToGeometry(IGrannyMesh grannyMesh, IGeometryInstanceBuildable geoData)
	{
		IGeoMesh geoMesh = geoData.AddGeometryMesh(grannyMesh.Name);
		geoMesh.VertexCount = (uint)grannyMesh.VertexCount;
		geoMesh.BoundBoneCount = (uint)grannyMesh.BoneBindings.Count;
		geoMesh.PrimitiveCount = 0u;
		IList<IGrannyMaterial> materialBindings = grannyMesh.MaterialBindings;
		foreach (IGrannyTriMaterialGroup triangleMaterialGroup in grannyMesh.TriangleMaterialGroups)
		{
			int materialIndex = triangleMaterialGroup.MaterialIndex;
			if (materialIndex < materialBindings.Count)
			{
				IGrannyMaterial grannyMaterial = materialBindings[materialIndex];
				if (!string.IsNullOrEmpty(grannyMaterial.Name))
				{
					geoMesh.PrimitiveCount += AddPrimGroupsToMesh(geoMesh, grannyMaterial, triangleMaterialGroup);
					continue;
				}
				return new ResultCode("A granny material has an unassigned name.  This is an exporter bug!");
			}
			return new ResultCode("A granny material index is out of bounds.  This is an exporter bug!");
		}
		return ResultCode.Success;
	}

	private static uint AddPrimGroupsToMesh(IGeoMesh geometryMesh, IGrannyMaterial material, IGrannyTriMaterialGroup triangleMeshGroup)
	{
		string name = material.Name;
		uint triFirst = (uint)triangleMeshGroup.TriFirst;
		uint triCount = (uint)triangleMeshGroup.TriCount;
		geometryMesh.AddGeoPrimGroup(name, triFirst, triCount);
		return triCount;
	}

	public static ResultCode ParseWigFiles(IGeometryInstanceBuildable entity, string localPantry)
	{
		if (entity == null)
		{
			return ResultCode.Success;
		}
		IInstanceDataFile instanceDataFile = entity.FindDataFileByID("WIG");
		if (instanceDataFile == null)
		{
			return ResultCode.Success;
		}
		string dataFilePath = entity.GetDataFilePath(instanceDataFile.RelativePath);
		if (File.Exists(dataFilePath))
		{
			if (Path.GetExtension(dataFilePath).Equals(".wig", StringComparison.InvariantCultureIgnoreCase))
			{
				ResultCode resultCode = ParseWigFile(entity, dataFilePath);
				if (!resultCode)
				{
					return resultCode;
				}
				return ResultCode.Success;
			}
			return new ResultCode("Could not parse WIG file at path ({0}) because its extension is not .wig.", dataFilePath);
		}
		return new ResultCode("Could not parse WIG file at path ({0}) because it does not exist.", dataFilePath);
	}

	public static ResultCode ParseWigFile(IGeometryInstanceBuildable geoData, string wigFilePath)
	{
		if (geoData == null || !File.Exists(wigFilePath))
		{
			return new ResultCode("Could not parse the wig file ({0}) at path ({1}).  The wig file or geometry do not exist.", (geoData == null) ? "<null>" : geoData.Name, wigFilePath);
		}
		WigFile wigFile = WigLoader.loadWigFile(wigFilePath);
		if (wigFile == null)
		{
			return new ResultCode("Could not parse wig file ({0}) at path ({1}).  The WigLoader could not load the file.  Ensure it is the correct type.", geoData.Name, wigFilePath);
		}
		foreach (HairSystem hairSystem in wigFile.hairData.HairSystemList)
		{
			IGeoMesh geoMesh = geoData.AddGeometryMesh(hairSystem.name);
			geoMesh.AddGeoPrimGroup(hairSystem.name, 0u, 0u);
		}
		return ResultCode.Success;
	}

	public static ResultCode ParseDDSFiles(ITextureInstance entity, string localPantry)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IInstanceDataFile dataFile in entity.DataFiles)
		{
			string dataFilePath = entity.GetDataFilePath(dataFile.RelativePath);
			if (File.Exists(dataFilePath) && Path.GetExtension(dataFilePath).Equals(".dds", StringComparison.InvariantCultureIgnoreCase))
			{
				ParseDDSFile(entity, dataFilePath);
				continue;
			}
			stringBuilder.AppendFormat("Could not parse entity {0}.  ", entity.Name);
			stringBuilder.AppendFormat("Tried to parse the file located at ({0}) as a DDS file and failed.", dataFilePath);
			stringBuilder.AppendLine();
		}
		if (stringBuilder.Length > 0)
		{
			return new ResultCode(stringBuilder.ToString());
		}
		return ResultCode.Success;
	}

	public static void ParseDDSFile(ITextureInstance texture, string ddsFilePath)
	{
		using IDDSDataExtractor iDDSDataExtractor = Context.Get<CivTechContext>().CreateInstance<IDDSDataExtractor>();
		iDDSDataExtractor.LoadDDSFile(ddsFilePath);
		texture.Height = iDDSDataExtractor.GetTextureHeight();
		texture.Width = iDDSDataExtractor.GetTextureWidth();
		texture.Depth = iDDSDataExtractor.GetTextureDepth();
		texture.NumMipMaps = iDDSDataExtractor.GetTextureMipMapCount();
	}
}

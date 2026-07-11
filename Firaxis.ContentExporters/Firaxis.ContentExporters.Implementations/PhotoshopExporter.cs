using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.IO;
using Firaxis.PhotoshopInterface;
using Firaxis.PhotoshopInterface.PhotoshopFile;
using Firaxis.Utility;

namespace Firaxis.ContentExporters.Implementations;

public class PhotoshopExporter : IContentExporter, IContentCreationTool
{
	private class PerProjectSourceFileComparer : IEqualityComparer<Tuple<ImportOperationResult, IClassEntity>>
	{
		public bool Equals(Tuple<ImportOperationResult, IClassEntity> x, Tuple<ImportOperationResult, IClassEntity> y)
		{
			return PathCompareHelper.Equals(x.Item1.Entity.SourceFilePath, y.Item1.Entity.SourceFilePath, bIgnoreCase: true);
		}

		public int GetHashCode(Tuple<ImportOperationResult, IClassEntity> obj)
		{
			return PathCompareHelper.GetHashCode(obj.Item1.Entity.SourceFilePath, bIgnoreCase: true);
		}
	}

	private static class PsdLoad
	{
		public static List<string> Load(Stream input)
		{
			PsdFile psdFile = new PsdFile();
			psdFile.Load(input, Encoding.Default);
			if (psdFile.ColorMode == PsdColorMode.Multichannel)
			{
				CreateLayersFromChannels(psdFile);
				psdFile.ColorMode = PsdColorMode.Grayscale;
			}
			psdFile.VerifyLayerSections();
			return GetLayerGroupNames(psdFile.Layers);
		}

		private static void CreateLayersFromChannels(PsdFile psdFile)
		{
			if (psdFile.ColorMode != PsdColorMode.Multichannel)
			{
				throw new Exception("Not a multichannel image.");
			}
			if (psdFile.Layers.Count > 0)
			{
				throw new PsdInvalidException("Multichannel image should not have layers.");
			}
			AlphaChannelNames alphaChannelNames = (AlphaChannelNames)psdFile.ImageResources.Get(ResourceID.AlphaChannelNames);
			UnicodeAlphaNames unicodeAlphaNames = (UnicodeAlphaNames)psdFile.ImageResources.Get(ResourceID.UnicodeAlphaNames);
			if (alphaChannelNames == null && unicodeAlphaNames == null)
			{
				throw new PsdInvalidException("No channel names found.");
			}
			List<string> list = ((unicodeAlphaNames != null) ? unicodeAlphaNames.ChannelNames : alphaChannelNames.ChannelNames);
			ChannelList channels = psdFile.BaseLayer.Channels;
			if (channels.Count > list.Count)
			{
				throw new PsdInvalidException("More channels than channel names.");
			}
			for (int num = channels.Count - 1; num >= 0; num--)
			{
				Channel channel = channels[num];
				string name = list[num];
				Layer layer = new Layer(psdFile);
				layer.Visible = true;
				layer.Masks = new MaskInfo();
				layer.BlendingRangesData = new BlendingRanges(layer);
				layer.Name = name;
				layer.BlendModeKey = "dark";
				layer.Opacity = byte.MaxValue;
				psdFile.Layers.Add(layer);
			}
		}

		private static List<string> GetLayerGroupNames(List<Layer> layers)
		{
			int num = int.MaxValue;
			Stack<string> stack = new Stack<string>();
			List<string> list = new List<string>();
			int num2 = 0;
			foreach (Layer item2 in Enumerable.Reverse(layers))
			{
				if (stack.Count > num)
				{
					item2.Visible = false;
				}
				LayerSectionInfo layerSectionInfo = (LayerSectionInfo)item2.AdditionalInfo.SingleOrDefault((LayerInfo x) => x is LayerSectionInfo);
				if (layerSectionInfo == null)
				{
					continue;
				}
				switch (layerSectionInfo.SectionType)
				{
				case LayerSectionType.OpenFolder:
				case LayerSectionType.ClosedFolder:
					if (!item2.Visible && num == int.MaxValue)
					{
						num = stack.Count;
					}
					stack.Push(item2.Name);
					num2++;
					break;
				case LayerSectionType.SectionDivider:
				{
					num2--;
					string item = stack.Pop();
					if (num2 == 0)
					{
						list.Add(item);
					}
					if (stack.Count == num)
					{
						num = int.MaxValue;
					}
					break;
				}
				}
			}
			return list;
		}
	}

	private Adobe_PhotoshopInterface PhotoshopInterface;

	private List<string> _supportedFileTypes;

	private List<InstanceType> _supportedInstanceTypes;

	public IEnumerable<string> SupportedFileTypes => _supportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	public PhotoshopExporter()
	{
		PhotoshopInterface = new Adobe_PhotoshopInterface();
		_supportedFileTypes = new List<string>();
		_supportedFileTypes.Add(".psd");
		_supportedFileTypes.Add(".bmp");
		_supportedFileTypes.Add(".jpg");
		_supportedFileTypes.Add(".jpeg");
		_supportedFileTypes.Add(".pcx");
		_supportedFileTypes.Add(".tga");
		_supportedFileTypes.Add(".raw");
		_supportedFileTypes.Add(".pict");
		_supportedFileTypes.Add(".pns");
		_supportedFileTypes.Add(".png");
		_supportedFileTypes.Add(".tif");
		_supportedFileTypes.Add(".tiff");
		_supportedInstanceTypes = new List<InstanceType>();
		_supportedInstanceTypes.Add(InstanceType.IT_TEXTURE);
	}

	private bool Start()
	{
		return PhotoshopInterface.EstablishConnectionToRunningPhotoshop();
	}

	public void OpenFile(string filepath)
	{
		if (File.Exists(filepath))
		{
			WindowsPath imageFilename = new WindowsPath(filepath);
			if (Start())
			{
				PhotoshopInterface.OpenImage(imageFilename);
			}
		}
	}

	public void RenderSourceObjectFromFile(string filePath, string sourceObject, string outputFolder)
	{
	}

	private void ExportLayerGroup(string filename_to_export, string destination_folder_path, IEnumerable<string> normal_layer_groups, IEnumerable<string> gradient_map_layer_groups, IEnumerable<string> normal_layer_group_names, IEnumerable<string> gradient_map_layer_group_names, IEnumerable<string> normal_export_params, IEnumerable<string> gradient_export_params)
	{
		if (Start())
		{
			PhotoshopInterface.ExportLayerGroup(filename_to_export, destination_folder_path, normal_layer_groups, gradient_map_layer_groups, normal_layer_group_names, gradient_map_layer_group_names, normal_export_params, gradient_export_params);
		}
	}

	public void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity classEntity)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity.Entity, ref project);
		if (!entityProject)
		{
			entity.Result = entityProject;
			return;
		}
		string destination_folder_path = StaticMethods.PantryRootForInstanceType(project.Paths.GamePantry, entity.Entity.Type).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		if (!Start())
		{
			entity.Result = new ResultCode("Could not connect to Photoshop.  Export failed.");
			return;
		}
		if (!(entity.Entity is ITextureInstance textureInstance))
		{
			entity.Result = new ResultCode("The Photoshop Exporter can only operate on textures.  Entity {0} has the type {1}.", entity.Entity.Name, entity.Entity.Type.ToString());
			return;
		}
		if (string.IsNullOrEmpty(textureInstance.SourceFilePath))
		{
			entity.Result = new ResultCode("Texture ({0}) does not have a source file path set so it cannot be exported.", textureInstance.Name);
			return;
		}
		if (!File.Exists(textureInstance.SourceFilePath))
		{
			entity.Result = new ResultCode("Texture ({0}) has a source file path that does not exist on disk.  Invalid path: ({1})", textureInstance.Name, textureInstance.SourceFilePath);
			return;
		}
		if (textureInstance.SourceObjectName.Equals("loose_layers_only"))
		{
			ExportPSDFile(textureInstance.SourceFilePath, destination_folder_path, textureInstance.GetExportSettingsString(), textureInstance.Name);
		}
		else
		{
			string sourceObjectName = textureInstance.SourceObjectName;
			string name = textureInstance.Name;
			string exportSettingsString = textureInstance.GetExportSettingsString();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			List<string> list5 = new List<string>();
			List<string> list6 = new List<string>();
			if (!textureInstance.IsGradientMapTexture())
			{
				list.Add(sourceObjectName);
				list2.Add(name);
				list3.Add(exportSettingsString);
			}
			else
			{
				list4.Add(sourceObjectName);
				list5.Add(name);
				list6.Add(exportSettingsString);
			}
			ExportLayerGroup(textureInstance.SourceFilePath, destination_folder_path, list, list4, list2, list5, list3, list6);
		}
		textureInstance.PopulateDataFiles(classEntity);
		entity.Result = ExportUtilityFunctions.ParseDDSFiles(textureInstance, project.Paths.GamePantry);
		if ((bool)entity.Result)
		{
			textureInstance.UpdateExportedTime();
		}
	}

	public void Export(ICivTechService civTechSvc, IEnumerable<Tuple<ImportOperationResult, IClassEntity>> entities)
	{
		if (!Start())
		{
			foreach (Tuple<ImportOperationResult, IClassEntity> entity in entities)
			{
				ImportOperationResult item = entity.Item1;
				item.Result = new ResultCode("Could not connect to Photoshop.  Export failed.");
			}
			return;
		}
		IEnumerable<Tuple<ImportOperationResult, IClassEntity>> enumerable = entities.Where((Tuple<ImportOperationResult, IClassEntity> ent) => ent.Item1.Result);
		foreach (Tuple<ImportOperationResult, IClassEntity> item2 in enumerable)
		{
			Export(civTechSvc, item2.Item1, item2.Item2);
		}
	}

	private void PopulateDataFiles(IEnumerable<Tuple<ImportOperationResult, IClassEntity>> validEntities)
	{
		foreach (Tuple<ImportOperationResult, IClassEntity> validEntity in validEntities)
		{
			IInstanceEntity entity = validEntity.Item1.Entity;
			IClassEntity item = validEntity.Item2;
			entity.PopulateDataFiles(item);
		}
	}

	private void ParseDDSFiles(IEnumerable<Tuple<ImportOperationResult, IClassEntity>> validEntities, string localPantry)
	{
		foreach (Tuple<ImportOperationResult, IClassEntity> validEntity in validEntities)
		{
			ImportOperationResult item = validEntity.Item1;
			ITextureInstance textureInstance = item.Entity as ITextureInstance;
			item.Result = ExportUtilityFunctions.ParseDDSFiles(textureInstance, localPantry);
			if ((bool)item.Result)
			{
				textureInstance.UpdateExportedTime();
			}
		}
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		ResultCode success = ResultCode.Success;
		instanceEntity.PopulateDataFiles(classEntity);
		ResultCode resultCode = ExportUtilityFunctions.ParseDDSFiles(instanceEntity as ITextureInstance, localPantry);
		instanceEntity.UpdateExportedTime();
		return success;
	}

	private string ExportPSDFile(string filename_to_export, string destination_folder_path, string export_params, string output_file_name)
	{
		if (!Start())
		{
			return string.Empty;
		}
		WindowsPath filename_to_export2 = new WindowsPath(filename_to_export);
		WindowsPath destination_folder_path2 = new WindowsPath(destination_folder_path);
		string output_file_name2 = $"{output_file_name}.dds";
		return PhotoshopInterface.ExportLooseLayers(filename_to_export2, destination_folder_path2, export_params, output_file_name2);
	}

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		if (entityType != InstanceType.IT_TEXTURE)
		{
			return null;
		}
		List<string> list = new List<string>();
		if (File.Exists(filePath) && Path.GetExtension(filePath) == ".psd")
		{
			using FileStream fileStream = File.OpenRead(filePath);
			list = PsdLoad.Load(fileStream);
			fileStream.Close();
		}
		if (list.Count == 0)
		{
			list.Add("loose_layers_only");
		}
		return list;
	}

	ResultCode IContentExporter.ValidateClass(IImportedEntity entity, IClassEntity entityClass)
	{
		return ExportUtilityFunctions.ValidateClass(entity, entityClass);
	}

	ResultCode IContentExporter.Validate(IImportedEntity entity, IClassEntity entityClass, string localPantry)
	{
		return ExportUtilityFunctions.Validate(entity, entityClass, localPantry);
	}

	public ResultCode RebuildExportedEntity(ICivTechService civTechSvc, IImportedEntity entity, IClassEntity entityClass)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			return entityProject;
		}
		return ProcessExportedEntity(entity, entityClass, project.Paths.GamePantry);
	}
}

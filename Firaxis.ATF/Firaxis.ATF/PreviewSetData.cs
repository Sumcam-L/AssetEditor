using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public class PreviewSetData
{
	public List<AssetData> OpenedAssets { get; set; }

	public KnobSetData PreviewerKnobSetData { get; set; }

	public string PrimaryAssetName { get; set; }

	public InstanceType PrimaryAssetType { get; set; }

	public PreviewSetData()
		: this(string.Empty, InstanceType.IT_INVALID)
	{
	}

	public PreviewSetData(IInstanceEntityAdapter entity)
		: this(entity.Name, entity.InstanceType)
	{
	}

	public PreviewSetData(string assetName, InstanceType assetType)
	{
		PrimaryAssetName = assetName;
		PrimaryAssetType = assetType;
		OpenedAssets = new List<AssetData>();
		PreviewerKnobSetData = new KnobSetData();
	}

	public void BuildPreviewerKnobSetData(IKnobSet knobSet)
	{
		PreviewerKnobSetData.BuildKnobSetData(knobSet);
	}

	public static PreviewSetData LoadFromFile(string filePath, out string errorMessage)
	{
		errorMessage = string.Empty;
		PreviewSetData result = null;
		if (File.Exists(filePath))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PreviewSetData));
			FileStream fileStream = null;
			try
			{
				fileStream = File.OpenRead(filePath);
				result = xmlSerializer.Deserialize(fileStream) as PreviewSetData;
			}
			catch (System.Exception ex)
			{
				errorMessage = "An error occurred while trying to deserialize the file at path (" + filePath + ").\n\nError: " + ex.Message;
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
					fileStream.Dispose();
				}
			}
		}
		else
		{
			errorMessage = "Cannot open the preview set at path (" + filePath + ") because it does not exist.";
		}
		return result;
	}

	public void SaveToFile(string filePath, bool overwrite, out string errorMessage)
	{
		SaveToFile(this, filePath, overwrite, out errorMessage);
	}

	public static void SaveToFile(PreviewSetData data, string filePath, bool overwrite, out string errorMessage)
	{
		errorMessage = string.Empty;
		if (File.Exists(filePath))
		{
			if (!overwrite)
			{
				errorMessage = "A file already exists at path (" + filePath + ") and you have not chosen to overwrite.";
				return;
			}
			try
			{
				File.Delete(filePath);
			}
			catch (System.Exception ex)
			{
				errorMessage = "An error occurred while trying to overwrite the file at path (" + filePath + ").\n\nError: " + ex.Message + ".";
				return;
			}
		}
		FileStream fileStream = null;
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PreviewSetData));
			fileStream = File.OpenWrite(filePath);
			xmlSerializer.Serialize(fileStream, data);
		}
		catch (System.Exception ex2)
		{
			errorMessage = "An error occurred while trying to serialize the Preview Set Data to disk at path (" + filePath + ").  Error: " + ex2.Message;
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream.Dispose();
			}
		}
	}

	public override string ToString()
	{
		return "Entity Name: " + PrimaryAssetName + "; Entity Type: " + PrimaryAssetType;
	}
}

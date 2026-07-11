using System;
using System.IO;

namespace Firaxis.Wig;

public static class WigLoader
{
	public static WigFile loadWigFile(string wigFilePath)
	{
		WigFile wigFile = new WigFile();
		try
		{
			using BinaryReader binaryReader = new BinaryReader(File.Open(wigFilePath, FileMode.Open, FileAccess.Read));
			string text = new string(binaryReader.ReadChars(3));
			if (text != "WIG")
			{
				return null;
			}
			wigFile.exporterVersion = binaryReader.ReadInt32();
			wigFile.sourceProgam = readString(binaryReader);
			wigFile.sourceFile = readString(binaryReader);
			wigFile.hairData.name = readString(binaryReader);
			wigFile.hairData.position = readFloatArray(binaryReader, 3);
			wigFile.hairData.rotation = readFloatArray(binaryReader, 3);
			wigFile.hairData.scale = readFloatArray(binaryReader, 3);
			wigFile.hairData.numberOfHairSystems = binaryReader.ReadInt32();
			for (int i = 0; i < wigFile.hairData.numberOfHairSystems; i++)
			{
				HairSystem hairSystem = new HairSystem();
				hairSystem.name = readString(binaryReader);
				hairSystem.position = readFloatArray(binaryReader, 3);
				hairSystem.rotation = readFloatArray(binaryReader, 3);
				hairSystem.scale = readFloatArray(binaryReader, 3);
				hairSystem.numberOfHairStrands = binaryReader.ReadInt32();
				for (int j = 0; j < hairSystem.numberOfHairStrands; j++)
				{
					HairStrand hairStrand = new HairStrand();
					hairStrand.name = readString(binaryReader);
					hairStrand.position = readFloatArray(binaryReader, 3);
					hairStrand.rotation = readFloatArray(binaryReader, 3);
					hairStrand.scale = readFloatArray(binaryReader, 3);
					hairStrand.UV = readFloatArray(binaryReader, 2);
					hairStrand.numberOfBoneNames = binaryReader.ReadInt32();
					for (int k = 0; k < hairStrand.numberOfBoneNames; k++)
					{
						hairStrand.BoneNameList.Add(readString(binaryReader));
					}
					hairStrand.numberOfControlPoints = binaryReader.ReadInt32();
					for (int l = 0; l < hairStrand.numberOfControlPoints; l++)
					{
						ControlPoint controlPoint = new ControlPoint();
						controlPoint.position = readFloatArray(binaryReader, 3);
						controlPoint.boneIndices = readIntArray(binaryReader, 8);
						controlPoint.boneWeights = readFloatArray(binaryReader, 8);
						hairStrand.ControlPointList.Add(controlPoint);
					}
					hairSystem.HairStrandList.Add(hairStrand);
				}
				wigFile.hairData.HairSystemList.Add(hairSystem);
			}
		}
		catch (Exception)
		{
			wigFile = null;
		}
		return wigFile;
	}

	private static string readString(BinaryReader streamReader)
	{
		int count = streamReader.ReadInt32();
		return new string(streamReader.ReadChars(count));
	}

	private static int[] readIntArray(BinaryReader streamReader, int numberOfEntries)
	{
		int[] array = new int[numberOfEntries];
		for (int i = 0; i < numberOfEntries; i++)
		{
			array[i] = streamReader.ReadInt32();
		}
		return array;
	}

	private static float[] readFloatArray(BinaryReader streamReader, int numberOfEntries)
	{
		float[] array = new float[numberOfEntries];
		for (int i = 0; i < numberOfEntries; i++)
		{
			array[i] = streamReader.ReadSingle();
		}
		return array;
	}
}

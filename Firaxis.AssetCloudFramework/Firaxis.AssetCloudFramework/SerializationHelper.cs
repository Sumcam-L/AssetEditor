using System;
using System.IO;
using System.Xml.Serialization;
using Firaxis.Error;

namespace Firaxis.AssetCloudFramework;

public static class SerializationHelper
{
	public static void Serialize<T>(Stream stream, T obj)
	{
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			xmlSerializer.Serialize(stream, obj);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
		}
	}

	public static void Serialize<T>(string sFilename, T obj)
	{
		try
		{
			if (Path.IsPathRooted(sFilename))
			{
				string directoryName = Path.GetDirectoryName(sFilename);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
			}
			using FileStream stream = new FileStream(sFilename, FileMode.Create, FileAccess.Write);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			xmlSerializer.Serialize(stream, obj);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
		}
	}

	public static T Deserialize<T>(Stream stream)
	{
		T val = default(T);
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			return (T)xmlSerializer.Deserialize(stream);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
			val = default(T);
		}
		return val;
	}

	public static T Deserialize<T>(string sFilename)
	{
		T result = default(T);
		if (File.Exists(sFilename))
		{
			try
			{
				using FileStream stream = new FileStream(sFilename, FileMode.Open, FileAccess.Read);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
				return (T)xmlSerializer.Deserialize(stream);
			}
			catch (Exception e)
			{
				ExceptionLogger.Log(e);
				result = default(T);
			}
		}
		return result;
	}
}

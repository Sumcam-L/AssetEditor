using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Sce.Atf;

public static class AssemblyUtil
{
	public static Assembly[] GetLoadedAssemblies()
	{
		return AppDomain.CurrentDomain.GetAssemblies();
	}

	public static AssemblyName GetAssemblyName(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentException("file");
		}
		if (!File.Exists(fileName))
		{
			throw new ArgumentException(fileName + " does not exist");
		}
		AssemblyName result = null;
		try
		{
			result = AssemblyName.GetAssemblyName(fileName);
		}
		catch (Exception ex)
		{
			Outputs.Write(OutputMessageType.Error, fileName + ": ");
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
		return result;
	}

	public static Dictionary<string, Assembly> GetDictionaryOfLoadedAssemblies()
	{
		Dictionary<string, Assembly> dictionary = new Dictionary<string, Assembly>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (dictionary.ContainsKey(assembly.FullName))
			{
				Outputs.WriteLine(OutputMessageType.Error, assembly.FullName + " has been loaded twice");
				Outputs.WriteLine(OutputMessageType.Error, "\t" + assembly.Location);
				Outputs.WriteLine(OutputMessageType.Error, "\t" + dictionary[assembly.FullName].Location);
			}
			else
			{
				dictionary.Add(assembly.FullName, assembly);
			}
		}
		return dictionary;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Firaxis.Error;
using Firaxis.Reflection;

namespace Firaxis.Utility;

public class PluginFactory
{
	private Type pluginType;

	private List<Type> plugins;

	public Type PluginType => pluginType;

	public List<Type> Plugins => plugins;

	public PluginFactory(Type pluginType)
	{
		this.pluginType = pluginType;
		plugins = new List<Type>();
	}

	public Type Find(string name)
	{
		return plugins.Find((Type type) => string.Compare(name, type.FullName) == 0);
	}

	public Type FindByDisplayName(string name)
	{
		return plugins.Find((Type type) => string.Compare(name, ReflectionHelper.GetDisplayName(type)) == 0);
	}

	public void ScanAssembly(Assembly assembly)
	{
		if (!(assembly != null))
		{
			return;
		}
		try
		{
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsAbstract && type.IsClass && pluginType.IsAssignableFrom(type))
				{
					plugins.Add(type);
				}
			}
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
		}
	}

	public void ScanDirectory(string path, string searchPattern)
	{
		ScanDirectory(path, searchPattern, SearchOption.AllDirectories);
	}

	public void ScanDirectory(string path, string searchPattern, SearchOption searchOption)
	{
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] array = null;
		try
		{
			array = Directory.GetFiles(path, searchPattern, searchOption);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
			return;
		}
		string[] array2 = array;
		foreach (string path2 in array2)
		{
			try
			{
				if (File.Exists(path2))
				{
					Assembly assembly = Assembly.LoadFile(path2);
					ScanAssembly(assembly);
				}
			}
			catch (FileLoadException)
			{
			}
			catch (FileNotFoundException)
			{
			}
			catch (BadImageFormatException)
			{
			}
		}
	}
}

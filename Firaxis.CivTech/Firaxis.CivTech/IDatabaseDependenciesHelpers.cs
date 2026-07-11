using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Firaxis.CivTech;

public static class IDatabaseDependenciesHelpers
{
	private class DepotDependencyInfoDeserializer
	{
		public long Timestamp = 0L;

		public uint Changelist = 0u;

		public IDictionary<string, IList<string>> Dependencies = new Dictionary<string, IList<string>>();

		public IList<DepotFileInfo> Files = new List<DepotFileInfo>();
	}

	private const int kMaxFileSize = 52428800;

	public static void GenerateDependants(this IDatabaseDependencies dbDeps)
	{
		dbDeps.Dependants.Clear();
		foreach (string key in dbDeps.Dependencies.Keys)
		{
			foreach (string item in dbDeps.Dependencies[key])
			{
				dbDeps.Dependants.AddIfUnique(item, key);
			}
		}
	}

	public static bool Load(this IDatabaseDependencies dbDeps, string filePath)
	{
		try
		{
			string input = string.Empty;
			using (TextReader textReader = File.OpenText(filePath))
			{
				input = textReader.ReadToEnd();
			}
			JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
			javaScriptSerializer.MaxJsonLength = 52428800;
			DepotDependencyInfoDeserializer deserializer = null;
			try
			{
				deserializer = javaScriptSerializer.Deserialize<DepotDependencyInfoDeserializer>(input);
				if (deserializer == null)
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
			dbDeps.Timestamp = deserializer.Timestamp;
			dbDeps.Changelist = deserializer.Changelist;
			Task task = Task.Factory.StartNew(delegate
			{
				Parallel.ForEach(deserializer.Files, delegate(DepotFileInfo infoFromDisk)
				{
					string filename = infoFromDisk.Filename;
					dbDeps.Files.AddOrUpdate(filename, infoFromDisk);
				});
			});
			Task task2 = Task.Factory.StartNew(delegate
			{
				Parallel.ForEach(deserializer.Dependencies, delegate(KeyValuePair<string, IList<string>> dep)
				{
					dbDeps.Dependencies.AddChildren(dep.Key, dep.Value);
				});
			});
			Task.WaitAll(task, task2);
			return true;
		}
		catch (IOException ex2)
		{
			Console.WriteLine(ex2.Message);
			return false;
		}
	}

	public static bool Save(this IDatabaseDependencies dbDeps, string filePath)
	{
		string empty = string.Empty;
		try
		{
			empty = WriteDependenciesToJSON(dbDeps);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			return false;
		}
		int num = 10;
		int num2 = 32;
		do
		{
			try
			{
				using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					using StreamWriter streamWriter = new StreamWriter(stream);
					streamWriter.Write(empty);
					streamWriter.Flush();
					streamWriter.Close();
				}
				return true;
			}
			catch (IOException ex2)
			{
				if (Marshal.GetHRForException(ex2) != -2147024864)
				{
					BugSubmitter.SilentException(ex2);
					return false;
				}
			}
			Thread.Sleep(num2);
			num2 <<= 1;
		}
		while (--num >= 0);
		return false;
	}

	private static string WriteDependenciesToJSON(IDatabaseDependencies databaseDependencies)
	{
		DepotDependencyInfoDeserializer serialObj = new DepotDependencyInfoDeserializer();
		serialObj.Timestamp = databaseDependencies.Timestamp;
		serialObj.Changelist = databaseDependencies.Changelist;
		foreach (string key in databaseDependencies.Dependencies.Keys)
		{
			serialObj.Dependencies[key] = new List<string>(databaseDependencies.Dependencies[key]);
		}
		serialObj.Files = new List<DepotFileInfo>();
		databaseDependencies.Files.ForEachValue(delegate(DepotFileInfo value)
		{
			serialObj.Files.Add(value);
		});
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 52428800;
		return javaScriptSerializer.Serialize(serialObj);
	}
}

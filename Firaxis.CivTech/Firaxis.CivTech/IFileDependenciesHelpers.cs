using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SipHash;

namespace Firaxis.CivTech;

public static class IFileDependenciesHelpers
{
	private class FileDependencyInfoDeserializer<TKey>
	{
		public long Timestamp = 0L;

		public uint Changelist = 0u;

		public IDictionary<string, IList<string>> Dependencies = new Dictionary<string, IList<string>>();

		public IList<DepotFileInfo> Files = new List<DepotFileInfo>();
	}

	private const int kMaxFileSize = 52428800;

	public static void GenerateDependants(this IFileDependencies<string> dbDeps)
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

	public static bool Load(this IFileDependencies<string> dbDeps, string filePath)
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
			FileDependencyInfoDeserializer<string> deserializer = null;
			try
			{
				deserializer = javaScriptSerializer.Deserialize<FileDependencyInfoDeserializer<string>>(input);
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
			global::SipHash.SipHash hasher = new global::SipHash.SipHash(new byte[16]
			{
				13, 17, 19, 23, 29, 31, 37, 41, 43, 47,
				53, 59, 61, 67, 71, 73
			});
			Task task = Task.Factory.StartNew(delegate
			{
				Parallel.ForEach(deserializer.Files, delegate(DepotFileInfo infoFromDisk)
				{
					string fileKey = hasher.Compute(Encoding.UTF8.GetBytes(infoFromDisk.Filename.ToLower())).ToString("X16");
					dbDeps.Files.AddOrUpdate(fileKey, infoFromDisk);
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

	public static bool Save(this IFileDependencies<string> dbDeps, string filePath)
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

	private static string WriteDependenciesToJSON(IFileDependencies<string> databaseDependencies)
	{
		FileDependencyInfoDeserializer<string> serialObj = new FileDependencyInfoDeserializer<string>();
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

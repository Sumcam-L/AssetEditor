using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
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

	private class DepotFileInfoDto
	{
		public string Filename { get; set; }
		public long Timestamp { get; set; }
		public int Status { get; set; }
		public int Type { get; set; }
		public int EntityType { get; set; }
		public string EntityClass { get; set; }
		public long Filesize { get; set; }
		public List<string> Tags { get; set; }
		public Dictionary<string, int> Stats { get; set; }
	}

	private class DepInfoDto
	{
		public long Timestamp { get; set; }
		public uint Changelist { get; set; }
		public Dictionary<string, List<string>> Dependencies { get; set; }
		public List<DepotFileInfoDto> Files { get; set; }
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
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			DepInfoDto dto;
			using (var stream = File.OpenRead(filePath))
			{
				dto = JsonSerializer.Deserialize<DepInfoDto>(stream, options);
			}
			if (dto == null)
			{
				LogDiagnostic("deps-load FAILED null-dto file=" + filePath);
				return false;
			}
			dbDeps.Timestamp = dto.Timestamp;
			dbDeps.Changelist = dto.Changelist;
			Task task = Task.Factory.StartNew(delegate
			{
				if (dto.Files != null)
				{
					Parallel.ForEach(dto.Files, delegate(DepotFileInfoDto infoDto)
					{
						var info = new DepotFileInfo
						{
							Filename = infoDto.Filename,
							Timestamp = infoDto.Timestamp,
							Status = infoDto.Status,
							Type = infoDto.Type,
							EntityType = infoDto.EntityType,
							EntityClass = infoDto.EntityClass,
							Filesize = infoDto.Filesize,
							Tags = infoDto.Tags ?? new List<string>(),
							Stats = infoDto.Stats ?? new Dictionary<string, int>()
						};
						dbDeps.Files.AddOrUpdate(info.Filename, info);
					});
				}
			});
			Task task2 = Task.Factory.StartNew(delegate
			{
				if (dto.Dependencies != null)
				{
					Parallel.ForEach(dto.Dependencies, delegate(KeyValuePair<string, List<string>> dep)
					{
						dbDeps.Dependencies.AddChildren(dep.Key, dep.Value);
					});
				}
			});
			Task.WaitAll(task, task2);
			return true;
		}
		catch (Exception ex)
		{
			LogDiagnostic("deps-load EXCEPTION file=" + filePath + " type=" + ex.GetType().Name + " msg=" + ex.Message);
			return false;
		}
	}

	private static void LogDiagnostic(string message)
	{
		try
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log");
			File.AppendAllText(path, string.Format("{0:O} Startup: {1}{2}", DateTime.Now, message, Environment.NewLine));
		}
		catch { }
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

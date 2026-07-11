using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.VersionControl;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(BatchEntitySourceControlService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BatchEntitySourceControlService
{
	private readonly ICivTechService m_civTechService;

	[ImportingConstructor]
	public BatchEntitySourceControlService(ICivTechService civTechService)
	{
		m_civTechService = civTechService;
		Outputs.WriteLine(OutputMessageType.Info, "Starting up Batch Entity Source Control Service");
	}

	public ResultCode OpenInPerforce(IEnumerable<IInstanceEntity> entities)
	{
		if (entities == null)
		{
			return new ResultCode("No files specified to open");
		}
		ISet<string> set = new HashSet<string>();
		_ = m_civTechService.PrimaryProject.Paths.GamePantry;
		foreach (IInstanceEntity item in entities.Where((IInstanceEntity ent) => ent != null))
		{
			IClassEntity classEntity = m_civTechService.PrimaryProject.Config.Classes.FindForInstance(item);
			if (classEntity == null)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Skipping open in perforce for instance entity of type {0} and class \"{1}\" because that class no longer exists in the project config.", item.Type.ToString(), item.ClassName);
				continue;
			}
			string entityPath = m_civTechService.GetEntityPath(item.Name, item.Type);
			set.Add(entityPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
			foreach (IInstanceDataFile df in item.DataFiles)
			{
				IClassDataFile classDataFile = classEntity.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == df.ID);
				if (classDataFile == null)
				{
					Outputs.WriteLine(OutputMessageType.Info, "Stripping data file with ID \"{0}\" that no longer exists in the class entity \"{1}\" configuration.", df.ID, classEntity.Name);
				}
				else if (classDataFile.IsGenerated)
				{
					Outputs.WriteLine(OutputMessageType.Info, "Skipping open in perforce for generated data file with ID \"{0}\".", df.ID);
				}
				else
				{
					string dataFilePath = item.GetDataFilePath(df.RelativePath);
					set.Add(dataFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
				}
			}
		}
		return OpenInPerforce(set);
	}

	public ResultCode OpenInPerforce(IEnumerable<string> uris)
	{
		IList<string> list = new List<string>();
		FileStatusRequestResultCode status = m_civTechService.PrimaryProject.VersionControl.GetStatus(uris);
		if (!status.Result)
		{
			IEnumerable<string> filePaths = status.Status.Where(delegate(KeyValuePair<string, FileStatusResultCode> result)
			{
				KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
				if (keyValuePair.Value.Status.Head.Revision > 0)
				{
					keyValuePair = result;
					return keyValuePair.Value.Status.Working.Action == VersionControlActionType.Delete;
				}
				return true;
			}).Select(delegate(KeyValuePair<string, FileStatusResultCode> result)
			{
				KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
				return keyValuePair.Key;
			});
			m_civTechService.PrimaryProject.VersionControl.AddFiles(filePaths, list);
			status = m_civTechService.PrimaryProject.VersionControl.GetStatus(uris);
			if (!status.Result)
			{
				IEnumerable<string> enumerable = status.Status.Where(delegate(KeyValuePair<string, FileStatusResultCode> result)
				{
					KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
					return !keyValuePair.Value.Result;
				}).Select(delegate(KeyValuePair<string, FileStatusResultCode> result)
				{
					KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
					return keyValuePair.Value.File.DepotPath;
				});
				if (enumerable.Any())
				{
					MessageBoxes.Show(string.Format("Could not get file status\n{0}\n\nStatus of the following files could not be obtained\n\n{1}", status.Result.Message, string.Join("\n", enumerable)), "Failed to get status from version control service", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					MessageBoxes.Show("Could not get file status\n\n" + status.Result.Message, "Failed to get status from version control service", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				return new ResultCode("Failed to get status of files");
			}
		}
		PlatformAssert.If(status.Status.Count() != uris.Count(), "Collection sizes don't match and need to!");
		IEnumerable<string> enumerable2 = status.Status.Where(delegate(KeyValuePair<string, FileStatusResultCode> result)
		{
			KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
			if (keyValuePair.Value.Status.Head.Revision > 0)
			{
				keyValuePair = result;
				return keyValuePair.Value.Status.Working.Action == VersionControlActionType.Delete;
			}
			return true;
		}).Select(delegate(KeyValuePair<string, FileStatusResultCode> result)
		{
			KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
			return keyValuePair.Key;
		});
		IEnumerable<string> enumerable3 = status.Status.Where(delegate(KeyValuePair<string, FileStatusResultCode> result)
		{
			KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
			return keyValuePair.Value.Status.Working.Action == VersionControlActionType.None;
		}).Select(delegate(KeyValuePair<string, FileStatusResultCode> result)
		{
			KeyValuePair<string, FileStatusResultCode> keyValuePair = result;
			return keyValuePair.Key;
		});
		foreach (string item in enumerable2)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Opening {0} for add in Source Control.", item);
		}
		foreach (string item2 in enumerable3)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Opening {0} for edit in Source Control.", item2);
		}
		if (enumerable2.Any() && !m_civTechService.PrimaryProject.VersionControl.AddFiles(enumerable2, list))
		{
			MessageBoxes.Show(string.Join("\n", list), "Failed to add to version control service", MessageBoxButton.OK, MessageBoxImage.Error);
			return new ResultCode("Failed to add to version control service");
		}
		PlatformAssert.If(list.Any(), "Error collection is not empty and it should be!");
		if (enumerable3.Any() && !m_civTechService.PrimaryProject.VersionControl.EditFiles(enumerable3, list))
		{
			MessageBoxes.Show(string.Join("\n", list), "Failed to open for edit in version control service", MessageBoxButton.OK, MessageBoxImage.Error);
			return new ResultCode("Failed to open for edit in version control service");
		}
		return ResultCode.Success;
	}
}

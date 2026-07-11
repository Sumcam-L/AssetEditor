using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Error;
using Firaxis.VersionControl;
using Sce.Atf.Applications;

namespace DatabaseWrapper;

public static class CivTechHelper
{
	static CivTechHelper()
	{
	}

	public static void OpenSourceFile(ICivTechService civTechSvc, IImportedEntity entity)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			string message = $"Could not determine project for for entity \"{entity.Name}\" of type {EnumToStringConverter.GetNameFromType(entity.Type)}.";
			MessageBoxes.Show(message, "Cannot open source file", MessageBoxButton.OK, MessageBoxImage.Error);
			return;
		}
		IContentCreationTool contentCreationTool = ExporterService.GetContentCreationTool(entity);
		if (contentCreationTool == null)
		{
			string message2 = $"Could not find content creation tool for entity {entity.Name} of type {EnumToStringConverter.GetNameFromType(entity.Type)}.";
			MessageBoxes.Show(message2, "Cannot open source file", MessageBoxButton.OK, MessageBoxImage.Error);
			return;
		}
		IVersionControlService versionControl = project.VersionControl;
		if (versionControl.IsVersionControlled(entity.SourceFilePath) && !versionControl.GetLatest(entity.SourceFilePath, out var errMsg))
		{
			string message3 = $"Could not get latest revision of file {entity.SourceFilePath}\n\nError: {errMsg}";
			MessageBoxes.Show(message3, "Cannot open source file", MessageBoxButton.OK, MessageBoxImage.Error);
			return;
		}
		if (versionControl.IsVersionControlled(entity.SourceFilePath) && !versionControl.IsEditible(entity.SourceFilePath))
		{
			string message4 = $"Would you like to check out the file from source control? ({entity.SourceFilePath})";
			MessageBoxResult messageBoxResult = MessageBoxes.Show(message4, "Check out file?", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (messageBoxResult == MessageBoxResult.Yes && !versionControl.EditFile(entity.SourceFilePath, out var errMsg2))
			{
				string message5 = $"Could not check out the file from source control {entity.SourceFilePath}\n\nError: {errMsg2}";
				MessageBoxes.Show(message5, "Cannot checkout file", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		string localPath = versionControl.GetLocalPath(entity.SourceFilePath);
		if (File.Exists(localPath))
		{
			contentCreationTool.OpenFile(localPath);
			return;
		}
		string message6 = $"File does not exist at the path: {localPath}";
		MessageBoxes.Show(message6, "Cannot open source file", MessageBoxButton.OK, MessageBoxImage.Error);
	}

	public static EntityFileInfo[] UpdateStatus(EntityFileInfo[] entities)
	{
		if (!entities.Any())
		{
			return new EntityFileInfo[0];
		}
		IEnumerable<string> filePaths = entities.Select((EntityFileInfo ent) => CivTechRegistry.CivTechService.GetEntityPath(ent.name, ent.instanceType));
		FileStatusRequestResultCode status = CivTechRegistry.CivTechService.PrimaryProject.VersionControl.GetStatus(filePaths);
		if (!status.Result)
		{
			return new EntityFileInfo[0];
		}
		EntityFileInfo[] array = new EntityFileInfo[entities.Length];
		for (int num = 0; num < entities.Length; num++)
		{
			EntityFileInfo entityFileInfo = entities[num];
			string path = CivTechRegistry.CivTechService.GetEntityPath(entityFileInfo.name, entityFileInfo.instanceType).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string depotPath = CivTechRegistry.CivTechService.PrimaryProject.VersionControl.GetDepotPath(path);
			if (status.Status.ContainsKey(depotPath))
			{
				FileStatusResultCode fileStatusResultCode = status.Status[depotPath];
				string status2 = GenerateStatusString(fileStatusResultCode.Status);
				array[num] = new EntityFileInfo(entityFileInfo.instanceType, entityFileInfo.name, status2, fileStatusResultCode.Status.Head.Modified);
			}
			else
			{
				array[num] = new EntityFileInfo(entityFileInfo.instanceType, entityFileInfo.name, "P4 Error", DateTime.MinValue);
			}
		}
		return array;
	}

	private static string GenerateStatusString(VersionControlStatus vcs)
	{
		if (vcs.Others.Count > 0)
		{
			return "Opened by " + vcs.Others[0].Owner;
		}
		if (vcs.Working.Action != VersionControlActionType.None)
		{
			return "Opened by you";
		}
		if (vcs.Head.Revision == vcs.LocalRevision)
		{
			return "Up to date";
		}
		return "Out of date";
	}
}

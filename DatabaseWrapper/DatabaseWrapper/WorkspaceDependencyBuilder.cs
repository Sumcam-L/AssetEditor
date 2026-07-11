using System;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Collections;
using Firaxis.Utility;
using Sce.Atf;

namespace DatabaseWrapper;

public class WorkspaceDependencyBuilder : WorkspaceDependencyBase
{
	private string m_changeList;

	private T LoadSerializable<T>(string filePath, params object[] ctorParams) where T : ISerializable, IAssemblyInstance
	{
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>(ctorParams);
		lock (civTechContext)
		{
			T result = civTechContext.CreateInstance<T>(ctorParams);
			if (!result.DeserializeFromFile(filePath))
			{
				result.Dispose();
				return default(T);
			}
			return result;
		}
	}

	public WorkspaceDependencyBuilder(IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, string targetProject, string changelist, bool testFilesExist)
		: base(projMapSvc, projCfgSvc, targetProject, testFilesExist)
	{
		m_changeList = changelist;
	}

	public override bool UpdateDependencies(IDatabaseDependencies workspaceDependencies)
	{
		if (!uint.TryParse(m_changeList, out var result))
		{
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Normal, "Failed to parse changelist from \"{0}\"", m_changeList);
			return false;
		}
		workspaceDependencies.Changelist = result;
		workspaceDependencies.Timestamp = DateTime.Now.ToFileTimeUtc();
		int totalAddFiles = 0;
		int totalChaseFiles = 0;
		float num = 0f;
		float num2 = 0f;
		IInstanceSet instSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.ProjectMapService.GetActivePantryPaths() });
		try
		{
			num += TimedOperation.Do(delegate
			{
				IEnumerableExtensions.ForEach(Enum.GetValues(typeof(InstanceType)).OfType<InstanceType>(), delegate(InstanceType insType)
				{
					if (insType != InstanceType.IT_INVALID && insType != InstanceType.IT_COUNT)
					{
						string searchPattern = "*" + StaticMethods.ExtensionForInstanceType(insType);
						string path = StaticMethods.PantryRootForInstanceType(base.PrimaryPantryRoot, insType);
						if (Directory.Exists(path))
						{
							IEnumerableExtensions.ForEach(Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories), delegate(string insPath)
							{
								float num3 = TimedOperation.Do(delegate
								{
									IInstanceEntity instanceEntity = instSet.LoadEntityByPath(insPath, insType);
									if (instanceEntity != null)
									{
										AddRootFileIfNew(insPath, insType, instanceEntity.ClassName, FileStatus.Normal, workspaceDependencies);
										int num4 = totalAddFiles + 1;
										totalAddFiles = num4;
									}
								});
								Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Adding {1} took {2} seconds", base.TargetProject, GetDepotRootedPath(insPath), num3);
							});
						}
					}
				});
			});
			num += TimedOperation.Do(delegate
			{
				if (Directory.Exists(base.ArtDefRoot))
				{
					IEnumerableExtensions.ForEach(Directory.EnumerateFiles(base.ArtDefRoot, "*.artdef", SearchOption.AllDirectories), delegate(string artDefPath)
					{
						float num3 = TimedOperation.Do(delegate
						{
							IArtDef artDef = LoadSerializable<IArtDef>(artDefPath, new object[1] { base.ProjectConfigService.Config });
							if (artDef != null)
							{
								AddRootFileIfNew(artDefPath, FileType.ArtDef, FileStatus.Normal, workspaceDependencies);
								int num4 = totalAddFiles + 1;
								totalAddFiles = num4;
							}
						});
						Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Adding {1} took {2} seconds", base.TargetProject, GetDepotRootedPath(artDefPath), num3);
					});
				}
			});
			num += TimedOperation.Do(delegate
			{
				if (Directory.Exists(base.XLPRoot))
				{
					IEnumerableExtensions.ForEach(Directory.EnumerateFiles(base.XLPRoot, "*.xlp", SearchOption.AllDirectories), delegate(string xlpPath)
					{
						float num3 = TimedOperation.Do(delegate
						{
							IXLP iXLP = LoadSerializable<IXLP>(xlpPath, new object[0]);
							if (iXLP != null)
							{
								AddRootFileIfNew(xlpPath, FileType.XLP, FileStatus.Normal, workspaceDependencies);
								int num4 = totalAddFiles + 1;
								totalAddFiles = num4;
							}
						});
						Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Adding {1} took {2} seconds", base.TargetProject, GetDepotRootedPath(xlpPath), num3);
					});
				}
			});
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "\n{0}: Adding {1} files took {2} seconds\n", base.TargetProject, totalAddFiles, num);
		}
		finally
		{
			if (instSet != null)
			{
				instSet.Dispose();
			}
		}
		IInstanceSet instSet2 = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.ProjectMapService.GetActivePantryPaths() });
		try
		{
			num2 += TimedOperation.Do(delegate
			{
				workspaceDependencies.Files.ForEachValue(delegate(DepotFileInfo dfi)
				{
					float num3 = TimedOperation.Do(delegate
					{
						if (dfi.Type == 0)
						{
							AddArtDefDependencies(dfi.Filename, instSet2, workspaceDependencies, ContainerCrawlOptions.DoBuild);
							int num4 = totalChaseFiles + 1;
							totalChaseFiles = num4;
						}
						else if (dfi.Type == 1)
						{
							int procdDeps = 0;
							float num5 = TimedOperation.Do(delegate
							{
								AddXLPDependencies(dfi.Filename, instSet2, workspaceDependencies, ContainerCrawlOptions.DoBuild, ref procdDeps);
							});
							int num4 = totalChaseFiles + 1;
							totalChaseFiles = num4;
						}
						else if (dfi.Type == 2)
						{
							int procdDeps2 = 0;
							float num6 = TimedOperation.Do(delegate
							{
								AddEntityDependencies((InstanceType)dfi.EntityType, dfi.Filename, instSet2, workspaceDependencies, ContainerCrawlOptions.DoBuild, ref procdDeps2);
							});
							int num4 = totalChaseFiles + 1;
							totalChaseFiles = num4;
						}
					});
				});
			});
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "\n{0}: Processing {1} files took {2} seconds\n", base.TargetProject, totalChaseFiles, num2);
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Building environment catalog for {1} files took {2} seconds", base.TargetProject, totalAddFiles + totalChaseFiles, num + num2);
		}
		finally
		{
			if (instSet2 != null)
			{
				instSet2.Dispose();
			}
		}
		return true;
	}
}

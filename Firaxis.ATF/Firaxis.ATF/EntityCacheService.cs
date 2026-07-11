using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firaxis.AssetCloudFramework.Data;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Threading;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.ATF;

[Export(typeof(IEntityCacheService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntityCacheService : IEntityCacheService, IDisposable
{
	private class CachedEntityData : IEntityCacheData
	{
		public string Name { get; private set; }

		public string Class { get; set; }

		public string Project { get; private set; }

		public IEnumerable<string> Tags { get; set; }

		public CachedEntityData(string project, string name, string className, IEnumerable<string> tags)
		{
			Name = name;
			Class = className;
			Project = project;
			Tags = tags.ToArray();
		}

		public CachedEntityData(IEntityCacheData other)
		{
			Name = other.Name;
			Class = other.Class;
			Project = other.Project;
			Tags = other.Tags.ToArray();
		}
	}

	private bool disposedValue;

	private ICivTechService CivTechService { get; set; }

	private IProjectSelectionService ProjectSelectionService { get; set; }

	private ICollection<IWorkspaceDependencyWatcher> RegistryWatchers { get; set; } = new List<IWorkspaceDependencyWatcher>();

	private IDictionary<EntityID, ICollection<CachedEntityData>> EntityDataCache { get; set; } = new Dictionary<EntityID, ICollection<CachedEntityData>>();

	private IDictionary<string, ISet<EntityID>> SeenEntities { get; set; } = new Dictionary<string, ISet<EntityID>>();

	private ReaderWriterLockSlim EntityDataLock { get; set; } = new ReaderWriterLockSlim();

	private ReaderWriterLockSlim SeenEntityLock { get; set; } = new ReaderWriterLockSlim();

	[ImportingConstructor]
	public EntityCacheService(ICivTechService civTechSvc, IProjectSelectionService pss)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			CivTechService = civTechSvc;
			ProjectSelectionService = pss;
			UpdateEntityMap();
			foreach (ProjectEnvironment project in CivTechService.ActiveProjectMap.Projects)
			{
				IWorkspaceDependencyWatcher workspaceDependencyWatcher = project.DependencyRegistry.As<IWorkspaceDependencyWatcher>();
				if (workspaceDependencyWatcher != null)
				{
					workspaceDependencyWatcher.WorkspaceItemAdded += HandleWorkspaceItemAdded;
					workspaceDependencyWatcher.WorkspaceItemRemoved += HandleWorkspaceItemRemoved;
					workspaceDependencyWatcher.WorkspaceItemChanged += HandleWorkspaceItemChanged;
					workspaceDependencyWatcher.WorkspaceItemRenamed += HandleWorkspaceItemRenamed;
					RegistryWatchers.Add(workspaceDependencyWatcher);
				}
			}
		}
		Context.Add(this);
	}

	public void HandleProjectChange()
	{
		UpdateEntityMap();
	}

	public IQueryService GetDependents(string entityName, InstanceType entityType)
	{
		DependencyService dependencyService = new DependencyService();
		if (Uri.TryCreate(CivTechService.GetEntityPath(entityName, entityType), UriKind.Absolute, out var result))
		{
			foreach (Uri dependent in CivTechService.PrimaryProject.DependencyRegistry.GetDependents(result))
			{
				EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, dependent.LocalPath);
				dependencyService.InstanceItems[entityIDFromPath.Type].Add(entityIDFromPath.Name);
			}
		}
		return dependencyService;
	}

	public IEnumerable<EntityID> GetAllEntities()
	{
		using (new ScopedReaderLock(EntityDataLock))
		{
			return EntityDataCache.Keys.ToArray();
		}
	}

	public IEnumerable<string> GetAllTags()
	{
		ISet<string> set = new SortedSet<string>();
		using (new ScopedReaderLock(EntityDataLock))
		{
			foreach (ICollection<CachedEntityData> value in EntityDataCache.Values)
			{
				foreach (IEntityCacheData item in (IEnumerable<IEntityCacheData>)value)
				{
					foreach (string tag in item.Tags)
					{
						set.Add(tag);
					}
				}
			}
			return set;
		}
	}

	public IEnumerable<IEntityCacheData> GetCacheData(EntityID entityID)
	{
		List<IEntityCacheData> list = new List<IEntityCacheData>();
		using (new ScopedReaderLock(EntityDataLock))
		{
			if (EntityDataCache.TryGetValue(entityID, out var value))
			{
				foreach (CachedEntityData item in value)
				{
					list.Add(new CachedEntityData(item));
				}
			}
		}
		return list;
	}

	public IEnumerable<IEntityCacheData> GetCacheData(IEnumerable<EntityID> entityIDs)
	{
		List<IEntityCacheData> list = new List<IEntityCacheData>();
		using (new ScopedReaderLock(EntityDataLock))
		{
			foreach (EntityID entityID in entityIDs)
			{
				if (!EntityDataCache.TryGetValue(entityID, out var value))
				{
					continue;
				}
				foreach (CachedEntityData item in value)
				{
					list.Add(new CachedEntityData(item));
				}
			}
			return list;
		}
	}

	public IEnumerable<string> FindFilesByType(InstanceType type)
	{
		using (new ScopedReaderLock(EntityDataLock))
		{
			return (from id in EntityDataCache.Keys
				where id.Type == type
				select id.Name).ToArray();
		}
	}

	private void HandleWorkspaceItemAdded(object sender, WorkspaceItemChangedEvent e)
	{
		AddFileToCache(e.Uri);
	}

	private void HandleWorkspaceItemRemoved(object sender, WorkspaceItemChangedEvent e)
	{
		RemoveFileFromCache(e.Uri);
	}

	private void HandleWorkspaceItemRenamed(object sender, WorkspaceItemRenamedEvent e)
	{
		RemoveFileFromCache(e.OldUri);
		AddFileToCache(e.Uri);
	}

	private void HandleWorkspaceItemChanged(object sender, WorkspaceItemChangedEvent e)
	{
		BugSubmitter.SilentAssert(CivTechService != null, "CivTechService was null during EntityCacheService.HandleWorkspaceItemChanged({0}, {1}) @summary CivTechService was null while EntityCacheService was handling HandleWorkspaceItemChanged @assign bwhitman", sender?.GetType(), e.ToString());
		if (CivTechService == null)
		{
			return;
		}
		BugSubmitter.SilentAssert(CivTechService.PrimaryProject != null, "CivTechService.PrimaryProject was null during EntityCacheService.HandleWorkspaceItemChanged({0}, {1}) @summary CivTechService.PrimaryProject was null while EntityCacheService was handling HandleWorkspaceItemChanged @assign bwhitman", sender?.GetType(), e.ToString());
		if (CivTechService.PrimaryProject == null)
		{
			return;
		}
		BugSubmitter.SilentAssert(CivTechService.PrimaryProject.DependencyRegistry != null, "CivTechService.PrimaryProject.DependencyRegistry was null during EntityCacheService.HandleWorkspaceItemChanged({0}, {1}) @summary CivTechService.PrimaryProject.DependencyRegistry was null while EntityCacheService was handling HandleWorkspaceItemChanged @assign bwhitman", sender?.GetType(), e.ToString());
		if (CivTechService.PrimaryProject.DependencyRegistry != null)
		{
			Uri uri = e.Uri;
			DepotFileInfo info = default(DepotFileInfo);
			if (CivTechService.PrimaryProject.DependencyRegistry.GetFileInfo(uri, ref info) && info.Type == 2 && info.Status == 0)
			{
				EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, uri.LocalPath);
				string projectName = CivTechService.GetProjectName(uri);
				UpdateCachedEntity(projectName, entityIDFromPath, info);
			}
		}
	}

	private void AddEntityToCache(EntityID entityID, DepotFileInfo fileInfo)
	{
		string entityPath = CivTechService.GetEntityPath(entityID.Name, entityID.Type);
		if (Uri.TryCreate(entityPath, UriKind.Absolute, out var result))
		{
			CachedEntityData entityData = new CachedEntityData(CivTechService.GetProjectName(result), entityID.Name, fileInfo.EntityClass, fileInfo.Tags);
			AddToCache_Locking(entityID, entityData);
		}
		else
		{
			BugSubmitter.SilentReport("EntityCacheService failed to create a Uri for path \"" + entityPath + "\" @summary EntityCacheService failed to create a Uri  @assign bwhitman");
		}
	}

	private void RemoveEntity(string projectName, EntityID entityID)
	{
		using (new ScopedWriterLock(EntityDataLock))
		{
			if (EntityDataCache.TryGetValue(entityID, out var value))
			{
				CachedEntityData cachedEntityData = value.FirstOrDefault((CachedEntityData x) => x.Project.Equals(projectName, StringComparison.CurrentCultureIgnoreCase));
				if (cachedEntityData != null)
				{
					value.Remove(cachedEntityData);
				}
			}
		}
		using (new ScopedWriterLock(SeenEntityLock))
		{
			if (SeenEntities.TryGetValue(projectName, out var value2))
			{
				value2.Remove(entityID);
			}
		}
	}

	private void AddFileToCache(Uri uri)
	{
		DepotFileInfo info = default(DepotFileInfo);
		if (CivTechService.PrimaryProject.DependencyRegistry.GetFileInfo(uri, ref info) && info.Type == 2 && info.Status != 1)
		{
			EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, uri.LocalPath);
			bool flag = false;
			using (new ScopedReaderLock(SeenEntityLock))
			{
				flag = SeenEntities[CivTechService.PrimaryProject.Name].Add(entityIDFromPath);
			}
			if (flag)
			{
				AddEntityToCache(entityIDFromPath, info);
			}
		}
	}

	private void RemoveFileFromCache(Uri uri)
	{
		EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, uri.LocalPath);
		if (entityIDFromPath.Type != InstanceType.IT_INVALID && entityIDFromPath.Type != InstanceType.IT_COUNT)
		{
			string projectName = CivTechService.GetProjectName(uri);
			RemoveEntity(projectName, entityIDFromPath);
		}
	}

	private void UpdateCachedEntity(string projectName, EntityID entityID, DepotFileInfo fileInfo)
	{
		using (new ScopedWriterLock(EntityDataLock))
		{
			if (EntityDataCache.TryGetValue(entityID, out var value))
			{
				CachedEntityData cachedEntityData = value.FirstOrDefault((CachedEntityData x) => x.Project.Equals(projectName, StringComparison.CurrentCultureIgnoreCase));
				if (cachedEntityData != null)
				{
					cachedEntityData.Class = fileInfo.EntityClass;
					cachedEntityData.Tags = fileInfo.Tags.ToArray();
				}
			}
		}
	}

	private void UpdateEntityMap()
	{
		using (new ScopedWriterLock(EntityDataLock))
		{
			EntityDataCache.Clear();
		}
		using (new ScopedWriterLock(SeenEntityLock))
		{
			SeenEntities.Clear();
			foreach (string projectName2 in CivTechService.ActiveProjectMap.ProjectNames)
			{
				SeenEntities[projectName2] = new HashSet<EntityID>();
			}
		}
		foreach (ProjectEnvironment project in CivTechService.ActiveProjectMap.Projects)
		{
			IWorkspaceDependencyRegistry depReg = project.DependencyRegistry;
			Parallel.ForEach(depReg.GetFiles(), delegate(Uri uri)
			{
				DepotFileInfo info = default(DepotFileInfo);
				if (depReg.GetFileInfo(uri, ref info) && info.Type == 2 && info.Status != 1)
				{
					EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, uri.LocalPath);
					string projectName = CivTechService.GetProjectName(uri);
					bool flag = false;
					using (new ScopedWriterLock(SeenEntityLock))
					{
						flag = SeenEntities.ContainsKey(projectName) && SeenEntities[projectName].Add(entityIDFromPath);
					}
					if (flag)
					{
						CachedEntityData entityData = new CachedEntityData(projectName, entityIDFromPath.Name, info.EntityClass, info.Tags);
						AddToCache_Locking(entityIDFromPath, entityData);
					}
				}
			});
		}
	}

	private void AddToCache_Locking(EntityID entityID, CachedEntityData entityData)
	{
		using (new ScopedWriterLock(EntityDataLock))
		{
			AddToCache_NonLocking(entityID, entityData);
		}
	}

	private void AddToCache_NonLocking(EntityID entityID, CachedEntityData entityData)
	{
		if (!EntityDataCache.TryGetValue(entityID, out var value))
		{
			value = new List<CachedEntityData>();
			EntityDataCache.Add(entityID, value);
		}
		value.Add(entityData);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			foreach (IWorkspaceDependencyWatcher registryWatcher in RegistryWatchers)
			{
				registryWatcher.WorkspaceItemAdded -= HandleWorkspaceItemAdded;
				registryWatcher.WorkspaceItemRemoved -= HandleWorkspaceItemRemoved;
				registryWatcher.WorkspaceItemChanged -= HandleWorkspaceItemChanged;
				registryWatcher.WorkspaceItemRenamed -= HandleWorkspaceItemRenamed;
			}
			RegistryWatchers.Clear();
			using (new ScopedWriterLock(EntityDataLock))
			{
				foreach (ICollection<CachedEntityData> value in EntityDataCache.Values)
				{
					value.Clear();
				}
				EntityDataCache.Clear();
			}
			using (new ScopedWriterLock(SeenEntityLock))
			{
				foreach (ISet<EntityID> value2 in SeenEntities.Values)
				{
					value2.Clear();
				}
				SeenEntities.Clear();
			}
			EntityDataLock.Dispose();
			SeenEntityLock.Dispose();
			Context.Remove(this);
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}

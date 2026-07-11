using System;
using System.ComponentModel.Composition;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

[Export(typeof(CivTechRegistry))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CivTechRegistry : IPartImportsSatisfiedNotification
{
	private static Lazy<ICivTechService> s_civTechService;

	private static Lazy<IEntityCacheService> s_entityCacheService;

	private static Lazy<IEntityQueryService> s_entityQueryService;

	private static Lazy<IEntityFilteringService> s_entityFilteringService;

	private static Lazy<IArtDefRegistry> s_artDefRegistry;

	private static Lazy<IXLPRegistry> s_xlpRegistry;

	private static Lazy<IVersionControlSelectionService> s_versionControlSelectionService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<ICivTechService> m_civTechService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IArtDefRegistry> m_artDefRegistry;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IXLPRegistry> m_xlpRegistry;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IEntityCacheService> m_entityCacheService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IEntityQueryService> m_entityQueryService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IEntityFilteringService> m_entityFilteringService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IVersionControlSelectionService> m_versionControlSelectionService;

	public static ICivTechService CivTechService => s_civTechService.Value;

	public static IArtDefRegistry ArtDefRegistry => s_artDefRegistry.Value;

	public static IXLPRegistry XLPRegistry => s_xlpRegistry.Value;

	public static IEntityCacheService EntityCacheService => s_entityCacheService.Value;

	public static IEntityQueryService EntityQueryService => s_entityQueryService.Value;

	public static IEntityFilteringService EntityFilteringService => s_entityFilteringService.Value;

	public static IVersionControlSelectionService VersionControlSelectionService => s_versionControlSelectionService.Value;

	static CivTechRegistry()
	{
	}

	public virtual void OnImportsSatisfied()
	{
		s_civTechService = m_civTechService;
		s_artDefRegistry = m_artDefRegistry;
		s_xlpRegistry = m_xlpRegistry;
		s_entityCacheService = m_entityCacheService;
		s_entityQueryService = m_entityQueryService;
		s_entityFilteringService = m_entityFilteringService;
		s_versionControlSelectionService = m_versionControlSelectionService;
	}
}

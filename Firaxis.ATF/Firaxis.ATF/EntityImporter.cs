using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

public class EntityImporter
{
	private readonly CivTechContext m_context;

	private readonly ICivTechService m_civTechService;

	private readonly AssetBrowserFileCommands m_fileCommands;

	private readonly IEnumerable<EntityID> m_entitiesToImport;

	private readonly IImportService m_importService;

	private readonly IDocumentRegistryMediator m_registryMediator;

	private readonly BatchEntitySourceControlService m_sourceControl;

	public EntityImporter(ICivTechService civTechSvc, IImportService importService, AssetBrowserFileCommands fileCommands, IDocumentRegistryMediator registryMediator, BatchEntitySourceControlService sourceControl, IEnumerable<EntityID> entitiesToImport, bool recurseIntoChildren)
	{
		m_context = Context.EnsureCreated<CivTechContext>();
		m_civTechService = civTechSvc;
		m_importService = importService;
		m_fileCommands = fileCommands;
		m_registryMediator = registryMediator;
		m_sourceControl = sourceControl;
		if (recurseIntoChildren)
		{
			m_entitiesToImport = GetEntitiesToImport(entitiesToImport);
		}
		else
		{
			m_entitiesToImport = entitiesToImport;
		}
		m_entitiesToImport = m_entitiesToImport.Where((EntityID entId) => StaticMethods.IsImportableType(entId.Type)).ToArray();
	}

	public void Import()
	{
		bool shadowMode = m_registryMediator.ShadowMode;
		ICollection<EntityID> collection = new List<EntityID>();
		IEnumerable<IImportableDocument> matchingOpenDocuments = GetMatchingOpenDocuments(collection);
		m_registryMediator.ShadowMode = true;
		IEnumerable<EntityID> entitiesToOpen = m_entitiesToImport.Except(collection);
		IEnumerable<IImportableDocument> enumerable = OpenShadowDocuments(entitiesToOpen);
		IEnumerable<IImportableDocument> enumerable2 = from doc in matchingOpenDocuments.Union(enumerable)
			where !doc.IsReadOnly
			select doc;
		if (enumerable2.Any())
		{
			IEnumerable<IInstanceEntity> enumerable3 = (from doc in enumerable2
				select doc.InstanceEntity into ent
				where ent != null
				select ent).ToArray();
			if (enumerable3.Any() && (bool)m_sourceControl.OpenInPerforce(enumerable3))
			{
				m_importService.Import(enumerable2);
				enumerable2.ForEach(delegate(IImportableDocument doc)
				{
					m_fileCommands.Save(doc);
				});
			}
		}
		enumerable.ForEach(delegate(IImportableDocument doc)
		{
			m_fileCommands.Close(doc);
		});
		m_registryMediator.ShadowMode = shadowMode;
	}

	private void AddToEntitiesToImport(ISet<EntityID> entitiesToImport, EntityID entityID, IInstanceSet instanceSet)
	{
		if (entitiesToImport.Add(entityID))
		{
			instanceSet.LoadEntityByName(entityID.Name, entityID.Type)?.CrawlCooktimeDependencies(delegate(InstanceType type, string name)
			{
				EntityID entityID2 = new EntityID(name, type);
				AddToEntitiesToImport(entitiesToImport, entityID2, instanceSet);
			});
		}
	}

	private IEnumerable<EntityID> GetEntitiesToImport(IEnumerable<EntityID> entities)
	{
		ISet<EntityID> set = new HashSet<EntityID>();
		using IInstanceSet instanceSet = m_context.CreateInstance<IInstanceSet>(new object[1] { m_civTechService.GetActivePantryPaths() });
		foreach (EntityID entity in entities)
		{
			AddToEntitiesToImport(set, entity, instanceSet);
		}
		return set;
	}

	private IEnumerable<IImportableDocument> GetMatchingOpenDocuments(ICollection<EntityID> openedEntities)
	{
		ICollection<IImportableDocument> collection = new HashSet<IImportableDocument>();
		IDocumentRegistryProvider registryMediator = m_registryMediator;
		for (int i = 0; i < 2; i++)
		{
			IEnumerable<IImportableDocument> source = registryMediator.DocumentRegistry.Documents.OfType<IImportableDocument>().ToArray();
			foreach (EntityID id in m_entitiesToImport)
			{
				foreach (IImportableDocument item in source.Where((IImportableDocument doc) => doc.InstanceEntity.Type == id.Type && doc.InstanceEntity.Name == id.Name))
				{
					collection.Add(item);
					openedEntities.Add(id);
				}
			}
			m_registryMediator.ShadowMode = !m_registryMediator.ShadowMode;
		}
		return collection;
	}

	private IEnumerable<IImportableDocument> OpenShadowDocuments(IEnumerable<EntityID> entitiesToOpen)
	{
		ICollection<IImportableDocument> collection = new List<IImportableDocument>();
		foreach (EntityID item2 in entitiesToOpen)
		{
			IDocument document = m_fileCommands.OpenExistingShadowDocument(item2.Type, item2.Name);
			if (document is IImportableDocument item)
			{
				collection.Add(item);
			}
			else
			{
				m_fileCommands.Close(document);
			}
		}
		return collection;
	}
}

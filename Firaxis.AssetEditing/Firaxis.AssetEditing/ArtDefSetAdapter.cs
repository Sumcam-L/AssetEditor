using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefSetAdapter : DomNodeAdapter, IArtDefSetContext, IFieldSerializer, IDisposable
{
	private IList<ArtDefCollectionAdapter> m_rootCollections;

	private readonly IDictionary<string, ArtDefCollectionAdapter> m_rootCollectionMap = new Dictionary<string, ArtDefCollectionAdapter>();

	private IArtDefTemplate m_artDefTemplate;

	private bool disposedValue;

	public string TemplateName
	{
		get
		{
			return GetAttribute<string>(ArtDefSchema.ArtDefType.TemplateAttribute);
		}
		set
		{
			if (TemplateName == value)
			{
				return;
			}
			base.DomNode.As<ArtDefSetAdapter>();
			ArtDefContext context = base.DomNode.As<ArtDefContext>();
			ArtDefDocument artDefDoc = base.DomNode.As<ArtDefDocument>();
			context.DoTransaction(delegate
			{
				while (m_rootCollections.Count > 0)
				{
					m_rootCollections.RemoveAt(0);
				}
				SetAttribute(ArtDefSchema.ArtDefType.TemplateAttribute, value);
				IArtDefTemplate artDefTmpl = ArtDefContext.FindArtDefTemplate(artDefDoc.CivTechService.PrimaryProject.Config, value);
				DomApplyTemplateChanges(artDefTmpl);
			}, "Change Template Type".Localize());
		}
	}

	public string Description
	{
		get
		{
			return GetAttribute<string>(ArtDefSchema.ArtDefType.DescriptionAttribute);
		}
		set
		{
			SetAttribute(ArtDefSchema.ArtDefType.DescriptionAttribute, value);
		}
	}

	public IEnumerable<ArtDefCollectionAdapter> RootCollections => m_rootCollections;

	public IArtDef ArtDef { get; private set; }

	public IArtDefTemplate ArtDefTemplate
	{
		get
		{
			return m_artDefTemplate;
		}
		private set
		{
			if (m_artDefTemplate != value)
			{
				m_artDefTemplate = value;
				if (m_artDefTemplate != null)
				{
					Description = m_artDefTemplate.Description;
				}
			}
		}
	}

	public ISerializable FieldSerializer => ArtDef;

	public event EventHandler<ItemChangedEventArgs<string>> TemplateChanged;

	private void RaiseTemplateChanged(string newName)
	{
		this.TemplateChanged?.Invoke(this, new ItemChangedEventArgs<string>(newName));
	}

	public void DomApplyTemplateChanges(IArtDefTemplate artDefTmpl)
	{
		DomUpdateRootCollectionsFromTemplate(artDefTmpl);
		foreach (ArtDefCollectionAdapter artDefColAdapter in RootCollections)
		{
			IArtDefElementTemplate artDefElemTmpl = artDefTmpl.Collections.FirstOrDefault((IArtDefElementTemplate adet) => adet.Name == artDefColAdapter.Name);
			artDefColAdapter.DomApplyTemplateChanges(artDefTmpl, artDefElemTmpl);
		}
	}

	private IEnumerable<Pair<string, IArtDefElementTemplate>> BuildAddedCollectionList(IArtDefTemplate artDefTmpl)
	{
		foreach (IArtDefElementTemplate colTmpl in artDefTmpl.Collections)
		{
			if (RootCollections.FirstOrDefault((ArtDefCollectionAdapter col) => col.Name == colTmpl.Name) == null)
			{
				yield return new Pair<string, IArtDefElementTemplate>(colTmpl.Name, colTmpl);
			}
		}
	}

	private IEnumerable<string> BuildRemovedCollectionList(IArtDefTemplate artDefTmpl)
	{
		foreach (ArtDefCollectionAdapter col in RootCollections)
		{
			if (artDefTmpl.Collections.FirstOrDefault((IArtDefElementTemplate colTmpl) => colTmpl.Name == col.Name) == null)
			{
				yield return col.Name;
			}
		}
	}

	private void DomUpdateRootCollectionsFromTemplate(IArtDefTemplate artDefTmpl)
	{
		if (artDefTmpl == ArtDefTemplate)
		{
			BuildRemovedCollectionList(artDefTmpl).ToArray().ForEach(delegate(string colName)
			{
				RemoveCollection(colName);
			});
		}
		else
		{
			m_rootCollections.Clear();
		}
		BuildAddedCollectionList(artDefTmpl).ToArray().ForEach(delegate(Pair<string, IArtDefElementTemplate> colInfo)
		{
			AddCollection(colInfo.First, colInfo.Second);
		});
	}

	private void AddCollection(string name, IArtDefElementTemplate elemTmpl)
	{
		DomNode domNode = new DomNode(ArtDefSchema.ArtDefCollectionType.Type);
		domNode.InitializeExtensions();
		ArtDefCollectionAdapter artDefCollectionAdapter = domNode.As<ArtDefCollectionAdapter>();
		artDefCollectionAdapter.Name = name;
		m_rootCollectionMap[name] = artDefCollectionAdapter;
		m_rootCollections.Add(artDefCollectionAdapter);
		artDefCollectionAdapter.ArtDef = ArtDef;
		artDefCollectionAdapter.ArtDefElementsTemplate = elemTmpl;
		artDefCollectionAdapter.ArtDefTemplate = ArtDefTemplate;
	}

	private void RemoveCollection(string name)
	{
		if (m_rootCollectionMap.TryGetValue(name, out var value))
		{
			m_rootCollectionMap.Remove(name);
			m_rootCollections.Remove(value);
		}
	}

	public void NativeApplyTemplateChanges(IArtDefTemplate artDefTmpl)
	{
		UnregisterForDomNotifications();
		ArtDef.UpdateRootCollectionsFromTemplate(artDefTmpl);
		Update(ArtDef, artDefTmpl, initialUpdate: false);
		foreach (ArtDefCollectionAdapter artDefColAdapter in m_rootCollections)
		{
			IArtDefElementTemplate artDefElemTmpl = artDefTmpl.Collections.FirstOrDefault((IArtDefElementTemplate adet) => adet.Name == artDefColAdapter.Name);
			artDefColAdapter.NativeApplyTemplateChanges(artDefTmpl, artDefElemTmpl);
		}
		RegisterForDomNotifications();
	}

	public void Update(IArtDef artDef, IArtDefTemplate artDefTemplate, bool initialUpdate)
	{
		ArtDef = artDef;
		ArtDefTemplate = artDefTemplate;
		if (TemplateName != artDef.ArtDefTemplate || initialUpdate)
		{
			SetAttribute(ArtDefSchema.ArtDefType.TemplateAttribute, artDef.ArtDefTemplate);
		}
		UnregisterForDomNotifications();
		List<ArtDefCollectionAdapter> list = new List<ArtDefCollectionAdapter>();
		string[] templateCollectionNames = artDefTemplate.Collections.Select((IArtDefElementTemplate x) => x.Name).ToArray();
		foreach (IArtDefCollection col in artDef.RootCollections.OrderBy((IArtDefCollection c) => templateCollectionNames.IndexOf(c.CollectionName)))
		{
			IArtDefElementTemplate artDefElementsTemplate = artDefTemplate.Collections.FirstOrDefault((IArtDefElementTemplate t) => t.Name == col.CollectionName);
			if (!m_rootCollectionMap.TryGetValue(col.CollectionName, out var value))
			{
				DomNode domNode = new DomNode(ArtDefSchema.ArtDefCollectionType.Type);
				domNode.InitializeExtensions();
				value = domNode.As<ArtDefCollectionAdapter>();
				value.ArtDef = artDef;
				value.ArtDefCollection = col;
				value.Name = col.CollectionName;
				value.ArtDefElementsTemplate = artDefElementsTemplate;
				value.ArtDefTemplate = artDefTemplate;
				m_rootCollectionMap[col.CollectionName] = value;
				m_rootCollections.Add(value);
			}
			else
			{
				value.ArtDef = artDef;
				value.ArtDefCollection = col;
				value.Name = col.CollectionName;
				value.ArtDefElementsTemplate = artDefElementsTemplate;
				value.ArtDefTemplate = artDefTemplate;
			}
			value.Update();
			list.Add(value);
		}
		ArtDefCollectionAdapter[] array = m_rootCollections.Except(list).ToArray();
		foreach (ArtDefCollectionAdapter artDefCollectionAdapter in array)
		{
			m_rootCollectionMap.Remove(artDefCollectionAdapter.Name);
			m_rootCollections.Remove(artDefCollectionAdapter);
		}
		RegisterForDomNotifications();
	}

	protected override void OnNodeSet()
	{
		m_rootCollections = new DomNodeListAdapter<ArtDefCollectionAdapter>(base.DomNode, ArtDefSchema.ArtDefType.RootCollectionChild);
		RegisterForDomNotifications();
		base.OnNodeSet();
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == ArtDefSchema.ArtDefType.RootCollectionChild)
		{
			ArtDefCollectionAdapter artDefCollectionAdapter = e.Child.As<ArtDefCollectionAdapter>();
			ArtDef.RemoveCollection(artDefCollectionAdapter.Name);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == ArtDefSchema.ArtDefType.RootCollectionChild)
		{
			ArtDefCollectionAdapter artDefCollectionAdapter = e.Child.As<ArtDefCollectionAdapter>();
			artDefCollectionAdapter.ArtDefCollection = ArtDef.AddCollection(artDefCollectionAdapter.Name);
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == ArtDefSchema.ArtDefType.TemplateAttribute && !string.IsNullOrEmpty(TemplateName))
		{
			string templateName = TemplateName;
			if (templateName != ArtDef.ArtDefTemplate)
			{
				base.DomNode.As<ArtDefDocument>();
				ArtDef.ArtDefTemplate = templateName;
				RaiseTemplateChanged(templateName);
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			if (ArtDef != null)
			{
				ArtDef.Dispose();
				ArtDef = null;
			}
			m_rootCollectionMap.Clear();
			UnregisterForDomNotifications();
		}
		m_artDefTemplate = null;
		m_rootCollections = null;
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(ICookableRegistry))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CookableRegistry : ICookableRegistry
{
	private readonly ICivTechService m_civTechService;

	private IDictionary<Uri, RegisteredCookableInfo> m_cookables = new Dictionary<Uri, RegisteredCookableInfo>();

	private ISet<Uri> m_dependenciesProcessed = new HashSet<Uri>();

	private IDocumentRegistry m_documentRegistry;

	public IEnumerable<Uri> Cookables => from Val in m_cookables.Values
		where Val.IsCookable()
		select Val.Uri;

	[ImportingConstructor]
	public CookableRegistry(IDocumentRegistry documentRegistry, ICivTechService civTechSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_documentRegistry = documentRegistry;
			m_civTechService = civTechSvc;
			m_documentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
			m_documentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
		}
	}

	public void EnableCooking(Uri cookableUri, bool enabled)
	{
		if (m_cookables.ContainsKey(cookableUri))
		{
			m_cookables[cookableUri].CookEnabled = enabled;
		}
	}

	public bool IsCookingEnabled(Uri cookableUri)
	{
		if (!m_cookables.ContainsKey(cookableUri))
		{
			return false;
		}
		return m_cookables[cookableUri].CookEnabled;
	}

	private void AddCookable(Uri uri, Uri referrer)
	{
		AddCookableOrReferrer(uri, referrer);
		AddCookableDependents(uri, referrer);
	}

	private void AddCookableDependents(Uri uri, Uri referrer)
	{
		if (m_dependenciesProcessed.Contains(uri))
		{
			return;
		}
		m_dependenciesProcessed.Add(uri);
		foreach (Uri dependent in m_civTechService.PrimaryProject.DependencyRegistry.GetDependents(uri))
		{
			AddCookable(dependent, uri);
		}
	}

	private void AddCookableOrReferrer(Uri uri, Uri referrer)
	{
		if (!IsPrimaryConnectionFor(uri, referrer))
		{
			if (!m_cookables.ContainsKey(uri))
			{
				FileType fileType = m_civTechService.PrimaryProject.DependencyRegistry.GetFileType(uri);
				m_cookables[uri] = new RegisteredCookableInfo(fileType, uri);
			}
			m_cookables[uri].Referrers.Add(referrer);
		}
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		AddCookable(e.Item.Uri, e.Item.Uri);
	}

	private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		RemoveCookable(e.Item.Uri, e.Item.Uri);
	}

	private bool IsPrimaryConnectionFor(Uri a, Uri b)
	{
		if (a == b)
		{
			return false;
		}
		if (!m_cookables.ContainsKey(b))
		{
			return false;
		}
		RegisteredCookableInfo registeredCookableInfo = m_cookables[b];
		if (registeredCookableInfo.Referrers.Count == 1)
		{
			if (registeredCookableInfo.Referrers[0] == a)
			{
				return true;
			}
			if (registeredCookableInfo.Referrers[0] == b)
			{
				return false;
			}
		}
		foreach (Uri referrer in registeredCookableInfo.Referrers)
		{
			if (!(referrer == b) && !IsPrimaryConnectionFor(a, referrer))
			{
				return false;
			}
		}
		return true;
	}

	private void RemoveCookable(Uri uri, Uri referrer)
	{
		if (RemoveCookableOrReferrer(uri, referrer))
		{
			RemoveCookableDependents(uri, referrer);
		}
	}

	private void RemoveCookableDependents(Uri uri, Uri referrer)
	{
		if (!m_dependenciesProcessed.Contains(uri))
		{
			return;
		}
		m_dependenciesProcessed.Remove(uri);
		foreach (Uri dependent in m_civTechService.PrimaryProject.DependencyRegistry.GetDependents(uri))
		{
			RemoveCookable(dependent, uri);
		}
	}

	private bool RemoveCookableOrReferrer(Uri uri, Uri referrer)
	{
		if (!m_cookables.ContainsKey(uri))
		{
			return false;
		}
		m_cookables[uri].Referrers.Remove(referrer);
		if (m_cookables[uri].Referrers.Count > 0)
		{
			return false;
		}
		m_cookables.Remove(uri);
		return true;
	}
}

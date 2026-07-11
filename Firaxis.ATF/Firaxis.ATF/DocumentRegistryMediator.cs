using System;
using System.ComponentModel.Composition;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IDocumentRegistryMediator))]
[Export(typeof(IShadowDocumentRegistryProvider))]
[Export(typeof(DocumentRegistryMediator))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DocumentRegistryMediator : IDocumentRegistryMediator, IDocumentRegistryProvider, IShadowDocumentRegistryProvider
{
	private readonly IDocumentRegistry m_documentRegistry;

	private readonly IDocumentRegistry m_shadowDocumentRegistry;

	IDocumentRegistry IDocumentRegistryProvider.DocumentRegistry
	{
		get
		{
			if (ShadowMode)
			{
				return m_shadowDocumentRegistry;
			}
			return m_documentRegistry;
		}
	}

	IDocumentRegistry IShadowDocumentRegistryProvider.DocumentRegistry => m_shadowDocumentRegistry;

	public bool ShadowMode { get; set; }

	[ImportingConstructor]
	public DocumentRegistryMediator(IDocumentRegistry documentRegistry)
	{
		m_documentRegistry = documentRegistry;
		m_shadowDocumentRegistry = new DocumentRegistry();
	}

	public IDocument GetDocument(Uri uri)
	{
		IDocument document = m_documentRegistry.GetDocument(uri);
		if (document == null)
		{
			document = m_shadowDocumentRegistry.GetDocument(uri);
		}
		return document;
	}
}

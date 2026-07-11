using System;
using System.ComponentModel.Composition;
using System.IO;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(IDocumentClient))]
[Export(typeof(SkinServiceEditor))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SkinServiceEditor : IDocumentClient
{
	public static readonly DocumentClientInfo DocumentInfo = new DocumentClientInfo("Skin", new string[1] { ".skn" }, null, null, multiDocument: false);

	public IDocument ActiveDocument { get; private set; }

	public SkinSchemaLoader SchemaLoader { get; private set; }

	public DocumentClientInfo Info { get; } = DocumentInfo;

	public bool AskWhenClosingDirtyDocument { get; } = true;

	[ImportingConstructor]
	public SkinServiceEditor(SkinSchemaLoader ssl)
	{
		SchemaLoader = ssl;
	}

	public bool CanOpen(Uri uri)
	{
		return Info.IsCompatibleUri(uri);
	}

	public void Close(IDocument document)
	{
		if (ActiveDocument == document)
		{
			ActiveDocument = null;
		}
	}

	public IDocument Open(Uri uri)
	{
		if (!File.Exists(uri.LocalPath))
		{
			return null;
		}
		SkinDocument skinDocument = null;
		using (FileStream strm = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read))
		{
			skinDocument = (SkinDocument)Open(strm);
		}
		if (skinDocument != null)
		{
			skinDocument.Uri = uri;
		}
		return skinDocument;
	}

	public IDocument Open(Stream strm)
	{
		SkinDocument skinDocument = null;
		DomXmlReader domXmlReader = new DomXmlReader(SchemaLoader);
		DomNode domNode = domXmlReader.Read(strm, null);
		domNode.InitializeExtensions();
		skinDocument = domNode.Cast<SkinDocument>();
		skinDocument?.Initialize(SchemaLoader);
		return skinDocument;
	}

	public void Reload(IDocument document)
	{
	}

	public bool Save(IDocument document, Uri uri)
	{
		try
		{
			SkinDocument skinDocument = (SkinDocument)document;
			using Stream stream = new FileStream(uri.LocalPath, FileMode.OpenOrCreate, FileAccess.Write);
			skinDocument.Write(stream);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public void Show(IDocument document)
	{
		ActiveDocument = document;
	}
}

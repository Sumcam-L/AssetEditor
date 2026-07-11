using System.IO;
using System.Xml;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

public class SkinDocument : DomDocument
{
	public ISkin Skin { get; private set; }

	public SkinSchemaLoader SchemaLoader { get; private set; }

	public override string Type => SkinServiceEditor.DocumentInfo.FileType;

	internal void Initialize(SkinSchemaLoader loader)
	{
		SchemaLoader = loader;
		Stream skinStream = GetSkinStream();
		UpdateSkinFromStream(skinStream);
	}

	internal void Write(Stream stream)
	{
		DomXmlWriter domXmlWriter = new DomXmlWriter(SchemaLoader.TypeCollection);
		domXmlWriter.Write(base.DomNode, stream, Uri);
	}

	private Stream GetSkinStream()
	{
		Stream stream = new MemoryStream();
		Write(stream);
		stream.Seek(0L, SeekOrigin.Begin);
		return stream;
	}

	private void UpdateSkinFromStream(Stream skinStream)
	{
		XmlDocument xmlDocument = new XmlDocument();
		using (XmlTextReader xmlTextReader = new XmlTextReader(skinStream))
		{
			xmlTextReader.Namespaces = false;
			xmlDocument.Load(xmlTextReader);
		}
		Skin = new Skin(xmlDocument, Uri);
	}
}

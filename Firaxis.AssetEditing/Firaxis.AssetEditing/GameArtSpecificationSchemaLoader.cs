using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(GameArtSpecificationSchemaLoader))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class GameArtSpecificationSchemaLoader : XmlSchemaTypeLoader
{
	private string m_namespace;

	private XmlSchemaTypeCollection m_typeCollection;

	public string NameSpace => m_namespace;

	public XmlSchemaTypeCollection TypeCollection => m_typeCollection;

	[ImportingConstructor]
	public GameArtSpecificationSchemaLoader()
	{
		base.SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "Firaxis.AssetEditing/GameArtSpecification");
		Load("GameArtSpecification.xsd");
	}

	protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
	{
		using IEnumerator<XmlSchemaTypeCollection> enumerator = GetTypeCollections().GetEnumerator();
		if (enumerator.MoveNext())
		{
			XmlSchemaTypeCollection current = enumerator.Current;
			m_namespace = current.TargetNamespace;
			m_typeCollection = current;
			GameArtSpecificationSchema.Initialize(current);
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<GameArtSpecificationDocument>());
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<GameArtSpecificationContext>());
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<ReferenceValidator>());
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
			GameArtSpecificationSchema.GameArtSpecificationType.Type.Define(new ExtensionInfo<GameArtSpecificationAdapter>());
			GameArtSpecificationSchema.ArtConsumerType.Type.Define(new ExtensionInfo<ArtConsumerContext>());
			GameArtSpecificationSchema.ArtConsumerType.Type.Define(new ExtensionInfo<ArtConsumerAdapter>());
			GameArtSpecificationSchema.RelativePathType.Type.Define(new ExtensionInfo<RelativePathAdapter>());
			GameArtSpecificationSchema.GameArtIDType.Type.Define(new ExtensionInfo<GameArtIDAdapter>());
			GameArtSpecificationSchema.GameLibraryType.Type.Define(new ExtensionInfo<LibraryContext>());
			GameArtSpecificationSchema.GameLibraryType.Type.Define(new ExtensionInfo<GameLibraryAdapter>());
			GameArtSpecificationSchema.GameLibraryContainerType.Type.Define(new ExtensionInfo<GameLibraryContainerAdapter>());
			GameArtSpecificationSchema.LibraryReferenceType.Type.Define(new ExtensionInfo<LibraryReferenceAdapter>());
			GameArtSpecificationSchema.LibraryReferenceContainerType.Type.Define(new ExtensionInfo<LibraryReferenceContainerAdapter>());
			GameArtSpecificationSchema.ArtConsumerContainerType.Type.Define(new ExtensionInfo<ArtConsumerContainerAdapter>());
			GameArtSpecificationSchema.RelativePathContainerType.Type.Define(new ExtensionInfo<RelativePathContainerAdapter>());
			GameArtSpecificationSchema.RequiredGameArtIDContainerType.Type.Define(new ExtensionInfo<RequiredGameArtIDContainerAdapter>());
			AdapterCreator<CustomTypeDescriptorNodeAdapter> creator = new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
			GameArtSpecificationSchema.GameArtSpecificationType.Type.AddAdapterCreator(creator);
			GameArtSpecificationSchema.ArtConsumerType.Type.AddAdapterCreator(creator);
			GameArtSpecificationSchema.ArtConsumerContainerType.Type.AddAdapterCreator(creator);
			GameArtSpecificationSchema.RelativePathContainerType.Type.AddAdapterCreator(creator);
			GameArtSpecificationSchema.RequiredGameArtIDContainerType.Type.AddAdapterCreator(creator);
		}
	}
}

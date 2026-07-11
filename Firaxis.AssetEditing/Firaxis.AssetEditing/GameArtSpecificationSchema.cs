using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class GameArtSpecificationSchema
{
	public static class GameArtSpecificationType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo IDAttribute;

		public static ChildInfo ArtConsumersChild;

		public static ChildInfo GameLibrariesChild;

		public static ChildInfo RequiredGameArtIDsChild;
	}

	public static class ArtConsumerContainerType
	{
		public static DomNodeType Type;

		public static ChildInfo ArtConsumersChild;
	}

	public static class GameLibraryContainerType
	{
		public static DomNodeType Type;

		public static ChildInfo GameLibrariesChild;
	}

	public static class ArtConsumerType
	{
		public static DomNodeType Type;

		public static AttributeInfo ConsumerNameAttribute;

		public static AttributeInfo LoadsLibrariesAttribute;

		public static ChildInfo RelativePathsChild;

		public static ChildInfo LibraryReferencesChild;
	}

	public static class GameLibraryType
	{
		public static DomNodeType Type;

		public static AttributeInfo LibraryNameAttribute;

		public static ChildInfo RelativePathsChild;
	}

	public static class RelativePathContainerType
	{
		public static DomNodeType Type;

		public static ChildInfo RelativePathsChild;
	}

	public static class RelativePathType
	{
		public static DomNodeType Type;

		public static AttributeInfo RelativePathAttribute;
	}

	public static class LibraryReferenceContainerType
	{
		public static DomNodeType Type;

		public static ChildInfo LibraryReferencesChild;
	}

	public static class LibraryReferenceType
	{
		public static DomNodeType Type;

		public static AttributeInfo LibraryNameAttribute;
	}

	public static class RequiredGameArtIDContainerType
	{
		public static DomNodeType Type;

		public static ChildInfo RequiredGameArtIDsChild;
	}

	public static class GameArtIDType
	{
		public static DomNodeType Type;

		public static AttributeInfo GameArtIDAttribute;

		public static AttributeInfo GameNameAttribute;
	}

	public const string NS = "GameArtSpecification";

	public static ChildInfo GameArtSpecificationRootElement;

	public static void Initialize(XmlSchemaTypeCollection typeCollection)
	{
		Initialize((string ns, string name) => typeCollection.GetNodeType(ns, name), (string ns, string name) => typeCollection.GetRootElement(ns, name));
	}

	public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
	{
		Initialize((string ns, string name) => typeCollections[ns].GetNodeType(name), (string ns, string name) => typeCollections[ns].GetRootElement(name));
	}

	private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
	{
		GameArtSpecificationType.Type = getNodeType("GameArtSpecification", "GameArtSpecificationType");
		GameArtSpecificationType.NameAttribute = GameArtSpecificationType.Type.GetAttributeInfo("Name");
		GameArtSpecificationType.IDAttribute = GameArtSpecificationType.Type.GetAttributeInfo("ID");
		GameArtSpecificationType.ArtConsumersChild = GameArtSpecificationType.Type.GetChildInfo("ArtConsumers");
		GameArtSpecificationType.GameLibrariesChild = GameArtSpecificationType.Type.GetChildInfo("GameLibraries");
		GameArtSpecificationType.RequiredGameArtIDsChild = GameArtSpecificationType.Type.GetChildInfo("RequiredGameArtIDs");
		ArtConsumerContainerType.Type = getNodeType("GameArtSpecification", "ArtConsumerContainerType");
		ArtConsumerContainerType.ArtConsumersChild = ArtConsumerContainerType.Type.GetChildInfo("ArtConsumers");
		GameLibraryContainerType.Type = getNodeType("GameArtSpecification", "GameLibraryContainerType");
		GameLibraryContainerType.GameLibrariesChild = GameLibraryContainerType.Type.GetChildInfo("GameLibraries");
		GameLibraryType.Type = getNodeType("GameArtSpecification", "GameLibraryType");
		GameLibraryType.LibraryNameAttribute = GameLibraryType.Type.GetAttributeInfo("LibraryName");
		GameLibraryType.RelativePathsChild = GameLibraryType.Type.GetChildInfo("RelativePaths");
		RelativePathContainerType.Type = getNodeType("GameArtSpecification", "RelativePathContainerType");
		RelativePathContainerType.RelativePathsChild = RelativePathContainerType.Type.GetChildInfo("RelativePaths");
		RelativePathType.Type = getNodeType("GameArtSpecification", "RelativePathType");
		RelativePathType.RelativePathAttribute = RelativePathType.Type.GetAttributeInfo("RelativePath");
		LibraryReferenceType.Type = getNodeType("GameArtSpecification", "LibraryReferenceType");
		LibraryReferenceType.LibraryNameAttribute = LibraryReferenceType.Type.GetAttributeInfo("LibraryName");
		LibraryReferenceContainerType.Type = getNodeType("GameArtSpecification", "LibraryReferenceContainerType");
		LibraryReferenceContainerType.LibraryReferencesChild = LibraryReferenceContainerType.Type.GetChildInfo("LibraryReferences");
		ArtConsumerType.Type = getNodeType("GameArtSpecification", "ArtConsumerType");
		ArtConsumerType.ConsumerNameAttribute = ArtConsumerType.Type.GetAttributeInfo("ConsumerName");
		ArtConsumerType.LoadsLibrariesAttribute = ArtConsumerType.Type.GetAttributeInfo("LoadsLibraries");
		ArtConsumerType.RelativePathsChild = ArtConsumerType.Type.GetChildInfo("RelativePaths");
		ArtConsumerType.LibraryReferencesChild = ArtConsumerType.Type.GetChildInfo("LibraryReferences");
		RequiredGameArtIDContainerType.Type = getNodeType("GameArtSpecification", "RequiredGameArtIDContainerType");
		RequiredGameArtIDContainerType.RequiredGameArtIDsChild = RequiredGameArtIDContainerType.Type.GetChildInfo("RequiredGameArtIDs");
		GameArtIDType.Type = getNodeType("GameArtSpecification", "GameArtIDType");
		GameArtIDType.GameNameAttribute = GameArtIDType.Type.GetAttributeInfo("GameName");
		GameArtIDType.GameArtIDAttribute = GameArtIDType.Type.GetAttributeInfo("GameArtID");
		GameArtSpecificationRootElement = getRootElement("GameArtSpecification", "GameArtSpecification");
	}
}

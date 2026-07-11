using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class FireFXLibrarySchema
{
	public static class FireFXLibraryType
	{
		public static DomNodeType Type;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo ScriptTextAttribute;
	}

	public static ChildInfo FireFXLibraryRootElement;

	public const string NS = "FireFX";

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
		FireFXLibraryType.Type = getNodeType("FireFX", "FireFXLibraryType");
		FireFXLibraryType.ClassNameAttribute = FireFXLibraryType.Type.GetAttributeInfo("ClassName");
		FireFXLibraryType.ScriptTextAttribute = FireFXLibraryType.Type.GetAttributeInfo("ScriptText");
		FireFXLibraryRootElement = getRootElement("FireFX", "FireFXLibrary");
	}
}

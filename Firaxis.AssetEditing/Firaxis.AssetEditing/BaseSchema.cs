using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class BaseSchema
{
	public static class TextElementType
	{
		public static DomNodeType Type;

		public static AttributeInfo TextAttribute;
	}

	public static class DataFileType
	{
		public static DomNodeType Type;

		public static AttributeInfo IDAttribute;

		public static AttributeInfo RelativePathAttribute;
	}

	public static class PathValueType
	{
		public static DomNodeType Type;

		public static AttributeInfo PathAttribute;
	}

	public static class DataFilesType
	{
		public static DomNodeType Type;

		public static ChildInfo ElementChild;
	}

	public static class TagsType
	{
		public static DomNodeType Type;

		public static ChildInfo ElementChild;
	}

	public static class GroupsType
	{
		public static DomNodeType Type;

		public static ChildInfo ElementChild;
	}

	public const string NS = "BaseTypes";

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
		TextElementType.Type = getNodeType("BaseTypes", "TextElementType");
		TextElementType.TextAttribute = TextElementType.Type.GetAttributeInfo("Text");
		PathValueType.Type = getNodeType("BaseTypes", "PathValueType");
		PathValueType.PathAttribute = PathValueType.Type.GetAttributeInfo("Path");
		DataFileType.Type = getNodeType("BaseTypes", "DataFileType");
		DataFileType.IDAttribute = DataFileType.Type.GetAttributeInfo("ID");
		DataFileType.RelativePathAttribute = DataFileType.Type.GetAttributeInfo("RelativePath");
		DataFilesType.Type = getNodeType("BaseTypes", "DataFilesType");
		DataFilesType.ElementChild = DataFilesType.Type.GetChildInfo("Element");
		TagsType.Type = getNodeType("BaseTypes", "TagsType");
		TagsType.ElementChild = TagsType.Type.GetChildInfo("Element");
		GroupsType.Type = getNodeType("BaseTypes", "GroupsType");
		GroupsType.ElementChild = GroupsType.Type.GetChildInfo("Element");
	}
}

using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class XLPSchema
{
	public static class PlatformContainerType
	{
		public static DomNodeType Type;

		public static ChildInfo PlatformsChild;
	}

	public static class PlatformType
	{
		public static DomNodeType Type;

		public static AttributeInfo PlatformAttribute;
	}

	public static class XLPEntryType
	{
		public static DomNodeType Type;

		public static AttributeInfo EntryIDAttribute;

		public static AttributeInfo ObjectNameAttribute;
	}

	public static class XLPType
	{
		public static DomNodeType Type;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo PackageNameAttribute;

		public static AttributeInfo ModuleNameAttribute;

		public static ChildInfo PlatformsChild;

		public static ChildInfo EntriesChild;
	}

	public static ChildInfo XLPRootElement;

	public const string NS = "XLP";

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
		XLPType.Type = getNodeType("XLP", "XLPType");
		XLPType.ClassNameAttribute = XLPType.Type.GetAttributeInfo("ClassName");
		XLPType.PackageNameAttribute = XLPType.Type.GetAttributeInfo("PackageName");
		XLPType.ModuleNameAttribute = XLPType.Type.GetAttributeInfo("ModuleName");
		XLPType.PlatformsChild = XLPType.Type.GetChildInfo("Platforms");
		XLPType.EntriesChild = XLPType.Type.GetChildInfo("Entries");
		PlatformContainerType.Type = getNodeType("XLP", "PlatformContainerType");
		PlatformContainerType.PlatformsChild = PlatformContainerType.Type.GetChildInfo("Platforms");
		PlatformType.Type = getNodeType("XLP", "PlatformType");
		PlatformType.PlatformAttribute = PlatformType.Type.GetAttributeInfo("Platform");
		XLPEntryType.Type = getNodeType("XLP", "XLPEntryType");
		XLPEntryType.EntryIDAttribute = XLPEntryType.Type.GetAttributeInfo("EntryID");
		XLPEntryType.ObjectNameAttribute = XLPEntryType.Type.GetAttributeInfo("ObjectName");
		XLPRootElement = getRootElement("XLP", "XLP");
	}
}

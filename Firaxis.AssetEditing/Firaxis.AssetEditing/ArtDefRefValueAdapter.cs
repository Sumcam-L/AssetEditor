using System;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefRefValueAdapter : DomNodeAdapter
{
	public string ArtDefPath
	{
		get
		{
			object attribute = GetAttribute<object>(FieldSchema.ArtDefRefValueType.ArtDefPathAttribute);
			if (attribute is string)
			{
				return attribute as string;
			}
			if (attribute is ArtDefReferenceInfo)
			{
				return ((ArtDefReferenceInfo)attribute).artDefPath;
			}
			Console.WriteLine("Error!");
			return string.Empty;
		}
		set
		{
			SetAttribute(FieldSchema.ArtDefRefValueType.ArtDefPathAttribute, value);
		}
	}

	public string CollectionName
	{
		get
		{
			object attribute = GetAttribute<object>(FieldSchema.ArtDefRefValueType.CollectionNameAttribute);
			if (attribute is string)
			{
				return attribute as string;
			}
			if (attribute is ArtDefReferenceInfo)
			{
				return ((ArtDefReferenceInfo)attribute).collectionName;
			}
			Console.WriteLine("Error!");
			return string.Empty;
		}
		set
		{
			SetAttribute(FieldSchema.ArtDefRefValueType.CollectionNameAttribute, value);
		}
	}

	public string ElementName
	{
		get
		{
			object attribute = GetAttribute<object>(FieldSchema.ArtDefRefValueType.ElementNameAttribute);
			if (attribute is string)
			{
				return attribute as string;
			}
			if (attribute is ArtDefReferenceInfo)
			{
				return ((ArtDefReferenceInfo)attribute).elementName;
			}
			Console.WriteLine("Error!");
			return string.Empty;
		}
		set
		{
			SetAttribute(FieldSchema.ArtDefRefValueType.ElementNameAttribute, value);
		}
	}

	public string TemplateName
	{
		get
		{
			object attribute = GetAttribute<object>(FieldSchema.ArtDefRefValueType.TemplateNameAttribute);
			if (attribute is string)
			{
				return attribute as string;
			}
			if (attribute is ArtDefReferenceInfo)
			{
				return ((ArtDefReferenceInfo)attribute).templateName;
			}
			Console.WriteLine("Error!");
			return string.Empty;
		}
		set
		{
			SetAttribute(FieldSchema.ArtDefRefValueType.TemplateNameAttribute, value);
		}
	}
}

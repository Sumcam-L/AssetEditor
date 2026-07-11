using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class ArtDefCollapser : IFlattenedArtDef, IArtDef, IAssemblyInstance, IDisposable, ISerializable, IVersionedData, IPathCollapser
{
	private readonly IList<string> m_artDefPantries;

	private readonly IFlattenedArtDef m_flattenedArtDef;

	private readonly SortedSet<int> m_pantryIndices = new SortedSet<int>();

	private readonly IProjectConfig m_projectConfig;

	private readonly string m_relativePath;

	private readonly string m_relativePathOriginalCase;

	public string ArtDefTemplate
	{
		get
		{
			return m_flattenedArtDef.ArtDefTemplate;
		}
		set
		{
			m_flattenedArtDef.ArtDefTemplate = value;
		}
	}

	public string RelativePath => m_relativePathOriginalCase;

	public IEnumerable<IArtDefCollection> RootCollections => m_flattenedArtDef.RootCollections;

	public Version Version => m_flattenedArtDef.Version;

	public ArtDefCollapser(IList<string> artDefPantries, string relativePath, IProjectConfig projectConfig)
	{
		m_flattenedArtDef = Context.EnsureCreated<CivTechContext>().CreateInstance<IFlattenedArtDef>();
		m_artDefPantries = artDefPantries;
		m_relativePathOriginalCase = relativePath;
		m_relativePath = relativePath.ToLower();
		m_projectConfig = projectConfig;
	}

	public void AddArtDef(IArtDef artDef)
	{
		m_flattenedArtDef.AddArtDef(artDef);
	}

	public void AddArtDefs(IEnumerable<IArtDef> artDefs)
	{
		m_flattenedArtDef.AddArtDefs(artDefs);
	}

	public IArtDefCollection AddCollection(string name)
	{
		return m_flattenedArtDef.AddCollection(name);
	}

	public bool AddRootPantry(string pantry)
	{
		int num = m_artDefPantries.IndexOf(pantry);
		if (num >= 0)
		{
			return m_pantryIndices.Add(num);
		}
		return false;
	}

	public ResultCode DeserializeFromFile(string filename)
	{
		return m_flattenedArtDef.DeserializeFromFile(filename);
	}

	public bool DeserializeFromXML(string xmlText)
	{
		return m_flattenedArtDef.DeserializeFromXML(xmlText);
	}

	public void Dispose()
	{
		m_flattenedArtDef.Dispose();
		m_pantryIndices.Clear();
	}

	public void Reload()
	{
		foreach (int pantryIndex in m_pantryIndices)
		{
			string filename = Path.Combine(m_artDefPantries[pantryIndex], m_relativePath);
			IArtDef artDef = Context.EnsureCreated<CivTechContext>().CreateInstance<IArtDef>(new object[1] { m_projectConfig });
			if ((bool)artDef.DeserializeFromFile(filename))
			{
				m_flattenedArtDef.AddArtDef(artDef);
			}
			else
			{
				artDef.Dispose();
			}
		}
	}

	public void RemoveCollection(string name)
	{
		m_flattenedArtDef.RemoveCollection(name);
	}

	public bool RemoveRootPantry(string pantry)
	{
		int num = m_artDefPantries.IndexOf(pantry);
		if (num >= 0)
		{
			return m_pantryIndices.Remove(num);
		}
		return false;
	}

	public void Reset()
	{
		m_flattenedArtDef.Reset();
	}

	public bool SerializeIntoFile(string filename)
	{
		return m_flattenedArtDef.SerializeIntoFile(filename);
	}

	public string SerializeIntoXML()
	{
		return m_flattenedArtDef.SerializeIntoXML();
	}

	public void SetVersion(string versionString)
	{
		m_flattenedArtDef.SetVersion(versionString);
	}

	public void SetVersion(int major, int minor, int build, int revision)
	{
		m_flattenedArtDef.SetVersion(major, minor, build, revision);
	}

	public override string ToString()
	{
		return m_flattenedArtDef.ToString();
	}

	public void UpdateRootCollectionsFromTemplate(IArtDefTemplate artDefTmpl)
	{
		m_flattenedArtDef.UpdateRootCollectionsFromTemplate(artDefTmpl);
	}
}

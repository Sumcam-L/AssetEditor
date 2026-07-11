using System;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.AssetPreviewing;

public class AttachmentPointDataCache : IAttachmentPoint, IEquatable<IAttachmentPoint>, IDisposable
{
	private readonly IAttachmentPointStandalone m_attachmentPoint;

	private bool m_disposedValue = false;

	public string Name
	{
		get
		{
			return m_attachmentPoint.Name;
		}
		set
		{
			m_attachmentPoint.Name = value;
		}
	}

	public string BoneName
	{
		get
		{
			return m_attachmentPoint.BoneName;
		}
		set
		{
			m_attachmentPoint.BoneName = value;
		}
	}

	public string ModelInstanceName
	{
		get
		{
			return m_attachmentPoint.ModelInstanceName;
		}
		set
		{
			m_attachmentPoint.ModelInstanceName = value;
		}
	}

	public IValueSet CookParameters => m_attachmentPoint.CookParameters;

	public IFloatVector3 Orientation
	{
		get
		{
			return m_attachmentPoint.Orientation;
		}
		set
		{
			m_attachmentPoint.Orientation = value;
		}
	}

	public IFloatVector3 Position
	{
		get
		{
			return m_attachmentPoint.Position;
		}
		set
		{
			m_attachmentPoint.Position = value;
		}
	}

	public float Scale
	{
		get
		{
			return m_attachmentPoint.Scale;
		}
		set
		{
			m_attachmentPoint.Scale = value;
		}
	}

	public AttachmentPointDataCache(IAttachmentPoint attachmentPoint)
	{
		m_attachmentPoint = Context.EnsureCreated<CivTechContext>().CreateInstance<IAttachmentPointStandalone>();
		m_attachmentPoint.Name = attachmentPoint.Name;
		m_attachmentPoint.ModelInstanceName = attachmentPoint.ModelInstanceName;
		m_attachmentPoint.BoneName = attachmentPoint.BoneName;
		m_attachmentPoint.Orientation = attachmentPoint.Orientation;
		m_attachmentPoint.Position = attachmentPoint.Position;
		string xml = attachmentPoint.CookParameters.SerializeIntoXML();
		m_attachmentPoint.CookParameters.DeserializeFromXML(xml);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public bool Equals(IAttachmentPoint other)
	{
		if (other == null)
		{
			return false;
		}
		return Name == other.Name && ModelInstanceName == other.ModelInstanceName && BoneName == other.BoneName;
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public override string ToString()
	{
		return $"Name: {Name}; Model Name: {ModelInstanceName}; Bone Name: {BoneName}";
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposedValue)
		{
			if (disposing)
			{
				m_attachmentPoint.Dispose();
			}
			m_disposedValue = true;
		}
	}
}

using System;

namespace Sce.Atf.Applications;

public class RecentDocumentInfo : IComparable, IPinnable
{
	private readonly Uri m_uri;

	private readonly string m_type;

	private int m_index = -1;

	public Uri Uri => m_uri;

	public string Type => m_type;

	public bool Pinned { get; set; }

	public int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public RecentDocumentInfo(Uri uri, string type)
	{
		m_uri = uri;
		m_type = type;
		Pinned = false;
	}

	public RecentDocumentInfo(Uri uri, string type, bool pinned)
	{
		m_uri = uri;
		m_type = type;
		Pinned = pinned;
	}

	public override bool Equals(object obj)
	{
		return obj is RecentDocumentInfo recentDocumentInfo && Uri == recentDocumentInfo.Uri && m_type == recentDocumentInfo.m_type;
	}

	public override int GetHashCode()
	{
		return Uri.GetHashCode() ^ m_type.GetHashCode();
	}

	int IComparable.CompareTo(object obj)
	{
		if (!(obj is RecentDocumentInfo recentDocumentInfo))
		{
			return 0;
		}
		return m_index.CompareTo(recentDocumentInfo.m_index);
	}
}

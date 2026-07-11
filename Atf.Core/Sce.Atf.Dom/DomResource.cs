using System;

namespace Sce.Atf.Dom;

public class DomResource : DomNodeAdapter, IResource
{
	private Uri m_uri;

	public virtual string Type => "Unknown".Localize();

	public virtual Uri Uri
	{
		get
		{
			return m_uri;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value != m_uri)
			{
				Uri uri = m_uri;
				m_uri = value;
				OnUriChanged(new UriChangedEventArgs(uri, m_uri));
			}
		}
	}

	public event EventHandler<UriChangedEventArgs> UriChanged;

	protected virtual void OnUriChanged(UriChangedEventArgs e)
	{
		this.UriChanged.Raise(this, e);
	}
}

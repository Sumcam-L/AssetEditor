using System;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameArtSpecificationDocument : DomNodeAdapter, IDocument, IResource, IDisposable
{
	private IGameArtSpecification m_gameArtSpecification;

	private bool _isNewGameArt;

	private bool m_dirty;

	private Uri m_uri;

	public IGameArtSpecification GameArtSpecification
	{
		get
		{
			return m_gameArtSpecification;
		}
		set
		{
			if (m_gameArtSpecification != value)
			{
				m_gameArtSpecification = value;
			}
		}
	}

	public bool IsNewGameArt
	{
		get
		{
			return _isNewGameArt;
		}
		set
		{
			_isNewGameArt = value;
		}
	}

	public ICivTechService CivTechService { get; set; }

	public bool IsReadOnly
	{
		get
		{
			if (CivTechService == null)
			{
				return false;
			}
			return !CivTechService.IsFromPrimaryModProject(Uri);
		}
	}

	public bool Dirty
	{
		get
		{
			return m_dirty;
		}
		set
		{
			if (value != m_dirty)
			{
				m_dirty = value;
				OnDirtyChanged(EventArgs.Empty);
			}
		}
	}

	public string Type => "Game Art Specification".Localize();

	public Uri Uri
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

	public event EventHandler DirtyChanged;

	public event EventHandler<UriChangedEventArgs> UriChanged;

	protected void OnDirtyChanged(EventArgs e)
	{
		UpdateControlInfo();
		this.DirtyChanged.Raise(this, e);
	}

	protected void OnUriChanged(UriChangedEventArgs e)
	{
		UpdateControlInfo();
		this.UriChanged.Raise(this, e);
	}

	public void UpdateControlInfo()
	{
		string localPath = Uri.LocalPath;
		string text = Path.GetFileName(localPath);
		if (Dirty)
		{
			text += "*";
		}
		if (IsReadOnly)
		{
			text += "(Read Only)";
		}
		GameArtSpecificationContext gameArtSpecificationContext = base.DomNode.As<GameArtSpecificationContext>();
		if (gameArtSpecificationContext.ControlInfo != null)
		{
			gameArtSpecificationContext.ControlInfo.Name = text;
			gameArtSpecificationContext.ControlInfo.Description = localPath;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			base.DomNode.As<GameArtSpecificationContext>().Dispose();
		}
	}
}

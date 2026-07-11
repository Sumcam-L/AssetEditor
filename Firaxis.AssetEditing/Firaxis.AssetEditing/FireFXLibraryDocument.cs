using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.ATF;
using Firaxis.CivTech.FireFX;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class FireFXLibraryDocument : CompositeDomDocument, IFireFXScriptResource, IResource
{
	public string m_text = string.Empty;

	private IFireFXEffect m_effect;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (!(m_text == value))
			{
				m_text = value;
				this.TextChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public IList<CompileIssue> Issues { get; set; } = new List<CompileIssue>();

	public IFireFXEffect Effect
	{
		get
		{
			return m_effect;
		}
		set
		{
			if (m_effect != value)
			{
				m_effect = value;
				this.EffectChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public override string Type => FireFXLibraryEditor.DocumentClientInfo.FileType;

	public event EventHandler TextChanged;

	public event EventHandler EffectChanged;

	protected override void OnUriChanged(UriChangedEventArgs e)
	{
		UpdateControlInfo();
		base.OnUriChanged(e);
	}

	protected override void OnDirtyChanged(EventArgs e)
	{
		UpdateControlInfo();
		base.OnDirtyChanged(e);
	}

	public override void UpdateControlInfo()
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
		FireFXLibraryContext fireFXLibraryContext = base.DomNode.As<FireFXLibraryContext>();
		fireFXLibraryContext.ControlInfo.Name = text;
		fireFXLibraryContext.ControlInfo.Description = localPath;
	}

	protected override void Dispose(bool disposing)
	{
	}
}

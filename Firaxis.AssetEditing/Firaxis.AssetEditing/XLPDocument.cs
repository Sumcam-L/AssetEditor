using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class XLPDocument : CompositeDomDocument, ICookable, IVersionProvider, IHotLoadableDocument, IHotloadable, IDocument, IResource, IProjectSpecificDocument
{
	private Image _dirtyImage;

	public ICivTechService CivTechService { get; set; }

	public IVersionService VersionService { get; set; }

	public IXLP XLP => base.DomNode.As<XLPAdapter>().XLP;

	private string RelativePath
	{
		get
		{
			string result = string.Empty;
			if (CivTechService != null)
			{
				result = Path.Combine(CivTechService.PrimaryProject.Paths.GamePantry, "XLPs");
				result = Uri.LocalPath.Replace(result, "").Trim(Path.DirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);
			}
			return result;
		}
	}

	public override string Type => XLPEditor.DocumentClientInfo.FileType;

	public override bool IsReadOnly
	{
		get
		{
			if (XLP == null)
			{
				return false;
			}
			if (!CivTechService.IsFromActiveProjectOrDependencies(Uri))
			{
				return true;
			}
			if (CivTechService.IsFromModDependencies(Uri))
			{
				return true;
			}
			if (VersionService.IsLocalBuild())
			{
				return false;
			}
			try
			{
				return Version > VersionService.ApplicationVersion;
			}
			catch (ArgumentOutOfRangeException ex)
			{
				BugSubmitter.SilentReport("Version error in XLP: \"" + Uri.LocalPath + "\n\nException:" + ex.Message + "\n\n@assign bwhitman @summary XLP Version error");
			}
			return false;
		}
	}

	public Version Version => XLP.Version;

	public string SubSystem => TunerHelper.GetXLPSubSystem(XLP);

	public IEnumerable<string> ConsumerNames => TunerHelper.GetXLPConsumerNames(XLP, CivTechService.PrimaryProject.Paths.GameDirectory);

	public Uri CookableUri => Uri;

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
		XLPContext xLPContext = this.As<XLPContext>();
		xLPContext.ControlInfo.Name = text;
		xLPContext.ControlInfo.Description = localPath;
	}

	private Image GetDirtyImage()
	{
		if (_dirtyImage == null)
		{
			_dirtyImage = ImageHelper.GetSolidTriangleImage(Color.Red, 16);
		}
		return _dirtyImage;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			base.DomNode.As<XLPContext>().Dispose();
		}
	}
}

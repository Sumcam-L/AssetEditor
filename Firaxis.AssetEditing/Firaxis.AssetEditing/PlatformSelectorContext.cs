using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class PlatformSelectorContext : EditingContext, IPlatformSelectorContext
{
	private XLPAdapter Adapter => base.DomNode.As<XLPAdapter>();

	private XLPDocument Document => base.DomNode.As<XLPDocument>();

	private IXLP XLP => Adapter?.XLP;

	public IEnumerable<Platforms> AllowedPlatforms => XLP.AllowedPlatforms;

	public void AllowPlatform(Platforms platform)
	{
		if (platform == Platforms.PLATFORM_INVALID)
		{
			return;
		}
		this.DoTransaction(delegate
		{
			if (platform == Platforms.PLATFORM_ALL)
			{
				foreach (Platforms usablePlatform in PlatformsAssistant.GetUsablePlatforms())
				{
					XLP.AllowPlatform(usablePlatform);
				}
				return;
			}
			XLP.AllowPlatform(platform);
		}, "Allow new Platform");
	}

	public void ClearAllowedPlatforms()
	{
		this.DoTransaction(delegate
		{
			XLP.ClearAllowedPlatforms();
		}, "Clear Allowed Platforms");
	}

	public bool IsPlatformAllowed(Platforms platform)
	{
		return XLP.IsPlatformAllowed(platform);
	}

	public void RemovePlatform(Platforms platform)
	{
		if (platform == Platforms.PLATFORM_INVALID)
		{
			return;
		}
		ISet<Platforms> allowedPlatforms = new HashSet<Platforms>(AllowedPlatforms);
		allowedPlatforms.Remove(platform);
		this.DoTransaction(delegate
		{
			XLP.ClearAllowedPlatforms();
			if (platform != Platforms.PLATFORM_ALL)
			{
				foreach (Platforms item in allowedPlatforms)
				{
					XLP.AllowPlatform(item);
				}
			}
		}, "Removing Platform " + platform);
	}

	protected override void OnCancelled()
	{
		XLPAdapter xLPAdapter = base.DomNode.As<XLPAdapter>();
		IXLP xLP = XLP;
		xLPAdapter.Update(xLP);
	}

	protected override void OnEnded()
	{
		IDisposable disposable = null;
		if (Document.IsReadOnly)
		{
			disposable = SuspendRecording();
		}
		base.OnEnded();
		disposable?.Dispose();
		if (Document.IsReadOnly && InTransaction)
		{
			MessageBoxes.Show("Can not modify assets that are not part of the active project", "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
			throw new InvalidTransactionException("Can not modify assets that are not part of the active project");
		}
	}
}

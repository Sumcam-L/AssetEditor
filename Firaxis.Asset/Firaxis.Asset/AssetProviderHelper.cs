using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Firaxis.Error;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

public static class AssetProviderHelper
{
	public static IEnumerable<IAssetProvider> GetAssociatedProviders(IAssetProvider provider)
	{
		yield return provider;
		if (!Context.TryGet<IAssetTargetCollector>(out var atc))
		{
			yield break;
		}
		foreach (IAssetTarget at in atc.AssetTargets)
		{
			if (at is IAssetProvider && at != provider && (at == provider.ParentAsset || at.ParentAsset == provider || (at.ParentAsset != null && at.ParentAsset == provider.ParentAsset)))
			{
				yield return (IAssetProvider)at;
			}
		}
	}

	public static void GetAssociatedProviders(IAssetProvider provider, List<IAssetProvider> providers)
	{
		providers.AddRange(GetAssociatedProviders(provider));
	}

	public static void SaveAll(IEnumerable<IAssetProvider> providers, string caption)
	{
		foreach (IAssetProvider provider in providers)
		{
			if (provider.Modified)
			{
				provider.ShowIt();
			}
			Save(provider, caption, SaveMethod.Save);
		}
	}

	public static void SaveAll(IAssetProvider provider, string caption)
	{
		SaveAll(GetAssociatedProviders(provider), caption);
		provider.ShowIt();
	}

	public static void SaveAll(string caption)
	{
		if (Context.TryGet<IAssetManager>(out var service))
		{
			SaveAll(service.Assets, caption);
		}
	}

	public static void SaveAll(IEnumerable<IAssetProvider> providers, string caption, Predicate<IAssetProvider> match)
	{
		foreach (IAssetProvider provider in providers)
		{
			if (match(provider))
			{
				if (provider.Modified)
				{
					provider.ShowIt();
				}
				Save(provider, caption, SaveMethod.Save);
			}
		}
	}

	public static void SaveAll(IAssetProvider provider, string caption, Predicate<IAssetProvider> match)
	{
		SaveAll(GetAssociatedProviders(provider), caption, match);
		provider.ShowIt();
	}

	public static void SaveAll(string caption, Predicate<IAssetProvider> match)
	{
		if (Context.TryGet<IAssetManager>(out var service))
		{
			SaveAll(service.Assets, caption, match);
		}
	}

	public static bool SaveAllModified(IEnumerable<IAssetProvider> providers, string caption)
	{
		bool result = true;
		foreach (IAssetProvider provider in providers)
		{
			if (provider.Modified)
			{
				provider.ShowIt();
			}
			if (!SaveModified(provider, caption))
			{
				result = false;
			}
		}
		return result;
	}

	public static bool SaveAllModified(IAssetProvider provider, string caption)
	{
		bool result = SaveAllModified(GetAssociatedProviders(provider), caption);
		provider.ShowIt();
		return result;
	}

	public static bool SaveAllModified(string caption)
	{
		if (Context.TryGet<IAssetManager>(out var service))
		{
			return SaveAllModified(service.Assets, caption);
		}
		return true;
	}

	public static bool SaveAllModified(IEnumerable<IAssetProvider> providers, string caption, Predicate<IAssetProvider> match)
	{
		bool result = true;
		foreach (IAssetProvider provider in providers)
		{
			if (match(provider))
			{
				if (provider.Modified)
				{
					provider.ShowIt();
				}
				if (!SaveModified(provider, caption))
				{
					result = false;
				}
			}
		}
		return result;
	}

	public static bool SaveAllModified(IAssetProvider provider, string caption, Predicate<IAssetProvider> match)
	{
		bool result = SaveAllModified(GetAssociatedProviders(provider), caption, match);
		provider.ShowIt();
		return result;
	}

	public static bool SaveAllModified(string caption, Predicate<IAssetProvider> match)
	{
		if (Context.TryGet<IAssetManager>(out var service))
		{
			return SaveAllModified(service.Assets, caption, match);
		}
		return true;
	}

	public static void Save(IAssetProvider provider, string caption, SaveMethod method)
	{
		if (provider == null || (!provider.Modified && method == SaveMethod.Save))
		{
			return;
		}
		if (method == SaveMethod.SaveAs || string.IsNullOrEmpty(provider.Asset))
		{
			FilterAttribute attribute = ReflectionHelper.GetAttribute<FilterAttribute>(provider);
			AssetTypeAttribute attribute2 = ReflectionHelper.GetAttribute<AssetTypeAttribute>(provider);
			string text = (string.IsNullOrEmpty(provider.Asset) ? "" : provider.Asset);
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = caption;
			saveFileDialog.Filter = ((attribute != null) ? attribute.Filter : "All Files (*.*)|*.*");
			saveFileDialog.DefaultExt = ((attribute2 != null) ? attribute2.Extension : "");
			saveFileDialog.FileName = Path.GetFileName(text);
			saveFileDialog.InitialDirectory = (string.IsNullOrEmpty(text) ? "" : Path.GetDirectoryName(text));
			DialogResult dialogResult = saveFileDialog.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				provider.SaveAs(saveFileDialog.FileName);
			}
		}
		else
		{
			provider.Save();
		}
	}

	public static bool SaveModified(IAssetProvider provider, string caption)
	{
		bool result = true;
		if (provider != null && provider.Modified)
		{
			string fileName = Path.GetFileName(provider.Asset);
			switch (MessageBox.Show(fileName + " has been modified. Save changes?", caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
			{
			case DialogResult.Yes:
				try
				{
					Save(provider, caption, SaveMethod.Save);
				}
				catch (Exception e)
				{
					ExceptionLogger.Log(e);
					MessageBox.Show("Error saving document", caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					result = false;
				}
				if (provider.Modified)
				{
					result = false;
				}
				break;
			case DialogResult.Cancel:
				result = false;
				break;
			}
		}
		return result;
	}
}

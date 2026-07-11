using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms.Design;
using Firaxis.CivTech;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class RootedFolderUriEditor : FolderNameEditor
{
	public enum FolderRoot
	{
		Depot,
		Pantry
	}

	private readonly string m_lookForFolder = string.Empty;

	private readonly IProjectMapService m_projectMapService;

	private readonly string m_replaceFolderWith = string.Empty;

	private readonly FolderRoot m_rootFolderType;

	public RootedFolderUriEditor(IProjectMapService prjMapSvc)
		: this(FolderRoot.Depot, prjMapSvc)
	{
	}

	public RootedFolderUriEditor(FolderRoot rootType, IProjectMapService prjMapSvc)
	{
		m_rootFolderType = rootType;
		m_projectMapService = prjMapSvc;
	}

	public RootedFolderUriEditor(FolderRoot rootType, IProjectMapService prjMapSvc, string lookForFolder, string replaceFolderWith)
		: this(rootType, prjMapSvc)
	{
		m_lookForFolder = lookForFolder;
		m_replaceFolderWith = replaceFolderWith;
		BugSubmitter.SilentAssert(!string.IsNullOrEmpty(m_lookForFolder) || string.IsNullOrEmpty(m_replaceFolderWith), "Invalid folder passed to RootedFolderUriEditor! @assign bwhitman");
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object origValue)
	{
		object obj = base.EditValue(context, provider, origValue);
		if (!HasBeenEdited(origValue, obj))
		{
			return origValue;
		}
		string fullPath = obj.ToString();
		if (!IsRelativeToRoot(fullPath))
		{
			MessageBoxes.Show("Full path must contain root \"" + GetRoot() + "\"");
			return origValue;
		}
		string pathRelativeToRoot = GetPathRelativeToRoot(fullPath);
		if (!RequiresSubstitution())
		{
			return pathRelativeToRoot;
		}
		int lookIdx = -1;
		if (!HasValidSubstitution(pathRelativeToRoot, ref lookIdx))
		{
			MessageBoxes.Show("Path must contain a folder named \"" + m_lookForFolder + "\"");
			return origValue;
		}
		string oldValue = pathRelativeToRoot.Substring(lookIdx, m_lookForFolder.Length);
		return pathRelativeToRoot.Replace(oldValue, m_replaceFolderWith);
	}

	public string GetPathRelativeToRoot(string fullPath)
	{
		string root = GetRoot();
		return fullPath.Replace(root, "").TrimStart(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	protected override void InitializeDialog(FolderBrowser dialog)
	{
		base.InitializeDialog(dialog);
		dialog.Style = FolderBrowserStyles.RestrictToFilesystem | FolderBrowserStyles.RestrictToSubfolders;
		dialog.StartLocation = FolderBrowserFolder.MyComputer;
	}

	private string GetRoot()
	{
		switch (m_rootFolderType)
		{
		case FolderRoot.Depot:
			return m_projectMapService.PrimaryProject.VersionControl.WorkspaceRoot;
		case FolderRoot.Pantry:
			return m_projectMapService.PrimaryProject.Paths.GamePantry;
		default:
			BugSubmitter.SilentReport("Enum modified without updated switch statement. @assign bwhitman");
			return string.Empty;
		}
	}

	private bool HasBeenEdited(object origValue, object newValue)
	{
		if (newValue == null)
		{
			return false;
		}
		string text = newValue.ToString();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (origValue != null && text == origValue.ToString())
		{
			return false;
		}
		return true;
	}

	private bool HasValidSubstitution(string derootedPath, ref int lookIdx)
	{
		lookIdx = derootedPath.IndexOf(m_lookForFolder, StringComparison.CurrentCultureIgnoreCase);
		return lookIdx != -1;
	}

	private bool IsRelativeToRoot(string fullPath)
	{
		string root = GetRoot();
		return fullPath.StartsWith(root);
	}

	private bool RequiresSubstitution()
	{
		return !string.IsNullOrEmpty(m_lookForFolder);
	}
}

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Firaxis.Controls.Properties;
using Firaxis.Controls.Scrollables;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class ExploreFileTree : ScrollableTree, IExploreFileTree
{
	public class PathContainer : IExpandable
	{
		public DirectoryInfo DirectoryInfo { get; private set; }

		public bool Expanded { get; set; }

		public PathContainer(DirectoryInfo di)
		{
			DirectoryInfo = di;
		}
	}

	public interface IFileContainer
	{
		DateTime LastWriteTime { get; }

		long Length { get; }

		string FullName { get; }
	}

	public class FileContainer : IFileContainer
	{
		public FileInfo FileInfo { get; private set; }

		public DateTime LastWriteTime => FileInfo.LastWriteTime;

		public long Length => FileInfo.Length;

		public string FullName => FileInfo.FullName;

		public FileContainer(FileInfo fi)
		{
			FileInfo = fi;
		}
	}

	public delegate IFileContainer FileContainerCreatorHandler(object kFileInfo);

	private string baseDirectory;

	private FileContainerCreatorHandler fileCreator;

	public bool ShowFiles { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public FileContainerCreatorHandler FileContainerCreator
	{
		get
		{
			return fileCreator ?? new FileContainerCreatorHandler(DefaultFileCreator);
		}
		set
		{
			fileCreator = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public FileContainerCreatorHandler PakFileContainerCreator
	{
		get
		{
			return fileCreator ?? new FileContainerCreatorHandler(DefaultFileCreator);
		}
		set
		{
			fileCreator = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public string BaseDirectory
	{
		get
		{
			return baseDirectory;
		}
		set
		{
			baseDirectory = value;
			RebuildTree();
		}
	}

	public ExploreFileTree()
	{
		baseDirectory = string.Empty;
		base.ExpandedItemChanged += ExploreFileTree_ExpandedItemChanged;
		base.SelectedItemChanged += ExploreFileTree_SelectedItemChanged;
		base.DisplayFilter = FilterFiles;
	}

	private bool FilterFiles(TreeNode node)
	{
		return ShowFiles || node.Item.Tag is PathContainer;
	}

	private IFileContainer DefaultFileCreator(object kFileInfo)
	{
		if (kFileInfo is FileInfo)
		{
			return new FileContainer(kFileInfo as FileInfo);
		}
		return null;
	}

	public void RebuildTree()
	{
		Cursor = Cursors.WaitCursor;
		BeginUpdate();
		Clear();
		try
		{
			PopulateChildren(null, new DirectoryInfo(BaseDirectory), recurse: true);
		}
		catch
		{
		}
		base.SelectedNodes.Clear();
		EndUpdate();
		Cursor = Cursors.Default;
	}

	private void PopulateChildren(TreeNode root, DirectoryInfo baseDir, bool recurse)
	{
		root?.Children.Clear();
		DirectoryInfo[] directories;
		try
		{
			directories = baseDir.GetDirectories();
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
			return;
		}
		DirectoryInfo[] array = directories;
		foreach (DirectoryInfo directoryInfo in array)
		{
			ScrollableItemTree scrollableItemTree = new ScrollableItemTree(directoryInfo.Name, Font);
			scrollableItemTree.Tag = new PathContainer(directoryInfo);
			scrollableItemTree.Image = Firaxis.Controls.Properties.Resources.dir_closed;
			scrollableItemTree.ShowSeparator = false;
			TreeNode root2 = Add(root, scrollableItemTree);
			if (recurse)
			{
				PopulateChildren(root2, directoryInfo, recurse: false);
			}
		}
		FileInfo[] files = baseDir.GetFiles();
		FileInfo[] array2 = files;
		foreach (FileInfo fileInfo in array2)
		{
			ScrollableItemTree scrollableItemTree2 = new ScrollableItemTree(fileInfo.Name, Font);
			scrollableItemTree2.Tag = FileContainerCreator(fileInfo);
			scrollableItemTree2.Image = Firaxis.Controls.Properties.Resources.file_document;
			scrollableItemTree2.ShowSeparator = false;
			scrollableItemTree2.Visible = ShowFiles;
			Add(root, scrollableItemTree2);
		}
	}

	private void ExploreFileTree_ExpandedItemChanged(object sender, TreeNodeEventArgs e)
	{
		if (e.Node.Item.Tag is IExpandable { Expanded: not false } expandable)
		{
			Cursor = Cursors.WaitCursor;
			BeginUpdate();
			if (expandable.GetType() == typeof(PathContainer))
			{
				PopulateChildren(e.Node, ((PathContainer)expandable).DirectoryInfo, recurse: true);
			}
			EndUpdate();
			Cursor = Cursors.Default;
		}
	}

	private void ExploreFileTree_SelectedItemChanged(object sender, TreeNodeEventArgs e)
	{
		if (base.SelectedNodes.Count > 0)
		{
			TreeNode treeNode = base.SelectedNodes[0];
			if (treeNode.Item.Tag is PathContainer pathContainer)
			{
				Cursor = Cursors.WaitCursor;
				BeginUpdate();
				PopulateChildren(treeNode, pathContainer.DirectoryInfo, recurse: true);
				EndUpdate();
				Cursor = Cursors.Default;
			}
		}
	}
}

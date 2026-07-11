using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(ResourceLister))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Any)]
public class ResourceLister : ICommandClient, IControlHostClient, IInitializable
{
	private enum Command
	{
		DetailsView,
		ThumbnailView
	}

	private class GuiSelection<T> : AdaptableSelection<T>, ISelectionContext where T : class
	{
		int ISelectionContext.SelectionCount => base.Count;

		IEnumerable<object> ISelectionContext.Selection
		{
			get
			{
				using IEnumerator<T> enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
			set
			{
				Clear();
				List<T> list = new List<T>();
				foreach (object item in value)
				{
					T val = item.As<T>();
					if (val != null)
					{
						list.Add(val);
					}
				}
				SetRange(list);
			}
		}

		object ISelectionContext.LastSelected => base.LastSelected;

		event EventHandler ISelectionContext.SelectionChanging
		{
			add
			{
				base.Changing += value;
			}
			remove
			{
				base.Changing -= value;
			}
		}

		event EventHandler ISelectionContext.SelectionChanged
		{
			add
			{
				base.Changed += value;
			}
			remove
			{
				base.Changed -= value;
			}
		}

		IEnumerable<U> ISelectionContext.GetSelection<U>()
		{
			using IEnumerator<T> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				U uObj = obj.As<U>();
				if (uObj != null)
				{
					yield return uObj;
				}
			}
		}

		bool ISelectionContext.SelectionContains(object item)
		{
			T val = item.As<T>();
			if (val != null)
			{
				return Contains(val);
			}
			return false;
		}

		T ISelectionContext.GetLastSelected<T>()
		{
			return GetLastSelected<T>();
		}
	}

	private class TreeViewContext : IAdaptable, ITreeView, IItemView, IObservableContext
	{
		private readonly ISelectionContext m_selectionContext = new GuiSelection<object>();

		private readonly IResourceFolder m_rootFolder;

		public object Root => m_rootFolder;

		public IResourceFolder RootFolder => m_rootFolder;

		public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

		public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

		public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

		public event EventHandler Reloaded;

		public TreeViewContext(IResourceFolder rootFolder)
		{
			m_rootFolder = rootFolder;
		}

		public object GetAdapter(Type type)
		{
			if (type.IsAssignableFrom(typeof(ISelectionContext)))
			{
				return m_selectionContext;
			}
			if (type.IsAssignableFrom(GetType()))
			{
				return this;
			}
			return null;
		}

		public IEnumerable<object> GetChildren(object parent)
		{
			IResourceFolder resourceFolder = parent.As<IResourceFolder>();
			if (resourceFolder == null)
			{
				yield break;
			}
			foreach (IResourceFolder folder in resourceFolder.Folders)
			{
				yield return folder;
			}
		}

		public virtual void GetInfo(object item, ItemInfo info)
		{
			IResourceFolder resourceFolder = item.As<IResourceFolder>();
			if (resourceFolder != null)
			{
				info.Label = resourceFolder.Name;
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.FolderImage);
				info.AllowLabelEdit = !resourceFolder.ReadOnlyName;
				info.IsLeaf = resourceFolder.Folders.Count == 0;
			}
		}

		protected virtual void OnItemInserted(ItemInsertedEventArgs<object> e)
		{
			if (this.ItemInserted != null)
			{
				this.ItemInserted(this, e);
			}
		}

		protected virtual void OnItemRemoved(ItemRemovedEventArgs<object> e)
		{
			if (this.ItemRemoved != null)
			{
				this.ItemRemoved(this, e);
			}
		}

		protected virtual void OnItemChanged(ItemChangedEventArgs<object> e)
		{
			if (this.ItemChanged != null)
			{
				this.ItemChanged(this, e);
			}
		}

		protected virtual void OnReloaded(EventArgs e)
		{
			if (this.Reloaded != null)
			{
				this.Reloaded(this, e);
			}
		}
	}

	private class ListViewContext : IListView, IItemView, IObservableContext, ISelectionContext
	{
		private static readonly string[] s_columnNames = new string[4] { "Name", "Size", "Type", "Date Modified" };

		private IResourceFolder m_selectedFolder;

		private readonly GuiSelection<object> m_selection = new GuiSelection<object>();

		public string[] ColumnNames => s_columnNames;

		public IEnumerable<object> Items
		{
			get
			{
				IResourceFolder resourceFolder = SelectedFolder;
				if (resourceFolder == null)
				{
					yield break;
				}
				foreach (Uri resourceUri in resourceFolder.ResourceUris)
				{
					yield return resourceUri;
				}
			}
		}

		public IEnumerable<object> Selection
		{
			get
			{
				return m_selection;
			}
			set
			{
				m_selection.SetRange(value);
			}
		}

		public object LastSelected => m_selection.LastSelected;

		public int SelectionCount => m_selection.Count;

		public IResourceFolder SelectedFolder
		{
			get
			{
				return m_selectedFolder;
			}
			set
			{
				m_selectedFolder = value;
				OnReloaded(EventArgs.Empty);
			}
		}

		public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

		public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

		public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

		public event EventHandler Reloaded;

		public event EventHandler SelectionChanging;

		public event EventHandler SelectionChanged;

		public ListViewContext()
		{
			m_selection.Changing += TheSelectionChanging;
			m_selection.Changed += TheSelectionChanged;
		}

		public object GetAdapter(Type type)
		{
			if (type.IsAssignableFrom(GetType()))
			{
				return this;
			}
			return null;
		}

		public virtual void GetInfo(object item, ItemInfo info)
		{
			Uri uriFromTag = GetUriFromTag(item);
			if (uriFromTag != null)
			{
				FileInfo fileInfo = new FileInfo(uriFromTag.LocalPath);
				info.Label = fileInfo.Name;
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.ResourceImage);
				Shell32.SHFILEINFO psfi = default(Shell32.SHFILEINFO);
				uint uFlags = 1040u;
				Shell32.SHGetFileInfo(fileInfo.FullName, 128u, ref psfi, (uint)Marshal.SizeOf((object)psfi), uFlags);
				string szTypeName = psfi.szTypeName;
				long num;
				DateTime dateTime;
				try
				{
					num = fileInfo.Length;
					dateTime = fileInfo.LastWriteTime;
				}
				catch (IOException)
				{
					num = 0L;
					dateTime = default(DateTime);
				}
				info.Properties = new object[3] { num, szTypeName, dateTime };
			}
		}

		protected virtual void OnItemInserted(ItemInsertedEventArgs<object> e)
		{
			if (this.ItemInserted != null)
			{
				this.ItemInserted(this, e);
			}
		}

		protected virtual void OnItemRemoved(ItemRemovedEventArgs<object> e)
		{
			if (this.ItemRemoved != null)
			{
				this.ItemRemoved(this, e);
			}
		}

		protected virtual void OnItemChanged(ItemChangedEventArgs<object> e)
		{
			if (this.ItemChanged != null)
			{
				this.ItemChanged(this, e);
			}
		}

		protected virtual void OnReloaded(EventArgs e)
		{
			if (this.Reloaded != null)
			{
				this.Reloaded(this, e);
			}
		}

		public IEnumerable<T> GetSelection<T>() where T : class
		{
			return m_selection.AsIEnumerable<T>();
		}

		public T GetLastSelected<T>() where T : class
		{
			return m_selection.GetLastSelected<T>();
		}

		public bool SelectionContains(object item)
		{
			return m_selection.Contains(item);
		}

		private void TheSelectionChanging(object sender, EventArgs e)
		{
			this.SelectionChanging.Raise(this, EventArgs.Empty);
		}

		private void TheSelectionChanged(object sender, EventArgs e)
		{
			this.SelectionChanged.Raise(this, EventArgs.Empty);
		}
	}

	private SplitContainer m_splitContainer;

	private TreeViewContext m_treeContext;

	private ISelectionContext m_treeSelectionContext;

	private TreeControlAdapter m_treeControlAdapter;

	private TreeControl m_treeControl;

	private ListViewContext m_listContext;

	private ListView m_listView;

	private ListViewAdapter m_listViewAdapter;

	private ThumbnailControl m_thumbnailControl;

	private readonly List<Uri> m_requestedThumbs = new List<Uri>();

	private object m_lastHit;

	private Point m_hitPoint;

	private Point m_assetDialogLocation;

	private Size m_assetDialogSize;

	private bool m_dragging;

	private bool m_selecting;

	private StandardControlGroup m_controlGroup = StandardControlGroup.Bottom;

	private readonly ICommandService m_commandService;

	private readonly IControlHostService m_controlHostService;

	private readonly ISettingsService m_settingsService;

	private readonly ThumbnailService m_thumbnailService;

	public StandardControlGroup ControlGroup
	{
		get
		{
			return m_controlGroup;
		}
		set
		{
			m_controlGroup = value;
		}
	}

	public TreeControl TreeControl => m_treeControl;

	public ListView ListView => m_listView;

	public ThumbnailControl ThumbnailControl => m_thumbnailControl;

	private Control ActiveItemControl
	{
		get
		{
			if (m_thumbnailControl.Visible)
			{
				return m_thumbnailControl;
			}
			return m_listView;
		}
	}

	public string AssetListViewMode
	{
		get
		{
			if (m_thumbnailControl.Visible)
			{
				return "Thumbnail";
			}
			return "Details";
		}
		set
		{
			if ("Details" == value)
			{
				DoCommand(Command.DetailsView);
			}
			else if ("Thumbnail" == value)
			{
				DoCommand(Command.ThumbnailView);
			}
		}
	}

	public Point AssetDialogLocation
	{
		get
		{
			return m_assetDialogLocation;
		}
		set
		{
			m_assetDialogLocation = value;
		}
	}

	public Size AssetDialogSize
	{
		get
		{
			return m_assetDialogSize;
		}
		set
		{
			m_assetDialogSize = value;
		}
	}

	public string ListViewSettings
	{
		get
		{
			return m_listViewAdapter.Settings;
		}
		set
		{
			m_listViewAdapter.Settings = value;
		}
	}

	public int SplitterDistance
	{
		get
		{
			return m_splitContainer.SplitterDistance;
		}
		set
		{
			m_splitContainer.SplitterDistance = value;
		}
	}

	[ImportingConstructor]
	public ResourceLister(ICommandService commandService, IControlHostService controlHostService, ISettingsService settingsService, ThumbnailService thumbnailService)
	{
		m_commandService = commandService;
		m_controlHostService = controlHostService;
		m_settingsService = settingsService;
		m_thumbnailService = thumbnailService;
		m_thumbnailService.ThumbnailReady += ThumbnailManager_ThumbnailReady;
	}

	public void SetRootFolder(IResourceFolder rootFolder)
	{
		m_treeContext = new TreeViewContext(rootFolder);
		m_treeSelectionContext = m_treeContext.As<ISelectionContext>();
		m_treeSelectionContext.SelectionChanged += TreeSelectionChanged;
		m_treeControlAdapter.TreeView = m_treeContext;
		m_listContext = new ListViewContext();
		m_listViewAdapter.ListView = m_listContext;
		m_treeControlAdapter.Refresh(rootFolder);
	}

	public void Initialize()
	{
		m_treeControl = new TreeControl();
		m_treeControl.Dock = DockStyle.Fill;
		m_treeControl.AllowDrop = true;
		m_treeControl.SelectionMode = SelectionMode.MultiExtended;
		m_treeControl.ImageList = ResourceUtil.GetImageList16();
		m_treeControl.StateImageList = ResourceUtil.GetImageList16();
		m_treeControl.DragOver += treeControl_DragOver;
		m_treeControl.DragDrop += treeControl_DragDrop;
		m_treeControl.MouseUp += treeControl_MouseUp;
		m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
		m_listView = new ListView();
		m_listView.View = View.Details;
		m_listView.Dock = DockStyle.Fill;
		m_listView.AllowDrop = true;
		m_listView.LabelEdit = false;
		m_listView.MouseUp += thumbnailControl_MouseUp;
		m_listView.MouseMove += thumbnailControl_MouseMove;
		m_listView.MouseLeave += thumbnailControl_MouseLeave;
		m_listView.DragOver += thumbnailControl_DragOver;
		m_listViewAdapter = new ListViewAdapter(m_listView);
		m_thumbnailControl = new ThumbnailControl();
		m_thumbnailControl.Dock = DockStyle.Fill;
		m_thumbnailControl.AllowDrop = true;
		m_thumbnailControl.BackColor = SystemColors.Window;
		m_thumbnailControl.SelectionChanged += thumbnailControl_SelectionChanged;
		m_thumbnailControl.MouseMove += thumbnailControl_MouseMove;
		m_thumbnailControl.MouseUp += thumbnailControl_MouseUp;
		m_thumbnailControl.MouseLeave += thumbnailControl_MouseLeave;
		m_thumbnailControl.DragOver += thumbnailControl_DragOver;
		m_splitContainer = new SplitContainer();
		m_splitContainer.Name = "Resources".Localize();
		m_splitContainer.Orientation = Orientation.Vertical;
		m_splitContainer.Panel1.Controls.Add(m_treeControl);
		m_splitContainer.Panel2.Controls.Add(m_thumbnailControl);
		m_splitContainer.Panel2.Controls.Add(m_listView);
		m_splitContainer.SplitterDistance = 10;
		m_listView.Hide();
		Image image = ResourceUtil.GetImage16(Resources.ResourceImage);
		m_controlHostService.RegisterControl(m_splitContainer, new ControlInfo("Resources".Localize(), "Lists available resources".Localize(), StandardControlGroup.Left, image, null), this);
		RegisterCommands(m_commandService);
		RegisterSettings();
	}

	public void Refresh()
	{
		if (m_treeControl != null)
		{
			RefreshThumbnails();
		}
	}

	private void TreeSelectionChanged(object sender, EventArgs e)
	{
		ISelectionContext selectionContext = sender.As<ISelectionContext>();
		if (selectionContext != null)
		{
			IResourceFolder resourceFolder = selectionContext.LastSelected.As<IResourceFolder>();
			if (resourceFolder != null)
			{
				m_listContext.SelectedFolder = resourceFolder;
				m_listViewAdapter.ListView = m_listViewAdapter.ListView;
				RefreshThumbnails();
			}
		}
	}

	private void treeControl_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			Point p = new Point(e.X, e.Y);
			List<object> list = new List<object>();
			list.Add(Command.DetailsView);
			list.Add(Command.ThumbnailView);
			Point screenPoint = m_treeControl.PointToScreen(p);
			m_commandService.RunContextMenu(list, screenPoint);
		}
	}

	private void treeControl_DragOver(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.None;
		Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
		m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);
		IResourceFolder hitFolder = GetHitFolder();
		if (hitFolder != null && !hitFolder.Folders.IsReadOnly)
		{
			e.Effect = DragDropEffects.Move;
		}
	}

	private void treeControl_DragDrop(object sender, DragEventArgs e)
	{
		Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
		m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);
		if (!(e.Data.GetData(DataFormats.FileDrop) is string[] array))
		{
			return;
		}
		IResourceFolder hitFolder = GetHitFolder();
		if (hitFolder != null)
		{
			string[] array2 = array;
			foreach (string uriString in array2)
			{
				CreateFolder(hitFolder, new Uri(uriString));
			}
		}
		m_treeControlAdapter.Refresh(m_treeContext.RootFolder);
	}

	private IResourceFolder GetHitFolder()
	{
		IResourceFolder resourceFolder = m_lastHit.As<IResourceFolder>();
		if (resourceFolder == null)
		{
			resourceFolder = m_treeContext.RootFolder;
		}
		return resourceFolder;
	}

	private IResourceFolder CreateFolder(IResourceFolder parentFolder, Uri uri)
	{
		return AddFolder(parentFolder, new DirectoryInfo(uri.LocalPath));
	}

	private IResourceFolder AddFolder(IResourceFolder parentFolder, DirectoryInfo dirInfo)
	{
		IResourceFolder resourceFolder = parentFolder.CreateFolder();
		if (resourceFolder != null)
		{
			resourceFolder.Name = dirInfo.Name;
			if (parentFolder != null)
			{
				IList<IResourceFolder> folders = parentFolder.Folders;
				if (!folders.IsReadOnly)
				{
					folders.Add(resourceFolder);
				}
			}
			DirectoryInfo[] directories = dirInfo.GetDirectories();
			foreach (DirectoryInfo dirInfo2 in directories)
			{
				AddFolder(resourceFolder, dirInfo2);
			}
			FileInfo[] files = dirInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				if (!fileInfo.Name.StartsWith("~"))
				{
					IList<Uri> resourceUris = resourceFolder.ResourceUris;
					if (!resourceUris.IsReadOnly)
					{
						Uri item = new Uri(fileInfo.FullName);
						resourceFolder.ResourceUris.Add(item);
					}
				}
			}
		}
		return resourceFolder;
	}

	private void thumbnailControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (m_dragging || e.Button != MouseButtons.Left)
		{
			return;
		}
		Size dragSize = SystemInformation.DragSize;
		if (Math.Abs(m_hitPoint.X - e.X) < dragSize.Width && Math.Abs(m_hitPoint.Y - e.Y) < dragSize.Height)
		{
			return;
		}
		m_dragging = true;
		List<string> list = new List<string>();
		Uri clickedItemUri = GetClickedItemUri(ActiveItemControl, new Point(e.X, e.Y));
		if (clickedItemUri != null && clickedItemUri.IsAbsoluteUri)
		{
			list.Add(clickedItemUri.LocalPath);
		}
		foreach (Uri selectedItemUri in GetSelectedItemUris(ActiveItemControl))
		{
			if (selectedItemUri != clickedItemUri && selectedItemUri.IsAbsoluteUri)
			{
				list.Add(selectedItemUri.LocalPath);
			}
		}
		if (list.Count > 0)
		{
			IDataObject dataObject = new DataObject();
			dataObject.SetData(DataFormats.FileDrop, autoConvert: true, list.ToArray());
			ActiveItemControl.DoDragDrop(dataObject, DragDropEffects.Move);
		}
	}

	private void thumbnailControl_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			Point p = new Point(e.X, e.Y);
			Uri clickedItemUri = GetClickedItemUri(ActiveItemControl, new Point(e.X, e.Y));
			if (clickedItemUri != null)
			{
				List<object> list = new List<object>();
				list.Add(Command.DetailsView);
				list.Add(Command.ThumbnailView);
				Point screenPoint = m_thumbnailControl.PointToScreen(p);
				m_commandService.RunContextMenu(list, screenPoint);
			}
			else
			{
				List<object> list2 = new List<object>();
				list2.Add(Command.DetailsView);
				list2.Add(Command.ThumbnailView);
				Point screenPoint2 = m_thumbnailControl.PointToScreen(p);
				m_commandService.RunContextMenu(list2, screenPoint2);
			}
		}
		m_dragging = false;
	}

	private void thumbnailControl_MouseLeave(object sender, EventArgs e)
	{
		m_dragging = false;
	}

	private void thumbnailControl_DragOver(object sender, DragEventArgs e)
	{
	}

	private IEnumerable<Uri> GetSelectedItemUris(Control control)
	{
		if (control is ThumbnailControl thumbnailControl)
		{
			foreach (ThumbnailControlItem item in thumbnailControl.Selection)
			{
				Uri uri = GetUriFromTag(item.Tag);
				if (uri != null)
				{
					yield return uri;
				}
			}
		}
		else
		{
			if (!(control is ListView fileListView))
			{
				yield break;
			}
			foreach (ListViewItem item2 in fileListView.SelectedItems)
			{
				Uri uri2 = GetUriFromTag(item2.Tag);
				if (uri2 != null)
				{
					yield return uri2;
				}
			}
		}
	}

	private Uri GetClickedItemUri(Control control, Point point)
	{
		Uri result = null;
		if (control is ThumbnailControl thumbnailControl)
		{
			ThumbnailControlItem thumbnailControlItem = thumbnailControl.PickThumbnail(point);
			if (thumbnailControlItem != null)
			{
				result = GetUriFromTag(thumbnailControlItem.Tag);
			}
		}
		else if (control is ListView listView && listView.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listView.SelectedItems[0];
			if (listViewItem != null)
			{
				result = GetUriFromTag(listViewItem.Tag);
			}
		}
		return result;
	}

	private void thumbnailControl_SelectionChanged(object sender, EventArgs e)
	{
		if (m_selecting)
		{
			return;
		}
		try
		{
			m_selecting = true;
			List<Path<object>> list = new List<Path<object>>();
			foreach (ThumbnailControlItem item in m_thumbnailControl.Selection)
			{
				if (item.Tag is IResource last)
				{
					list.Add(new AdaptablePath<object>(last));
				}
			}
		}
		finally
		{
			m_selecting = false;
		}
	}

	private void ThumbnailManager_ThumbnailReady(object sender, ThumbnailReadyEventArgs e)
	{
		Uri resourceUri = e.ResourceUri;
		if (m_requestedThumbs.Contains(resourceUri))
		{
			ThumbnailControlItem item = GetItem(resourceUri);
			if (item == null)
			{
				item = NewItem(resourceUri, e.Image);
				m_thumbnailControl.Items.Add(item);
			}
			else
			{
				item.Image = e.Image;
			}
			m_requestedThumbs.Remove(resourceUri);
		}
	}

	public bool CanDoCommand(object commandTag)
	{
		if (m_treeContext == null || m_treeContext.Root == null)
		{
			return false;
		}
		if (commandTag is Command command)
		{
			return command switch
			{
				Command.DetailsView => !m_listView.Visible, 
				Command.ThumbnailView => !m_thumbnailControl.Visible, 
				_ => false, 
			};
		}
		return false;
	}

	public void DoCommand(object commandTag)
	{
		if (commandTag is Command command)
		{
			switch (command)
			{
			case Command.DetailsView:
				m_thumbnailControl.Hide();
				m_listView.Show();
				RefreshThumbnails();
				break;
			case Command.ThumbnailView:
				m_listView.Hide();
				m_thumbnailControl.Show();
				RefreshThumbnails();
				break;
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}

	public void Activate(Control control)
	{
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		return true;
	}

	private void RefreshThumbnails()
	{
		if (m_listContext == null)
		{
			return;
		}
		IResourceFolder selectedFolder = m_listContext.SelectedFolder;
		if (selectedFolder == null)
		{
			return;
		}
		m_thumbnailControl.Items.Clear();
		foreach (Uri resourceUri in selectedFolder.ResourceUris)
		{
			AddThumbnail(resourceUri);
		}
	}

	private void AddThumbnail(Uri resourceUri)
	{
		if (m_thumbnailControl.Visible)
		{
			m_requestedThumbs.Add(resourceUri);
			m_thumbnailService.ResolveThumbnail(resourceUri);
			string localPath = resourceUri.LocalPath;
			string fileName = Path.GetFileName(localPath);
			Icon fileIcon = FileIconUtil.GetFileIcon(fileName, FileIconUtil.IconSize.Large, linkOverlay: false);
			Bitmap image = fileIcon.ToBitmap();
			ThumbnailControlItem item = GetItem(resourceUri);
			if (item == null)
			{
				item = NewItem(resourceUri, image);
				m_thumbnailControl.Items.Add(item);
			}
			else
			{
				item.Image = image;
			}
		}
		else if (m_listView.Visible)
		{
			string originalString = resourceUri.OriginalString;
			string localPath2 = resourceUri.LocalPath;
		}
	}

	private ThumbnailControlItem GetItem(Uri resourceUri)
	{
		foreach (ThumbnailControlItem item in m_thumbnailControl.Items)
		{
			Uri uriFromTag = GetUriFromTag(item.Tag);
			if (uriFromTag == resourceUri)
			{
				return item;
			}
		}
		return null;
	}

	private ThumbnailControlItem NewItem(Uri resourceUri, Image image)
	{
		ThumbnailControlItem thumbnailControlItem = new ThumbnailControlItem(image);
		thumbnailControlItem.Tag = resourceUri;
		thumbnailControlItem.Name = Path.GetFileName(resourceUri.LocalPath);
		thumbnailControlItem.Description = resourceUri.LocalPath;
		return thumbnailControlItem;
	}

	private void RegisterCommands(ICommandService commandService)
	{
		m_commandService.RegisterCommand(Command.DetailsView, null, StandardCommandGroup.FileOther, "Details View".Localize(), "Switch to details view".Localize(), this);
		m_commandService.RegisterCommand(Command.ThumbnailView, null, StandardCommandGroup.FileOther, "Thumbnail View".Localize(), "Switch to thumbnail view".Localize(), this);
	}

	private void RegisterSettings()
	{
		m_settingsService.RegisterSettings(GetType().ToString(), new BoundPropertyDescriptor(this, () => AssetListViewMode, "AssetListViewMode", null, null), new BoundPropertyDescriptor(this, () => AssetDialogSize, "AssetDialogSize", null, null), new BoundPropertyDescriptor(this, () => AssetDialogLocation, "AssetDialogLocation", null, null), new BoundPropertyDescriptor(this, () => SplitterDistance, "SplitterDistance", null, null), new BoundPropertyDescriptor(this, () => ListViewSettings, "ListViewSettings", null, null));
	}

	private static Uri GetUriFromTag(object tag)
	{
		Uri uri = tag as Uri;
		if (uri == null)
		{
			Path<object> path = tag as Path<object>;
			if (path != null)
			{
				uri = path.Last as Uri;
			}
		}
		return uri;
	}
}

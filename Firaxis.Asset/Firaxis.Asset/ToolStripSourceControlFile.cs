using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Asset.Properties;
using Firaxis.Error;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class ToolStripSourceControlFile : ToolStripStatusLabel
{
	private ContextMenuStrip contextMenuStrip;

	private IContainer components;

	private ToolStripMenuItem menuRefresh;

	private ToolStripMenuItem menuEnableEdit;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem menuCheckOut;

	private ToolStripMenuItem menuAddSC;

	private ToolStripMenuItem menuGetLatest;

	private ToolStripMenuItem menuUndoCheckout;

	private ToolStripSeparator toolStripSeparator2;

	private IAssetProvider assetProvider;

	public IAssetProvider AssetProvider
	{
		get
		{
			return assetProvider;
		}
		set
		{
			if (assetProvider != null)
			{
				assetProvider.ModifiedChanged -= assetProvider_ModifiedChanged;
				assetProvider.EnableEditChanged -= assetProvider_ModifiedChanged;
			}
			assetProvider = value;
			if (assetProvider != null)
			{
				assetProvider.ModifiedChanged += assetProvider_ModifiedChanged;
				assetProvider.EnableEditChanged += assetProvider_ModifiedChanged;
			}
			Refresh();
		}
	}

	private bool HasFile => AssetProvider != null && AssetProvider.FileAsset != null;

	public bool AllowEmptyEdit
	{
		get
		{
			if (AssetProvider != null)
			{
				AssetEditStyleAttribute attribute = ReflectionHelper.GetAttribute<AssetEditStyleAttribute>(AssetProvider);
				return HasFile || (attribute != null && (attribute.EditStyle & AssetEditStyle.NoEmptyEdit) == 0);
			}
			return false;
		}
	}

	public event EventHandler FileGetLatest;

	public event EventHandler FileCheckout;

	public event EventHandler FileUndoCheckout;

	public event EventHandler FileAddToSourceControl;

	public ToolStripSourceControlFile()
	{
		InitializeComponent();
		DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
		Text = "Ready";
		base.ToolTipText = "Dbl-Click to enable edit or Right-Click for options";
		base.IsLink = true;
	}

	public ToolStripSourceControlFile(IAssetProvider asset)
		: this()
	{
		AssetProvider = asset;
	}

	private void assetProvider_ModifiedChanged(object sender, EventArgs e)
	{
		Refresh();
	}

	private void Refresh()
	{
		string text;
		if (assetProvider != null && assetProvider.FileAsset != null)
		{
			SourceControlFile fileAsset = assetProvider.FileAsset;
			fileAsset.Refresh();
			Image = fileAsset.StatusImage;
			text = fileAsset.StatusText;
			if (assetProvider.EnableEdit)
			{
				text += " (Editing)";
			}
		}
		else
		{
			Image = SourceControlFile.DefaultStatusImage;
			text = "Ready";
		}
		Text = text;
	}

	private void InitializeComponent()
	{
		components = new Container();
		contextMenuStrip = new ContextMenuStrip(components);
		menuEnableEdit = new ToolStripMenuItem();
		toolStripSeparator1 = new ToolStripSeparator();
		menuAddSC = new ToolStripMenuItem();
		menuCheckOut = new ToolStripMenuItem();
		menuGetLatest = new ToolStripMenuItem();
		menuUndoCheckout = new ToolStripMenuItem();
		toolStripSeparator2 = new ToolStripSeparator();
		menuRefresh = new ToolStripMenuItem();
		contextMenuStrip.SuspendLayout();
		contextMenuStrip.Items.AddRange(new ToolStripItem[8] { menuEnableEdit, toolStripSeparator1, menuAddSC, menuCheckOut, menuGetLatest, menuUndoCheckout, toolStripSeparator2, menuRefresh });
		contextMenuStrip.Name = "contextMenuStrip1";
		contextMenuStrip.Size = new Size(193, 148);
		contextMenuStrip.Opening += contextMenuStrip_Opening;
		menuEnableEdit.Image = Resources.enable_edit;
		menuEnableEdit.Name = "menuEnableEdit";
		menuEnableEdit.Size = new Size(192, 22);
		menuEnableEdit.Text = "Enable Edit";
		menuEnableEdit.Click += menuEnableEdit_Click;
		toolStripSeparator1.Name = "toolStripSeparator1";
		toolStripSeparator1.Size = new Size(189, 6);
		menuAddSC.Image = Resources.sc_addsc;
		menuAddSC.Name = "menuAddSC";
		menuAddSC.Size = new Size(192, 22);
		menuAddSC.Text = "Add to Source Control";
		menuAddSC.Click += menuAddSC_Click;
		menuCheckOut.Image = Resources.sc_checkout;
		menuCheckOut.Name = "menuCheckOut";
		menuCheckOut.Size = new Size(192, 22);
		menuCheckOut.Text = "Check Out";
		menuCheckOut.Click += menuCheckOut_Click;
		menuGetLatest.Image = Resources.sc_getlatest;
		menuGetLatest.Name = "menuGetLatest";
		menuGetLatest.Size = new Size(192, 22);
		menuGetLatest.Text = "Get Latest";
		menuGetLatest.Click += menuGetLatest_Click;
		menuUndoCheckout.Image = Resources.sc_undocheckout;
		menuUndoCheckout.Name = "menuUndoCheckout";
		menuUndoCheckout.Size = new Size(192, 22);
		menuUndoCheckout.Text = "Undo Checkout";
		menuUndoCheckout.Click += menuUndoCheckout_Click;
		toolStripSeparator2.Name = "toolStripSeparator2";
		toolStripSeparator2.Size = new Size(189, 6);
		menuRefresh.Image = Resources.file_refresh;
		menuRefresh.Name = "menuRefresh";
		menuRefresh.Size = new Size(192, 22);
		menuRefresh.Text = "Refresh";
		menuRefresh.Click += menuRefresh_Click;
		base.DoubleClickEnabled = true;
		base.IsLink = true;
		base.LinkBehavior = LinkBehavior.HoverUnderline;
		base.MouseUp += ToolStripSourceControlFile_MouseUp;
		base.DoubleClick += ToolStripSourceControlFile_DoubleClick;
		base.Click += ToolStripSourceControlFile_Click;
		contextMenuStrip.ResumeLayout(performLayout: false);
	}

	private void ToolStripSourceControlFile_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			contextMenuStrip.Show(Cursor.Position);
		}
	}

	private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
	{
		bool hasFile = HasFile;
		SourceControlFile sourceControlFile = (hasFile ? assetProvider.FileAsset : null);
		menuRefresh.Enabled = hasFile;
		menuEnableEdit.Checked = assetProvider != null && assetProvider.EnableEdit;
		menuEnableEdit.Enabled = hasFile || AllowEmptyEdit;
		menuCheckOut.Enabled = hasFile && sourceControlFile.Controlled && !sourceControlFile.Owner;
		menuGetLatest.Enabled = hasFile && sourceControlFile.Controlled && !sourceControlFile.Owner && !assetProvider.Modified && !assetProvider.EnableEdit && !sourceControlFile.HaveLatest;
		menuAddSC.Enabled = Firaxis.Utility.Available.SourceControl && hasFile && !sourceControlFile.Controlled && Context.Get<SourceControl>().IsConnected && !sourceControlFile.NewAdd;
		menuUndoCheckout.Enabled = hasFile && sourceControlFile.Controlled && sourceControlFile.Owner && !assetProvider.Modified && !assetProvider.EnableEdit;
	}

	private void menuRefresh_Click(object sender, EventArgs e)
	{
		Refresh();
	}

	private void menuAddSC_Click(object sender, EventArgs e)
	{
		if (HasFile)
		{
			string name = AssetProvider.FileAsset.File.Name;
			try
			{
				AssetProvider.FileAsset.Add(string.Empty);
				this.FileAddToSourceControl?.Invoke(sender, e);
				Refresh();
			}
			catch (Exception e2)
			{
				ExceptionLogger.Log(e2, "Adding file to source control");
				MessageBox.Show("Unable to add file '" + name + "' to source control. Check revision before attempting to add.", "File Add", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}

	private void menuCheckOut_Click(object sender, EventArgs e)
	{
		if (HasFile)
		{
			string name = AssetProvider.FileAsset.File.Name;
			try
			{
				AssetProvider.FileAsset.Checkout(GetChangeList(AssetProvider));
				this.FileCheckout?.Invoke(sender, e);
				Refresh();
			}
			catch (Exception e2)
			{
				ExceptionLogger.Log(e2, "File checkout");
				MessageBox.Show("Unable to checkout file '" + name + "'. Check revision before attempting to edit.", "File Checkout", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}

	private void menuEnableEdit_Click(object sender, EventArgs e)
	{
		if (EnableEdit(AssetProvider))
		{
			Refresh();
		}
	}

	public static string GetChangeList(IAssetProvider assetProvider)
	{
		if (assetProvider != null)
		{
			if (assetProvider.ParentAsset != null && assetProvider.ParentAsset.FileAsset != null)
			{
				return assetProvider.ParentAsset.FileAsset.ChangeList;
			}
			if (assetProvider.FileAsset != null)
			{
				return assetProvider.FileAsset.ChangeList;
			}
		}
		return string.Empty;
	}

	public static bool EnableEdit(SourceControlFile file, string sChangeList)
	{
		return EnableEdit(file, sChangeList, bShowPrompts: true);
	}

	public static bool EnableEdit(SourceControlFile file, string sChangeList, bool bShowPrompts)
	{
		if (file == null)
		{
			return false;
		}
		if (file.Controlled)
		{
			if (!file.HaveLatest)
			{
				try
				{
					file.GetLatest();
				}
				catch (Exception e)
				{
					ExceptionLogger.Log(e, "File Get Latest");
					MessageBox.Show("Unable to get the latest version of the file '" + file.File.Name + "'. Check revision before attempting to edit.", "Enable Edit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return false;
				}
			}
			if (!file.Owner)
			{
				if (bShowPrompts)
				{
					DialogResult dialogResult = MessageBox.Show("The file '" + file.File.Name + "' is under source control but is not checked out. Would you like to check out the file?", "Enable Edit", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
					if (dialogResult == DialogResult.No)
					{
						return false;
					}
				}
				try
				{
					file.Checkout(sChangeList);
				}
				catch (Exception e2)
				{
					ExceptionLogger.Log(e2, "File checkout");
					MessageBox.Show("Unable to checkout file '" + file.File.Name + "'. Check revision before attempting to edit.", "Enable Edit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return false;
				}
			}
		}
		else if (file.File.Exists && file.File.IsReadOnly)
		{
			if (bShowPrompts)
			{
				DialogResult dialogResult2 = MessageBox.Show("The file '" + file.File.Name + "' is marked as read-only. Would you like to edit the file?", "Enable Edit", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
				if (dialogResult2 == DialogResult.No)
				{
					return false;
				}
			}
			try
			{
				file.File.IsReadOnly = false;
			}
			catch (Exception exception)
			{
				ErrorHandling.Error(exception, ErrorLevel.Log);
				MessageBox.Show("Unable to change read-only attribute of the file '" + file.File.Name + "'. Check file before attempting to edit.", "Enable Edit", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
		}
		return true;
	}

	public static bool EnableEdit(IAssetProvider assetProvider)
	{
		if (assetProvider != null && !assetProvider.EnableEdit)
		{
			if (!EnableEdit(assetProvider.FileAsset, GetChangeList(assetProvider)))
			{
				return false;
			}
			assetProvider.EnableEdit = true;
			return true;
		}
		return assetProvider?.EnableEdit ?? false;
	}

	private void menuGetLatest_Click(object sender, EventArgs e)
	{
		if (HasFile)
		{
			string name = AssetProvider.FileAsset.File.Name;
			try
			{
				AssetProvider.FileAsset.GetLatest();
				this.FileGetLatest?.Invoke(sender, e);
				AssetProvider.Reload();
				Refresh();
			}
			catch (Exception e2)
			{
				ExceptionLogger.Log(e2, "File Get Latest");
				MessageBox.Show("Unable to get the latest version of the file '" + name + "'. Check revision before attempting to edit.", "Get Latest", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}

	private void menuUndoCheckout_Click(object sender, EventArgs e)
	{
		if (HasFile)
		{
			string name = AssetProvider.FileAsset.File.Name;
			try
			{
				AssetProvider.FileAsset.UndoCheckout();
				this.FileUndoCheckout?.Invoke(sender, e);
				AssetProvider.Reload();
				Refresh();
			}
			catch (Exception e2)
			{
				ExceptionLogger.Log(e2, "Undo file checkout");
				MessageBox.Show("Unable to undo checkout of the file '" + name + "'. Check revision before attempting to edit.", "Undo File", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}

	private void ToolStripSourceControlFile_DoubleClick(object sender, EventArgs e)
	{
		if (AllowEmptyEdit)
		{
			menuEnableEdit_Click(sender, e);
		}
	}

	private void ToolStripSourceControlFile_Click(object sender, EventArgs e)
	{
		contextMenuStrip.Show(Cursor.Position);
	}
}

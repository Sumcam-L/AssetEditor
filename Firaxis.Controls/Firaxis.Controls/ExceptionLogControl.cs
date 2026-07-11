using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Collections;
using Firaxis.Error;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class ExceptionLogControl : UserControl
{
	private OperationResultLevel filterLevel;

	private IContainer components = null;

	private SplitterControl splitterControl;

	private ListView listView;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private RichTextBox richTextBox;

	private ImageList imageList1;

	public bool Attached { get; private set; }

	public OperationResultLevel FilterLevel
	{
		get
		{
			return filterLevel;
		}
		set
		{
			filterLevel = value;
			PopulateList();
		}
	}

	public ExceptionLogControl()
	{
		InitializeComponent();
		filterLevel = OperationResultLevel.None;
		richTextBox.MaxLength = int.MaxValue;
	}

	public void Attach()
	{
		if (!Attached)
		{
			Attached = true;
			PopulateList();
			ExceptionLogger.ExceptionLog.AddedItem += ExceptionLog_AddedItem;
			ExceptionLogger.ExceptionLog.ClearedItems += ExceptionLog_ClearedItems;
		}
	}

	public void Detach()
	{
		if (Attached)
		{
			ExceptionLogger.ExceptionLog.AddedItem -= ExceptionLog_AddedItem;
			ExceptionLogger.ExceptionLog.ClearedItems -= ExceptionLog_ClearedItems;
			listView.Items.Clear();
			Attached = false;
		}
	}

	private void PopulateList()
	{
		listView.BeginUpdate();
		listView.Items.Clear();
		lock (ExceptionLogger.ExceptionLog.LockObject)
		{
			foreach (ExceptionLogger.ExceptionEntry item in ExceptionLogger.ExceptionLog)
			{
				AddEntry(item);
			}
		}
		listView.EndUpdate();
		richTextBox.Text = "";
	}

	private void AddEntry(ExceptionLogger.ExceptionEntry entry)
	{
		if (entry.Level >= filterLevel)
		{
			string text = entry.Time.ToShortDateString() + " " + entry.Time.ToShortTimeString();
			ListViewItem listViewItem = listView.Items.Add(text, (int)entry.Level);
			listViewItem.SubItems.Add(entry.Caption);
			listViewItem.Tag = entry;
		}
	}

	private void ExceptionLog_AddedItem(object sender, ListEvent<ExceptionLogger.ExceptionEntry>.ListEventArgs e)
	{
		if (e.Item != null)
		{
			AddEntry(e.Item);
		}
		if (e.Collection == null)
		{
			return;
		}
		foreach (ExceptionLogger.ExceptionEntry item in e.Collection)
		{
			AddEntry(e.Item);
		}
	}

	private void ExceptionLog_ClearedItems(object sender, ListEvent<ExceptionLogger.ExceptionEntry>.ListEventArgs e)
	{
		PopulateList();
	}

	private void listView_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listView.SelectedItems.Count > 0)
		{
			ListViewItem listViewItem = listView.SelectedItems[0];
			ExceptionLogger.ExceptionEntry exceptionEntry = (ExceptionLogger.ExceptionEntry)listViewItem.Tag;
			string empty = string.Empty;
			if (exceptionEntry.Exception != null)
			{
				empty = $"<color name=\"FireBrick\"><b>{ReflectionHelper.GetDisplayName(exceptionEntry.Exception)}</b></color><br>{exceptionEntry.Note}<br>{exceptionEntry.Exception.Message}<br><i>{exceptionEntry.Exception.StackTrace}</i>";
				if (exceptionEntry.Exception.InnerException != null)
				{
					empty += $"<br><br><color name=\"FireBrick\"><b>{ReflectionHelper.GetDisplayName(exceptionEntry.Exception.InnerException)}</b></color><br>{exceptionEntry.Exception.InnerException.Message}<br><i>{exceptionEntry.Exception.InnerException.StackTrace}</i>";
				}
			}
			else
			{
				empty = exceptionEntry.Note;
			}
			richTextBox.Rtf = StringHelper.HTMLToRTF(empty);
		}
		else
		{
			richTextBox.Text = "";
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.Controls.ExceptionLogControl));
		this.splitterControl = new Firaxis.Controls.SplitterControl();
		this.listView = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		this.richTextBox = new System.Windows.Forms.RichTextBox();
		this.splitterControl.Panel1.SuspendLayout();
		this.splitterControl.Panel2.SuspendLayout();
		this.splitterControl.SuspendLayout();
		base.SuspendLayout();
		this.splitterControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitterControl.Location = new System.Drawing.Point(0, 0);
		this.splitterControl.Name = "splitterControl";
		this.splitterControl.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitterControl.Panel1.Controls.Add(this.listView);
		this.splitterControl.Panel2.Controls.Add(this.richTextBox);
		this.splitterControl.Size = new System.Drawing.Size(533, 280);
		this.splitterControl.SplitterDistance = 210;
		this.splitterControl.TabIndex = 0;
		this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.columnHeader1, this.columnHeader2 });
		this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listView.FullRowSelect = true;
		this.listView.GridLines = true;
		this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.listView.HideSelection = false;
		this.listView.Location = new System.Drawing.Point(0, 0);
		this.listView.MultiSelect = false;
		this.listView.Name = "listView";
		this.listView.Size = new System.Drawing.Size(533, 210);
		this.listView.SmallImageList = this.imageList1;
		this.listView.TabIndex = 0;
		this.listView.UseCompatibleStateImageBehavior = false;
		this.listView.View = System.Windows.Forms.View.Details;
		this.listView.SelectedIndexChanged += new System.EventHandler(listView_SelectedIndexChanged);
		this.columnHeader1.Text = "Time";
		this.columnHeader1.Width = 147;
		this.columnHeader2.Text = "Error";
		this.columnHeader2.Width = 300;
		this.imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
		this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
		this.imageList1.Images.SetKeyName(0, "ver_none.png");
		this.imageList1.Images.SetKeyName(1, "ver_ok.png");
		this.imageList1.Images.SetKeyName(2, "ver_old.png");
		this.imageList1.Images.SetKeyName(3, "ver_new.png");
		this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.richTextBox.Location = new System.Drawing.Point(0, 0);
		this.richTextBox.Name = "richTextBox";
		this.richTextBox.ReadOnly = true;
		this.richTextBox.Size = new System.Drawing.Size(533, 66);
		this.richTextBox.TabIndex = 0;
		this.richTextBox.Text = "";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitterControl);
		base.Name = "ExceptionLogControl";
		base.Size = new System.Drawing.Size(533, 280);
		this.splitterControl.Panel1.ResumeLayout(false);
		this.splitterControl.Panel2.ResumeLayout(false);
		this.splitterControl.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

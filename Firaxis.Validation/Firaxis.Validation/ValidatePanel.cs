using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls;
using Firaxis.Error;
using Firaxis.Reflection;
using Firaxis.Utility;
using Firaxis.Validation.Properties;

namespace Firaxis.Validation;

public class ValidatePanel : UserControl
{
	private ValidatorProvider provider;

	private int sortCol;

	private bool sortAsc;

	private IContainer components = null;

	private CaptionControl captionControl;

	private ListView listView;

	private ColumnHeader columnCondition;

	private ColumnHeader columnDescription;

	private ImageList imageList;

	private ColumnHeader columnTest;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ValidatorProvider ValidatorProvider
	{
		get
		{
			return provider;
		}
		set
		{
			if (provider != null)
			{
				provider.ResultsCleared -= provider_ResultsCleared;
				provider.RunTestsEnd -= provider_RunTestsEnd;
			}
			provider = value;
			if (provider != null)
			{
				provider.ResultsCleared += provider_ResultsCleared;
				provider.RunTestsEnd += provider_RunTestsEnd;
				UpdateStatusCaption();
			}
		}
	}

	public ValidatePanel(ValidatorProvider provider)
	{
		InitializeComponent();
		ValidatorProvider = provider;
	}

	public ValidatePanel()
	{
		InitializeComponent();
		if (Context.TryGet<ValidatorProvider>(out var service))
		{
			ValidatorProvider = service;
		}
	}

	public void RefreshResults()
	{
		RebuildList();
		UpdateStatusCaption();
	}

	private void provider_RunTestsEnd(object sender, EventArgs e)
	{
		RefreshResults();
	}

	private void provider_ResultsCleared(object sender, EventArgs e)
	{
		listView.Items.Clear();
		UpdateStatusCaption();
	}

	public void UpdateStatusCaption()
	{
		if (ValidatorProvider != null && ValidatorProvider.UpdateStatus)
		{
			switch (ValidatorProvider.HighestResultLevel)
			{
			case OperationResultLevel.None:
				captionControl.Image = Resources.ver_none;
				captionControl.Caption = Resources.ValidateNone;
				break;
			case OperationResultLevel.Success:
				captionControl.Image = Resources.ver_ok;
				captionControl.Caption = Resources.ValidateSuccess;
				break;
			case OperationResultLevel.Warning:
				captionControl.Image = Resources.ver_old;
				captionControl.Caption = Resources.ValidateWarning;
				break;
			case OperationResultLevel.Error:
				captionControl.Image = Resources.ver_new;
				captionControl.Caption = Resources.ValidateError;
				break;
			}
		}
	}

	private void RebuildList()
	{
		listView.BeginUpdate();
		listView.Items.Clear();
		if (ValidatorProvider != null)
		{
			ValidatorProvider validatorProvider = ValidatorProvider;
			foreach (ValidatorProvider.ResultInfo result in validatorProvider.Results)
			{
				ListViewItem listViewItem = listView.Items.Add(result.Level.ToString(), (int)result.Level);
				listViewItem.SubItems.Add(result.Brief);
				listViewItem.SubItems.Add(ReflectionHelper.GetDisplayName(result.Validator));
				listViewItem.Tag = result;
			}
			columnDescription.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			columnTest.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}
		ListViewItemComparer.Sort(listView, sortCol, sortAsc);
		listView.EndUpdate();
	}

	private void listView_DoubleClick(object sender, EventArgs e)
	{
		if (listView.SelectedItems.Count > 0 && listView.SelectedItems[0].Tag is ValidatorProvider.ResultInfo resultInfo)
		{
			provider.ResultObject.Sender = resultInfo.Sender;
			resultInfo.Validator.Inspect(provider.ResultObject, resultInfo.Context);
		}
	}

	private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (sortCol == e.Column)
		{
			sortAsc = !sortAsc;
		}
		else
		{
			sortCol = e.Column;
			sortAsc = true;
		}
		ListViewItemComparer.Sort(listView, sortCol, sortAsc);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.Validation.ValidatePanel));
		this.listView = new System.Windows.Forms.ListView();
		this.columnCondition = new System.Windows.Forms.ColumnHeader();
		this.columnDescription = new System.Windows.Forms.ColumnHeader();
		this.columnTest = new System.Windows.Forms.ColumnHeader();
		this.imageList = new System.Windows.Forms.ImageList(this.components);
		this.captionControl = new Firaxis.Controls.CaptionControl();
		base.SuspendLayout();
		this.listView.AllowColumnReorder = true;
		this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.columnCondition, this.columnDescription, this.columnTest });
		this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listView.FullRowSelect = true;
		this.listView.GridLines = true;
		this.listView.HideSelection = false;
		this.listView.Location = new System.Drawing.Point(0, 0);
		this.listView.Name = "listView";
		this.listView.ShowItemToolTips = true;
		this.listView.Size = new System.Drawing.Size(426, 193);
		this.listView.SmallImageList = this.imageList;
		this.listView.TabIndex = 2;
		this.listView.UseCompatibleStateImageBehavior = false;
		this.listView.View = System.Windows.Forms.View.Details;
		this.listView.DoubleClick += new System.EventHandler(listView_DoubleClick);
		this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(listView_ColumnClick);
		this.columnCondition.Text = "Condition";
		this.columnCondition.Width = 80;
		this.columnDescription.Text = "Description";
		this.columnDescription.Width = 240;
		this.columnTest.Text = "Test";
		this.columnTest.Width = 100;
		this.imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
		this.imageList.TransparentColor = System.Drawing.Color.Transparent;
		this.imageList.Images.SetKeyName(0, "ver_none.png");
		this.imageList.Images.SetKeyName(1, "ver_ok.png");
		this.imageList.Images.SetKeyName(2, "ver_old.png");
		this.imageList.Images.SetKeyName(3, "ver_new.png");
		this.captionControl.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.captionControl.Caption = "Results";
		this.captionControl.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.captionControl.ForeColor = System.Drawing.Color.White;
		this.captionControl.Image = Firaxis.Validation.Properties.Resources.ver_none;
		this.captionControl.Location = new System.Drawing.Point(0, 193);
		this.captionControl.Name = "captionControl";
		this.captionControl.Size = new System.Drawing.Size(426, 22);
		this.captionControl.TabIndex = 1;
		this.captionControl.Transparent = System.Drawing.Color.Magenta;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.listView);
		base.Controls.Add(this.captionControl);
		base.Name = "ValidatePanel";
		base.Size = new System.Drawing.Size(426, 215);
		base.ResumeLayout(false);
	}
}

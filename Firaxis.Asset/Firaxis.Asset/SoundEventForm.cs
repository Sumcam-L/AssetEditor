using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class SoundEventForm : Form
{
	private class GeometrySettings
	{
		private SoundEventForm form;

		public int SoundEventForm_Column1Width
		{
			get
			{
				return form.listView.Columns[0].Width;
			}
			set
			{
				form.listView.Columns[0].Width = value;
			}
		}

		public int SoundEventForm_Column2Width
		{
			get
			{
				return form.listView.Columns[1].Width;
			}
			set
			{
				form.listView.Columns[1].Width = value;
			}
		}

		public int SoundEventForm_Column3Width
		{
			get
			{
				return form.listView.Columns[2].Width;
			}
			set
			{
				form.listView.Columns[2].Width = value;
			}
		}

		public int SoundEventForm_Column4Width
		{
			get
			{
				return form.listView.Columns[3].Width;
			}
			set
			{
				form.listView.Columns[3].Width = value;
			}
		}

		public int SoundEventForm_Width
		{
			get
			{
				return form.Width;
			}
			set
			{
				form.Width = value;
			}
		}

		public int SoundEventForm_Height
		{
			get
			{
				return form.Height;
			}
			set
			{
				form.Height = value;
			}
		}

		public int SoundEventForm_X
		{
			get
			{
				return form.Location.X;
			}
			set
			{
				form.Location = new Point(value, form.Location.Y);
			}
		}

		public int SoundEventForm_Y
		{
			get
			{
				return form.Location.Y;
			}
			set
			{
				form.Location = new Point(form.Location.Y, value);
			}
		}

		public GeometrySettings(SoundEventForm form)
		{
			this.form = form;
		}
	}

	private AudioScriptType filterType;

	private bool updating;

	private static string lastSoundNameUsed;

	private static AudioScriptType lastFilterType = AudioScriptType.Sound3d;

	private GeometrySettings geometrySettings;

	private IContainer components = null;

	private GroupBox groupBox1;

	private TextBox textFilter;

	private RadioButton radioAll;

	private RadioButton radio3d;

	private RadioButton radio2d;

	private Button buttonCancel;

	private Button buttonOk;

	private GroupBox groupBox2;

	private ListView listView;

	private Button buttonStop;

	private Button buttonPlay;

	private ColumnHeader columnHeader1;

	private ComboBox comboBox2;

	private ComboBox comboBox1;

	public List<string> UnitSwitches { get; set; }

	public List<string> TerrainSwitches { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string SoundEventName { get; set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public AudioScriptType FilterType
	{
		get
		{
			return filterType;
		}
		set
		{
			if (filterType != value)
			{
				filterType = value;
				PopulateList();
				buttonOk.Enabled = false;
				buttonPlay.Enabled = false;
				buttonStop.Enabled = false;
			}
			radioAll.Checked = false;
			radio2d.Checked = false;
			radio3d.Checked = false;
			switch (filterType)
			{
			case AudioScriptType.All:
				radioAll.Checked = true;
				break;
			case AudioScriptType.Sound2d:
				radio2d.Checked = true;
				break;
			case AudioScriptType.Sound3d:
				radio3d.Checked = true;
				break;
			}
		}
	}

	public SoundEventForm()
	{
		InitializeComponent();
		SoundEventName = "";
		PopulateList();
		SoundEventProvider soundEventProvider = Context.Get<SoundEventProvider>();
		IAudioPreviewer audioPreviewer = soundEventProvider.AudioPreviewer;
		UnitSwitches = new List<string>();
		TerrainSwitches = new List<string>();
		uint numSwitchSettings = audioPreviewer.GetNumSwitchSettings("Unit");
		for (uint num = 0u; num < numSwitchSettings; num++)
		{
			UnitSwitches.Add(audioPreviewer.GetSwitchSettingName("Unit", num));
		}
		uint numSwitchSettings2 = audioPreviewer.GetNumSwitchSettings("Terrain");
		for (uint num = 0u; num < numSwitchSettings2; num++)
		{
			TerrainSwitches.Add(audioPreviewer.GetSwitchSettingName("Terrain", num));
		}
		UnitSwitches.Sort();
		TerrainSwitches.Sort();
		for (uint num = 0u; num < numSwitchSettings; num++)
		{
			comboBox1.Items.Add(UnitSwitches[(int)num]);
		}
		for (uint num = 0u; num < numSwitchSettings2; num++)
		{
			comboBox2.Items.Add(TerrainSwitches[(int)num]);
		}
		if (comboBox1.SelectedIndex == -1)
		{
			comboBox1.SelectedIndex = 0;
		}
		if (comboBox2.SelectedIndex == -1)
		{
			comboBox2.SelectedIndex = 0;
		}
		if (lastSoundNameUsed == null)
		{
			lastSoundNameUsed = "";
		}
	}

	private void radio2d_Click(object sender, EventArgs e)
	{
		FilterType = AudioScriptType.Sound2d;
	}

	private void radio3d_Click(object sender, EventArgs e)
	{
		FilterType = AudioScriptType.Sound3d;
	}

	private void radioAll_Click(object sender, EventArgs e)
	{
		FilterType = AudioScriptType.All;
	}

	private void PopulateList()
	{
		updating = true;
		listView.BeginUpdate();
		listView.Items.Clear();
		string text = textFilter.Text;
		SoundEventProvider soundEventProvider = Context.Get<SoundEventProvider>();
		foreach (SoundEvent soundEvent in soundEventProvider.SoundEvents)
		{
			if ((string.IsNullOrEmpty(text) || Contains(soundEvent.Name, text)) && (FilterType == AudioScriptType.All || soundEvent.Type == FilterType))
			{
				ListViewItem listViewItem = listView.Items.Add(soundEvent.Name);
				listViewItem.SubItems.Add(soundEvent.Files.Count.ToString());
				listViewItem.SubItems.Add("0");
				listViewItem.SubItems.Add("0");
				listViewItem.Tag = soundEvent;
			}
		}
		listView.EndUpdate();
		updating = false;
	}

	private bool Contains(string name, string filter)
	{
		return name.ToLower().Contains(filter.ToLower());
	}

	private void textFilter_TextChanged(object sender, EventArgs e)
	{
		if (!updating)
		{
			PopulateList();
		}
	}

	private void buttonOk_Click(object sender, EventArgs e)
	{
		if (listView.SelectedItems.Count > 0)
		{
			SoundEventName = ((SoundEvent)listView.SelectedItems[0].Tag).Name;
			base.DialogResult = DialogResult.OK;
			SoundEventProvider soundEventProvider = Context.Get<SoundEventProvider>();
			IAudioPreviewer audioPreviewer = soundEventProvider.AudioPreviewer;
			if (comboBox1.SelectedItem != null)
			{
				audioPreviewer.SetPlaybackSwitch("Unit", comboBox1.SelectedItem.ToString());
			}
			if (comboBox2.SelectedItem != null)
			{
				audioPreviewer.SetPlaybackSwitch("Terrain", comboBox2.SelectedItem.ToString());
			}
		}
	}

	private void listView_SelectedIndexChanged(object sender, EventArgs e)
	{
		bool enabled = listView.SelectedItems.Count > 0;
		buttonOk.Enabled = enabled;
		buttonPlay.Enabled = enabled;
		buttonStop.Enabled = enabled;
	}

	private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		buttonOk_Click(sender, e);
	}

	private void buttonPlay_Click(object sender, EventArgs e)
	{
		if (listView.SelectedItems.Count > 0)
		{
			SoundEvent soundEvent = (SoundEvent)listView.SelectedItems[0].Tag;
			SoundEventProvider soundEventProvider = Context.Get<SoundEventProvider>();
			IAudioPreviewer audioPreviewer = soundEventProvider.AudioPreviewer;
			if (comboBox1.SelectedItem != null)
			{
				audioPreviewer.SetPlaybackSwitch("Unit", comboBox1.SelectedItem.ToString());
			}
			if (comboBox2.SelectedItem != null)
			{
				audioPreviewer.SetPlaybackSwitch("Terrain", comboBox2.SelectedItem.ToString());
			}
			Cursor = Cursors.WaitCursor;
			try
			{
				Context.Get<SoundEventProvider>().PlayScriptSound(soundEvent.Name);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}
	}

	private void buttonStop_Click(object sender, EventArgs e)
	{
		Context.Get<SoundEventProvider>().StopAllSounds();
	}

	private void SoundEventForm_Shown(object sender, EventArgs e)
	{
		if (SoundEventName != null && SoundEventName.Length == 0)
		{
			SoundEventName = lastSoundNameUsed;
		}
		geometrySettings = new GeometrySettings(this);
		FilterType = lastFilterType;
	}

	private void SoundEventForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (SoundEventName.Length != 0)
		{
			lastSoundNameUsed = SoundEventName;
		}
		lastFilterType = filterType;
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.radioAll = new System.Windows.Forms.RadioButton();
		this.radio3d = new System.Windows.Forms.RadioButton();
		this.radio2d = new System.Windows.Forms.RadioButton();
		this.textFilter = new System.Windows.Forms.TextBox();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOk = new System.Windows.Forms.Button();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.buttonStop = new System.Windows.Forms.Button();
		this.buttonPlay = new System.Windows.Forms.Button();
		this.listView = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.comboBox2 = new System.Windows.Forms.ComboBox();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox1.Controls.Add(this.radioAll);
		this.groupBox1.Controls.Add(this.radio3d);
		this.groupBox1.Controls.Add(this.radio2d);
		this.groupBox1.Controls.Add(this.textFilter);
		this.groupBox1.Location = new System.Drawing.Point(12, 12);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(775, 76);
		this.groupBox1.TabIndex = 0;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Filter";
		this.radioAll.AutoSize = true;
		this.radioAll.Location = new System.Drawing.Point(174, 45);
		this.radioAll.Name = "radioAll";
		this.radioAll.Size = new System.Drawing.Size(36, 17);
		this.radioAll.TabIndex = 3;
		this.radioAll.Text = "All";
		this.radioAll.UseVisualStyleBackColor = true;
		this.radioAll.Click += new System.EventHandler(radioAll_Click);
		this.radio3d.AutoSize = true;
		this.radio3d.Checked = true;
		this.radio3d.Location = new System.Drawing.Point(90, 45);
		this.radio3d.Name = "radio3d";
		this.radio3d.Size = new System.Drawing.Size(78, 17);
		this.radio3d.TabIndex = 2;
		this.radio3d.TabStop = true;
		this.radio3d.Text = "3D Sounds";
		this.radio3d.UseVisualStyleBackColor = true;
		this.radio3d.Click += new System.EventHandler(radio3d_Click);
		this.radio2d.AutoSize = true;
		this.radio2d.Location = new System.Drawing.Point(6, 45);
		this.radio2d.Name = "radio2d";
		this.radio2d.Size = new System.Drawing.Size(78, 17);
		this.radio2d.TabIndex = 1;
		this.radio2d.Text = "2D Sounds";
		this.radio2d.UseVisualStyleBackColor = true;
		this.radio2d.Click += new System.EventHandler(radio2d_Click);
		this.textFilter.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.textFilter.Location = new System.Drawing.Point(6, 19);
		this.textFilter.Name = "textFilter";
		this.textFilter.Size = new System.Drawing.Size(763, 20);
		this.textFilter.TabIndex = 0;
		this.textFilter.TextChanged += new System.EventHandler(textFilter_TextChanged);
		this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(712, 537);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 1;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonOk.Enabled = false;
		this.buttonOk.Location = new System.Drawing.Point(631, 537);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(75, 23);
		this.buttonOk.TabIndex = 1;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonOk.Click += new System.EventHandler(buttonOk_Click);
		this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox2.Controls.Add(this.comboBox2);
		this.groupBox2.Controls.Add(this.comboBox1);
		this.groupBox2.Controls.Add(this.buttonStop);
		this.groupBox2.Controls.Add(this.buttonPlay);
		this.groupBox2.Controls.Add(this.listView);
		this.groupBox2.Location = new System.Drawing.Point(12, 94);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(775, 437);
		this.groupBox2.TabIndex = 2;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Sounds";
		this.buttonStop.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonStop.Enabled = false;
		this.buttonStop.Location = new System.Drawing.Point(87, 408);
		this.buttonStop.Name = "buttonStop";
		this.buttonStop.Size = new System.Drawing.Size(75, 23);
		this.buttonStop.TabIndex = 2;
		this.buttonStop.Text = "Stop";
		this.buttonStop.UseVisualStyleBackColor = true;
		this.buttonStop.Click += new System.EventHandler(buttonStop_Click);
		this.buttonPlay.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonPlay.Enabled = false;
		this.buttonPlay.Location = new System.Drawing.Point(6, 408);
		this.buttonPlay.Name = "buttonPlay";
		this.buttonPlay.Size = new System.Drawing.Size(75, 23);
		this.buttonPlay.TabIndex = 1;
		this.buttonPlay.Text = "Play";
		this.buttonPlay.UseVisualStyleBackColor = true;
		this.buttonPlay.Click += new System.EventHandler(buttonPlay_Click);
		this.listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader1 });
		this.listView.FullRowSelect = true;
		this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.listView.HideSelection = false;
		this.listView.Location = new System.Drawing.Point(6, 19);
		this.listView.MultiSelect = false;
		this.listView.Name = "listView";
		this.listView.Size = new System.Drawing.Size(763, 383);
		this.listView.TabIndex = 0;
		this.listView.UseCompatibleStateImageBehavior = false;
		this.listView.View = System.Windows.Forms.View.Details;
		this.listView.SelectedIndexChanged += new System.EventHandler(listView_SelectedIndexChanged);
		this.listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(listView_MouseDoubleClick);
		this.columnHeader1.Text = "Wwise Event";
		this.columnHeader1.Width = 567;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Location = new System.Drawing.Point(492, 408);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(121, 21);
		this.comboBox1.TabIndex = 3;
		this.comboBox2.FormattingEnabled = true;
		this.comboBox2.Location = new System.Drawing.Point(619, 408);
		this.comboBox2.Name = "comboBox2";
		this.comboBox2.Size = new System.Drawing.Size(150, 21);
		this.comboBox2.TabIndex = 4;
		base.AcceptButton = this.buttonOk;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.buttonCancel;
		base.ClientSize = new System.Drawing.Size(799, 581);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.buttonOk);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.groupBox1);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(307, 349);
		base.Name = "SoundEventForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Sound Events";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(SoundEventForm_FormClosing);
		base.Shown += new System.EventHandler(SoundEventForm_Shown);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

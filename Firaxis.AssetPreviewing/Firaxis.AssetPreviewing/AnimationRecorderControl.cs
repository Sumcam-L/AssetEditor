using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firaxis.ATF;

namespace Firaxis.AssetPreviewing;

public class AnimationRecorderControl : UserControl
{
	private readonly IAnimationRecorderService m_animationRecorderService;

	private readonly object m_updateLocker = new object();

	private IContainer components = null;

	private CheckedListBox _fromStateListBox;

	private CheckedListBox _toStateListBox;

	private Button _recordButton;

	private Label _fromStateLabel;

	private Label _toStateLabel;

	private CheckBox _fromStateCheckAll;

	private CheckBox _toStateCheckAll;

	private ComboBox _codecComboBox;

	private Label _codecLabel;

	private Label _compressionLabel;

	private TextBox _compressionTextBox;

	private Label lblRecording;

	private bool Recording { get; set; }

	public AnimationRecorderControl(IAnimationRecorderService animationRecorderService)
	{
		InitializeComponent();
		Recording = false;
		m_animationRecorderService = animationRecorderService;
		PopulateListBoxes();
		m_animationRecorderService.BoundAnimationsChanged += HandleBoundAnimationsChanged;
		_compressionTextBox.Text = m_animationRecorderService.CompressionLevel.ToString();
	}

	protected override void Dispose(bool disposing)
	{
		m_animationRecorderService.BoundAnimationsChanged -= HandleBoundAnimationsChanged;
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		_toStateCheckAll.Top = _toStateListBox.Bottom + 5;
		_fromStateCheckAll.Top = _fromStateListBox.Bottom + 5;
	}

	private void _codecComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		string text = (string)_codecComboBox.SelectedItem;
		m_animationRecorderService.SelectedCodec = text;
		if (m_animationRecorderService.SelectedCodec != text)
		{
			_codecComboBox.SelectedItem = m_animationRecorderService.SelectedCodec;
		}
	}

	private void _compressionTextBox_TextChanged(object sender, EventArgs e)
	{
		if (int.TryParse(_compressionTextBox.Text, out var result))
		{
			m_animationRecorderService.CompressionLevel = result;
		}
		if (m_animationRecorderService.CompressionLevel != result)
		{
			_compressionTextBox.Text = m_animationRecorderService.CompressionLevel.ToString();
		}
	}

	private void FinishRecording()
	{
		base.Enabled = true;
		lblRecording.Dock = DockStyle.None;
		lblRecording.Visible = false;
	}

	private IEnumerable<Tuple<string, string>> GetTransitions()
	{
		ICollection<Tuple<string, string>> collection = new List<Tuple<string, string>>();
		foreach (string checkedItem in _fromStateListBox.CheckedItems)
		{
			foreach (string checkedItem2 in _toStateListBox.CheckedItems)
			{
				collection.Add(Tuple.Create(checkedItem, checkedItem2));
			}
		}
		return collection;
	}

	private void HandleBoundAnimationsChanged(object sender, EventArgs e)
	{
		Action method = delegate
		{
			lock (m_updateLocker)
			{
				PopulateListBoxes();
			}
		};
		BeginInvoke(method);
	}

	private void PopulateListBoxes()
	{
		SuspendLayout();
		_fromStateListBox.Items.Clear();
		_toStateListBox.Items.Clear();
		_codecComboBox.Items.Clear();
		string[] items = m_animationRecorderService.FromAnimationStates.ToArray();
		_fromStateListBox.Items.AddRange(items);
		string[] items2 = m_animationRecorderService.ToAnimationStates.ToArray();
		_toStateListBox.Items.AddRange(items2);
		string[] items3 = m_animationRecorderService.AvailableCodecs.ToArray();
		_codecComboBox.Items.AddRange(items3);
		_codecComboBox.SelectedItem = m_animationRecorderService.SelectedCodec;
		ResumeLayout();
	}

	private void Record(object sender, EventArgs e)
	{
		Record();
	}

	private void Record()
	{
		if (Recording)
		{
			return;
		}
		Task.Factory.StartNew(delegate
		{
			BeginInvoke((Action)delegate
			{
				StartRecording();
			});
			Recording = true;
			IEnumerable<Tuple<string, string>> transitions = GetTransitions();
			foreach (Tuple<string, string> item in transitions)
			{
				string fromState = item.Item1;
				string toState = item.Item2;
				BeginInvoke((Action)delegate
				{
					SetRecordingText(fromState, toState);
				});
				m_animationRecorderService.Record(fromState, toState);
			}
			Recording = false;
		}).ContinueWith(delegate
		{
			BeginInvoke((Action)delegate
			{
				FinishRecording();
			});
		});
	}

	private void SetRecordingText(string fromState, string toState)
	{
		lblRecording.Text = $"Recording animation:\nFrom: {fromState}\nTo: {toState}";
	}

	private void StartRecording()
	{
		lblRecording.Dock = DockStyle.Fill;
		lblRecording.Visible = true;
		base.Enabled = false;
	}

	private void UpdateCheckedItems(CheckedListBox listBox, bool value)
	{
		for (int i = 0; i < listBox.Items.Count; i++)
		{
			listBox.SetItemChecked(i, value);
		}
	}

	private void UpdateFromStateListBox(object sender, EventArgs e)
	{
		bool value = _fromStateCheckAll.Checked;
		UpdateCheckedItems(_fromStateListBox, value);
	}

	private void UpdateToStateListBox(object sender, EventArgs e)
	{
		bool value = _toStateCheckAll.Checked;
		UpdateCheckedItems(_toStateListBox, value);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this._fromStateListBox = new System.Windows.Forms.CheckedListBox();
		this._toStateListBox = new System.Windows.Forms.CheckedListBox();
		this._recordButton = new System.Windows.Forms.Button();
		this._fromStateLabel = new System.Windows.Forms.Label();
		this._toStateLabel = new System.Windows.Forms.Label();
		this._fromStateCheckAll = new System.Windows.Forms.CheckBox();
		this._toStateCheckAll = new System.Windows.Forms.CheckBox();
		this._codecComboBox = new System.Windows.Forms.ComboBox();
		this._codecLabel = new System.Windows.Forms.Label();
		this._compressionLabel = new System.Windows.Forms.Label();
		this._compressionTextBox = new System.Windows.Forms.TextBox();
		this.lblRecording = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this._fromStateListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this._fromStateListBox.FormattingEnabled = true;
		this._fromStateListBox.Location = new System.Drawing.Point(10, 26);
		this._fromStateListBox.Name = "_fromStateListBox";
		this._fromStateListBox.Size = new System.Drawing.Size(148, 214);
		this._fromStateListBox.TabIndex = 0;
		this._toStateListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this._toStateListBox.FormattingEnabled = true;
		this._toStateListBox.Location = new System.Drawing.Point(164, 26);
		this._toStateListBox.Name = "_toStateListBox";
		this._toStateListBox.Size = new System.Drawing.Size(148, 214);
		this._toStateListBox.TabIndex = 2;
		this._recordButton.Location = new System.Drawing.Point(318, 26);
		this._recordButton.Name = "_recordButton";
		this._recordButton.Size = new System.Drawing.Size(75, 23);
		this._recordButton.TabIndex = 4;
		this._recordButton.Text = "Record";
		this._recordButton.UseVisualStyleBackColor = true;
		this._recordButton.Click += new System.EventHandler(Record);
		this._fromStateLabel.AutoSize = true;
		this._fromStateLabel.Location = new System.Drawing.Point(7, 10);
		this._fromStateLabel.Name = "_fromStateLabel";
		this._fromStateLabel.Size = new System.Drawing.Size(58, 13);
		this._fromStateLabel.TabIndex = 3;
		this._fromStateLabel.Text = "From State";
		this._toStateLabel.AutoSize = true;
		this._toStateLabel.Location = new System.Drawing.Point(161, 10);
		this._toStateLabel.Name = "_toStateLabel";
		this._toStateLabel.Size = new System.Drawing.Size(48, 13);
		this._toStateLabel.TabIndex = 4;
		this._toStateLabel.Text = "To State";
		this._fromStateCheckAll.AutoSize = true;
		this._fromStateCheckAll.Location = new System.Drawing.Point(10, 246);
		this._fromStateCheckAll.Name = "_fromStateCheckAll";
		this._fromStateCheckAll.Size = new System.Drawing.Size(71, 17);
		this._fromStateCheckAll.TabIndex = 1;
		this._fromStateCheckAll.Text = "Check All";
		this._fromStateCheckAll.UseVisualStyleBackColor = true;
		this._fromStateCheckAll.CheckedChanged += new System.EventHandler(UpdateFromStateListBox);
		this._toStateCheckAll.AutoSize = true;
		this._toStateCheckAll.Location = new System.Drawing.Point(164, 246);
		this._toStateCheckAll.Name = "_toStateCheckAll";
		this._toStateCheckAll.Size = new System.Drawing.Size(71, 17);
		this._toStateCheckAll.TabIndex = 3;
		this._toStateCheckAll.Text = "Check All";
		this._toStateCheckAll.UseVisualStyleBackColor = true;
		this._toStateCheckAll.CheckedChanged += new System.EventHandler(UpdateToStateListBox);
		this._codecComboBox.FormattingEnabled = true;
		this._codecComboBox.Location = new System.Drawing.Point(318, 84);
		this._codecComboBox.Name = "_codecComboBox";
		this._codecComboBox.Size = new System.Drawing.Size(121, 21);
		this._codecComboBox.TabIndex = 5;
		this._codecComboBox.SelectedIndexChanged += new System.EventHandler(_codecComboBox_SelectedIndexChanged);
		this._codecLabel.AutoSize = true;
		this._codecLabel.Location = new System.Drawing.Point(318, 68);
		this._codecLabel.Name = "_codecLabel";
		this._codecLabel.Size = new System.Drawing.Size(89, 13);
		this._codecLabel.TabIndex = 6;
		this._codecLabel.Text = "Available Codecs";
		this._compressionLabel.AutoSize = true;
		this._compressionLabel.Location = new System.Drawing.Point(318, 117);
		this._compressionLabel.Name = "_compressionLabel";
		this._compressionLabel.Size = new System.Drawing.Size(96, 13);
		this._compressionLabel.TabIndex = 7;
		this._compressionLabel.Text = "Compression Level";
		this._compressionTextBox.Location = new System.Drawing.Point(318, 133);
		this._compressionTextBox.Name = "_compressionTextBox";
		this._compressionTextBox.Size = new System.Drawing.Size(118, 20);
		this._compressionTextBox.TabIndex = 8;
		this._compressionTextBox.TextChanged += new System.EventHandler(_compressionTextBox_TextChanged);
		this.lblRecording.Font = new System.Drawing.Font("Microsoft Sans Serif", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblRecording.Location = new System.Drawing.Point(318, 156);
		this.lblRecording.Name = "lblRecording";
		this.lblRecording.Size = new System.Drawing.Size(459, 282);
		this.lblRecording.TabIndex = 9;
		this.lblRecording.Text = "Recording animation: ";
		this.lblRecording.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.lblRecording);
		base.Controls.Add(this._compressionTextBox);
		base.Controls.Add(this._compressionLabel);
		base.Controls.Add(this._codecLabel);
		base.Controls.Add(this._codecComboBox);
		base.Controls.Add(this._toStateCheckAll);
		base.Controls.Add(this._fromStateCheckAll);
		base.Controls.Add(this._toStateLabel);
		base.Controls.Add(this._fromStateLabel);
		base.Controls.Add(this._recordButton);
		base.Controls.Add(this._toStateListBox);
		base.Controls.Add(this._fromStateListBox);
		base.Name = "AnimationRecorderControl";
		base.Size = new System.Drawing.Size(459, 282);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

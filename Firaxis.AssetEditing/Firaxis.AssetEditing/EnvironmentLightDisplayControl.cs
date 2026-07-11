using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Firaxis.Controls;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class EnvironmentLightDisplayControl : UserControl
{
	private IEnvironmentLightEditingContext m_context;

	private ICubeMap m_lastCube;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_lightTagPropertyGrid;

	private LightDirectionTagAdapter m_selectedLightTag;

	private readonly float m_fMaxExposure = 6f;

	private readonly float m_fMinExposure = -6f;

	private readonly float m_fInitialExposure;

	private readonly float m_fMarkerRadius = 6f;

	private IContainer components;

	private Button btnFindSun;

	private CheckBox sampleIntensity;

	private Label widthLabel;

	private ComboBox widthComboBox;

	private Label sampleCountLabel;

	private ComboBox sampleCountDropdown;

	private Button btnAddMark;

	private Label label5;

	private ListBox lstMarks;

	private Label label3;

	private PictureBox pictureBox1;

	private Label label1;

	private Button browseButton;

	private Label lblExposure;

	private TrackBar slider;

	private Label exposureLabel;

	private Button applyExposureButton;

	private Label disabledLabel;

	private Button deleteMarkerButton;

	private ComboBox cmbParameterization;

	public float Exposure => GetExposureFromSlider();

	public float Multiplier => (float)Math.Pow(2.0, GetExposureFromSlider());

	public EnvironmentLightDisplayControl()
	{
		InitializeComponent();
		m_lightTagPropertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips);
		m_lightTagPropertyGrid.Location = new Point(602, 30);
		m_lightTagPropertyGrid.Size = new Size(240, 298);
		m_lightTagPropertyGrid.PropertySorting = PropertySorting.Categorized;
		m_lightTagPropertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		base.Controls.Add(m_lightTagPropertyGrid);
		lstMarks.DisplayMember = "Name";
		cmbParameterization.Enabled = false;
		btnFindSun.Enabled = false;
		float num = (m_fInitialExposure - m_fMinExposure) / (m_fMaxExposure - m_fMinExposure);
		float num2 = (float)(slider.Maximum - slider.Minimum) * num;
		slider.Value = (int)num2;
	}

	private void CreateNewLightTag(object sender, EventArgs e)
	{
		bool flag = true;
		int num = 0;
		while (flag)
		{
			flag = false;
			foreach (LightDirectionTagAdapter item in lstMarks.Items)
			{
				if (item.Name.Equals("light_tag_" + (lstMarks.Items.Count + num)))
				{
					num++;
					flag = true;
					break;
				}
			}
		}
		string name = $"light_tag_{lstMarks.Items.Count + num}";
		LightDirectionTagAdapter lightDirectionTagAdapter = m_context.AddDirectionTag(name, 0.5f, 0.5f);
		if (lightDirectionTagAdapter != null)
		{
			lstMarks.Items.Add(lightDirectionTagAdapter);
			lstMarks.SelectedItem = lightDirectionTagAdapter;
			pictureBox1.Refresh();
		}
		else
		{
			BugSubmitter.SilentReport("Invalid environment light direction tag adapter @assign bwhitman");
		}
	}

	private void TryDeleteSelectedLightTag(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Delete && lstMarks.SelectedItem != null && lstMarks.SelectedItem is LightDirectionTagAdapter item)
		{
			m_context.LightDirectionTags.Remove(item);
			lstMarks.Items.Remove(lstMarks.SelectedItem);
			pictureBox1.Refresh();
		}
	}

	private void MoveSelectedLightTagOnClick(object sender, EventArgs e)
	{
		if (pictureBox1.Image == null)
		{
			return;
		}
		MouseEventArgs obj = (MouseEventArgs)e;
		int num = obj.X;
		int num2 = obj.Y;
		int num3 = pictureBox1.Width;
		int num4 = pictureBox1.Height;
		int num5 = pictureBox1.Width - num3;
		int num6 = pictureBox1.Height - num4;
		int num7 = num5 / 2;
		int num8 = num6 / 2;
		int num9 = num7 + num3;
		int num10 = num8 + num4;
		if (num < num7 || num > num9 || num2 > num10 || num2 < num8)
		{
			return;
		}
		bool flag = false;
		LightDirectionTagAdapter lightDirectionTagAdapter = null;
		foreach (LightDirectionTagAdapter item in lstMarks.Items)
		{
			if (lstMarks.SelectedItem != item)
			{
				float u = item.U;
				float v = item.V;
				float num11 = (float)num7 + u * (float)num3;
				float num12 = (float)num8 + v * (float)num4;
				if ((float)Math.Sqrt((num11 - (float)num) * (num11 - (float)num) + (num12 - (float)num2) * (num12 - (float)num2)) < m_fMarkerRadius)
				{
					lightDirectionTagAdapter = item;
					pictureBox1.Refresh();
					flag = true;
				}
			}
		}
		if (flag && lightDirectionTagAdapter != null)
		{
			lstMarks.SelectedItem = lightDirectionTagAdapter;
		}
		if (lstMarks.SelectedItem != null && !flag)
		{
			LightDirectionTagAdapter lightDirectionTagAdapter3 = lstMarks.SelectedItem as LightDirectionTagAdapter;
			float newU = (float)(num - num7) / (float)num3;
			float newV = (float)(num2 - num8) / (float)num4;
			lightDirectionTagAdapter3.Update(newU, newV, m_context.SampleIntensityFromMap);
			BindAdapterToPropertyGrid(lightDirectionTagAdapter3);
			ApplyChanges();
			pictureBox1.Refresh();
			m_lightTagPropertyGrid.Refresh();
		}
	}

	private void DrawLightTags(object sender, PaintEventArgs e)
	{
		if (pictureBox1.Image == null)
		{
			return;
		}
		int num = pictureBox1.Width;
		int num2 = pictureBox1.Height;
		int num3 = pictureBox1.Width - num;
		int num4 = pictureBox1.Height - num2;
		float num5 = (float)num3 / 2f;
		float num6 = (float)num4 / 2f;
		LightDirectionTagAdapter lightDirectionTagAdapter = lstMarks.SelectedItem as LightDirectionTagAdapter;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (LightDirectionTagAdapter item in lstMarks.Items)
		{
			float u = item.U;
			float v = item.V;
			float num7 = num5 + u * (float)num;
			float num8 = num6 + v * (float)num2;
			if (lightDirectionTagAdapter == item)
			{
				e.Graphics.FillEllipse(Brushes.Black, num7 - 6f, num8 - 6f, 12f, 12f);
				e.Graphics.FillEllipse(Brushes.CornflowerBlue, num7 - 5f, num8 - 5f, 10f, 10f);
				e.Graphics.FillEllipse(Brushes.White, num7 - 2f, num8 - 2f, 4f, 4f);
			}
			else
			{
				e.Graphics.FillEllipse(Brushes.Crimson, num7 - 5f, num8 - 5f, 10f, 10f);
				e.Graphics.FillEllipse(Brushes.White, num7 - 2f, num8 - 2f, 4f, 4f);
			}
		}
	}

	private void InitializeLightTagUI()
	{
		lstMarks.Items.Clear();
		foreach (LightDirectionTagAdapter lightDirectionTag in m_context.LightDirectionTags)
		{
			lstMarks.Items.Add(lightDirectionTag);
			if (lightDirectionTag == m_selectedLightTag)
			{
				lstMarks.SelectedItem = m_selectedLightTag;
			}
		}
	}

	private void SetMarkUIEnabled(bool enable)
	{
		btnAddMark.Enabled = enable;
		lstMarks.Enabled = enable;
	}

	private void UpdatePropertyGrid(object sender, EventArgs e)
	{
		LightDirectionTagAdapter adapter = lstMarks.SelectedItem as LightDirectionTagAdapter;
		BindAdapterToPropertyGrid(adapter);
		btnFindSun.Enabled = m_selectedLightTag != null;
		pictureBox1.Refresh();
	}

	private void slider_ValueChanged(object sender, EventArgs e)
	{
		float exposureFromSlider = GetExposureFromSlider();
		lblExposure.Text = exposureFromSlider.ToString("0.000");
	}

	private float GetExposureFromSlider()
	{
		float num = slider.Value;
		num /= (float)(slider.Maximum - slider.Minimum);
		return m_fMinExposure + (m_fMaxExposure - m_fMinExposure) * num;
	}

	private void applyExposureButton_Click(object sender, EventArgs e)
	{
		if (m_context.Cube != null)
		{
			using (new WaitCursor())
			{
				m_context.Cube.ReExposeFrom(m_lastCube, Multiplier);
				RefreshPictureBox(m_context.Cube.CreateThumbnail());
				m_context.ApplyChanges();
				return;
			}
		}
		BugSubmitter.SilentReport("Invalid environment context light cube @assign bwhitman");
	}

	public void Bind(IEnvironmentLightEditingContext context)
	{
		UnregisterEvents();
		m_context = context;
		bool isCube = false;
		if (context != null)
		{
			if (m_context.Cube != null)
			{
				m_lastCube = m_context.Cube;
				if (m_context.Source != null)
				{
					isCube = m_context.Source.IsCubeMap;
				}
				sampleIntensity.Checked = m_context.SampleIntensityFromMap;
				SetMarkUIEnabled(enable: true);
				InitializeLightTagUI();
				EnableNumberComboBox(sampleCountDropdown, m_context.LightClass.ImportOptions.MinWidth, m_context.LightClass.ImportOptions.MaxWidth, m_context.HasSource);
				sampleCountDropdown.SelectedItem = m_context.LastSampleCount;
				EnableNumberComboBox(widthComboBox, m_context.LightClass.ImportOptions.MinWidth, m_context.LightClass.ImportOptions.MaxWidth, m_context.HasSource);
				widthLabel.Visible = true;
				widthComboBox.SelectedItem = m_context.Cube.Width;
				disabledLabel.Visible = !m_context.HasSource;
				sampleIntensity.Enabled = m_context.HasSource;
				btnFindSun.Enabled = m_context.HasSource;
				applyExposureButton.Enabled = m_context.HasSource;
				slider.Enabled = m_context.HasSource;
				RefreshPictureBox(m_context.CubeImage);
			}
			RegisterEvents();
		}
		FillParameterizationBox(isCube);
	}

	public void RefreshUIStateFromContext()
	{
		sampleIntensity.Checked = m_context.SampleIntensityFromMap;
	}

	private void ApplyChanges()
	{
		m_context.ApplyChanges();
	}

	private void BindAdapterToPropertyGrid(LightDirectionTagAdapter adapter)
	{
		m_selectedLightTag = adapter;
		m_lightTagPropertyGrid.Bind(adapter);
		m_lightTagPropertyGrid.Refresh();
	}

	private void btnBrowse_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		List<string> list = new List<string>();
		foreach (string supportedFileType in m_context.Importer.SupportedFileTypes)
		{
			list.Add("*" + supportedFileType);
		}
		openFileDialog.Filter = string.Format("Environment maps ({0})|{1}", string.Join(",", list), string.Join(";", list));
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			string fileName = openFileDialog.FileName;
			OpenSourceFile(fileName);
		}
	}

	private void DeleteMarkButton_Click(object sender, EventArgs e)
	{
		if (lstMarks.SelectedItem != null && lstMarks.SelectedItem is LightDirectionTagAdapter item)
		{
			m_context.LightDirectionTags.Remove(item);
			lstMarks.Items.Remove(lstMarks.SelectedItem);
			pictureBox1.Refresh();
		}
	}

	private void EnableNumberComboBox(ComboBox box, uint startingValue, uint endingValue, bool enabled)
	{
		PlatformAssert.If(startingValue >= endingValue, "Starting value cannot be greater than ending value.");
		PlatformAssert.If(startingValue != m_context.LightClass.ImportOptions.MinWidth, "Starting value should be {0}.  Value is {1}.", m_context.LightClass.ImportOptions.MinWidth, startingValue);
		PlatformAssert.If(endingValue != m_context.LightClass.ImportOptions.MaxWidth, "Ending value should be {0}.  Value is {1}.", m_context.LightClass.ImportOptions.MaxWidth, endingValue);
		box.FillDropdownInOrder((uint val) => val << 1, startingValue, endingValue);
		box.Enabled = enabled;
		box.Visible = true;
	}

	private void EnvironmentMapImportForm_DragDrop(object sender, DragEventArgs e)
	{
		string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
		OpenSourceFile(array[0]);
	}

	private void EnvironmentMapImportForm_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			string text = Path.GetExtension(((string[])e.Data.GetData(DataFormats.FileDrop))[0]).ToLower().Replace(".", "");
			e.Effect = DragDropEffects.None;
			{
				foreach (string supportedFileType in m_context.Importer.SupportedFileTypes)
				{
					if (text.Equals(supportedFileType, StringComparison.InvariantCultureIgnoreCase))
					{
						e.Effect = DragDropEffects.Copy;
						break;
					}
				}
				return;
			}
		}
		e.Effect = DragDropEffects.None;
	}

	private void FillParameterizationBox(bool isCube)
	{
		cmbParameterization.Items.Clear();
		foreach (object value in Enum.GetValues(typeof(EnvironmentMapParameterization)))
		{
			cmbParameterization.Items.Add(value);
		}
		if (!isCube)
		{
			cmbParameterization.Items.Remove(EnvironmentMapParameterization.ENVMAP_CUBE);
			cmbParameterization.SelectedIndex = 0;
		}
		else
		{
			cmbParameterization.SelectedItem = EnvironmentMapParameterization.ENVMAP_CUBE;
		}
		cmbParameterization.Enabled = !isCube;
	}

	private void MoveLightTagToSun(object sender, EventArgs e)
	{
		LightDirectionTagAdapter lightDirectionTagAdapter = lstMarks.SelectedItem as LightDirectionTagAdapter;
		IFloatVector3 floatVector = m_context.Cube.FindSun();
		float[] array = m_context.Importer.DirectionToThumbnailUV(floatVector.X, floatVector.Y, floatVector.Z);
		lightDirectionTagAdapter.Update(array[0], array[1], resampleLights: true);
		BindAdapterToPropertyGrid(lightDirectionTagAdapter);
		pictureBox1.Refresh();
	}

	private void OpenSourceFile(string path)
	{
		using (new WaitCursor())
		{
			m_context.SetSourceFile(path);
		}
	}

	private void RecreateCube()
	{
		uint sampleCount = ((sampleCountDropdown.SelectedIndex >= 0) ? ((uint)sampleCountDropdown.SelectedItem) : m_context.LastSampleCount);
		uint num = ((widthComboBox.SelectedIndex >= 0) ? ((uint)widthComboBox.SelectedItem) : m_context.LastSampleCount);
		EnvironmentMapParameterization eSourceParametrization = (EnvironmentMapParameterization)cmbParameterization.SelectedItem;
		m_context.CreateCube(eSourceParametrization, sampleCount, num);
		m_lastCube = m_context.Cube;
		RefreshPictureBox(m_context.CubeImage);
	}

	private void RefreshCubeOnSelectionChanged(object sender, EventArgs e)
	{
		if ((sender as ComboBox).SelectedIndex >= 0)
		{
			using (new WaitCursor())
			{
				RecreateCube();
				UpdateAdapters();
				ApplyChanges();
			}
		}
	}

	private void RefreshPictureBox(Image image)
	{
		pictureBox1.Image = image;
		pictureBox1.Refresh();
	}

	private void RegisterEvents()
	{
		sampleCountDropdown.SelectedIndexChanged += RefreshCubeOnSelectionChanged;
		widthComboBox.SelectedIndexChanged += RefreshCubeOnSelectionChanged;
		lstMarks.SelectedIndexChanged += UpdatePropertyGrid;
		base.DragEnter += EnvironmentMapImportForm_DragEnter;
		base.DragDrop += EnvironmentMapImportForm_DragDrop;
	}

	private void sampleIntensity_CheckedChanged(object sender, EventArgs e)
	{
		m_context.SampleIntensityFromMap = sampleIntensity.Checked;
	}

	private void UnregisterEvents()
	{
		sampleCountDropdown.SelectedIndexChanged -= RefreshCubeOnSelectionChanged;
		widthComboBox.SelectedIndexChanged -= RefreshCubeOnSelectionChanged;
		lstMarks.SelectedIndexChanged -= UpdatePropertyGrid;
		base.DragEnter -= EnvironmentMapImportForm_DragEnter;
		base.DragDrop -= EnvironmentMapImportForm_DragDrop;
	}

	private void UpdateAdapters()
	{
		foreach (LightDirectionTagAdapter lightDirectionTag in m_context.LightDirectionTags)
		{
			lightDirectionTag.Update(lightDirectionTag.U, lightDirectionTag.V, resampleLights: false);
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
		this.btnFindSun = new System.Windows.Forms.Button();
		this.sampleIntensity = new System.Windows.Forms.CheckBox();
		this.widthLabel = new System.Windows.Forms.Label();
		this.widthComboBox = new System.Windows.Forms.ComboBox();
		this.sampleCountLabel = new System.Windows.Forms.Label();
		this.sampleCountDropdown = new System.Windows.Forms.ComboBox();
		this.btnAddMark = new System.Windows.Forms.Button();
		this.label5 = new System.Windows.Forms.Label();
		this.lstMarks = new System.Windows.Forms.ListBox();
		this.label3 = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.label1 = new System.Windows.Forms.Label();
		this.browseButton = new System.Windows.Forms.Button();
		this.lblExposure = new System.Windows.Forms.Label();
		this.slider = new System.Windows.Forms.TrackBar();
		this.exposureLabel = new System.Windows.Forms.Label();
		this.applyExposureButton = new System.Windows.Forms.Button();
		this.disabledLabel = new System.Windows.Forms.Label();
		this.deleteMarkerButton = new System.Windows.Forms.Button();
		this.cmbParameterization = new System.Windows.Forms.ComboBox();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.slider).BeginInit();
		base.SuspendLayout();
		this.btnFindSun.Location = new System.Drawing.Point(596, 363);
		this.btnFindSun.Name = "btnFindSun";
		this.btnFindSun.Size = new System.Drawing.Size(117, 21);
		this.btnFindSun.TabIndex = 46;
		this.btnFindSun.Text = "Find Brightest Spot";
		this.btnFindSun.UseVisualStyleBackColor = true;
		this.btnFindSun.Click += new System.EventHandler(MoveLightTagToSun);
		this.sampleIntensity.AutoSize = true;
		this.sampleIntensity.Location = new System.Drawing.Point(596, 340);
		this.sampleIntensity.Name = "sampleIntensity";
		this.sampleIntensity.Size = new System.Drawing.Size(150, 17);
		this.sampleIntensity.TabIndex = 45;
		this.sampleIntensity.Text = "Sample Intensity from Map";
		this.sampleIntensity.UseVisualStyleBackColor = true;
		this.sampleIntensity.CheckedChanged += new System.EventHandler(sampleIntensity_CheckedChanged);
		this.widthLabel.AutoSize = true;
		this.widthLabel.Location = new System.Drawing.Point(16, 271);
		this.widthLabel.Name = "widthLabel";
		this.widthLabel.Size = new System.Drawing.Size(35, 13);
		this.widthLabel.TabIndex = 41;
		this.widthLabel.Text = "Width";
		this.widthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.widthLabel.Visible = false;
		this.widthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.widthComboBox.Enabled = false;
		this.widthComboBox.FormattingEnabled = true;
		this.widthComboBox.Location = new System.Drawing.Point(95, 267);
		this.widthComboBox.Name = "widthComboBox";
		this.widthComboBox.Size = new System.Drawing.Size(181, 21);
		this.widthComboBox.TabIndex = 40;
		this.widthComboBox.Visible = false;
		this.sampleCountLabel.AutoSize = true;
		this.sampleCountLabel.Location = new System.Drawing.Point(16, 298);
		this.sampleCountLabel.Name = "sampleCountLabel";
		this.sampleCountLabel.Size = new System.Drawing.Size(73, 13);
		this.sampleCountLabel.TabIndex = 39;
		this.sampleCountLabel.Text = "Sample Count";
		this.sampleCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.sampleCountDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.sampleCountDropdown.Enabled = false;
		this.sampleCountDropdown.FormattingEnabled = true;
		this.sampleCountDropdown.Location = new System.Drawing.Point(95, 294);
		this.sampleCountDropdown.Name = "sampleCountDropdown";
		this.sampleCountDropdown.Size = new System.Drawing.Size(181, 21);
		this.sampleCountDropdown.TabIndex = 38;
		this.btnAddMark.Location = new System.Drawing.Point(548, 36);
		this.btnAddMark.Name = "btnAddMark";
		this.btnAddMark.Size = new System.Drawing.Size(44, 21);
		this.btnAddMark.TabIndex = 37;
		this.btnAddMark.Text = "Add";
		this.btnAddMark.UseVisualStyleBackColor = true;
		this.btnAddMark.Click += new System.EventHandler(CreateNewLightTag);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(480, 40);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(71, 13);
		this.label5.TabIndex = 36;
		this.label5.Text = "Light Markers";
		this.lstMarks.DisplayMember = "Name";
		this.lstMarks.FormattingEnabled = true;
		this.lstMarks.Location = new System.Drawing.Point(483, 63);
		this.lstMarks.Name = "lstMarks";
		this.lstMarks.Size = new System.Drawing.Size(110, 212);
		this.lstMarks.TabIndex = 35;
		this.lstMarks.SelectedIndexChanged += new System.EventHandler(UpdatePropertyGrid);
		this.lstMarks.KeyDown += new System.Windows.Forms.KeyEventHandler(TryDeleteSelectedLightTag);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(160, 17);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(137, 13);
		this.label3.TabIndex = 29;
		this.label3.Text = "or Drag in File Below (*.dds)";
		this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlDark;
		this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.pictureBox1.Location = new System.Drawing.Point(16, 40);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(442, 221);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 28;
		this.pictureBox1.TabStop = false;
		this.pictureBox1.Click += new System.EventHandler(MoveSelectedLightTagOnClick);
		this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(DrawLightTags);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(16, 324);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(79, 13);
		this.label1.TabIndex = 26;
		this.label1.Text = "Parametrization";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label1.Visible = false;
		this.browseButton.Location = new System.Drawing.Point(16, 13);
		this.browseButton.Name = "browseButton";
		this.browseButton.Size = new System.Drawing.Size(138, 21);
		this.browseButton.TabIndex = 48;
		this.browseButton.Text = "Add Environment Image...";
		this.browseButton.UseVisualStyleBackColor = true;
		this.browseButton.Click += new System.EventHandler(btnBrowse_Click);
		this.lblExposure.AutoSize = true;
		this.lblExposure.Location = new System.Drawing.Point(419, 288);
		this.lblExposure.Name = "lblExposure";
		this.lblExposure.Size = new System.Drawing.Size(35, 13);
		this.lblExposure.TabIndex = 50;
		this.lblExposure.Text = "label1";
		this.slider.Location = new System.Drawing.Point(281, 288);
		this.slider.Maximum = 100;
		this.slider.Name = "slider";
		this.slider.Size = new System.Drawing.Size(132, 45);
		this.slider.TabIndex = 49;
		this.slider.TickFrequency = 0;
		this.slider.TickStyle = System.Windows.Forms.TickStyle.None;
		this.slider.ValueChanged += new System.EventHandler(slider_ValueChanged);
		this.exposureLabel.AutoSize = true;
		this.exposureLabel.Location = new System.Drawing.Point(287, 271);
		this.exposureLabel.Name = "exposureLabel";
		this.exposureLabel.Size = new System.Drawing.Size(51, 13);
		this.exposureLabel.TabIndex = 51;
		this.exposureLabel.Text = "Exposure";
		this.applyExposureButton.Location = new System.Drawing.Point(290, 317);
		this.applyExposureButton.Name = "applyExposureButton";
		this.applyExposureButton.Size = new System.Drawing.Size(97, 26);
		this.applyExposureButton.TabIndex = 52;
		this.applyExposureButton.Text = "Apply Exposure";
		this.applyExposureButton.UseVisualStyleBackColor = true;
		this.applyExposureButton.Click += new System.EventHandler(applyExposureButton_Click);
		this.disabledLabel.AutoSize = true;
		this.disabledLabel.Font = new System.Drawing.Font("Calibri", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.disabledLabel.ForeColor = System.Drawing.Color.Red;
		this.disabledLabel.Location = new System.Drawing.Point(17, 362);
		this.disabledLabel.MaximumSize = new System.Drawing.Size(500, 0);
		this.disabledLabel.Name = "disabledLabel";
		this.disabledLabel.Size = new System.Drawing.Size(441, 38);
		this.disabledLabel.TabIndex = 53;
		this.disabledLabel.Text = "Controls are disabled because the source file could not be loaded. Previewing image from the last succesful export";
		this.deleteMarkerButton.Location = new System.Drawing.Point(483, 281);
		this.deleteMarkerButton.Name = "deleteMarkerButton";
		this.deleteMarkerButton.Size = new System.Drawing.Size(109, 20);
		this.deleteMarkerButton.TabIndex = 54;
		this.deleteMarkerButton.Text = "Delete Marker";
		this.deleteMarkerButton.UseVisualStyleBackColor = true;
		this.deleteMarkerButton.Click += new System.EventHandler(DeleteMarkButton_Click);
		this.cmbParameterization.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cmbParameterization.FormattingEnabled = true;
		this.cmbParameterization.Location = new System.Drawing.Point(95, 321);
		this.cmbParameterization.Name = "cmbParameterization";
		this.cmbParameterization.Size = new System.Drawing.Size(180, 21);
		this.cmbParameterization.TabIndex = 25;
		this.cmbParameterization.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.deleteMarkerButton);
		base.Controls.Add(this.disabledLabel);
		base.Controls.Add(this.applyExposureButton);
		base.Controls.Add(this.exposureLabel);
		base.Controls.Add(this.lblExposure);
		base.Controls.Add(this.slider);
		base.Controls.Add(this.browseButton);
		base.Controls.Add(this.btnFindSun);
		base.Controls.Add(this.sampleIntensity);
		base.Controls.Add(this.widthLabel);
		base.Controls.Add(this.widthComboBox);
		base.Controls.Add(this.sampleCountLabel);
		base.Controls.Add(this.sampleCountDropdown);
		base.Controls.Add(this.btnAddMark);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.lstMarks);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cmbParameterization);
		base.Name = "EnvironmentLightDisplayControl";
		base.Size = new System.Drawing.Size(842, 446);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.slider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

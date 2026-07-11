using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.Controls;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

public class KeyFrameEditor : UserControl, IATFEditor, IDisposable
{
	private KeyFrameEditingContextBase m_keyframeContext;

	private KeyFrameControl m_keyFrameControl;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propControl;

	public const string EditorName = "KeyFrame";

	private IContainer components;

	private TimeRulerControl m_keyFrameRuler;

	private Panel m_keyFramePanel;

	private Panel keyFrameControlPanel;

	private Panel m_propPanel;

	public KeyFrameEditor()
	{
		InitializeComponent();
		m_keyFrameRuler.ShuttleVisible = false;
		m_keyFrameRuler.MajorScale = 40f;
		m_propControl = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisableSearchControls);
		m_propControl.Dock = DockStyle.Fill;
		m_propPanel.Controls.Add(m_propControl);
		m_keyFrameControl = new KeyFrameControl();
		m_keyFrameControl.Dock = DockStyle.Fill;
		m_keyFrameControl.Ruler = m_keyFrameRuler;
		m_keyFrameControl.SelectionChanged += KeyFrameControl_SelectionChanged;
		m_keyFrameControl.KeyFrameTimeChanged += KeyFrameControl_KeyFrameTimeChanged;
		m_keyFramePanel.Controls.Add(m_keyFrameControl);
	}

	public void Bind(IATFEditingContext context)
	{
		if (m_keyframeContext != null)
		{
			m_keyframeContext.ItemInserted -= KeyframeContext_ItemInserted;
			m_keyframeContext.ItemRemoved -= KeyframeContext_ItemRemoved;
			m_keyframeContext.ItemChanged -= KeyframeContext_ItemChanged;
			m_keyframeContext.Reloaded -= KeyframeContext_Reloaded;
			m_keyframeContext = null;
		}
		m_keyframeContext = (KeyFrameEditingContextBase)context;
		if (m_keyframeContext != null)
		{
			m_keyframeContext.ItemInserted += KeyframeContext_ItemInserted;
			m_keyframeContext.ItemRemoved += KeyframeContext_ItemRemoved;
			m_keyframeContext.ItemChanged += KeyframeContext_ItemChanged;
			m_keyframeContext.Reloaded += KeyframeContext_Reloaded;
			m_keyFrameControl.Ruler.Origin = m_keyframeContext.MinTime;
			m_keyFrameControl.Ruler.MaxTime = m_keyframeContext.MaxTime;
			RefreshKeyFrames();
		}
		SkinService.ApplyActiveSkin(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_keyFrameControl.SelectionChanged -= KeyFrameControl_SelectionChanged;
			m_keyFrameControl.KeyFrameTimeChanged -= KeyFrameControl_KeyFrameTimeChanged;
			if (m_keyframeContext != null)
			{
				m_keyframeContext.ItemInserted -= KeyframeContext_ItemInserted;
				m_keyframeContext.ItemRemoved -= KeyframeContext_ItemRemoved;
				m_keyframeContext.ItemChanged -= KeyframeContext_ItemChanged;
				m_keyframeContext.Reloaded -= KeyframeContext_Reloaded;
				m_keyframeContext = null;
			}
			if (m_propControl != null)
			{
				m_propControl.Bind(null);
				m_propControl.Dispose();
				m_propControl = null;
			}
			if (m_keyFrameControl != null)
			{
				m_keyFrameControl.Dispose();
				m_keyFrameControl = null;
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void KeyframeContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		m_keyFrameControl.Invalidate();
	}

	private void KeyframeContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		RefreshKeyFrames();
	}

	private void KeyframeContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		RefreshKeyFrames();
	}

	private void KeyframeContext_Reloaded(object sender, EventArgs e)
	{
		RefreshKeyFrames();
	}

	private void KeyFrameControl_KeyFrameTimeChanged(object sender, KeyFrameTimeChangeArgs e)
	{
		m_keyframeContext.KeyFrames.ElementAt(e.SelectedIndex).Time = e.Time;
	}

	private void KeyFrameControl_SelectionChanged(object sender, EventArgs e)
	{
		m_keyframeContext.SelectedIndices = new int[1] { m_keyFrameControl.SelectedIndex };
		m_propControl.Bind(null);
		m_propControl.Bind(m_keyframeContext);
	}

	private void RefreshKeyFrames()
	{
		int selectedIndex = m_keyFrameControl.SelectedIndex;
		m_keyFrameControl.KeyFrames.Clear();
		foreach (IKeyFrame keyFrame in m_keyframeContext.KeyFrames)
		{
			m_keyFrameControl.KeyFrames.Add(keyFrame);
		}
		m_keyFrameControl.SelectedIndex = Math.Max(Math.Min(selectedIndex, m_keyFrameControl.KeyFrames.Count - 1), -1);
		if (!m_keyframeContext.SelectedIndices.Any() || m_keyframeContext.SelectedIndices.First() == m_keyFrameControl.SelectedIndex)
		{
			m_keyframeContext.SelectedIndices = new int[1] { m_keyFrameControl.SelectedIndex };
		}
		m_keyFrameControl.Invalidate();
	}

	private void InitializeComponent()
	{
		this.m_keyFrameRuler = new Firaxis.Controls.TimeRulerControl();
		this.m_keyFramePanel = new System.Windows.Forms.Panel();
		this.keyFrameControlPanel = new System.Windows.Forms.Panel();
		this.m_propPanel = new System.Windows.Forms.Panel();
		this.keyFrameControlPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_keyFrameRuler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_keyFrameRuler.BackColor = System.Drawing.Color.FromArgb(232, 232, 232);
		this.m_keyFrameRuler.CurrentTime = 0f;
		this.m_keyFrameRuler.ForeColor = System.Drawing.Color.Black;
		this.m_keyFrameRuler.Location = new System.Drawing.Point(0, 0);
		this.m_keyFrameRuler.MajorScale = 540f;
		this.m_keyFrameRuler.Name = "m_keyFrameRuler";
		this.m_keyFrameRuler.Origin = 0f;
		this.m_keyFrameRuler.RangeColor = System.Drawing.Color.FromArgb(245, 164, 83);
		this.m_keyFrameRuler.RangeDuration = 3f;
		this.m_keyFrameRuler.RangeStart = 0f;
		this.m_keyFrameRuler.ShuttleColor = System.Drawing.Color.FromArgb(216, 30, 0);
		this.m_keyFrameRuler.ShuttleVisible = true;
		this.m_keyFrameRuler.Size = new System.Drawing.Size(600, 25);
		this.m_keyFrameRuler.TabIndex = 0;
		this.m_keyFrameRuler.TickColor = System.Drawing.Color.FromArgb(186, 182, 169);
		this.m_keyFrameRuler.TrackRangeVisible = false;
		this.m_keyFramePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_keyFramePanel.BackColor = System.Drawing.SystemColors.ControlDark;
		this.m_keyFramePanel.Location = new System.Drawing.Point(0, 25);
		this.m_keyFramePanel.Name = "m_keyFramePanel";
		this.m_keyFramePanel.Size = new System.Drawing.Size(600, 75);
		this.m_keyFramePanel.TabIndex = 1;
		this.keyFrameControlPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.keyFrameControlPanel.Controls.Add(this.m_keyFramePanel);
		this.keyFrameControlPanel.Controls.Add(this.m_keyFrameRuler);
		this.keyFrameControlPanel.Location = new System.Drawing.Point(0, 0);
		this.keyFrameControlPanel.Name = "keyFrameControlPanel";
		this.keyFrameControlPanel.Size = new System.Drawing.Size(600, 100);
		this.keyFrameControlPanel.TabIndex = 0;
		this.m_propPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_propPanel.Location = new System.Drawing.Point(0, 101);
		this.m_propPanel.Name = "m_propPanel";
		this.m_propPanel.Size = new System.Drawing.Size(600, 333);
		this.m_propPanel.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_propPanel);
		base.Controls.Add(this.keyFrameControlPanel);
		base.Name = "KeyFrameEditor";
		base.Size = new System.Drawing.Size(600, 434);
		this.keyFrameControlPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

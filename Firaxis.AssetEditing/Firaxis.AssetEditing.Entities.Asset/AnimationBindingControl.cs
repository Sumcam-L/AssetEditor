using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing.Entities.Asset;

public class AnimationBindingControl : UserControl
{
	private const int m_topAndLeftMargin = 1;

	private readonly PropertyEditorControlContext m_context;

	private StateTransitionInfo m_transitionInfo;

	private ICivTechService m_civTechService;

	private IAssetBrowserDialogService m_assetBrowser;

	private ITimelinePlaybackService m_playbackService;

	private IAnimationKnobService m_animationKnobService;

	private IContainer components;

	private Button m_btnPlayAnimation;

	private Button m_btnOpenAssetBrowser;

	private TextBox m_textBox;

	private ToolTip m_toolTip;

	private Button m_btnClear;

	public AnimationBindingControl(PropertyEditorControlContext context, ICivTechService civtechService, IAssetBrowserDialogService assetBrowserSvc, ITimelinePlaybackService timelinePlaybackSvc, IAnimationKnobService animationKnobSvc)
	{
		m_context = context;
		m_civTechService = civtechService;
		m_assetBrowser = assetBrowserSvc;
		m_playbackService = timelinePlaybackSvc;
		m_animationKnobService = animationKnobSvc;
		InitializeComponent();
		m_btnPlayAnimation.Image = ResourceUtil.GetImage16(Resources.StartPlaybackTimelineIcon);
		m_btnPlayAnimation.BackgroundImageLayout = ImageLayout.Center;
		m_btnPlayAnimation.Enabled = m_playbackService != null && m_animationKnobService != null;
		m_btnClear.Image = ResourceUtil.GetImage16(Resources.DeleteIcon);
		m_btnClear.BackgroundImageLayout = ImageLayout.Center;
		m_toolTip.SetToolTip(m_textBox, "Animation name for this Binding");
		m_toolTip.SetToolTip(m_btnClear, "Clear this animation binding");
		m_toolTip.SetToolTip(m_btnOpenAssetBrowser, "Add an existing Animation");
		m_toolTip.SetToolTip(m_btnPlayAnimation, "Play this Animation");
		RefreshTextFromData();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_textBox.Leave -= textBox_leavefocus;
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	public override void Refresh()
	{
		RefreshTextFromData();
		base.Refresh();
	}

	private void RefreshTextFromData()
	{
		object value = m_context.GetValue();
		string value2 = string.Empty;
		if (value != null)
		{
			value2 = value.ToString();
		}
		if (!m_textBox.Text.Equals(value2, StringComparison.CurrentCultureIgnoreCase))
		{
			m_textBox.Text = value2;
			m_transitionInfo = new StateTransitionInfo();
			if (value != null)
			{
				SetCurrentAnimation(value.ToString());
			}
		}
	}

	private void SetCurrentAnimation(string animationName)
	{
		AnimationBindingAdapter animationBindingAdapter = m_context.LastSelectedObject as AnimationBindingAdapter;
		m_transitionInfo.Source = "ANY";
		m_transitionInfo.Destination = animationName;
		m_transitionInfo.AnimationGraphIndex = 0;
		m_transitionInfo.Duration = 4f;
		if (animationBindingAdapter != null && m_animationKnobService != null && m_playbackService != null)
		{
			string slotName = animationBindingAdapter.SlotName;
			IList<StateTransitionInfo> value = null;
			if (m_animationKnobService.TimelineStateTransitions.TryGetValue(slotName, out value))
			{
				m_transitionInfo = value[0];
			}
			m_playbackService.PlayAnimation(m_transitionInfo, 0f, 0f, m_transitionInfo.Duration);
		}
	}

	private void m_btnClear_Click(object sender, EventArgs e)
	{
		SetValue(string.Empty);
	}

	private void btn_playAnimation(object sender, EventArgs e)
	{
		if (m_playbackService != null && !m_playbackService.Playing)
		{
			m_playbackService.Playing = true;
		}
	}

	private void btn_openAssetBrowser(object sender, EventArgs e)
	{
		openAssetBrowser();
	}

	private void openAssetBrowser()
	{
		IAssetBrowserTypeProvider assetBrowserTypeProvider = (m_context.Descriptor as Sce.Atf.Dom.PropertyDescriptor).GetNode(m_context.LastSelectedObject).As<IAssetBrowserTypeProvider>();
		string pathName = "";
		m_assetBrowser.OpenFileName(ref pathName, assetBrowserTypeProvider.EntityFilteringContext);
		if (!string.IsNullOrEmpty(pathName))
		{
			StaticMethods.GetInstanceNameAndType(m_civTechService.ProjectMapService, pathName, out var instanceName, out var _);
			SetValue(StaticMethods.SanitizeEntityName(instanceName));
		}
	}

	private void SetValue(string newValue)
	{
		m_context.SetValue(newValue);
		RefreshTextFromData();
	}

	private void textBox_leavefocus(object sender, EventArgs e)
	{
		object value = m_context.GetValue();
		if (value != null && !m_textBox.Text.Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase))
		{
			SetValue(m_textBox.Text);
		}
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.m_btnPlayAnimation = new System.Windows.Forms.Button();
		this.m_btnOpenAssetBrowser = new System.Windows.Forms.Button();
		this.m_textBox = new System.Windows.Forms.TextBox();
		this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
		this.m_btnClear = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_btnPlayAnimation.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnPlayAnimation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.m_btnPlayAnimation.Location = new System.Drawing.Point(376, 0);
		this.m_btnPlayAnimation.Name = "m_btnPlayAnimation";
		this.m_btnPlayAnimation.Size = new System.Drawing.Size(24, 22);
		this.m_btnPlayAnimation.TabIndex = 2;
		this.m_btnPlayAnimation.UseVisualStyleBackColor = true;
		this.m_btnPlayAnimation.Click += new System.EventHandler(btn_playAnimation);
		this.m_btnOpenAssetBrowser.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnOpenAssetBrowser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.m_btnOpenAssetBrowser.Location = new System.Drawing.Point(354, 0);
		this.m_btnOpenAssetBrowser.Name = "m_btnOpenAssetBrowser";
		this.m_btnOpenAssetBrowser.Size = new System.Drawing.Size(24, 22);
		this.m_btnOpenAssetBrowser.TabIndex = 1;
		this.m_btnOpenAssetBrowser.Text = "…";
		this.m_btnOpenAssetBrowser.UseVisualStyleBackColor = true;
		this.m_btnOpenAssetBrowser.Click += new System.EventHandler(btn_openAssetBrowser);
		this.m_textBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.m_textBox.Location = new System.Drawing.Point(0, 0);
		this.m_textBox.Multiline = true;
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.Size = new System.Drawing.Size(332, 22);
		this.m_textBox.TabIndex = 0;
		this.m_textBox.WordWrap = false;
		this.m_btnClear.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.m_btnClear.Location = new System.Drawing.Point(331, 0);
		this.m_btnClear.Name = "m_btnClear";
		this.m_btnClear.Size = new System.Drawing.Size(24, 22);
		this.m_btnClear.TabIndex = 0;
		this.m_btnClear.UseVisualStyleBackColor = true;
		this.m_btnClear.Click += new System.EventHandler(m_btnClear_Click);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.m_btnClear);
		base.Controls.Add(this.m_textBox);
		base.Controls.Add(this.m_btnOpenAssetBrowser);
		base.Controls.Add(this.m_btnPlayAnimation);
		base.Name = "AnimationBindingControl";
		base.Size = new System.Drawing.Size(400, 22);
		base.Leave += new System.EventHandler(textBox_leavefocus);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

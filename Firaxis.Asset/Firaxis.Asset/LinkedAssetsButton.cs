using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Asset.Properties;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class LinkedAssetsButton : UserControl
{
	public bool hover;

	private IContainer components = null;

	private ContextMenuStrip popupMenu;

	private ToolStripMenuItem toolStripMenuItem1;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IAssetTarget AssetProvider { get; set; }

	public LinkedAssetsButton()
	{
		InitializeComponent();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		Rectangle clientRectangle = base.ClientRectangle;
		graphics.FillRectangle(SystemBrushes.ControlDark, clientRectangle);
		int num = clientRectangle.Y + clientRectangle.Height / 2;
		DrawingHelper.DrawImageCentered(graphics, Resources.links, clientRectangle.X + 2 + 8, num);
		using StringFormat stringFormat = new StringFormat();
		using Font font = new Font(Font, hover ? FontStyle.Underline : FontStyle.Regular);
		stringFormat.LineAlignment = StringAlignment.Center;
		graphics.DrawString("Linked Assets", font, Brushes.Black, clientRectangle.X + 20, num, stringFormat);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		hover = true;
		Invalidate();
		Update();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hover = false;
		Invalidate();
		Update();
		base.OnMouseLeave(e);
	}

	protected override void OnClick(EventArgs e)
	{
		popupMenu.Show(Cursor.Position);
	}

	private void popupMenu_Opening(object sender, CancelEventArgs e)
	{
		popupMenu.Items.Clear();
		if (AssetProvider != null && Context.TryGet<IAssetTargetCollector>(out var service))
		{
			foreach (IAssetTarget assetTarget in service.AssetTargets)
			{
				if (assetTarget != AssetProvider && (assetTarget == AssetProvider.ParentAsset || assetTarget.ParentAsset == AssetProvider || (assetTarget.ParentAsset != null && assetTarget.ParentAsset == AssetProvider.ParentAsset)))
				{
					Image assetImage = GetAssetImage(assetTarget);
					ToolStripItem toolStripItem = popupMenu.Items.Add(assetTarget.TargetName, assetImage, OnClickAssetProvider);
					toolStripItem.Tag = assetTarget;
				}
			}
		}
		if (popupMenu.Items.Count == 0)
		{
			e.Cancel = true;
		}
	}

	private Image GetAssetImage(IAssetTarget at)
	{
		ImageProviderAttribute attribute = ReflectionHelper.GetAttribute<ImageProviderAttribute>(at);
		return (attribute != null) ? ImageHelper.GetResourceImage(attribute.Location, attribute.Name) : Resources.multi_windows;
	}

	private void OnClickAssetProvider(object sender, EventArgs e)
	{
		IAssetTarget assetTarget = (IAssetTarget)((ToolStripItem)sender).Tag;
		assetTarget.ShowIt();
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
		this.popupMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.popupMenu.SuspendLayout();
		base.SuspendLayout();
		this.popupMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.toolStripMenuItem1 });
		this.popupMenu.Name = "popupMenu";
		this.popupMenu.Size = new System.Drawing.Size(181, 26);
		this.popupMenu.Opening += new System.ComponentModel.CancelEventHandler(popupMenu_Opening);
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
		this.toolStripMenuItem1.Text = "toolStripMenuItem1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Name = "LinkedAssetsButton";
		base.Size = new System.Drawing.Size(117, 21);
		this.popupMenu.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

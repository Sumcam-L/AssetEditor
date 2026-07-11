using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.ATF;

public class CollapsiblePropertyEditingListControl : CollapsibleControl
{
	private IPropertyEditingListContext m_context;

	private PropertyEditingListControl m_gridControl;

	private IContainer components;

	public CollapsiblePropertyEditingListControl()
	{
		InitializeComponent();
		m_gridControl = new PropertyEditingListControl();
		m_gridControl.Dock = DockStyle.Fill;
		base.ChildControl = m_gridControl;
	}

	public void Bind(IPropertyEditingListContext context)
	{
		m_context = context;
		m_gridControl.Bind(m_context);
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
		new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.ATF.CollapsiblePropertyEditingListControl));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.Name = "PropertyEditingListControl";
		base.Size = new System.Drawing.Size(445, 290);
		base.ResumeLayout(false);
	}
}

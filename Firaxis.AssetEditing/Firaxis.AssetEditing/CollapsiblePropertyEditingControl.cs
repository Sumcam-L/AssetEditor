using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class CollapsiblePropertyEditingControl : CollapsibleControl
{
	private IPropertyEditingListContext m_context;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyControl;

	private IContainer components;

	public CollapsiblePropertyEditingControl()
	{
		InitializeComponent();
		m_propertyControl = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
		m_propertyControl.Dock = DockStyle.Fill;
		base.ChildControl = m_propertyControl;
	}

	public void Bind(IPropertyEditingListContext context)
	{
		m_context = context;
		m_propertyControl.Bind(m_context);
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
		new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.AssetEditing.CollapsiblePropertyEditingControl));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.Name = "CollapsiblePropertyEditingControl";
		base.Size = new System.Drawing.Size(445, 290);
		base.ResumeLayout(false);
	}
}

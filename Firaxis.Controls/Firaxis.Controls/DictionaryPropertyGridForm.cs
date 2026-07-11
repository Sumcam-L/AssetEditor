using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class DictionaryPropertyGridForm : Form
{
	private IContainer components = null;

	private PropertyGridEx propertyGrid;

	public DictionaryPropertyGridForm(IDictionary dictionary)
		: this(dictionary, null)
	{
	}

	public DictionaryPropertyGridForm(IDictionary dictionary, IDictionary descriptions)
	{
		InitializeComponent();
		propertyGrid.SelectedObject = new DictionaryPropertyGridAdapter(dictionary, descriptions);
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
		this.propertyGrid = new Firaxis.Controls.PropertyGridEx();
		base.SuspendLayout();
		this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.propertyGrid.Filter = "";
		this.propertyGrid.Location = new System.Drawing.Point(0, 0);
		this.propertyGrid.Name = "propertyGrid";
		this.propertyGrid.ReadOnly = false;
		this.propertyGrid.Size = new System.Drawing.Size(284, 264);
		this.propertyGrid.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(284, 264);
		base.Controls.Add(this.propertyGrid);
		base.Name = "DictionaryPropertyGridForm";
		this.Text = "Editor";
		base.ResumeLayout(false);
	}
}

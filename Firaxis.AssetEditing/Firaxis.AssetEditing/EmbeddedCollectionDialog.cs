using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EmbeddedCollectionDialog : Form
{
	private AttachmentPointAdapter m_attachmentAdapter;

	private DynamicFieldPropertyDescriptorBase m_descriptor;

	private readonly PropertyEditorControlContext m_context;

	private EmbeddedCollectionEditor.CollectionControl m_lvValues;

	private Button m_btnCancel;

	public EmbeddedCollectionDialog(EmbeddedCollectionEditor editor, PropertyEditorControlContext context)
	{
		Text = "Collection Editor";
		m_context = context;
		m_attachmentAdapter = m_context.LastSelectedObject as AttachmentPointAdapter;
		m_descriptor = m_context.Descriptor.As<DynamicFieldPropertyDescriptorBase>();
		editor.GetItemInsertersFunc = delegate
		{
			CollectionFieldValueAdapter obj = m_descriptor.GetFieldAdapter(m_attachmentAdapter) as CollectionFieldValueAdapter;
			ICollectionParameter collectionParameter = obj.Parameter as ICollectionParameter;
			_ = obj.Value;
			EmbeddedCollectionEditor.ItemInserter itemInserter = new EmbeddedCollectionEditor.ItemInserter(FieldValueHelper.GetFieldDomNodeType(collectionParameter.EntryValueType).Name, ResourceUtil.GetImage16(Resources.AddItemIcon), delegate
			{
				CollectionFieldValueAdapter collectionFieldValueAdapter = m_descriptor.GetFieldAdapter(m_attachmentAdapter) as CollectionFieldValueAdapter;
				ICollectionParameter cp = collectionFieldValueAdapter.Parameter as ICollectionParameter;
				ICollectionValue cv = collectionFieldValueAdapter.Value as ICollectionValue;
				m_attachmentAdapter.DomNode.GetRoot().As<TransactionContext>().DoTransaction(delegate
				{
					int num = 1;
					string entryName = cp.Name;
					while (cv.Items.Any((IValue val) => val.ParameterName == entryName))
					{
						entryName = $"{cp.Name}_{num++}";
					}
					if (cv.EntryValueType == Firaxis.CivTech.AssetObjects.ValueType.VT_OBJECT)
					{
						cv.Push<IObjectValue>(entryName, cv.EntryObjectType, entryName);
					}
					else
					{
						cv.AddValue(entryName, cp.EntryParameter);
					}
				}, "Add Value");
				return EmptyArray<EmbeddedCollectionEditor.ItemInserter>.Instance;
			});
			return new EmbeddedCollectionEditor.ItemInserter[1] { itemInserter };
		};
		editor.RemoveItemFunc = delegate(PropertyEditorControlContext ctxt, object item)
		{
			CollectionFieldValueAdapter collectionFieldValueAdapter = m_descriptor.GetFieldAdapter(m_attachmentAdapter) as CollectionFieldValueAdapter;
			ICollectionValue collectionValue = collectionFieldValueAdapter.Value as ICollectionValue;
			m_attachmentAdapter.DomNode.GetRoot().As<TransactionContext>().DoTransaction(delegate
			{
				IFieldValueAdapter fieldValueAdapter = item.Cast<DomNode>().As<IFieldValueAdapter>();
				if (fieldValueAdapter != null)
				{
					collectionValue.Remove(fieldValueAdapter.Value);
				}
			}, "Remove Value");
		};
		m_lvValues = new EmbeddedCollectionEditor.CollectionControl(editor, context, toolStripLabelsEnabled: true);
		InitializeComponent();
		base.Load += EmbeddedCollectionDialog_Load;
	}

	private void EmbeddedCollectionDialog_Load(object sender, EventArgs e)
	{
		RefreshListItems();
	}

	private void RefreshListItems()
	{
		m_lvValues.Refresh();
	}

	private void InitializeComponent()
	{
		new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.AssetEditing.EmbeddedCollectionDialog));
		this.m_btnCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_lvValues.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_lvValues.Location = new System.Drawing.Point(12, 12);
		this.m_lvValues.Name = "lvValues";
		this.m_lvValues.Size = new System.Drawing.Size(500, 312);
		this.m_lvValues.TabIndex = 0;
		this.m_btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnCancel.Location = new System.Drawing.Point(436, 350);
		this.m_btnCancel.Name = "btnCancel";
		this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
		this.m_btnCancel.TabIndex = 1;
		this.m_btnCancel.Text = "&Close";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		base.ClientSize = new System.Drawing.Size(524, 381);
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_lvValues);
		base.Name = "EmbeddedCollectionDialog";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Controls;
using Firaxis.Error;
using Firaxis.MathEx;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using UtilityTools.Helpers;

namespace Firaxis.AssetEditing;

public class ObjectCookParameterEditor : IPropertyEditor
{
	private class UserCanceledResultCode : ResultCode
	{
		public UserCanceledResultCode(string msg)
			: base(msg)
		{
		}

		public UserCanceledResultCode(string msg, params object[] args)
			: base(msg, args)
		{
		}
	}

	private class ObjectCookParameterControl : Control
	{
		private readonly PropertyEditorControlContext m_context;

		private readonly ContextMenuStrip m_contextMenu;

		private readonly Label m_valueDisplay;

		private readonly Button m_btnAddNew;

		private readonly Button m_btnAddExisting;

		private readonly Button m_btnReimport;

		private readonly Button m_btnGotoEntity;

		private readonly Button m_btnClear;

		private readonly Button m_btnOpenSourceExternally;

		private readonly Button m_btnDropMenu;

		private const int m_topAndLeftMargin = 1;

		private readonly IAssetBrowserDialogService m_assetBrowser;

		private readonly IFileDialogService m_fileBrowser;

		private readonly AssetBrowserFileCommands m_commands;

		private readonly IImportService m_importService;

		private readonly ICivTechService m_civtechService;

		private readonly IEnumerable<IDocumentClient> m_documentClients;

		private readonly IDocumentRegistryMediator m_registryMediator;

		private readonly BatchEntitySourceControlService m_sourceControl;

		private IAssetPreviewerService PreviewerService { get; }

		public ObjectCookParameterControl(PropertyEditorControlContext context, IAssetBrowserDialogService assetBrowser, IFileDialogService fileBrowser, AssetBrowserFileCommands commands, IImportService importService, ICivTechService civtechService, IEnumerable<IDocumentClient> documentClients, IDocumentRegistryMediator registryMediator, BatchEntitySourceControlService sourceControl, IAssetPreviewerService previewerService)
		{
			m_assetBrowser = assetBrowser;
			m_fileBrowser = fileBrowser;
			m_commands = commands;
			m_importService = importService;
			m_context = context;
			m_civtechService = civtechService;
			m_documentClients = documentClients;
			m_registryMediator = registryMediator;
			m_sourceControl = sourceControl;
			PreviewerService = previewerService;
			m_contextMenu = new ContextMenuStrip();
			ContextMenuStrip = m_contextMenu;
			m_valueDisplay = new Label();
			m_valueDisplay.Text = "";
			m_valueDisplay.TextAlign = ContentAlignment.MiddleLeft;
			m_valueDisplay.ForeColor = Color.Black;
			m_valueDisplay.AutoEllipsis = true;
			m_valueDisplay.DoubleClick += HandleOpenDocumentButtonClick;
			m_valueDisplay.TextChanged += HandleBoundObjectChanged;
			base.Controls.Add(m_valueDisplay);
			m_btnAddNew = new Button();
			m_btnAddNew.Click += HandleNewButtonClick;
			m_btnAddNew.Image = ResourceUtil.GetImage16(Firaxis.ATF.Resources.AddNewEntityIcon);
			m_btnAddNew.FlatStyle = FlatStyle.Standard;
			m_btnAddNew.FlatAppearance.BorderSize = 0;
			m_btnAddNew.BackgroundImageLayout = ImageLayout.Center;
			m_btnAddNew.Size = CalculateImageButtonSize(m_btnAddNew);
			base.Controls.Add(m_btnAddNew);
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("Add New".Localize());
			toolStripMenuItem.Click += HandleNewButtonClick;
			toolStripMenuItem.Image = ResourceUtil.GetImage16(Firaxis.ATF.Resources.AddNewEntityIcon);
			m_contextMenu.Items.Add(toolStripMenuItem);
			m_btnAddExisting = new Button();
			m_btnAddExisting.Click += HandleOpenButtonClick;
			m_btnAddExisting.Image = ResourceUtil.GetImage16(Firaxis.ATF.Resources.AddExistingEntityIcon);
			m_btnAddExisting.FlatStyle = FlatStyle.Standard;
			m_btnAddExisting.FlatAppearance.BorderSize = 0;
			m_btnAddExisting.BackgroundImageLayout = ImageLayout.Center;
			m_btnAddExisting.Size = CalculateImageButtonSize(m_btnAddExisting);
			base.Controls.Add(m_btnAddExisting);
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("Add Existing".Localize());
			toolStripMenuItem2.Click += HandleOpenButtonClick;
			toolStripMenuItem2.Image = ResourceUtil.GetImage16(Firaxis.ATF.Resources.AddExistingEntityIcon);
			m_contextMenu.Items.Add(toolStripMenuItem2);
			m_contextMenu.Items.Add("-");
			m_btnReimport = new Button();
			m_btnReimport.Click += HandleReimportButtonClick;
			m_btnReimport.ImageAlign = ContentAlignment.MiddleCenter;
			m_btnReimport.Image = ResourceUtil.GetImage16(Resources.ReimportFileIcon);
			m_btnReimport.FlatStyle = FlatStyle.Standard;
			m_btnReimport.FlatAppearance.BorderSize = 0;
			m_btnReimport.BackgroundImageLayout = ImageLayout.Center;
			m_btnReimport.Size = CalculateImageButtonSize(m_btnReimport);
			base.Controls.Add(m_btnReimport);
			ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem("Reimport".Localize());
			toolStripMenuItem3.Click += HandleReimportButtonClick;
			toolStripMenuItem3.Image = ResourceUtil.GetImage16(Resources.ReimportFileIcon);
			m_contextMenu.Items.Add(toolStripMenuItem3);
			m_btnGotoEntity = new Button();
			m_btnGotoEntity.Click += HandleOpenDocumentButtonClick;
			m_btnGotoEntity.Image = ResourceUtil.GetImage16(Firaxis.ATF.Resources.GotoFileIcon);
			m_btnGotoEntity.FlatStyle = FlatStyle.Standard;
			m_btnGotoEntity.FlatAppearance.BorderSize = 0;
			m_btnGotoEntity.BackgroundImageLayout = ImageLayout.Center;
			m_btnGotoEntity.Size = CalculateImageButtonSize(m_btnGotoEntity);
			base.Controls.Add(m_btnGotoEntity);
			ToolStripMenuItem value = new ToolStripMenuItem("Open".Localize(), ResourceUtil.GetImage16(Firaxis.ATF.Resources.GotoFileIcon), HandleOpenDocumentButtonClick, "Open");
			m_contextMenu.Items.Add(value);
			m_btnClear = new Button();
			m_btnClear.Click += HandleClearButtonClick;
			m_btnClear.Image = ResourceUtil.GetImage16(Resources.RemoveItemIcon);
			m_btnClear.FlatStyle = FlatStyle.Standard;
			m_btnClear.FlatAppearance.BorderSize = 0;
			m_btnClear.BackgroundImageLayout = ImageLayout.Center;
			m_btnClear.Size = CalculateImageButtonSize(m_btnClear);
			base.Controls.Add(m_btnClear);
			ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem("Remove".Localize());
			toolStripMenuItem4.Click += HandleClearButtonClick;
			toolStripMenuItem4.Image = ResourceUtil.GetImage16(Resources.RemoveItemIcon);
			m_contextMenu.Items.Add(toolStripMenuItem4);
			m_btnOpenSourceExternally = new Button();
			m_btnOpenSourceExternally.Click += PreviewSourceFile;
			m_btnOpenSourceExternally.Image = ResourceUtil.GetImage16(Resources.OpenSourceFileIcon);
			m_btnOpenSourceExternally.FlatStyle = FlatStyle.Standard;
			m_btnOpenSourceExternally.FlatAppearance.BorderSize = 0;
			m_btnOpenSourceExternally.BackgroundImageLayout = ImageLayout.Center;
			m_btnOpenSourceExternally.Size = CalculateImageButtonSize(m_btnOpenSourceExternally);
			base.Controls.Add(m_btnOpenSourceExternally);
			ToolStripMenuItem value2 = new ToolStripMenuItem("Open Source File".Localize(), ResourceUtil.GetImage16(Resources.OpenSourceFileIcon), PreviewSourceFile, "OpenSourceFile");
			m_contextMenu.Items.Add(value2);
			m_btnDropMenu = new Button();
			m_btnDropMenu.Text = "▼";
			m_btnDropMenu.TextAlign = ContentAlignment.MiddleCenter;
			m_btnDropMenu.AutoSize = true;
			m_btnDropMenu.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			m_btnDropMenu.FlatStyle = FlatStyle.Standard;
			m_btnDropMenu.FlatAppearance.BorderSize = 0;
			m_btnDropMenu.BackColor = ControlPaint.Light(m_btnDropMenu.BackColor);
			m_btnDropMenu.MouseClick += ShowContextMenu;
			m_valueDisplay.DoubleClick += HandleOpenDocumentButtonClick;
			base.Controls.Add(m_btnDropMenu);
			UpdateControlPlacement();
			RefreshLabelFromData();
			base.Height = m_btnClear.Image.Height + 2;
			base.SizeChanged += ObjectCookParameterControl_SizeChanged;
			base.VisibleChanged += ObjectCookParameterControl_VisibleChanged;
			base.GotFocus += ShowContextMenu;
			base.BackColorChanged += ObjectCookParameterControl_BackColorChanged;
			base.ForeColorChanged += ObjectCookParameterControl_ForeColorChanged;
			AllowDrop = true;
			base.DragDrop += HandleDragDrop;
			base.DragEnter += HandleDragEnter;
		}

		private void HandleDragDrop(object sender, DragEventArgs e)
		{
			if (CanReceiveDrop(e) && e.Data.GetData("Object") is EntityDragDropDataObject entityDragDropDataObject)
			{
				SetValue(entityDragDropDataObject.ID.Name);
			}
		}

		private void HandleDragEnter(object sender, DragEventArgs e)
		{
			if (CanReceiveDrop(e))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private bool CanReceiveDrop(DragEventArgs e)
		{
			if (!e.Data.GetDataPresent("Object"))
			{
				return false;
			}
			if (!(e.Data.GetData("Object") is EntityDragDropDataObject entityDragDropDataObject))
			{
				return false;
			}
			if (!CheckValidState("CanReceiveDrop"))
			{
				return false;
			}
			ObjectFieldValueAdapter objectFieldValueAdapter = (m_context.Descriptor as PropertyDescriptor).GetNode(m_context.LastSelectedObject).As<ObjectFieldValueAdapter>();
			if (objectFieldValueAdapter.ValidTypes.Contains(entityDragDropDataObject.ID.Type))
			{
				return objectFieldValueAdapter.ValidClassNames.Contains(entityDragDropDataObject.ClassName);
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			base.SizeChanged -= ObjectCookParameterControl_SizeChanged;
			base.VisibleChanged -= ObjectCookParameterControl_VisibleChanged;
			base.GotFocus -= ShowContextMenu;
			AllowDrop = false;
			base.DragDrop -= HandleDragDrop;
			base.DragEnter -= HandleDragEnter;
			m_valueDisplay.DoubleClick -= HandleOpenDocumentButtonClick;
			m_valueDisplay.TextChanged -= HandleBoundObjectChanged;
			m_btnAddNew.Click -= HandleNewButtonClick;
			m_btnAddExisting.Click -= HandleOpenButtonClick;
			m_btnReimport.Click -= HandleReimportButtonClick;
			m_btnGotoEntity.Click -= HandleOpenDocumentButtonClick;
			m_btnClear.Click -= HandleClearButtonClick;
			m_btnOpenSourceExternally.Click -= PreviewSourceFile;
			m_btnDropMenu.MouseClick -= ShowContextMenu;
			m_valueDisplay.DoubleClick -= HandleOpenDocumentButtonClick;
			if (disposing)
			{
				m_contextMenu.Dispose();
				m_valueDisplay.Dispose();
				m_btnAddNew.Dispose();
				m_btnAddExisting.Dispose();
				m_btnReimport.Dispose();
				m_btnGotoEntity.Dispose();
				m_btnClear.Dispose();
				m_btnOpenSourceExternally.Dispose();
				m_btnDropMenu.Dispose();
			}
			base.Dispose(disposing);
		}

		private void ShowContextMenu(object sender, EventArgs e)
		{
			if (m_btnDropMenu.ContainsCursor())
			{
				m_contextMenu.Show(m_btnDropMenu, new Point(m_btnDropMenu.Width - m_contextMenu.Width, m_btnDropMenu.Height - 2));
			}
		}

		private Size CalculateImageButtonSize(Button button)
		{
			if (button != null && button.Image != null)
			{
				return new Size(button.Image.Width + 2, button.Image.Height + 2);
			}
			return new Size(0, 0);
		}

		private void UpdateButtonVisibility(bool buttonsVisible)
		{
			m_btnAddNew.Visible = buttonsVisible;
			m_btnAddExisting.Visible = buttonsVisible;
			m_btnReimport.Visible = buttonsVisible;
			m_btnGotoEntity.Visible = buttonsVisible;
			m_btnClear.Visible = buttonsVisible;
			m_btnOpenSourceExternally.Visible = buttonsVisible;
			m_btnDropMenu.Visible = !buttonsVisible;
		}

		private void ObjectCookParameterControl_ForeColorChanged(object sender, EventArgs e)
		{
			m_valueDisplay.ForeColor = ForeColor;
			m_btnDropMenu.ForeColor = ForeColor;
		}

		private void ObjectCookParameterControl_BackColorChanged(object sender, EventArgs e)
		{
			m_valueDisplay.BackColor = BackColor;
			m_btnAddNew.BackColor = BackColor;
			m_btnAddExisting.BackColor = BackColor;
			m_btnReimport.BackColor = BackColor;
			m_btnGotoEntity.BackColor = BackColor;
			m_btnClear.BackColor = BackColor;
			m_btnOpenSourceExternally.BackColor = BackColor;
		}

		private void ObjectCookParameterControl_VisibleChanged(object sender, EventArgs e)
		{
			if (base.Visible)
			{
				SkinService.ApplyActiveSkin(this);
				SkinService.ApplyActiveSkin(m_contextMenu);
				m_valueDisplay.ForeColor = ForeColor;
				object value = m_context.GetValue();
				if (value != null)
				{
					string empty = string.Empty;
					empty = ((!(value is string)) ? value.ToString() : ((string)value));
					m_valueDisplay.Text = empty;
					UpdateControlPlacement();
					UpdateButtonEnabledStatus(empty);
				}
			}
		}

		private void RefreshLabelFromData()
		{
			object value = m_context.GetValue();
			string value2 = string.Empty;
			if (value != null)
			{
				value2 = value.ToString();
			}
			if (!m_valueDisplay.Text.Equals(value2, StringComparison.CurrentCultureIgnoreCase))
			{
				m_valueDisplay.Text = value2;
				UpdateButtonEnabledStatus(value2);
			}
		}

		public override void Refresh()
		{
			RefreshLabelFromData();
			base.Refresh();
		}

		private void SetValue(string newValue)
		{
			m_context.SetValue(newValue);
			object value = m_context.GetValue();
			m_valueDisplay.Text = value.ToString();
			UpdateButtonEnabledStatus(newValue);
		}

		private void ObjectCookParameterControl_SizeChanged(object sender, EventArgs e)
		{
			UpdateControlPlacement();
		}

		private void UpdateButtonEnabledStatus(string value)
		{
			bool flag = !CanOpenDocument();
			foreach (ToolStripItem item in m_contextMenu.Items)
			{
				item.Enabled = true;
			}
			Color mouseOverBackColor = m_btnAddNew.BackColor.Blend(m_btnAddNew.ForeColor, 0.65f);
			m_btnAddNew.Enabled = true;
			m_btnAddNew.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
			m_btnAddExisting.Enabled = true;
			m_btnAddExisting.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
			m_btnReimport.Enabled = true;
			m_btnReimport.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
			m_btnGotoEntity.Enabled = true;
			m_btnGotoEntity.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
			m_btnClear.Enabled = true;
			m_btnClear.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
			m_btnOpenSourceExternally.Enabled = true;
			m_btnOpenSourceExternally.FlatAppearance.MouseOverBackColor = mouseOverBackColor;
			if (m_context.LastSelectedObject is DomNodeAdapter domNodeAdapter)
			{
				IDocument document = domNodeAdapter.DomNode.GetRoot().As<IDocument>();
				if (document != null && document.IsReadOnly)
				{
					m_btnAddNew.Enabled = false;
					m_btnAddExisting.Enabled = false;
					m_btnReimport.Enabled = false;
					m_btnClear.Enabled = false;
					m_btnGotoEntity.Enabled = !flag;
					{
						foreach (ToolStripItem item2 in m_contextMenu.Items)
						{
							if (item2.Name == "Open" || item2.Name == "OpenSourceFile")
							{
								item2.Enabled = true;
							}
							else
							{
								item2.Enabled = false;
							}
						}
						return;
					}
				}
			}
			m_btnClear.Enabled = !string.IsNullOrWhiteSpace(value);
			m_btnGotoEntity.Enabled = !flag;
		}

		private void UpdateControlPlacement()
		{
			Point location = new Point(base.ClientSize.Width, 0);
			int num = CreateGraphics().MeasureString(m_valueDisplay.Text, m_valueDisplay.Font).ToSize().Width + base.Controls[0].Padding.Left + base.Controls[0].Padding.Right + 8;
			int num2 = base.ClientSize.Width - num;
			if (num2 > 120)
			{
				num2 = 120;
			}
			if (num2 < 20)
			{
				num2 = 20;
			}
			base.Controls[1].Location = location;
			for (int num3 = base.Controls.Count - 1; num3 >= 1; num3--)
			{
				Control control = base.Controls[num3];
				location.X -= CalculateImageButtonSize((Button)control).Width;
				control.Location = location;
			}
			m_btnDropMenu.Location = new Point(base.ClientSize.Width - m_btnDropMenu.Width, (int)(((double)(base.ClientSize.Height - m_btnDropMenu.Height) + 0.5) / 2.0));
			if (base.ClientSize.Width - location.X > num2)
			{
				UpdateButtonVisibility(buttonsVisible: false);
				location.X = base.ClientSize.Width - m_btnDropMenu.Width;
			}
			else
			{
				UpdateButtonVisibility(buttonsVisible: true);
			}
			if (base.Controls.Count > 0)
			{
				base.Controls[0].Location = new Point(1, 1);
				base.Controls[0].Width = location.X - 2;
				base.Controls[0].Height = base.Height - 2;
			}
		}

		private void HandleOpenButtonClick(object sender, EventArgs e)
		{
			if (CheckValidState("HandleOpenButtonClick"))
			{
				ObjectFieldValueAdapter objectFieldValueAdapter = (m_context.Descriptor as PropertyDescriptor).GetNode(m_context.LastSelectedObject).As<ObjectFieldValueAdapter>();
				string pathName = "";
				m_assetBrowser.OpenFileName(ref pathName, objectFieldValueAdapter.EntityFilteringContext);
				if (!string.IsNullOrEmpty(pathName))
				{
					StaticMethods.GetInstanceNameAndType(m_civtechService.ProjectMapService, pathName, out var instanceName, out var _);
					SetValue(StaticMethods.SanitizeEntityName(instanceName));
				}
			}
		}

		private void PreviewSourceFile(object sender, EventArgs e)
		{
			if (!CheckValidState("PreviewSourceFile"))
			{
				return;
			}
			using (new Firaxis.Utility.WaitCursor())
			{
				ObjectFieldValueAdapter objectFieldValueAdapter = (m_context.Descriptor as PropertyDescriptor).GetNode(m_context.LastSelectedObject).As<ObjectFieldValueAdapter>();
				string objectName = objectFieldValueAdapter.ObjectName;
				InstanceType result = InstanceType.IT_INVALID;
				if (!Enum.TryParse<InstanceType>(objectFieldValueAdapter.ObjectType, out result))
				{
					return;
				}
				using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { m_civtechService.GetActivePantryPaths() });
				IInstanceEntity instanceEntity = instanceSet.LoadEntityByName(objectName, result);
				if (instanceEntity != null)
				{
					if (!StaticMethods.IsImportableType(result))
					{
						foreach (IImportedEntity importableEntity in global::DatabaseWrapper.DatabaseWrapper.GetImportableEntities(m_civtechService, instanceEntity, instanceSet, recursive: true))
						{
							OpenEntitySourceFileForPreview(m_civtechService, importableEntity);
						}
						return;
					}
					if (instanceEntity is IImportedEntity entity)
					{
						OpenEntitySourceFileForPreview(m_civtechService, entity);
					}
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, "Could not load the instance entity with the name {0} and the type {1} for the purpose of getting its children to preview sources with.", objectFieldValueAdapter.ObjectName, EnumToStringConverter.GetNameFromType(result));
				}
			}
		}

		private void OpenEntitySourceFileForPreview(ICivTechService civTechSvc, IImportedEntity entity)
		{
			CivTechHelper.OpenSourceFile(civTechSvc, entity);
		}

		private void HandleReimportButtonClick(object sender, EventArgs e)
		{
			if (!m_btnReimport.Enabled)
			{
				MessageBox.Show("Unable to reimport this item since it is not from the active project.");
			}
			else
			{
				if (!CheckValidState("HandleReimportButtonClick"))
				{
					return;
				}
				DomNode node = (m_context.Descriptor as PropertyDescriptor).GetNode(m_context.LastSelectedObject);
				ObjectFieldValueAdapter objectFieldValueAdapter = node.As<ObjectFieldValueAdapter>();
				InstanceType result = InstanceType.IT_INVALID;
				if (!Enum.TryParse<InstanceType>(objectFieldValueAdapter.ObjectType, out result))
				{
					return;
				}
				_ = m_civtechService.PrimaryProject.Paths.GamePantry;
				IEnumerable<EntityID> entitiesToReimport = GetEntitiesToReimport(objectFieldValueAdapter.ObjectName, result);
				IEnumerable<EntityID> source = entitiesToReimport.Where((EntityID fup) => !m_civtechService.IsFromActiveProjectOrDependencies(fup));
				IEnumerable<EntityID> source2 = entitiesToReimport.Where((EntityID npp) => m_civtechService.IsFromProjectDependencies(npp));
				if (source.Any())
				{
					MessageBoxes.Show(string.Format("The following entities are from an unrelated project and can not be edited:\n\n{0}", string.Join("\n", source.Select((EntityID fups) => fups.Name))), "Unable to edit entities", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				else
				{
					if (source2.Any() && MessageBoxes.Show(string.Format("The following entities from dependency project are about to be edited:\n\n{0}\n\nAre you sure?", string.Join("\n", source2.Select((EntityID npps) => npps.Name))), "Editing dependency project", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						return;
					}
					using (new Firaxis.Utility.WaitCursor())
					{
						new EntityImporter(m_civtechService, m_importService, m_commands, m_registryMediator, m_sourceControl, entitiesToReimport, recurseIntoChildren: false).Import();
						RestoreParentDocument(node);
						if (PreviewerService != null)
						{
							IEntityChangeList changeList = Context.EnsureCreated<CivTechContext>().CreateInstance<IEntityChangeList>();
							changeList.AddGenericEntityChangedEvents(entitiesToReimport);
							PreviewerService.SendChanges(changeList);
						}
					}
				}
			}
		}

		private IEnumerable<EntityID> GetEntitiesToReimport(string baseEntityName, InstanceType baseEntityType)
		{
			List<EntityID> entitiesToReimport = new List<EntityID>();
			if (StaticMethods.IsImportableType(baseEntityType))
			{
				entitiesToReimport.Add(new EntityID(baseEntityName, baseEntityType));
			}
			else
			{
				_ = m_civtechService.PrimaryProject.Paths.GamePantry;
				using IInstanceSet instanceSet = Context.Get<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { m_civtechService.GetActivePantryPaths() });
				IInstanceEntity instanceEntity = instanceSet.LoadEntityByName(baseEntityName, baseEntityType);
				if (instanceEntity != null)
				{
					instanceEntity.LoadDependentEntities(instanceSet);
					instanceSet.Items.OfType<IImportedEntity>().ToList().ForEach(delegate(IImportedEntity ent)
					{
						entitiesToReimport.Add(new EntityID(ent.Name, ent.Type));
					});
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, "Could not load the instance entity with the name {0} and the type {1} for the purpose of getting its children to reimport.", baseEntityName, EnumToStringConverter.GetNameFromType(baseEntityType));
				}
			}
			return entitiesToReimport;
		}

		private ResultCode RestoreParentDocument()
		{
			DomNode dn = null;
			ResultCode contextDomNode = GetContextDomNode(out dn);
			if (!contextDomNode)
			{
				return contextDomNode;
			}
			return RestoreParentDocument(dn);
		}

		private ResultCode RestoreParentDocument(DomNode node)
		{
			DomNode root = node.GetRoot();
			if (ShowExistingDocument(root))
			{
				return ResultCode.Success;
			}
			if (OpenAsEntityDocument(root))
			{
				return ResultCode.Success;
			}
			if (OpenStandardDocument(root))
			{
				return ResultCode.Success;
			}
			BugSubmitter.SilentReport("@summary Unable to restore the parent document\nTarget for the DomNode: " + node.ToString() + " @assign bwhitman");
			return new ResultCode("Failed to restore parent document as active document!");
		}

		private bool ShowExistingDocument(DomNode node)
		{
			IEntityDocument entityDocument = node.As<IEntityDocument>();
			if (entityDocument == null)
			{
				return false;
			}
			IDocumentClient firstClientForPath = m_documentClients.GetFirstClientForPath(entityDocument.Uri.LocalPath);
			if (firstClientForPath != null)
			{
				return m_commands.OpenExistingDocument(firstClientForPath, entityDocument.Uri) != null;
			}
			return false;
		}

		private bool OpenAsEntityDocument(DomNode node)
		{
			IEntityDocument entityDocument = node.As<IEntityDocument>();
			if (entityDocument == null)
			{
				return false;
			}
			IInstanceEntity instanceEntity = entityDocument.InstanceEntity;
			if (instanceEntity != null)
			{
				return m_commands.OpenExistingDocument(instanceEntity.Type, instanceEntity.Name) != null;
			}
			return false;
		}

		private bool OpenStandardDocument(DomNode node)
		{
			IDocument document = node.As<IDocument>();
			if (document == null)
			{
				return false;
			}
			IDocumentClient firstClientForPath = m_documentClients.GetFirstClientForPath(document.Uri.LocalPath);
			if (firstClientForPath != null)
			{
				return m_commands.OpenExistingDocument(firstClientForPath, document.Uri) != null;
			}
			return false;
		}

		private bool CanOpenDocument()
		{
			ObjectFieldValueAdapter objectFieldValueAdapter = ((m_context.Descriptor is PropertyDescriptor propertyDescriptor) ? propertyDescriptor.GetNode(m_context.LastSelectedObject) : null).As<ObjectFieldValueAdapter>();
			if (string.IsNullOrEmpty(objectFieldValueAdapter?.ObjectName))
			{
				return false;
			}
			InstanceType result = InstanceType.IT_INVALID;
			if (!Enum.TryParse<InstanceType>(objectFieldValueAdapter.ObjectType, out result))
			{
				return false;
			}
			return File.Exists(m_civtechService.ProjectMapService.LayeredPantry.GetPantryPath(objectFieldValueAdapter.ObjectName, result));
		}

		private void HandleOpenDocumentButtonClick(object sender, EventArgs e)
		{
			if (!CheckValidState("HandleOpenDocumentButtonClick"))
			{
				return;
			}
			using (new Firaxis.Utility.WaitCursor())
			{
				ObjectFieldValueAdapter objectFieldValueAdapter = (m_context.Descriptor as PropertyDescriptor).GetNode(m_context.LastSelectedObject).As<ObjectFieldValueAdapter>();
				InstanceType result = InstanceType.IT_INVALID;
				if (!Enum.TryParse<InstanceType>(objectFieldValueAdapter.ObjectType, out result))
				{
					return;
				}
				using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { m_civtechService.GetActivePantryPaths() });
				if (instanceSet.LoadEntityIfUnique(objectFieldValueAdapter.ObjectName, result) != null)
				{
					m_commands.OpenExistingDocument(result, objectFieldValueAdapter.ObjectName);
					return;
				}
				Outputs.WriteLine(OutputMessageType.Error, "Cannot open a document for the {0} entity {1} because it does not exist on disk.  Please create a new document first (using the Add New button).", EnumToStringConverter.GetNameFromType(result), objectFieldValueAdapter.ObjectName);
			}
		}

		private InstanceType GetBoundInstanceType()
		{
			if (!CheckValidState("GetBoundInstanceType"))
			{
				return InstanceType.IT_INVALID;
			}
			ObjectFieldValueAdapter objectFieldValueAdapter = (m_context.Descriptor as PropertyDescriptor).GetNode(m_context.LastSelectedObject).As<ObjectFieldValueAdapter>();
			InstanceType result = InstanceType.IT_INVALID;
			Enum.TryParse<InstanceType>(objectFieldValueAdapter.ObjectType, out result);
			return result;
		}

		private void HandleBoundObjectChanged(object sender, EventArgs e)
		{
			string text = m_valueDisplay.Text;
			InstanceType boundInstanceType = GetBoundInstanceType();
			if (boundInstanceType == InstanceType.IT_INVALID || string.IsNullOrEmpty(text))
			{
				m_btnReimport.Enabled = false;
				return;
			}
			bool enabled = false;
			string entityPath = m_civtechService.GetEntityPath(text, boundInstanceType);
			if (!string.IsNullOrEmpty(entityPath))
			{
				Uri uri = null;
				try
				{
					uri = new Uri(entityPath);
				}
				catch (System.Exception)
				{
					m_btnReimport.Enabled = false;
					return;
				}
				if (m_civtechService.IsFromActiveProjectOrDependencies(uri))
				{
					enabled = true;
				}
			}
			m_btnReimport.Enabled = enabled;
		}

		private void HandleNewButtonClick(object sender, EventArgs e)
		{
			if (!CheckValidState("HandleNewButtonClick"))
			{
				return;
			}
			InstanceType currentType = InstanceType.IT_INVALID;
			ObjectFieldValueAdapter objValAdapter = null;
			ResultCode adapterAndObjectType = GetAdapterAndObjectType(out currentType, out objValAdapter);
			if (!adapterAndObjectType)
			{
				BugSubmitter.SilentReport("@assign bwhitman @summary ObjectCookParameterEditor: Failed to get adapter or object type!\n\n" + adapterAndObjectType.Message);
				return;
			}
			string objName = string.Empty;
			ResultCode resultCode = CreateDocumentAndGetObjectName(objValAdapter, currentType, out objName);
			if (!resultCode)
			{
				BugSubmitter.SilentAssert(resultCode is UserCanceledResultCode, "@assign bwhitman @summary ObjectCookParameterEditor: Failed to create new document!\n\n" + resultCode.Message);
				return;
			}
			ResultCode resultCode2 = UpdateParentDocumentReference(objValAdapter, objName);
			if (!resultCode2)
			{
				BugSubmitter.SilentReport("@assign bwhitman @summary ObjectCookParameterEditor: Failed to update reference to new document!\n\n" + resultCode2.Message);
			}
		}

		private ResultCode GetContextDomNode(out DomNode dn)
		{
			if (!(m_context.Descriptor is PropertyDescriptor propertyDescriptor))
			{
				dn = null;
				return new ResultCode("Property descriptor \"{0}\" was not of type PropertyDescriptor", m_context.Descriptor.Name);
			}
			if (m_context.LastSelectedObject == null)
			{
				dn = null;
				return new ResultCode("LastSelectedObject for context \"{0}\" was null", m_context.ToString());
			}
			dn = propertyDescriptor.GetNode(m_context.LastSelectedObject);
			if (dn == null)
			{
				return new ResultCode("Property descriptor \"{0}\" fialed to GetNode for LastSelectedObject \"{1}\"", m_context.Descriptor.Name, m_context.LastSelectedObject.ToString());
			}
			return ResultCode.Success;
		}

		private ResultCode GetAdapterAndObjectType(out InstanceType currentType, out ObjectFieldValueAdapter objValAdapter)
		{
			currentType = InstanceType.IT_INVALID;
			objValAdapter = null;
			DomNode dn = null;
			ResultCode contextDomNode = GetContextDomNode(out dn);
			if (!contextDomNode)
			{
				return contextDomNode;
			}
			objValAdapter = dn.As<ObjectFieldValueAdapter>();
			if (objValAdapter == null)
			{
				return new ResultCode("Failed to get ObjectFieldValueAdapter from DomNode: \"{0}\"", string.Join(".", dn.Lineage));
			}
			if (!Enum.TryParse<InstanceType>(objValAdapter.ObjectType, out currentType))
			{
				return new ResultCode("Failed to parse \"{0}\" from ObjectFieldValueAdapter \"{1}\" as an InstanceType enum", objValAdapter.ObjectType, objValAdapter.Name);
			}
			return ResultCode.Success;
		}

		private ResultCode CreateDocumentAndGetObjectName(ObjectFieldValueAdapter objValAdapter, InstanceType objType, out string objName)
		{
			if (StaticMethods.IsImportableType(objType))
			{
				return CreateNewImportedDocumentAndGetName(objValAdapter, objType, out objName);
			}
			return CreateNewEntityDocumentAndGetName(objValAdapter, objType, out objName);
		}

		private ResultCode CreateNewImportedDocumentAndGetName(ObjectFieldValueAdapter objValAdapter, InstanceType objType, out string objName)
		{
			string srcFileName = string.Empty;
			ResultCode resultCode = AskUserForSourceFilePath(objType, out srcFileName);
			if (!resultCode)
			{
				objName = string.Empty;
				return resultCode;
			}
			IEntityDocument entityDoc = null;
			ResultCode resultCode2 = CreateInstanceEntityDocument(objType, out entityDoc);
			if (!resultCode2)
			{
				objName = string.Empty;
				return resultCode2;
			}
			ImportedEntityAdapter importedEntityAdapter = entityDoc.As<ImportedEntityAdapter>();
			if (importedEntityAdapter == null)
			{
				objName = string.Empty;
				return new ResultCode("Failed to get ImportedEntityAdapter from IEntityDocument \"{0}\"", entityDoc.Uri.LocalPath);
			}
			ResultCode resultCode3 = EnsureValidEntityNameAndClass(importedEntityAdapter.GetSmartName(srcFileName), objType, objValAdapter.Class, importedEntityAdapter);
			if (!resultCode3)
			{
				objName = string.Empty;
				return resultCode3;
			}
			ResultCode resultCode4 = ImportEntitySource(srcFileName, objValAdapter, importedEntityAdapter, entityDoc, out objName);
			m_commands.Close(entityDoc);
			if (!resultCode4)
			{
				return resultCode4;
			}
			return ResultCode.Success;
		}

		private ResultCode CreateNewEntityDocumentAndGetName(ObjectFieldValueAdapter objValAdapter, InstanceType objType, out string objName)
		{
			IEntityDocument entityDoc = null;
			ResultCode resultCode = CreateInstanceEntityDocument(objType, out entityDoc);
			if (!resultCode)
			{
				objName = string.Empty;
				return resultCode;
			}
			InstanceEntityAdapter entAdapter = entityDoc.As<InstanceEntityAdapter>();
			if (entAdapter == null)
			{
				objName = string.Empty;
				return new ResultCode("Failed to get InstanceEntityAdapter from IEntityDocument \"{0}\"", entityDoc.Uri.LocalPath);
			}
			GetTextInputValidationForm(entityDoc.InstanceEntity.Name, objType);
			TransactionContext transactionContext = entAdapter.As<TransactionContext>();
			if (transactionContext == null)
			{
				entityDoc.Dirty = true;
				entAdapter.UpdateClassName(objValAdapter.Class);
				entAdapter.Update();
			}
			else
			{
				transactionContext.DoTransaction(delegate
				{
					entAdapter.UpdateClassName(objValAdapter.Class);
					entAdapter.Update();
				}, "Updating entity class.");
			}
			return SaveNewEntityDocument(entityDoc, out objName, closeAfterSave: false);
		}

		private ResultCode CreateInstanceEntityDocument(InstanceType objType, out IEntityDocument entityDoc)
		{
			entityDoc = null;
			IDocument document = m_commands.OpenNewDocument(objType);
			if (document == null)
			{
				return new ResultCode("Failed to open new document of type \"{0}\"", objType);
			}
			entityDoc = document as IEntityDocument;
			if (entityDoc == null)
			{
				return new ResultCode("Failed to cast new document of type \"{0}\" into an IEntityDocument", objType);
			}
			return ResultCode.Success;
		}

		private ResultCode ImportEntitySource(string srcFileName, ObjectFieldValueAdapter objValAdapter, ImportedEntityAdapter entAdapter, IEntityDocument entityDoc, out string objName)
		{
			using (entAdapter.As<HistoryContext>()?.SuspendRecording())
			{
				TransactionContext transactionContext = entAdapter.As<TransactionContext>();
				if (transactionContext == null || transactionContext.InTransaction)
				{
					entAdapter.SourceFilePath = srcFileName;
				}
				else
				{
					transactionContext.DoTransaction(delegate
					{
						entAdapter.SourceFilePath = srcFileName;
					}, "Update entity source file.");
				}
			}
			if (entAdapter.GetCurrentSourceObjects().Length == 1)
			{
				return SaveNewEntityDocument(entityDoc, out objName, closeAfterSave: true);
			}
			string srcObj = string.Empty;
			IInstanceEntity entityCreated = null;
			ResultCode resultCode = AskUSerToSelectSourceObject(entityDoc, srcFileName, objValAdapter, out srcObj, out entityCreated);
			if (!resultCode)
			{
				objName = string.Empty;
				return resultCode;
			}
			objName = srcObj;
			return ResultCode.Success;
		}

		private ResultCode SaveNewEntityDocument(IEntityDocument entityDoc, out string objName, bool closeAfterSave)
		{
			if (!m_commands.Save(entityDoc))
			{
				if (m_commands.IsUntitled(entityDoc))
				{
					entityDoc.Dirty = false;
				}
				m_commands.Close(entityDoc);
				objName = string.Empty;
				return new ResultCode("Failed to save document!");
			}
			if (closeAfterSave)
			{
				m_commands.Close(entityDoc);
			}
			objName = entityDoc.InstanceEntity.Name;
			return ResultCode.Success;
		}

		private ResultCode AskUSerToSelectSourceObject(IEntityDocument entityDoc, string srcFileName, ObjectFieldValueAdapter objValAdapter, out string srcObj, out IInstanceEntity entityCreated)
		{
			IInstanceEntity instanceEntity = entityDoc.InstanceEntity;
			List<SourceFileModel> list = new List<SourceFileModel>();
			list.Add(new SourceFileModel(srcFileName, instanceEntity.Type));
			IEnumerable<IImportedEntity> source = DialogHelper.LaunchSourceClassAssociationView(m_civtechService, list, objValAdapter.ObjectParameter, (IObjectValue)objValAdapter.Value, instanceEntity.Type, global::DatabaseWrapper.DatabaseWrapper.ImportEntities, (IImportedEntity ent) => true);
			if (source.Any() && source.FirstOrDefault() != null)
			{
				srcObj = source.FirstOrDefault().Name;
				entityCreated = source.FirstOrDefault();
				return ResultCode.Success;
			}
			srcObj = string.Empty;
			entityCreated = null;
			return new UserCanceledResultCode("User failed to select source object!");
		}

		private ResultCode EnsureValidEntityNameAndClass(string entityName, InstanceType objType, string className, InstanceEntityAdapter entAdapter)
		{
			if (!entAdapter.IsNameChangeValid(entityName))
			{
				TextInputValidationForm textInputValidationForm = GetTextInputValidationForm(entityName, objType);
				if (textInputValidationForm.ShowDialog() != DialogResult.OK)
				{
					return new UserCanceledResultCode("User failed to select valid name for new entity");
				}
				entityName = StaticMethods.SanitizeEntityName(textInputValidationForm.UserText);
			}
			entAdapter.Name = entityName;
			entAdapter.ClassName = className;
			return ResultCode.Success;
		}

		private ResultCode AskUserForSourceFilePath(InstanceType objType, out string srcFileName)
		{
			srcFileName = string.Empty;
			IEnumerable<string> supportedSourceFileExtensions = ExporterService.GetSupportedSourceFileExtensions(objType);
			string filter = BuildFilter("Source Files", supportedSourceFileExtensions);
			m_fileBrowser.ForcedInitialDirectory = null;
			if (m_fileBrowser.OpenFileName(ref srcFileName, filter) != FileDialogResult.OK)
			{
				return new UserCanceledResultCode("User failed to select valid source file");
			}
			return ResultCode.Success;
		}

		private ResultCode UpdateParentDocumentReference(ObjectFieldValueAdapter objValAdapter, string objectName)
		{
			ResultCode resultCode = RestoreParentDocument();
			if (!resultCode)
			{
				return resultCode;
			}
			ResultCode resultCode2 = EnsureTransactionContextActive();
			if (!resultCode2)
			{
				return resultCode2;
			}
			SetValue(objectName);
			return ResultCode.Success;
		}

		private ResultCode EnsureTransactionContextActive()
		{
			DomNode dn = null;
			ResultCode contextDomNode = GetContextDomNode(out dn);
			if (!contextDomNode)
			{
				return contextDomNode;
			}
			ITransactionContext transactionContext = null;
			DomNode domNode = dn.Parent;
			while (transactionContext == null && domNode != null)
			{
				transactionContext = domNode.As<ITransactionContext>();
				domNode = domNode.Parent;
			}
			if (transactionContext == null)
			{
				return new ResultCode("Failed to get ITransactionContext from DomNode \"{0}\"", string.Join(".", dn.Lineage));
			}
			m_context.TransactionContext = transactionContext;
			return ResultCode.Success;
		}

		private TextInputValidationForm GetTextInputValidationForm(string name, InstanceType type)
		{
			string startingName = name;
			return new TextInputValidationForm(delegate(string text)
			{
				string text2 = StaticMethods.SanitizeEntityName(text);
				bool num = text2 != startingName;
				bool flag = global::DatabaseWrapper.DatabaseWrapper.IsEntityNameAvailable(m_civtechService.PrimaryProject.Name, type, text2);
				return num && flag;
			}, name)
			{
				FormTitle = "Entity Name Selector",
				InputLabel = "Choose a new name for your entity.",
				InvalidInputLabel = "An entity with that name already exists, choose another."
			};
		}

		private bool CheckValidState(string caller)
		{
			AssertValidState(caller);
			if (m_context == null)
			{
				return false;
			}
			if (m_context.Descriptor == null)
			{
				return false;
			}
			if (m_context.LastSelectedObject == null)
			{
				return false;
			}
			if (!(m_context.Descriptor is PropertyDescriptor propertyDescriptor))
			{
				return false;
			}
			DomNode node = propertyDescriptor.GetNode(m_context.LastSelectedObject);
			if (node == null)
			{
				return false;
			}
			return node.As<ObjectFieldValueAdapter>() != null;
		}

		private void AssertValidState(string caller)
		{
			BugSubmitter.SilentAssert(m_context != null, "Context is null.  Caller: '{0}'  @assign bwhitman @summary invalid_cookparam_editor_state", caller);
			BugSubmitter.SilentAssert(m_context.Descriptor != null, "Context's property descriptor is null.  Caller: '{0}'  @assign bwhitman @summary invalid_cookparam_editor_state", caller);
			BugSubmitter.SilentAssert(m_context.LastSelectedObject != null, "Context's does not have a Last Selected Object.  Caller: '{0}'  @assign bwhitman @summary invalid_cookparam_editor_state", caller);
			PropertyDescriptor propertyDescriptor = m_context.Descriptor as PropertyDescriptor;
			BugSubmitter.SilentAssert(propertyDescriptor != null, "Context's property descriptor is of the wrong type (it's a non-ATF type).  Caller: '{0}'  Descriptor: '{1}'  @assign bwhitman @summary invalid_cookparam_editor_state", caller, m_context.Descriptor.ToString());
			DomNode node = propertyDescriptor.GetNode(m_context.LastSelectedObject);
			bool condition = node != null;
			string fmtText = "Context's property descriptor can not get a DomNode from the last selected object.  Caller: '{0}'  LastSelectedObject: '{1}'  @assign bwhitman @summary invalid_cookparam_editor_state";
			object[] array = new object[2] { caller, null };
			int num = 1;
			array[num] = m_context.LastSelectedObject?.ToString();
			BugSubmitter.SilentAssert(condition, fmtText, array);
			BugSubmitter.SilentAssert(node.As<ObjectFieldValueAdapter>() != null, "The retrieved DomNode is not an ObjectFieldValueAdapter.  Caller: '{0}'  DomNode: '{1}'  @assign bwhitman @summary invalid_cookparam_editor_state", caller, node?.ToString());
		}

		private void HandleDocumentDirtyChanged(object sender, EventArgs e)
		{
			if (sender is IEntityDocument { Dirty: false } entityDocument)
			{
				m_valueDisplay.Text = entityDocument.InstanceEntity.Name;
			}
		}

		private void HandleClearButtonClick(object sender, EventArgs e)
		{
			SetValue("");
		}

		private string BuildFilter(string filterName, IEnumerable<string> supportedExtensions)
		{
			IEnumerable<string> enumerable2;
			if (supportedExtensions == null || !supportedExtensions.Any())
			{
				IEnumerable<string> enumerable = new List<string>(new string[1] { ".*" });
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = supportedExtensions;
			}
			IEnumerable<string> enumerable3 = enumerable2;
			StringBuilder stringBuilder = new StringBuilder(filterName + " (");
			foreach (string item in enumerable3)
			{
				stringBuilder.Append("*" + item + ";");
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			stringBuilder.Append(")|");
			foreach (string item2 in enumerable3)
			{
				stringBuilder.Append("*" + item2 + ";");
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			return stringBuilder.ToString();
		}
	}

	private readonly IAssetBrowserDialogService m_assetBrowser;

	private readonly IFileDialogService m_fileBrowser;

	private readonly AssetBrowserFileCommands m_commands;

	private readonly IImportService m_importService;

	private readonly ICivTechService m_civtechService;

	private readonly IEnumerable<IDocumentClient> m_documentClients;

	private readonly IDocumentRegistryMediator m_registryMediator;

	private readonly BatchEntitySourceControlService m_sourceControl;

	private ObjectCookParameterControl m_objectControl;

	private IAssetPreviewerService PreviewerService { get; }

	public ObjectCookParameterEditor(IAssetBrowserDialogService assetBrowser, IFileDialogService fileBrowser, AssetBrowserFileCommands commands, IImportService importService, ICivTechService civtechService, IEnumerable<IDocumentClient> documentClients, IDocumentRegistryMediator registryMediator, BatchEntitySourceControlService sourceControl, IAssetPreviewerService previewerService)
	{
		m_assetBrowser = assetBrowser;
		m_fileBrowser = fileBrowser;
		m_commands = commands;
		m_importService = importService;
		m_civtechService = civtechService;
		m_documentClients = documentClients;
		m_registryMediator = registryMediator;
		m_sourceControl = sourceControl;
		PreviewerService = previewerService;
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		m_objectControl = new ObjectCookParameterControl(context, m_assetBrowser, m_fileBrowser, m_commands, m_importService, m_civtechService, m_documentClients, m_registryMediator, m_sourceControl, PreviewerService);
		SkinService.ApplyActiveSkin(m_objectControl);
		return m_objectControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}

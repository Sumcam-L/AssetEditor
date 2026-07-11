using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class AttachmentPointSetAdapter : BehaviorComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	public enum Command
	{
		CmdAddAttachmentPoint,
		CmdRemoveAttachmentPoint,
		CmdAddAllAttachmentPoints,
		CmdBindAttachmentPointByName,
		CmdCutAttachmentPoints,
		CmdCopyAttachmentPoints,
		CmdPasteAttachmentPoints,
		CmdDuplicateSelected,
		CmdFixupBLPReferences
	}

	public struct AttachmentPointCommandTag
	{
		public readonly Command Command;

		private Action<AttachmentPointSetAdapter> m_action;

		public AttachmentPointCommandTag(Command command, Action<AttachmentPointSetAdapter> action)
		{
			Command = command;
			m_action = action;
		}

		public void DoCommand(AttachmentPointSetAdapter adapter)
		{
			m_action(adapter);
		}
	}

	[Serializable]
	internal class Vec3F
	{
		public float X;

		public float Y;

		public float Z;

		public Vec3F()
		{
			X = 0f;
			Y = 0f;
			Z = 0f;
		}

		public Vec3F(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vec3F(IFloatVector3 vector)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
		}
	}

	[Serializable]
	internal class AttachmentPointData
	{
		public string BoneName { get; set; }

		public string ModelInstanceName { get; set; }

		public string Name { get; set; }

		public string CookParams { get; set; }

		public Vec3F Orientation { get; set; }

		public Vec3F Position { get; set; }

		public float Scale { get; set; }
	}

	public static AttachmentPointCommandTag AddAttachmentPointCommandTag = new AttachmentPointCommandTag(Command.CmdAddAttachmentPoint, delegate(AttachmentPointSetAdapter adapter)
	{
		AddUndoTransaction("Add Attachment", adapter, delegate
		{
			string modelName = string.Empty;
			string boneName = string.Empty;
			_ = adapter.EntityAdapter.CivTechService.PrimaryProject.Paths.GamePantry;
			string text = adapter.BehaviorProvider.ReferenceGeometryNames.FirstOrDefault();
			if (!string.IsNullOrEmpty(text))
			{
				using IInstanceSet instanceSet = Context.Get<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { adapter.EntityAdapter.CivTechService.GetActivePantryPaths() });
				IGeometryInstance geometryInstance = instanceSet.LoadEntity<IGeometryInstance>(adapter.EntityAdapter.CivTechService.ProjectMapService.LayeredPantry, text);
				if (geometryInstance != null)
				{
					modelName = geometryInstance.ModelName;
					if (geometryInstance.BoneNames.Count() > 0)
					{
						boneName = geometryInstance.BoneNames.FirstOrDefault();
					}
				}
			}
			adapter.AddAttachmentPoint(GenerateUniqueName(adapter, "newAttachmentPoint_"), boneName, modelName);
		});
	});

	public static AttachmentPointCommandTag RemoveAttachmentPointCommandTag = new AttachmentPointCommandTag(Command.CmdRemoveAttachmentPoint, delegate(AttachmentPointSetAdapter adapter)
	{
		adapter.DomNode.GetRoot().As<AssetContext>();
		AddUndoTransaction("Remove Attachment", adapter, delegate
		{
			object[] selected = adapter.Selection.ToArray();
			if (selected.Length == 0) return;
			List<string> names = new List<string>(selected.Length);
			foreach (object obj in selected)
			{
				AttachmentPointAdapter ap = obj.As<AttachmentPointAdapter>();
				names.Add(ap.Name);
			}
			adapter.BulkRemoveAttachmentPoints(names);
		});
	});

	public static AttachmentPointCommandTag AddAllAttachmentPointsCommandTag = new AttachmentPointCommandTag(Command.CmdAddAllAttachmentPoints, delegate(AttachmentPointSetAdapter adapter)
	{
		AddUndoTransaction("Add all Attachments", adapter, delegate
		{
			AddAllAttachmentPoints(adapter);
		});
	});

	public static AttachmentPointCommandTag BindAttachmentPointByNameCommandTag = new AttachmentPointCommandTag(Command.CmdBindAttachmentPointByName, delegate(AttachmentPointSetAdapter adapter)
	{
		AddUndoTransaction("Bind Attachments to all Attachment Points by name", adapter, delegate
		{
			AutomaticallyFillOutBLPEntries(adapter);
		});
	});

	public static AttachmentPointCommandTag CutAttachmentPointsCommandTag = new AttachmentPointCommandTag(Command.CmdCutAttachmentPoints, delegate(AttachmentPointSetAdapter adapter)
	{
		AddUndoTransaction("Cut Attachments".Localize(), adapter, delegate
		{
			adapter.CopyInternal();
			adapter.DeleteInternal();
		});
	});

	public static AttachmentPointCommandTag CopyAttachmentPointsCommandTag = new AttachmentPointCommandTag(Command.CmdCopyAttachmentPoints, delegate(AttachmentPointSetAdapter adapter)
	{
		adapter.CopyInternal();
	});

	public static AttachmentPointCommandTag PasteAttachmentPointsCommandTag = new AttachmentPointCommandTag(Command.CmdPasteAttachmentPoints, delegate(AttachmentPointSetAdapter adapter)
	{
		AddUndoTransaction("Paste Attachments".Localize(), adapter, delegate
		{
			adapter.PasteInternal();
			IInstanceEntity instanceEntity = adapter.BehaviorProvider.InstanceEntity;
			adapter.BatchChangelist?.CreateEntityChangedEvent(instanceEntity.Type, instanceEntity.Name);
		});
	});

	public static AttachmentPointCommandTag FixupBLPReferencesCommandTag = new AttachmentPointCommandTag(Command.CmdFixupBLPReferences, delegate(AttachmentPointSetAdapter adapter)
	{
		AddUndoTransaction("Fixup BLP References".Localize(), adapter, delegate
		{
			adapter.FixupBLPReferences();
		});
	});

	public static AttachmentPointCommandTag DuplicatedSelectedAttachmentsCommandTag = new AttachmentPointCommandTag(Command.CmdDuplicateSelected, delegate(AttachmentPointSetAdapter adapter)
	{
		adapter.CopyInternal();
		IEnumerable<AttachmentPointAdapter> copiedAttachments = Enumerable.Empty<AttachmentPointAdapter>();
		AddUndoTransaction("Duplicate Attachments".Localize(), adapter, delegate
		{
			copiedAttachments = adapter.PasteInternal();
			IInstanceEntity instanceEntity = adapter.BehaviorProvider.InstanceEntity;
			adapter.BatchChangelist?.CreateEntityChangedEvent(instanceEntity.Type, instanceEntity.Name);
			if (copiedAttachments.Any())
			{
				adapter.SelectionContext.SetRange(copiedAttachments);
			}
		});
	});

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	private bool m_initialUpdate = true;

	private static readonly string kCopyPasteName = "AttachmentPoint.Json";

	private static uint m_attachmentClipboardFormat = NativeMethods.RegisterClipboardFormat(kCopyPasteName);

	private static char[] _badChars = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

	private ISelectionContext SelectionContext { get; set; }

	public IList<AttachmentPointAdapter> AttachmentPoints { get; private set; }

	public ICommandClient CommandClient => this;

	public IEnumerable<CommandInfo> Commands => m_commands;

	public string DefaultSortPropertyName
	{
		get
		{
			System.ComponentModel.PropertyDescriptor propertyDescriptor = PropertyDescriptors.FirstOrDefault();
			if (propertyDescriptor == null)
			{
				return string.Empty;
			}
			return propertyDescriptor.Name;
		}
	}

	public ListSortDirection DefaultListSortDirection => ListSortDirection.Ascending;

	public IEnumerable<object> Selection
	{
		get
		{
			return SelectionContext.Selection;
		}
		set
		{
			SelectionContext.Selection = value;
		}
	}

	public object LastSelected => SelectionContext.LastSelected;

	public int SelectionCount => SelectionContext.SelectionCount;

	public IEnumerable<object> Items => AttachmentPoints;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (System.ComponentModel.PropertyDescriptor item in EntitySchema.AttachmentPointType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return item;
			}
			AttachmentPointAdapter attachmentPointAdapter = AttachmentPoints.FirstOrDefault((AttachmentPointAdapter ap) => ap.CookParameterSet != null && ap.CookParameterSet.Fields.Any());
			if (attachmentPointAdapter == null)
			{
				yield break;
			}
			foreach (IFieldValueAdapter field in attachmentPointAdapter.CookParameterSet.Fields)
			{
				foreach (FieldPropertyDescriptorBase item2 in field.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
				{
					yield return item2;
				}
			}
		}
	}

	public bool InTransaction => base.DomNode.GetRoot().As<ITransactionContext>().InTransaction;

	public int PendingOperationCount => base.DomNode.GetRoot().As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public override object GetAdapter(Type type)
	{
		if (typeof(IValidationContext).IsAssignableFrom(type))
		{
			return base.DomNode.GetRoot().As<IValidationContext>();
		}
		return base.GetAdapter(type);
	}

	public static string GenerateUniqueName(AttachmentPointSetAdapter attPntSetAdapter, string baseName)
	{
		int num = 0;
		string uniqueName;
		do
		{
			uniqueName = $"{baseName}{++num}";
		}
		while (attPntSetAdapter.AttachmentPoints.Any((AttachmentPointAdapter ap) => ap.Name == uniqueName));
		return uniqueName;
	}

	public static void AddUndoTransaction(string name, AttachmentPointSetAdapter adapter, Action action)
	{
		adapter.DomNode.GetRoot().As<TransactionContext>().DoTransaction(action, name.Localize());
	}

	public AttachmentPointSetAdapter()
	{
		m_commands.Add(new CommandInfo(AddAttachmentPointCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Add".Localize("Name of a command"), "Adds an attachment point to the attachment point set.".Localize(), Sce.Atf.Input.Keys.None, Resources.AddItemIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(RemoveAttachmentPointCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Remove".Localize("Name of a command"), "Removes an attachment point from the attachment point set.".Localize(), Sce.Atf.Input.Keys.None, Resources.RemoveItemIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(AddAllAttachmentPointsCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Add Attachment Points to all Bones".Localize("Name of a command"), "Add an attachment point to every single bone found int the geometries assigned to the asset.".Localize(), Sce.Atf.Input.Keys.None, Sce.Atf.Resources.FactoryImage, CommandVisibility.All));
		m_commands.Add(new CommandInfo(BindAttachmentPointByNameCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Bind Attachments to all Attachment Points by name".Localize("Name of a command"), ".".Localize(), Sce.Atf.Input.Keys.None, Resources.BindAttachments, CommandVisibility.All));
		m_commands.Add(new CommandInfo(CopyAttachmentPointsCommandTag, StandardMenu.Edit, StandardCommandGroup.EditCut, "Copy Attachments".Localize("Name of a command"), "Copy selected Attachments to clipboard".Localize("Name of a command"), Sce.Atf.Input.Keys.None, Resources.CopyIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(CutAttachmentPointsCommandTag, StandardMenu.Edit, StandardCommandGroup.EditCut, "Cut Attachments".Localize("Name of a command"), "Cut selected Attachments to clipboard".Localize("Name of a command"), Sce.Atf.Input.Keys.None, Resources.CutIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(PasteAttachmentPointsCommandTag, StandardMenu.Edit, StandardCommandGroup.EditCut, "Paste Attachments".Localize("Name of a command"), "Paste selected Attachments from clipboard".Localize("Name of a command"), Sce.Atf.Input.Keys.None, Resources.PasteIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(DuplicatedSelectedAttachmentsCommandTag, StandardMenu.Edit, StandardCommandGroup.EditCut, "Duplicate Attachments".Localize("Name of a command"), "Duplicate selected Attachments".Localize("Name of a command"), Sce.Atf.Input.Keys.None, Resources.PausePlaybackTimelineIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(FixupBLPReferencesCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Fixup BLP References".Localize("Name of a command"), "Attempts to automatically find matches for missing BLP references.".Localize(), Sce.Atf.Input.Keys.None, Resources.FixLinksIcon, CommandVisibility.All));
		_ = this.ItemChanged;
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		this.SelectionChanged.Raise(sender, e);
	}

	private void SelectionContext_SelectionChanging(object sender, EventArgs e)
	{
		this.SelectionChanging.Raise(sender, e);
	}

	public static void AddAllAttachmentPoints(AttachmentPointSetAdapter adapter)
	{
		IEnumerable<string> referenceGeometryNames = adapter.BehaviorProvider.ReferenceGeometryNames;
		using IInstanceSet tempSet = Context.Get<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { adapter.EntityAdapter.CivTechService.GetActivePantryPaths() });
		IEnumerable<IGeometryInstance> referencedGeometry = LoadReferencedGeometry(adapter.EntityAdapter.CivTechService, referenceGeometryNames, tempSet);
		List<string> invalidNames = BuildInvalidAttachmentPointList(adapter, referencedGeometry).ToList();
		if (invalidNames.Count > 0)
		{
			adapter.BulkRemoveAttachmentPoints(invalidNames);
		}
		AddAttachmentPointsForEachUniqueBone(adapter, referencedGeometry);
	}

	private static IEnumerable<IGeometryInstance> LoadReferencedGeometry(ICivTechService civTechSvc, IEnumerable<string> geoNames, IInstanceSet tempSet)
	{
		List<IGeometryInstance> list = new List<IGeometryInstance>();
		foreach (string geoName in geoNames)
		{
			IGeometryInstance geometryInstance = tempSet.LoadEntityIfUnique<IGeometryInstance>(geoName);
			if (geometryInstance != null)
			{
				list.Add(geometryInstance);
				continue;
			}
			Outputs.WriteLine(OutputMessageType.Error, "Unable to load the referenced geometry with the name {0}.  Is this the right geometry?", geoName);
		}
		return list;
	}

	private static IEnumerable<string> BuildInvalidAttachmentPointList(AttachmentPointSetAdapter attachmentPoints, IEnumerable<IGeometryInstance> referencedGeometry)
	{
		List<string> list = new List<string>();
		foreach (AttachmentPointAdapter attachmentPoint in attachmentPoints.AttachmentPoints)
		{
			IGeometryInstance geometryInstance = referencedGeometry.FirstOrDefault((IGeometryInstance geo) => geo.ModelName == attachmentPoint.ModelInstanceName);
			if (geometryInstance == null || !geometryInstance.BoneNames.Contains(attachmentPoint.BoneName))
			{
				list.Add(attachmentPoint.Name);
			}
		}
		return list;
	}

	public static void AddAttachmentPointsForEachUniqueBone(AttachmentPointSetAdapter attachmentPoints, IEnumerable<IGeometryInstance> referencedGeometry)
	{
		foreach (IGeometryInstance item in referencedGeometry)
		{
			ISet<string> set = new HashSet<string>(item.GeometryMeshes.Select((IGeoMesh mesh) => mesh.Name));
			foreach (string boneName in item.BoneNames)
			{
				if (attachmentPoints.AttachmentPoints.FirstOrDefault((AttachmentPointAdapter attachmentPoint) => attachmentPoint.Name == boneName) == null && boneName != item.ModelName && !set.Contains(boneName))
				{
					attachmentPoints.AddAttachmentPoint(boneName, boneName, item.ModelName);
				}
			}
		}
	}

	public static void AutomaticallyFillOutBLPEntries(AttachmentPointSetAdapter adapter)
	{
		foreach (AttachmentPointAdapter attachmentPoint in adapter.AttachmentPoints)
		{
			foreach (object item in attachmentPoint.CookParameterSet.Items)
			{
				CookParameterSetAdapter cookParameterSetAdapter = item.As<CookParameterSetAdapter>();
				if (cookParameterSetAdapter == null)
				{
					continue;
				}
				foreach (IFieldValueAdapter cookParameter in cookParameterSetAdapter.CookParameters)
				{
					BLPEntryFieldValueAdapter bLPEntryFieldValueAdapter = cookParameter.As<BLPEntryFieldValueAdapter>();
					if (bLPEntryFieldValueAdapter == null)
					{
						continue;
					}
					string text = bLPEntryFieldValueAdapter.Name.ToLower();
					if (text.Contains("asset") || text.Contains("attached"))
					{
						string text2 = attachmentPoint.Name.TrimEnd(_badChars);
						IXLPCacheData iXLPCacheData = CivTechRegistry.XLPRegistry.FindXLPData(text2);
						if (iXLPCacheData != null)
						{
							bLPEntryFieldValueAdapter.EntryName = text2;
							bLPEntryFieldValueAdapter.XLPPath = iXLPCacheData.XLPPath;
							bLPEntryFieldValueAdapter.PackagePath = iXLPCacheData.BLPPackage;
						}
					}
				}
			}
		}
	}

	public AttachmentPointAdapter AddAttachmentPoint(string name, string boneName, string modelName)
	{
		IClassEntity cls = global::DatabaseWrapper.DatabaseWrapper.GetClass(base.EntityAdapter.CivTechService.PrimaryProject.Name, base.EntityAdapter.InstanceType, base.EntityAdapter.InstanceEntity.ClassName);
		return AddAttachmentPointInternal(name, boneName, modelName, cls);
	}

	private AttachmentPointAdapter AddAttachmentPointInternal(string name, string boneName, string modelName, IClassEntity cls)
	{
		DomNode domNode = new DomNode(EntitySchema.AttachmentPointType.Type);
		domNode.InitializeExtensions();
		AttachmentPointAdapter attachmentPointAdapter = domNode.As<AttachmentPointAdapter>();
		attachmentPointAdapter.Name = name;
		attachmentPointAdapter.BoneName = boneName;
		attachmentPointAdapter.ModelInstanceName = modelName;
		attachmentPointAdapter.EntityAdapter = base.EntityAdapter;
		AttachmentPoints.Add(attachmentPointAdapter);
		if (cls != null)
		{
			attachmentPointAdapter.InitializeCookParameters(cls);
		}
		return attachmentPointAdapter;
	}

	public void RemoveAttachmentPoint(string name)
	{
		AttachmentPointAdapter attachmentPointAdapter = null;
		foreach (AttachmentPointAdapter ata in AttachmentPoints)
		{
			if (ata.Name == name)
			{
				attachmentPointAdapter = ata;
				break;
			}
		}
		if (attachmentPointAdapter != null)
		{
			SelectionContext.Remove(attachmentPointAdapter);
			attachmentPointAdapter.RemoveCookParameters();
			AttachmentPoints.Remove(attachmentPointAdapter);
		}
	}

	private void BulkRemoveAttachmentPoints(List<string> names)
	{
		HashSet<string> nameSet = new HashSet<string>(names);

		SelectionContext.SetRange(Enumerable.Empty<object>());

		UnregisterHandlers();
		try
		{
			for (int i = AttachmentPoints.Count - 1; i >= 0; i--)
			{
				AttachmentPointAdapter ap = AttachmentPoints[i];
				if (nameSet.Contains(ap.Name))
				{
					ap.RemoveCookParameters();
					AttachmentPoints.RemoveAt(i);
				}
			}
		}
		finally
		{
			RegisterHandlers();
		}

		base.BehaviorProvider.BehaviorData.AttachmentPointSet.RemoveAttachmentPoints(names);
		ResyncAttachmentPoints();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		AttachmentPoints = new DomNodeListAdapter<AttachmentPointAdapter>(base.DomNode, EntitySchema.AttachmentPointSetType.AttachmentPointChild);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		SelectionContext = base.DomNode.GetRoot().As<ISelectionContext>();
		SelectionContext.SelectionChanging += SelectionContext_SelectionChanging;
		SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		OnItemChanged(e.DomNode);
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (m_initialUpdate)
		{
			return;
		}
		if (e.ChildInfo == EntitySchema.AttachmentPointSetType.AttachmentPointChild)
		{
			AttachmentPointAdapter attachmentPointAdapter = e.Child.As<AttachmentPointAdapter>();
			attachmentPointAdapter.AttachmentPoint = base.BehaviorProvider.BehaviorData.AttachmentPointSet.AddAttachmentPoint(attachmentPointAdapter.Name, attachmentPointAdapter.BoneName, attachmentPointAdapter.ModelInstanceName);
			ResyncAttachmentPoints();
			IInstanceEntity instanceEntity = base.BehaviorProvider.InstanceEntity;
			base.BatchChangelist?.CreateAttachmentChangedEvent(instanceEntity, string.Empty, attachmentPointAdapter.Name, attachmentPointAdapter.ModelInstanceName, attachmentPointAdapter.BoneName, new float[3], new float[3], 1f);
		}
		OnItemInserted(e.Index, e.Child);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (m_initialUpdate)
		{
			return;
		}
		if (e.ChildInfo == EntitySchema.AttachmentPointSetType.AttachmentPointChild)
		{
			AttachmentPointAdapter attachmentPointAdapter = e.Child.As<AttachmentPointAdapter>();
			base.BehaviorProvider.BehaviorData.AttachmentPointSet.RemoveAttachmentPoint(attachmentPointAdapter.Name);
			ResyncAttachmentPoints();
			base.BatchChangelist?.CreateAttachmentRemovedEvent(base.BehaviorProvider.InstanceEntity, attachmentPointAdapter.Name);
		}
		OnItemRemoved(e.Index, e.Child);
	}

	private void ResyncAttachmentPoints()
	{
		foreach (AttachmentPointAdapter attachmentPoint2 in AttachmentPoints)
		{
			IAttachmentPoint attachmentPoint = base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(attachmentPoint2.Name);
			BugSubmitter.SilentAssert(attachmentPoint != null, "Hmm");
			if (attachmentPoint != null)
			{
				attachmentPoint2.AttachmentPoint = attachmentPoint;
			}
		}
		OnReloaded();
	}

	private AttachmentPointAdapter CreateAttachmentPointAdapter()
	{
		DomNode domNode = new DomNode(EntitySchema.AttachmentPointType.Type);
		domNode.InitializeExtensions();
		return domNode.As<AttachmentPointAdapter>();
	}

	private void UpdateAttachmentPointAdapter(AttachmentPointAdapter adapter, IAttachmentPoint point)
	{
		adapter.EntityAdapter = base.EntityAdapter;
		adapter.Name = point.Name;
		adapter.ModelInstanceName = point.ModelInstanceName;
		adapter.BoneName = adapter.BoneName;
		adapter.Position = new float[3]
		{
			point.Position.X,
			point.Position.Y,
			point.Position.Z
		};
		adapter.Orientation = new float[3]
		{
			point.Orientation.X,
			point.Orientation.Y,
			point.Orientation.Z
		};
		adapter.Scale = point.Scale;
	}

	public void Update()
	{
		UnregisterHandlers();
		bool attachmentPointSetChanged = false;
		IList<AttachmentPointAdapter> list = new List<AttachmentPointAdapter>();
		_ = base.BehaviorProvider.InstanceEntity;
		foreach (IAttachmentPoint point in base.BehaviorProvider.BehaviorData.AttachmentPointSet.Items)
		{
			AttachmentPointAdapter attachmentPointAdapter = AttachmentPoints.FirstOrDefault((AttachmentPointAdapter uap) => uap.Name == point.Name);
			if (attachmentPointAdapter == null)
			{
				attachmentPointAdapter = CreateAttachmentPointAdapter();
				UpdateAttachmentPointAdapter(attachmentPointAdapter, point);
				AttachmentPoints.Add(attachmentPointAdapter);
				attachmentPointSetChanged = true;
			}
			attachmentPointAdapter.Update(point);
			list.Add(attachmentPointAdapter);
		}
		foreach (var entryAdapter in AttachmentPoints.Except(list).ToArray())
		{
			AttachmentPoints.Remove(entryAdapter);
			attachmentPointSetChanged = true;
		}
		if (attachmentPointSetChanged)
		{
			OnReloaded();
		}
		m_initialUpdate = false;
		RegisterHandlers();
	}

	private void UnregisterHandlers()
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	private void RegisterHandlers()
	{
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return SelectionContext.GetSelection<T>();
	}

	public T GetLastSelected<T>() where T : class
	{
		return SelectionContext.GetLastSelected<T>();
	}

	public bool SelectionContains(object item)
	{
		return SelectionContext.SelectionContains(item);
	}

	public bool CanDoCommand(object commandTag)
	{
		if (base.BehaviorProvider.InstanceEntity == null)
		{
			return false;
		}
		if (base.EntityAdapter.As<IEntityDocument>().IsReadOnly)
		{
			return false;
		}
		AttachmentPointCommandTag attachmentPointCommandTag = (AttachmentPointCommandTag)commandTag;
		if (attachmentPointCommandTag.Command == Command.CmdAddAttachmentPoint)
		{
			return base.BehaviorProvider.ReferenceGeometryNames.Any();
		}
		if (attachmentPointCommandTag.Command == Command.CmdRemoveAttachmentPoint || attachmentPointCommandTag.Command == Command.CmdCutAttachmentPoints || attachmentPointCommandTag.Command == Command.CmdCopyAttachmentPoints)
		{
			return Selection.Any();
		}
		if (attachmentPointCommandTag.Command == Command.CmdAddAllAttachmentPoints || attachmentPointCommandTag.Command == Command.CmdBindAttachmentPointByName)
		{
			return base.BehaviorProvider.ReferenceGeometryNames.Any();
		}
		if (attachmentPointCommandTag.Command != Command.CmdPasteAttachmentPoints)
		{
			return (attachmentPointCommandTag.Command == Command.CmdFixupBLPReferences) ? (AttachmentPoints.Count > 0) : (attachmentPointCommandTag.Command == Command.CmdDuplicateSelected && Selection.Any());
		}
		try
		{
			return NativeMethods.IsClipboardFormatAvailable(m_attachmentClipboardFormat);
		}
		catch (SystemException)
		{
		}
		return false;
	}

	public void DoCommand(object commandTag)
	{
		((AttachmentPointCommandTag)commandTag).DoCommand(this);
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(index, item));
	}

	protected virtual void OnItemChanged(object item)
	{
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(item));
	}

	protected virtual void OnReloaded()
	{
		this.Reloaded.Raise(this, EventArgs.Empty);
	}

	public void Begin(string transactionName)
	{
		GetParentAs<ITransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.GetRoot().As<ITransactionContext>().Cancel();
	}

	public void End()
	{
		base.DomNode.GetRoot().As<ITransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.GetRoot().As<ITransactionContext>().SuspendTransactions();
	}

	private AttachmentPointAdapter AddAttachmentPointFromData(AttachmentPointData ap)
	{
		IClassEntity cls = global::DatabaseWrapper.DatabaseWrapper.GetClass(base.EntityAdapter.CivTechService.PrimaryProject.Name, base.EntityAdapter.InstanceType, base.EntityAdapter.InstanceEntity.ClassName);
		AttachmentPointAdapter attachmentPointAdapter = AddAttachmentPointInternal(EnsureUniqueName(ap.Name), ap.BoneName, ap.ModelInstanceName, cls);
		attachmentPointAdapter.Position = new float[3]
		{
			ap.Position.X,
			ap.Position.Y,
			ap.Position.Z
		};
		attachmentPointAdapter.Orientation = new float[3]
		{
			ap.Orientation.X,
			ap.Orientation.Y,
			ap.Orientation.Z
		};
		attachmentPointAdapter.Scale = ap.Scale;
		if (attachmentPointAdapter.AttachmentPoint != null && attachmentPointAdapter.AttachmentPoint.CookParameters != null)
		{
			attachmentPointAdapter.AttachmentPoint.CookParameters.UpdateFromXML(ap.CookParams);
			attachmentPointAdapter.Update(attachmentPointAdapter.AttachmentPoint);
		}
		return attachmentPointAdapter;
	}

	private string GenerateNumberedName(string name)
	{
		int result = 1;
		string arg = name;
		int num = name.LastIndexOf("_");
		if (num != -1)
		{
			if (int.TryParse(name.Substring(num + 1), out result))
			{
				result++;
				arg = name.Substring(0, num);
			}
			else
			{
				result = 1;
			}
		}
		return $"{arg}_{result}";
	}

	private string EnsureUniqueName(string name)
	{
		do
		{
			name = GenerateNumberedName(name);
		}
		while (base.BehaviorProvider.BehaviorData.AttachmentPointSet.FindByName(name) != null);
		return name;
	}

	private void DeleteInternal()
	{
		if (!Selection.Any())
		{
			return;
		}
		List<string> names = new List<string>();
		foreach (object obj in Selection)
		{
			names.Add(obj.As<AttachmentPointAdapter>().Name);
		}
		BulkRemoveAttachmentPoints(names);
	}

	private void CopyInternal()
	{
		IList<AttachmentPointData> list = new List<AttachmentPointData>();
		foreach (object item2 in Selection)
		{
			IAttachmentPoint attachmentPoint = item2.As<AttachmentPointAdapter>().AttachmentPoint;
			AttachmentPointData item = new AttachmentPointData
			{
				BoneName = attachmentPoint.BoneName,
				ModelInstanceName = attachmentPoint.ModelInstanceName,
				Name = attachmentPoint.Name,
				CookParams = attachmentPoint.CookParameters.SerializeIntoXML(),
				Orientation = new Vec3F(attachmentPoint.Orientation),
				Position = new Vec3F(attachmentPoint.Position),
				Scale = attachmentPoint.Scale
			};
			list.Add(item);
		}
		string s = new JavaScriptSerializer
		{
			MaxJsonLength = 20971520
		}.Serialize(list);
		if (NativeMethods.OpenClipboard(Application.OpenForms.OfType<Form>().FirstOrDefault((Form fod) => fod.Visible).Handle))
		{
			NativeMethods.EmptyClipboard();
			IntPtr hMem = Marshal.StringToHGlobalAnsi(s);
			NativeMethods.SetClipboardData(m_attachmentClipboardFormat, hMem);
			NativeMethods.CloseClipboard();
		}
	}

	private IEnumerable<AttachmentPointAdapter> PasteInternal()
	{
		if (!NativeMethods.OpenClipboard(Application.OpenForms.OfType<Form>().FirstOrDefault((Form fod) => fod.Visible).Handle))
		{
			return Enumerable.Empty<AttachmentPointAdapter>();
		}
		string input = Marshal.PtrToStringAnsi(NativeMethods.GetClipboardData(m_attachmentClipboardFormat));
		NativeMethods.CloseClipboard();
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 20971520;
		IList<AttachmentPointAdapter> aplist = new List<AttachmentPointAdapter>();
		IList<AttachmentPointData> attList = javaScriptSerializer.Deserialize<IList<AttachmentPointData>>(input);
		if (attList != null)
		{
			AddUndoTransaction("Paste".Localize(), this, delegate
			{
				IEnumerable<string> referenceGeometryNames = base.BehaviorProvider.ReferenceGeometryNames;
				HashSet<string> hashSet = new HashSet<string>();
				HashSet<string> hashSet2 = new HashSet<string>();
				_ = base.EntityAdapter.CivTechService.PrimaryProject.Paths.GamePantry;
				using (IInstanceSet tempSet = Context.Get<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() }))
				{
					foreach (IGeometryInstance item in LoadReferencedGeometry(base.EntityAdapter.CivTechService, referenceGeometryNames, tempSet))
					{
						hashSet.Add(item.ModelName);
					}
				}
				foreach (AttachmentPointData item2 in attList)
				{
					if (!hashSet.Contains(item2.ModelInstanceName))
					{
						Outputs.WriteLine(OutputMessageType.Error, "Attachment point " + item2.Name + " references non-existent model " + item2.ModelInstanceName + ". Please Fix this attachment point by setting it to reference model instance that does exist in the asset");
						hashSet2.Add(item2.ModelInstanceName);
						item2.ModelInstanceName = string.Empty;
					}
					aplist.Add(AddAttachmentPointFromData(item2));
				}
				if (hashSet2.Count > 0)
				{
					string text = string.Join("\n", hashSet2.ToArray());
					MessageBoxes.Show("One or more of the attachment points are referencing Model Instances that don't exist in this asset: \n\n" + text + "\n\nPlease Fix those attachment points by setting them to reference model instances that do exist in the asset");
				}
			});
		}
		return aplist;
	}

	private void FixupBLPReferences()
	{
		IXLPRegistry xLPRegistry = CivTechRegistry.XLPRegistry;
		Action<string> outputDelegate = delegate(string message)
		{
			Outputs.WriteLine(OutputMessageType.Info, message);
		};
		foreach (AttachmentPointAdapter attachmentPoint in AttachmentPoints)
		{
			foreach (IValue item in attachmentPoint.AttachmentPoint.CookParameters.Items)
			{
				FixupBLPReference(item, xLPRegistry, outputDelegate);
			}
		}
	}

	private void FixupBLPReference(IValue value, IXLPRegistry xlpRegistry, Action<string> outputDelegate)
	{
		switch (value.ParameterType)
		{
		case Firaxis.CivTech.AssetObjects.ValueType.VT_BLP_ENTRY:
		{
			IBLPEntryValue blpEntryValue2 = (IBLPEntryValue)value;
			xlpRegistry.FixupBLPReference(blpEntryValue2, outputDelegate);
			break;
		}
		case Firaxis.CivTech.AssetObjects.ValueType.VT_COLLECTION:
		{
			ICollectionValue collectionValue = (ICollectionValue)value;
			if (collectionValue.EntryValueType != Firaxis.CivTech.AssetObjects.ValueType.VT_BLP_ENTRY)
			{
				break;
			}
			{
				foreach (IBLPEntryValue item in collectionValue.Items)
				{
					xlpRegistry.FixupBLPReference(item, outputDelegate);
				}
				break;
			}
		}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class SplineSetAdapter : AssetComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum Command
	{
		AddSpline,
		RemoveSpline
	}

	private struct SplineCommandTag
	{
		public readonly Command Command;

		private Action<SplineSetAdapter> m_action;

		public SplineCommandTag(Command command, Action<SplineSetAdapter> action)
		{
			Command = command;
			m_action = action;
		}

		public void DoCommand(SplineSetAdapter adapter)
		{
			m_action(adapter);
		}
	}

	private static SplineCommandTag AddSplineCommandTag = new SplineCommandTag(Command.AddSpline, delegate(SplineSetAdapter adapter)
	{
		AddUndoTransaction("Add Spline", adapter, delegate
		{
			_ = adapter.AssetAdapter.CivTechService.PrimaryProject.Paths.GamePantry;
			string clsName = adapter.AssetAdapter.AssetClass.AllowedSplineClasses.FirstOrDefault();
			adapter.AddSpline(clsName, GenerateUniqueName(adapter, "newSpline_"));
		});
	});

	private static SplineCommandTag RemoveSplineCommandTag = new SplineCommandTag(Command.RemoveSpline, delegate(SplineSetAdapter adapter)
	{
		adapter.DomNode.GetRoot().As<AssetContext>();
		AddUndoTransaction("Remove Spline", adapter, delegate
		{
			object[] array = adapter.Selection.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				SplineAdapter splineAdapter = array[i].As<SplineAdapter>();
				adapter.RemoveSpline(splineAdapter.Name);
			}
		});
	});

	private ISplineSet _splineSet;

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	public ISplineSet SplineSet
	{
		get
		{
			if (_splineSet == null)
			{
				_splineSet = base.AssetAdapter.Asset.SplineSet;
			}
			return _splineSet;
		}
	}

	private ISelectionContext SelectionContext { get; set; }

	public IList<SplineAdapter> Splines { get; private set; }

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

	public ICommandClient CommandClient => this;

	public IEnumerable<CommandInfo> Commands => m_commands;

	public IEnumerable<object> Items => Splines;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (System.ComponentModel.PropertyDescriptor item in EntitySchema.SplineType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return item;
			}
			SplineAdapter splineAdapter = Splines.FirstOrDefault((SplineAdapter s) => s.CookParameterSet != null && s.CookParameterSet.Fields.Any());
			if (splineAdapter == null)
			{
				yield break;
			}
			foreach (IFieldValueAdapter field in splineAdapter.CookParameterSet.Fields)
			{
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in field.PropertyDescriptors)
				{
					yield return propertyDescriptor;
				}
			}
		}
	}

	public bool InTransaction => base.DomNode.GetRoot().As<ITransactionContext>()?.InTransaction ?? false;

	public int PendingOperationCount => base.DomNode.GetRoot().As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public static string GenerateUniqueName(SplineSetAdapter splineSetAdapter, string baseName)
	{
		int num = 0;
		string uniqueName;
		do
		{
			num++;
			uniqueName = $"{baseName}{num}";
		}
		while (splineSetAdapter.Splines.Any((SplineAdapter s) => s.Name == uniqueName));
		return uniqueName;
	}

	public static void AddUndoTransaction(string name, SplineSetAdapter adapter, Action action)
	{
		adapter.DomNode.GetRoot().As<TransactionContext>().DoTransaction(action, name.Localize());
	}

	private void RegisterCommands()
	{
		m_commands.Add(new CommandInfo(AddSplineCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Add".Localize("Name of a command"), "Adds a spline to the spline set.".Localize(), Keys.None, Resources.AddItemIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(RemoveSplineCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Remove".Localize("Name of a command"), "Removes a spline from the spline set.".Localize(), Keys.None, Resources.RemoveItemIcon, CommandVisibility.All));
	}

	public SplineSetAdapter()
	{
		RegisterCommands();
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

	public SplineAdapter AddSpline(string clsName, string name)
	{
		SplineAdapter splineAdapter = CreateSplineAdapter();
		splineAdapter.ClassName = clsName;
		Splines.Add(splineAdapter);
		splineAdapter.Name = name;
		if (base.AssetAdapter.CivTechService.PrimaryProject.Config.Classes.Items.FirstOrDefault((IClassEntity spl) => spl.Name == clsName) is ISplineClass config)
		{
			splineAdapter.InitializeCookParameters(config);
		}
		return splineAdapter;
	}

	public void RemoveSpline(string name)
	{
		SplineAdapter splineAdapter = Splines.FirstOrDefault((SplineAdapter spl) => spl.Name == name);
		if (splineAdapter != null)
		{
			splineAdapter.RemoveCookParameters();
			Splines.Remove(splineAdapter);
		}
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		Splines = new DomNodeListAdapter<SplineAdapter>(base.DomNode, EntitySchema.SplineSetType.SplineChild);
		RegisterForDomNotifications();
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
		if (e.ChildInfo == EntitySchema.SplineSetType.SplineChild)
		{
			SplineAdapter splineAdapter = e.Child.As<SplineAdapter>();
			splineAdapter.Spline = SplineSet.AddSpline(splineAdapter.ClassName);
			splineAdapter.Spline.Name = splineAdapter.Name;
		}
		OnItemInserted(e.Index, e.Child);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.SplineSetType.SplineChild)
		{
			OnItemRemoved(e.Index, e.Child);
			SplineAdapter splineAdapter = e.Child.As<SplineAdapter>();
			SplineSet.RemoveSpline(splineAdapter.Spline);
		}
	}

	private SplineAdapter CreateSplineAdapter()
	{
		DomNode domNode = new DomNode(EntitySchema.SplineType.Type);
		domNode.InitializeExtensions();
		return domNode.As<SplineAdapter>();
	}

	public void InitializeFromNative(ISplineSet splineSet)
	{
		UnregisterForDomNotifications();
		IList<SplineAdapter> list = new List<SplineAdapter>();
		foreach (ISpline spline in SplineSet.Splines)
		{
			SplineAdapter splineAdapter = Splines.FirstOrDefault((SplineAdapter spl) => spl.Name == spline.Name);
			if (splineAdapter == null)
			{
				splineAdapter = CreateSplineAdapter();
				Splines.Add(splineAdapter);
			}
			splineAdapter.InitializeFromNative(spline);
			list.Add(splineAdapter);
		}
		foreach (var spl in Splines.Except(list).ToArray())
		{
			Splines.Remove(spl);
		}
		RegisterForDomNotifications();
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
		if (base.ParentAsset == null || string.IsNullOrEmpty(base.AssetAdapter.Asset.ClassName))
		{
			return false;
		}
		if (base.AssetAdapter.As<IEntityDocument>().IsReadOnly)
		{
			return false;
		}
		SplineCommandTag splineCommandTag = (SplineCommandTag)commandTag;
		if (splineCommandTag.Command == Command.AddSpline)
		{
			return true;
		}
		if (splineCommandTag.Command == Command.RemoveSpline)
		{
			return Selection.Any();
		}
		return false;
	}

	public void DoCommand(object commandTag)
	{
		((SplineCommandTag)commandTag).DoCommand(this);
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(index, item));
	}

	protected virtual void OnItemChanged(object item)
	{
		this.ItemChanged?.Invoke(this, new ItemChangedEventArgs<object>(item));
	}

	protected virtual void OnReloaded()
	{
		this.Reloaded?.Invoke(this, EventArgs.Empty);
	}

	public void Begin(string transactionName)
	{
		GetParentAs<ITransactionContext>()?.Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.GetRoot().As<ITransactionContext>()?.Cancel();
	}

	public void End()
	{
		base.DomNode.GetRoot().As<ITransactionContext>()?.End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.GetRoot().As<ITransactionContext>()?.SuspendTransactions();
	}
}

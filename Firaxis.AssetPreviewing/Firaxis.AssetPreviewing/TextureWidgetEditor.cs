using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

public class TextureWidgetEditor : IWidgetEditor, IDisposable
{
	public class TextureLocatorWidget
	{
		private readonly CookParameterSetAdapter cookParameterSet;

		private readonly IPreviewWindow previewWindow;

		private readonly CivTechContext civContext;

		private readonly TextureEntityAdapter textureAdapter;

		private readonly TextureEntityDocument entityDocument;

		public IFloatVector2 SliceCoord1 { get; set; }

		public IFloatVector2 SliceCoord2 { get; set; }

		public IWidget Widget { get; }

		private TransactionContext TransactionContext => entityDocument.DomNode.GetRoot().As<TransactionContext>();

		public TextureLocatorWidget(IPreviewWindow previewWindow, CivTechContext civContext, IEntityDocument targetDocument, bool isReadOnly)
		{
			this.civContext = civContext;
			this.previewWindow = previewWindow;
			SliceCoord1 = civContext.CreateInstance<IFloatVector2>();
			SliceCoord2 = civContext.CreateInstance<IFloatVector2>();
			entityDocument = (TextureEntityDocument)targetDocument;
			textureAdapter = (TextureEntityAdapter)entityDocument.EntityAdapter;
			cookParameterSet = textureAdapter.CookParameterSet;
			IValueSet valueSet = civContext.CreateInstance<IValueSet>();
			valueSet.Push<IRGBValue>("Color").ParameterValue = COLOR_SELECTED;
			valueSet.Push<IStringValue>("ClassName").ParameterValue = entityDocument.InstanceEntity.ClassName;
			valueSet.Push<IBoolValue>("IsReadOnly").ParameterValue = isReadOnly;
			Widget = previewWindow.CreateWidget("2DLocator", valueSet, this);
			Widget.OnStartEdit += OnStartEdit;
			Widget.OnEdit += OnEdit;
			Widget.OnFinishEdit += OnFinishEdit;
			Widget.OnCancelEdit += OnCancelEdit;
		}

		private void OnStartEdit(object sender, EventArgs e)
		{
			TransactionContext.Begin("Update Slice Coord");
		}

		private void OnEdit(object sender, EventArgs e)
		{
			TransactionContext transactionContext = TransactionContext;
			BugSubmitter.Assert(transactionContext.InTransaction, "TextureLocator_OnEdit triggered while not in a transaction!");
			using IEntityChangeList changeList = civContext.CreateInstance<IEntityChangeList>();
			Coord2DFieldValueAdapter coord2DFieldValueAdapter = cookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter f) => f.Name == "SliceCoord1" && f is Coord2DFieldValueAdapter).Cast<Coord2DFieldValueAdapter>();
			Coord2DFieldValueAdapter coord2DFieldValueAdapter2 = cookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter f) => f.Name == "SliceCoord2" && f is Coord2DFieldValueAdapter).Cast<Coord2DFieldValueAdapter>();
			changeList.CreateEntityCookParameterChangedEvent(textureAdapter.InstanceType, textureAdapter.Name, "SliceCoord1", coord2DFieldValueAdapter.Value);
			changeList.CreateEntityCookParameterChangedEvent(textureAdapter.InstanceType, textureAdapter.Name, "SliceCoord2", coord2DFieldValueAdapter2.Value);
		}

		private void OnFinishEdit(object sender, EventArgs e)
		{
			TransactionContext transactionContext = TransactionContext;
			Coord2DFieldValueAdapter coord2DFieldValueAdapter = cookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter f) => f.Name == "SliceCoord1" && f is Coord2DFieldValueAdapter).Cast<Coord2DFieldValueAdapter>();
			if (coord2DFieldValueAdapter != null)
			{
				coord2DFieldValueAdapter.ParameterValue = new float[2] { SliceCoord1.X, SliceCoord1.Y };
			}
			Coord2DFieldValueAdapter coord2DFieldValueAdapter2 = cookParameterSet.Fields.FirstOrDefault((IFieldValueAdapter f) => f.Name == "SliceCoord2" && f is Coord2DFieldValueAdapter).Cast<Coord2DFieldValueAdapter>();
			if (coord2DFieldValueAdapter2 != null)
			{
				coord2DFieldValueAdapter2.ParameterValue = new float[2] { SliceCoord2.X, SliceCoord2.Y };
			}
			transactionContext.End();
		}

		private void OnCancelEdit(object sender, EventArgs e)
		{
			TransactionContext.Cancel();
		}

		public void Dispose()
		{
			if (Widget != null)
			{
				Widget.CancelPendingEdits();
				Widget.OnStartEdit -= OnStartEdit;
				Widget.OnEdit -= OnEdit;
				Widget.OnFinishEdit -= OnFinishEdit;
				Widget.OnCancelEdit -= OnCancelEdit;
				Widget.Dispose();
			}
		}
	}

	public const string WidgetName = "Texture";

	public static readonly Color COLOR_SELECTED = Color.FromArgb(255, 0, 196, 196);

	private readonly List<IWidgetState> states = new List<IWidgetState>();

	private readonly CivTechContext civContext;

	private TextureLocatorWidget activeWidget;

	private IPreviewWindow previewWindow = null;

	private IEntityDocument targetDocument = null;

	private bool disposed = false;

	private bool isReadOnly = true;

	public string Name => "Texture";

	public IEnumerable<IWidgetState> States => states;

	public event EventHandler SelectionChanged;

	public TextureWidgetEditor()
	{
		civContext = Context.EnsureCreated<CivTechContext>();
	}

	public void SetTarget(IPreviewWindow window, IEntityDocument target)
	{
		if (previewWindow == null || targetDocument == null || !previewWindow.Equals(window) || !targetDocument.Equals(target))
		{
			previewWindow = window;
			targetDocument = target;
			UpdateWidgetEditor();
		}
	}

	private void UpdateWidgetEditor()
	{
	}

	private void SetSelectedAttachment(bool attach)
	{
		activeWidget?.Dispose();
		activeWidget = null;
		if (attach && previewWindow != null)
		{
			activeWidget = new TextureLocatorWidget(previewWindow, civContext, targetDocument, isReadOnly);
			this.SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void Activate(bool isReadOnly)
	{
		this.isReadOnly = isReadOnly;
		SetSelectedAttachment(attach: true);
	}

	public void Deactivate()
	{
		SetSelectedAttachment(attach: false);
	}

	public void OnKeyDown(KeyEventArgs evt)
	{
	}

	public void OnKeyUp(KeyEventArgs evt)
	{
	}

	public void OnMouseMove(MouseEventArgs evt)
	{
	}

	public void OnMouseDown(MouseEventArgs evt)
	{
	}

	public void OnMouseUp(MouseEventArgs evt)
	{
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			activeWidget?.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}

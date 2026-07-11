using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefCollectionKeyFrameContext : KeyFrameEditingContextBase, IAdaptable, IDisposable
{
	private ArtDefCollectionAdapter m_adapter;

	public override float InitialTime => GetTimeParameter()?.Default ?? 0f;

	public override IEnumerable<IKeyFrame> KeyFrames
	{
		get
		{
			foreach (IKeyFrame item in from e in m_adapter.Elements
				where e.Is<IKeyFrame>()
				select e.As<IKeyFrame>())
			{
				yield return item;
			}
		}
	}

	public override float MaxTime => GetTimeParameter()?.Max ?? 3600f;

	public override float MinTime => GetTimeParameter()?.Min ?? 0f;

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			if (!base.SelectedIndices.Any((int idx) => idx >= 0 && idx < m_adapter.Elements.Count))
			{
				yield break;
			}
			int index = base.SelectedIndices.First((int idx) => idx >= 0 && idx < m_adapter.Elements.Count);
			ArtDefElementAdapter artDefElementAdapter = m_adapter.Elements[index];
			foreach (FieldValueAdapter field in artDefElementAdapter.Fields)
			{
				foreach (FieldPropertyDescriptorBase item in field.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
				{
					yield return item;
				}
			}
		}
	}

	public override IEnumerable<object> SelectedObjects
	{
		get
		{
			IList<IKeyFrame> keyFrames = KeyFrames.ToList();
			IEnumerable<int> enumerable = base.SelectedIndices.Where((int idx) => idx >= 0 && idx < keyFrames.Count).ToList();
			foreach (int item in enumerable)
			{
				ArtDefElementKeyFrame artDefElementKeyFrame = keyFrames[item] as ArtDefElementKeyFrame;
				yield return artDefElementKeyFrame.ArtDefElementAdapter;
			}
		}
	}

	public override event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public override event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public override event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public override event EventHandler Reloaded;

	public ArtDefCollectionKeyFrameContext(ArtDefCollectionAdapter adapter)
	{
		m_adapter = adapter;
		RegisterInitialEvents();
		if (Reloaded == null)
		{
			_ = ItemChanged;
		}
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(typeof(ITransactionContext)))
		{
			return m_adapter;
		}
		return null;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			RemoveEventHandlers();
		}
	}

	private void AddTimeChangedEventHandler(DomNode node)
	{
		if (node.As<ArtDefElementAdapter>().As<IKeyFrame>() is ArtDefElementKeyFrame artDefElementKeyFrame)
		{
			artDefElementKeyFrame.TimeChanged += ArtDefCollectionKeyFrameContext_TimeChanged;
			artDefElementKeyFrame.ValueChanged += ArtDefCollectionKeyFrame_ValueChanged;
		}
	}

	private void ArtDefCollectionKeyFrame_ValueChanged(object sender, DomNode dn)
	{
		ItemChanged?.Invoke(sender, new ItemChangedEventArgs<object>(dn));
	}

	private void ArtDefCollectionKeyFrameContext_TimeChanged(object sender, EventArgs e)
	{
		ItemChanged?.Invoke(sender, new ItemChangedEventArgs<object>(this));
	}

	private IFloatParameter GetTimeParameter()
	{
		return m_adapter.ArtDefElementsTemplate.Parameters.FindByName("Time") as IFloatParameter;
	}

	private void OnKeyFrameAdded(object sender, ItemInsertedEventArgs<object> e)
	{
		ItemInserted.Raise(sender, e);
		AddTimeChangedEventHandler(e.Item.As<DomNode>());
	}

	private void OnKeyFrameChanged(object sender, ItemChangedEventArgs<object> e)
	{
		ItemChanged.Raise(sender, e);
	}

	private void OnKeyFrameRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		RemoveTimeChangedEventHandler(e.Item.As<DomNode>());
		ItemRemoved.Raise(sender, e);
	}

	private void RegisterInitialEvents()
	{
		m_adapter.ItemInserted += OnKeyFrameAdded;
		m_adapter.ItemRemoved += OnKeyFrameRemoved;
		m_adapter.ItemChanged += OnKeyFrameChanged;
		foreach (ArtDefElementAdapter element in m_adapter.Elements)
		{
			if (element.As<IKeyFrame>() is ArtDefElementKeyFrame artDefElementKeyFrame)
			{
				artDefElementKeyFrame.TimeChanged += ArtDefCollectionKeyFrameContext_TimeChanged;
				artDefElementKeyFrame.ValueChanged += ArtDefCollectionKeyFrame_ValueChanged;
			}
		}
	}

	private void RemoveEventHandlers()
	{
		m_adapter.ItemInserted -= OnKeyFrameAdded;
		m_adapter.ItemRemoved -= OnKeyFrameRemoved;
		m_adapter.ItemChanged -= OnKeyFrameChanged;
		foreach (ArtDefElementAdapter element in m_adapter.Elements)
		{
			if (element.As<IKeyFrame>() is ArtDefElementKeyFrame artDefElementKeyFrame)
			{
				artDefElementKeyFrame.TimeChanged -= ArtDefCollectionKeyFrameContext_TimeChanged;
				artDefElementKeyFrame.ValueChanged -= ArtDefCollectionKeyFrame_ValueChanged;
			}
		}
	}

	private void RemoveTimeChangedEventHandler(DomNode node)
	{
		if (node.As<ArtDefElementAdapter>().As<IKeyFrame>() is ArtDefElementKeyFrame artDefElementKeyFrame)
		{
			artDefElementKeyFrame.TimeChanged -= ArtDefCollectionKeyFrameContext_TimeChanged;
			artDefElementKeyFrame.ValueChanged -= ArtDefCollectionKeyFrame_ValueChanged;
		}
	}
}

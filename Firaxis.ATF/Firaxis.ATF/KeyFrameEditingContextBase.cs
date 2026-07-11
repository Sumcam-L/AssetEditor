using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public abstract class KeyFrameEditingContextBase : IATFEditingContext, IPropertyEditingContext, IIndexSelectionContext, IObservableContext
{
	private ICollection<int> m_selectedKeyFrames = new List<int>();

	public abstract float InitialTime { get; }

	public abstract IEnumerable<IKeyFrame> KeyFrames { get; }

	public abstract float MaxTime { get; }

	public abstract float MinTime { get; }

	public IEnumerable<object> Items
	{
		get
		{
			if (!SelectedIndices.Any())
			{
				yield break;
			}
			IList<IKeyFrame> keyFrames = new List<IKeyFrame>(KeyFrames);
			foreach (int selectedIndex in SelectedIndices)
			{
				if (selectedIndex >= 0 && selectedIndex < keyFrames.Count)
				{
					yield return keyFrames[selectedIndex];
				}
			}
		}
	}

	public abstract IEnumerable<PropertyDescriptor> PropertyDescriptors { get; }

	public IEnumerable<int> SelectedIndices
	{
		get
		{
			return m_selectedKeyFrames;
		}
		set
		{
			m_selectedKeyFrames = new List<int>(value);
		}
	}

	public abstract IEnumerable<object> SelectedObjects { get; }

	public abstract event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public abstract event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public abstract event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public abstract event EventHandler Reloaded;
}

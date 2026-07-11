using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AnimationBindingAdapter : AnimatableComponentAdapterBase, IAssetBrowserTypeProvider, IPropertyEditingContext, IObservableContext, ITimelineBindingAdapter
{
	public TimelineBindingType BindingType => TimelineBindingType.Animation;

	public string AnimationName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AnimationBindingType.AnimationNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AnimationBindingType.AnimationNameAttribute, value);
		}
	}

	public string SlotName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.AnimationBindingType.SlotNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.AnimationBindingType.SlotNameAttribute, value);
		}
	}

	public IEnumerable<object> Items
	{
		get
		{
			yield return base.DomNode;
		}
	}

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	public IEnumerable<string> ValidClassNames
	{
		get
		{
			if (base.DomNode.GetRoot().As<BaseEntityPropertyContext>().CivTechService.PrimaryProject.Config.Classes.FindForInstance(base.AnimatableEntityAdapter.InstanceEntity) is IAnimatableClass animatableClass)
			{
				return animatableClass.AllowedAnimationClasses;
			}
			return null;
		}
	}

	public IEnumerable<InstanceType> ValidTypes => new InstanceType[1] { InstanceType.IT_ANIMATION };

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public void ClearBinding()
	{
		AnimationName = string.Empty;
	}

	public AnimationBindingAdapter()
	{
		if (this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	public void Update(string slotName, string animationName)
	{
		UnregisterFromDomChanges();
		SlotName = slotName;
		AnimationName = animationName;
		RegisterForDomChanges();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RegisterForDomChanges();
	}

	private void HandleAnimationBindingChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.AnimationBindingType.AnimationNameAttribute)
		{
			string text = (string)e.NewValue;
			if (!string.IsNullOrEmpty(text))
			{
				base.AnimatableEntityAdapter.AnimationData.AnimationBindings.Bind(SlotName, text);
			}
			else
			{
				base.AnimatableEntityAdapter.AnimationData.AnimationBindings.Unbind(SlotName);
			}
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += HandleAnimationBindingChanged;
	}

	private void UnregisterFromDomChanges()
	{
		base.DomNode.AttributeChanged -= HandleAnimationBindingChanged;
	}
}

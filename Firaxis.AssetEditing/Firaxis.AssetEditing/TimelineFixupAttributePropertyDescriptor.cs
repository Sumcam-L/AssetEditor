using System;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TimelineFixupAttributePropertyDescriptor : AttributePropertyDescriptor
{
	public TimelineFixupAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly)
		: this(name, attribute, category, description, isReadOnly, null, null)
	{
	}

	public TimelineFixupAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor)
		: this(name, attribute, category, description, isReadOnly, editor, null)
	{
	}

	public TimelineFixupAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, attribute, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public TimelineFixupAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, Attribute[] attributes)
		: base(name, attribute, category, description, isReadOnly, editor, typeConverter, attributes)
	{
	}

	public override void SetValue(object component, object value)
	{
		DomNode node = GetNode(component);
		if (node != null)
		{
			node.SetAttribute(base.AttributeInfo, value);
			IBehaviorProviderAdapter behaviorProviderAdapter = node.GetRoot().As<IBehaviorProviderAdapter>();
			AnimationBindingAdapter animationBindingAdapter = node.As<AnimationBindingAdapter>();
			TimelineAdapter timelineAdapter = behaviorProviderAdapter.TimelineSet.FindTimeline(animationBindingAdapter.SlotName);
			if (timelineAdapter != null)
			{
				timelineAdapter.AnimationName = animationBindingAdapter.AnimationName;
			}
		}
	}
}

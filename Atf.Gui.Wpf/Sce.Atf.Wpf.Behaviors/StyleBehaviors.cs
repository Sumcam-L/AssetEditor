using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class StyleBehaviors
{
	private static readonly DependencyProperty BehaviorIdProperty = DependencyProperty.RegisterAttached("BehaviorId", typeof(Guid), typeof(StyleBehaviors), new UIPropertyMetadata(Guid.Empty));

	public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors", typeof(StyleBehaviorCollection), typeof(StyleBehaviors), new FrameworkPropertyMetadata(null, OnPropertyChanged));

	public static StyleBehaviorCollection GetBehaviors(DependencyObject uie)
	{
		return (StyleBehaviorCollection)uie.GetValue(BehaviorsProperty);
	}

	public static void SetBehaviors(DependencyObject uie, StyleBehaviorCollection value)
	{
		uie.SetValue(BehaviorsProperty, value);
	}

	private static Guid GetBehaviorId(DependencyObject obj)
	{
		return (Guid)obj.GetValue(BehaviorIdProperty);
	}

	private static int GetIndexOf(BehaviorCollection itemBehaviors, Behavior behavior)
	{
		int result = -1;
		Guid behaviorId = GetBehaviorId(behavior);
		for (int i = 0; i < itemBehaviors.Count; i++)
		{
			Behavior behavior2 = itemBehaviors[i];
			if (behavior2 == behavior)
			{
				result = i;
				break;
			}
			Guid behaviorId2 = GetBehaviorId(behavior2);
			if (behaviorId2 == behaviorId)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private static void OnPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
	{
		if (!(dpo is UIElement obj))
		{
			return;
		}
		BehaviorCollection behaviors = Interaction.GetBehaviors(obj);
		StyleBehaviorCollection styleBehaviorCollection = e.NewValue as StyleBehaviorCollection;
		StyleBehaviorCollection styleBehaviorCollection2 = e.OldValue as StyleBehaviorCollection;
		if (styleBehaviorCollection == styleBehaviorCollection2)
		{
			return;
		}
		behaviors.Clear();
		if (styleBehaviorCollection == null)
		{
			return;
		}
		foreach (Behavior item in styleBehaviorCollection)
		{
			int indexOf = GetIndexOf(behaviors, item);
			if (indexOf < 0)
			{
				Behavior behavior = (Behavior)item.Clone();
				Guid behaviorId = GetBehaviorId(behavior);
				if (behaviorId == Guid.Empty)
				{
					SetBehaviorId(behavior, Guid.NewGuid());
				}
				behaviors.Add(behavior);
			}
		}
	}

	private static void SetBehaviorId(DependencyObject obj, Guid value)
	{
		obj.SetValue(BehaviorIdProperty, value);
	}
}

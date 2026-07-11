using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class TriggerInstancingHelper
{
	public static bool CanCopy(Selection<object> selection)
	{
		return selection?.Any((object sel) => sel.Is<TriggerAdapter>()) ?? false;
	}

	public static object Copy(Selection<object> selection)
	{
		string result = string.Empty;
		if (selection == null)
		{
			return result;
		}
		IList<SerializedTrigger> triggers = new List<SerializedTrigger>();
		selection.ForEach(delegate(object sel)
		{
			TriggerAdapter triggerAdapter = sel.As<TriggerAdapter>();
			if (triggerAdapter?.Trigger != null)
			{
				triggers.Add(new SerializedTrigger(triggerAdapter.Trigger));
			}
		});
		try
		{
			result = new JavaScriptSerializer().Serialize(triggers);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		return result;
	}

	public static bool CanInsert(Selection<object> selection, object dataObject)
	{
		IEnumerable<SerializedTrigger> enumerable = DeserializeDataObject(dataObject);
		if (!enumerable.Any())
		{
			return false;
		}
		TriggerType trigType = TriggerType.TT_COUNT;
		if (!GetTriggerTypeIfHomogenious(enumerable, out trigType))
		{
			return false;
		}
		if (InsertionSelectionIsTimeline(selection))
		{
			return true;
		}
		if (InsertionSelectionIsTrack(selection) || InsertionSelectionIsTrigger(selection))
		{
			TrackAdapter trackFromSelection = GetTrackFromSelection(selection);
			if (trackFromSelection != null)
			{
				return trackFromSelection.TriggerType == trigType;
			}
			return false;
		}
		return false;
	}

	public static void Insert(Selection<object> selection, object dataObject)
	{
		IEnumerable<SerializedTrigger> enumerable = DeserializeDataObject(dataObject);
		if (enumerable.Any())
		{
			InsertWithOffset(selection, 0f, enumerable);
		}
	}

	public static void InsertAtTime(Selection<object> selection, float time, object dataObject)
	{
		IEnumerable<SerializedTrigger> enumerable = DeserializeDataObject(dataObject);
		if (enumerable.Any())
		{
			float offset = DetermineTriggerOffset(time, enumerable);
			InsertWithOffset(selection, offset, enumerable);
		}
	}

	public static bool CanDelete(Selection<object> selection)
	{
		if (selection == null)
		{
			return false;
		}
		if (selection.LastSelected.Is<ITimelineBindingAdapter>() || selection.LastSelected.Is<TrackAdapter>())
		{
			return true;
		}
		if (!selection.Any())
		{
			return false;
		}
		return selection.All((object sel) => sel.Is<TriggerAdapter>());
	}

	public static void Delete(Selection<object> selection)
	{
		if (selection.LastSelected.Is<ITimelineBindingAdapter>())
		{
			ITimelineBindingAdapter timelineBindingAdapter = selection.LastSelected.As<ITimelineBindingAdapter>();
			IBehaviorProviderAdapter behaviorProviderAdapter = selection.LastSelected.As<DomNode>().GetRoot().As<IBehaviorProviderAdapter>();
			TimelineAdapter timelineAdapter = behaviorProviderAdapter.TimelineSet.FindTimeline(timelineBindingAdapter.SlotName);
			int num = timelineAdapter.Triggers.Count();
			if (num > 0 && MessageBoxes.Show($"Timeline {timelineAdapter.Name} has {num} triggers. Are you sure you want to delete this timeline and its triggers?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
			{
				throw new InvalidTransactionException("Canceled delete at user's request", reportError: false);
			}
			behaviorProviderAdapter.TimelineBindingSet.TimelineBindings.FirstOrDefault((TimelineBindingAdapter tlBinding) => tlBinding.SlotName == timelineAdapter.Name)?.ClearBinding();
			if (timelineBindingAdapter.BindingType == TimelineBindingType.Animation && !string.IsNullOrEmpty(timelineAdapter.AnimationName))
			{
				string animationName = timelineAdapter.AnimationName;
				timelineAdapter.AnimationName = string.Empty;
				if (MessageBoxes.Show("Timeline \"" + timelineAdapter.Name + "\" is bound to animation \"" + animationName + "\". Would you like to remove the animation binding as well?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					timelineBindingAdapter.ClearBinding();
				}
			}
			behaviorProviderAdapter.TimelineSet.RemoveTimeline(timelineBindingAdapter.SlotName);
		}
		else if (selection.LastSelected.Is<TrackAdapter>())
		{
			TrackAdapter trackAdapter = selection.LastSelected.As<TrackAdapter>();
			TimelineAdapter timeline = trackAdapter.Timeline;
			int num2 = trackAdapter.Triggers.Count();
			if (num2 > 0 && MessageBoxes.Show($"Track {trackAdapter.Name} in timeline {timeline.Name} has {num2} triggers. Are you sure you want to delete this track and its triggers?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
			{
				throw new InvalidTransactionException("Canceled delete at user's request", reportError: false);
			}
			timeline.RemoveTrack(trackAdapter);
		}
		else if (selection.Any() && selection.All((object sel) => sel.Is<TriggerAdapter>()))
		{
			TriggerAdapter[] array = (from sel in selection
				where sel.Is<TriggerAdapter>()
				select sel.As<TriggerAdapter>()).ToArray();
			foreach (TriggerAdapter triggerAdapter in array)
			{
				triggerAdapter.DomNode.Parent.As<TimelineAdapter>().RemoveTrigger(triggerAdapter);
			}
		}
	}

	private static float DetermineTriggerOffset(float time, IEnumerable<SerializedTrigger> triggers)
	{
		float num = triggers.Min((SerializedTrigger trig) => trig.StartTime);
		return time - num;
	}

	private static void InsertWithOffset(Selection<object> selection, float offset, IEnumerable<SerializedTrigger> triggers)
	{
		TrackAdapter trackAdapter = null;
		if (InsertionSelectionIsTimeline(selection))
		{
			TriggerType trigType = TriggerType.TT_COUNT;
			if (GetTriggerTypeIfHomogenious(triggers, out trigType))
			{
				trackAdapter = GetNewTrackForSelectedBinding(selection, trigType);
			}
		}
		else if (InsertionSelectionIsTrack(selection) || InsertionSelectionIsTrigger(selection))
		{
			trackAdapter = GetTrackFromSelection(selection);
		}
		if (trackAdapter == null)
		{
			return;
		}
		foreach (SerializedTrigger trigger in triggers)
		{
			trackAdapter?.AddTriggerCopyWithOffset(offset, trigger);
		}
	}

	private static bool GetTriggerTypeIfHomogenious(IEnumerable<SerializedTrigger> triggers, out TriggerType trigType)
	{
		trigType = TriggerType.TT_COUNT;
		foreach (SerializedTrigger trigger in triggers)
		{
			if (trigger.Type != trigType && trigType != TriggerType.TT_COUNT)
			{
				return false;
			}
			trigType = trigger.Type;
		}
		return trigType != TriggerType.TT_COUNT;
	}

	private static TrackAdapter GetNewTrackForSelectedBinding(Selection<object> selection, TriggerType trigType)
	{
		if (selection == null || selection.Count() != 1)
		{
			return null;
		}
		DomNode domNode = selection.ElementAt(0).As<DomNode>();
		IBehaviorProviderAdapter behaviorProviderAdapter = domNode?.GetRoot()?.As<IBehaviorProviderAdapter>();
		BugSubmitter.SilentAssert(behaviorProviderAdapter != null, "Selection could not provide IBehaviorProviderAdapter interface!");
		if (behaviorProviderAdapter == null)
		{
			return null;
		}
		ITimelineBindingAdapter timelineBindingAdapter = domNode.As<ITimelineBindingAdapter>();
		if (timelineBindingAdapter == null)
		{
			return null;
		}
		return behaviorProviderAdapter.TimelineSet.FindTimeline(timelineBindingAdapter.SlotName).AddTrack(trigType);
	}

	private static TrackAdapter GetTrackFromSelection(Selection<object> selection)
	{
		if (selection == null || !selection.Any())
		{
			return null;
		}
		TrackAdapter trackAdapter = selection.ElementAt(0).As<TrackAdapter>();
		if (trackAdapter == null)
		{
			trackAdapter = selection.ElementAt(0).As<TriggerAdapter>()?.TrackAdapter;
		}
		return trackAdapter;
	}

	private static bool InsertionSelectionIsTimeline(Selection<object> selection)
	{
		if (selection == null || selection.Count() != 1)
		{
			return false;
		}
		object reference = selection.ElementAt(0);
		return reference.Is<TimelineBindingAdapter>() || reference.Is<AnimationBindingAdapter>();
	}

	private static bool InsertionSelectionIsTrack(Selection<object> selection)
	{
		if (selection != null && selection.Count() == 1)
		{
			return selection.ElementAt(0).Is<TrackAdapter>();
		}
		return false;
	}

	private static bool InsertionSelectionIsTrigger(Selection<object> selection)
	{
		if (selection != null && selection.Any())
		{
			return selection.ElementAt(0).Is<TriggerAdapter>();
		}
		return false;
	}

	private static IEnumerable<SerializedTrigger> DeserializeDataObject(object dataObject)
	{
		if (!(dataObject is IDataObject dataObject2))
		{
			return Enumerable.Empty<SerializedTrigger>();
		}
		string text = dataObject2.GetData(typeof(string)) as string;
		if (string.IsNullOrEmpty(text))
		{
			return Enumerable.Empty<SerializedTrigger>();
		}
		return DeserializeCopyJson(text);
	}

	private static IEnumerable<SerializedTrigger> DeserializeCopyJson(string copyJson)
	{
		IList<SerializedTrigger> result = new List<SerializedTrigger>();
		try
		{
			result = new JavaScriptSerializer().Deserialize<IList<SerializedTrigger>>(copyJson);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		return result;
	}

	private static string SerializeCopyJson(IList<SerializedTrigger> triggers)
	{
		try
		{
			return new JavaScriptSerializer().Serialize(triggers);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		return string.Empty;
	}
}

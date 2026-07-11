using System;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels.KnobViewModels;

public class KnobViewModel : Notifier, IDisposable
{
	public string GroupName => Knob.GroupName;

	public string KnobName => Knob.Name;

	public string Label => Knob.Label;

	public string SubgroupName => Knob.SubgroupName;

	public string ToolTip => string.IsNullOrEmpty(Knob.ToolTip) ? null : Knob.ToolTip;

	protected IKnob Knob { get; private set; }

	public KnobViewModel(IKnob knob)
	{
		if (Knob != null)
		{
			Knob.HasUpdateEvent -= HandleKnobUpdate;
		}
		Knob = knob;
		Knob.HasUpdateEvent += HandleKnobUpdate;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && Knob != null)
		{
			Knob.HasUpdateEvent -= HandleKnobUpdate;
			Knob = null;
		}
	}

	protected virtual void HandleKnobUpdate(object sender, EventArgs e)
	{
		RaisePropertyChangedEvents();
	}

	protected virtual void RaisePropertyChangedEvents()
	{
		OnPropertyChanged("Label");
		OnPropertyChanged("KnobName");
		OnPropertyChanged("GroupName");
		OnPropertyChanged("ToolTip");
	}
}

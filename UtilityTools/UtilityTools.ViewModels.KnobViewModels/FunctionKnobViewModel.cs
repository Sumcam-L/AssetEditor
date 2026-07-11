using System.Windows.Input;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels.KnobViewModels;

public class FunctionKnobViewModel : KnobViewModel
{
	private DelegateCommand m_executeFunctionCommand;

	private IFunctionKnob FunctionKnob => (IFunctionKnob)base.Knob;

	public ICommand ExecuteFunctionCommand
	{
		get
		{
			if (m_executeFunctionCommand == null)
			{
				m_executeFunctionCommand = new DelegateCommand(ExecuteFunction);
			}
			return m_executeFunctionCommand;
		}
	}

	public FunctionKnobViewModel(IFunctionKnob knob)
		: base(knob)
	{
	}

	private void ExecuteFunction(object context)
	{
		FunctionKnob.CallFunction();
	}
}

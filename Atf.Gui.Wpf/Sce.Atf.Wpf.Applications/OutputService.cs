using System;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(OutputService))]
[Export(typeof(IOutputWriter))]
[Export(typeof(IControlHostClient))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OutputService : IOutputWriter, IControlHostClient, IInitializable, ICommandClient
{
	private OutputVm m_viewModel;

	private OutputView m_view;

	private Dispatcher m_uiDispatcher;

	private static Guid kId = new Guid(3570505697u, 49033, 20352, 151, 113, 91, 121, 50, 99, 78, 56);

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	[Import]
	protected IControlHostService ControlHostService { get; set; }

	public void Initialize()
	{
		m_view = new OutputView();
		m_viewModel = new OutputVm();
		m_view.DataContext = m_viewModel;
		ControlHostService.RegisterControl(m_view, "Output".Localize(), "View errors, warnings, and informative messages".Localize(), StandardControlGroup.Bottom, kId.ToString(), this);
		m_uiDispatcher = Dispatcher.CurrentDispatcher;
	}

	public void Write(OutputMessageType type, string message)
	{
		m_uiDispatcher.BeginInvokeIfRequired(delegate
		{
			m_viewModel.OutputItems.Add(new OutputItemVm(DateTime.Now, type, message));
		});
	}

	public void Clear()
	{
		m_uiDispatcher.BeginInvokeIfRequired(delegate
		{
			m_viewModel.OutputItems.Clear();
		});
	}

	public void Activate(object control)
	{
		if (m_commandService != null)
		{
			m_commandService.SetActiveClient(this);
		}
	}

	public void Deactivate(object control)
	{
		if (m_commandService != null)
		{
			m_commandService.SetActiveClient(null);
		}
	}

	public bool Close(object control, bool mainWindowClosing)
	{
		return true;
	}

	public bool CanDoCommand(object commandObj)
	{
		ICommandItem obj = commandObj as ICommandItem;
		Requires.NotNull(obj, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
		throw new NotImplementedException();
	}

	public void DoCommand(object commandObj)
	{
		ICommandItem obj = commandObj as ICommandItem;
		Requires.NotNull(obj, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
		throw new NotImplementedException();
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		throw new NotImplementedException();
	}
}

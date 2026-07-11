using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(CrashHandlerService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CrashHandlerService : IInitializable
{
	[Import(AllowDefault = true, AllowRecomposition = true)]
	private Form m_mainForm;

	[Import(AllowDefault = true, AllowRecomposition = true)]
	private INativeCrashHandlerService m_nativeCrashHandlerService;

	private bool m_automationEnabled;

	private ICrashSubmissionService CrashSubmissionService { get; set; }

	[ImportingConstructor]
	public CrashHandlerService(ICrashSubmissionService crashSubmissionSvc)
	{
		SetupAutomationFlag();
		CrashSubmissionService = crashSubmissionSvc;
	}

	public virtual void Initialize()
	{
		Application.ThreadException += Application_ThreadException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
	}

	[DebuggerNonUserCode]
	private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
	{
		m_nativeCrashHandlerService?.EnableCollection(bEnabled: false);
		HandleExceptionImpl(e.Exception);
	}

	[DebuggerNonUserCode]
	private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		m_nativeCrashHandlerService?.EnableCollection(bEnabled: false);
		System.Exception ex = e.ExceptionObject as System.Exception;
		if (ex == null)
		{
			ex = new System.Exception(e.ExceptionObject.ToString());
		}
		HandleExceptionImpl(ex);
	}

	private void SetupAutomationFlag()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		foreach (string text in commandLineArgs)
		{
			if (text.StartsWith("-automation", StringComparison.CurrentCultureIgnoreCase) || text.StartsWith("--teamcity", StringComparison.CurrentCultureIgnoreCase))
			{
				m_automationEnabled = true;
			}
		}
	}

	[DebuggerNonUserCode]
	private void HandleExceptionImpl(System.Exception exception)
	{
		if (Debugger.IsAttached)
		{
			throw exception;
		}
		SubmitCrash(exception);
		if (!m_automationEnabled)
		{
			ShowExceptionDialog(exception);
		}
		ExitApplication();
	}

	private void SubmitCrash(System.Exception exception)
	{
		CrashSubmissionService.SubmitIssue(SubmissionType.kCrash, exception);
	}

	private void ShowExceptionDialog(System.Exception exception)
	{
		try
		{
			UnhandledExceptionDialog unhandledExceptionDialog = new UnhandledExceptionDialog();
			unhandledExceptionDialog.ExceptionTextBox.Text = exception.ToString();
			unhandledExceptionDialog.HideContinueButton();
			unhandledExceptionDialog.ShowDialog(m_mainForm);
		}
		catch (System.Exception)
		{
		}
	}

	private void ExitApplication()
	{
		if (Application.MessageLoop)
		{
			Application.Exit();
		}
		else
		{
			Environment.Exit(1);
		}
	}
}

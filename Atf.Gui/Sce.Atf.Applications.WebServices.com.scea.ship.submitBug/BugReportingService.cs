using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Sce.Atf.Applications.WebServices.com.scea.ship.submitBug;

[GeneratedCode("wsdl", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[WebServiceBinding(Name = "SubmitBugSoapBinding", Namespace = "http://scea.com/tnt/bugs")]
public class BugReportingService : SoapHttpClientProtocol
{
	private SendOrPostCallback submitBugOperationCompleted;

	private SendOrPostCallback submitBug1OperationCompleted;

	public event submitBugCompletedEventHandler submitBugCompleted;

	public event submitBug1CompletedEventHandler submitBug1Completed;

	public BugReportingService()
	{
		base.Url = "http://www.ship.scea.com/portal/services/SubmitBug";
	}

	[SoapRpcMethod("", RequestNamespace = "http://scea.com/tnt/bugs", ResponseNamespace = "http://scea.com/tnt/bugs")]
	[return: SoapElement("submitBugReturn")]
	public bool submitBug(string shortcutName, string title, string description, int priority)
	{
		object[] array = Invoke("submitBug", new object[4] { shortcutName, title, description, priority });
		return (bool)array[0];
	}

	public IAsyncResult BeginsubmitBug(string shortcutName, string title, string description, int priority, AsyncCallback callback, object asyncState)
	{
		return BeginInvoke("submitBug", new object[4] { shortcutName, title, description, priority }, callback, asyncState);
	}

	public bool EndsubmitBug(IAsyncResult asyncResult)
	{
		object[] array = EndInvoke(asyncResult);
		return (bool)array[0];
	}

	public void submitBugAsync(string shortcutName, string title, string description, int priority)
	{
		submitBugAsync(shortcutName, title, description, priority, null);
	}

	public void submitBugAsync(string shortcutName, string title, string description, int priority, object userState)
	{
		if (submitBugOperationCompleted == null)
		{
			submitBugOperationCompleted = OnsubmitBugOperationCompleted;
		}
		InvokeAsync("submitBug", new object[4] { shortcutName, title, description, priority }, submitBugOperationCompleted, userState);
	}

	private void OnsubmitBugOperationCompleted(object arg)
	{
		if (this.submitBugCompleted != null)
		{
			InvokeCompletedEventArgs e = (InvokeCompletedEventArgs)arg;
			this.submitBugCompleted(this, new submitBugCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	[WebMethod(MessageName = "submitBug1")]
	[SoapRpcMethod("", RequestNamespace = "http://scea.com/tnt/bugs", ResponseNamespace = "http://scea.com/tnt/bugs")]
	[return: SoapElement("submitBugReturn")]
	public bool submitBug(string shortcutName, string username, string password, string title, string description, int priority)
	{
		object[] array = Invoke("submitBug1", new object[6] { shortcutName, username, password, title, description, priority });
		return (bool)array[0];
	}

	public IAsyncResult BeginsubmitBug1(string shortcutName, string username, string password, string title, string description, int priority, AsyncCallback callback, object asyncState)
	{
		return BeginInvoke("submitBug1", new object[6] { shortcutName, username, password, title, description, priority }, callback, asyncState);
	}

	public bool EndsubmitBug1(IAsyncResult asyncResult)
	{
		object[] array = EndInvoke(asyncResult);
		return (bool)array[0];
	}

	public void submitBug1Async(string shortcutName, string username, string password, string title, string description, int priority)
	{
		submitBug1Async(shortcutName, username, password, title, description, priority, null);
	}

	public void submitBug1Async(string shortcutName, string username, string password, string title, string description, int priority, object userState)
	{
		if (submitBug1OperationCompleted == null)
		{
			submitBug1OperationCompleted = OnsubmitBug1OperationCompleted;
		}
		InvokeAsync("submitBug1", new object[6] { shortcutName, username, password, title, description, priority }, submitBug1OperationCompleted, userState);
	}

	private void OnsubmitBug1OperationCompleted(object arg)
	{
		if (this.submitBug1Completed != null)
		{
			InvokeCompletedEventArgs e = (InvokeCompletedEventArgs)arg;
			this.submitBug1Completed(this, new submitBug1CompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	public new void CancelAsync(object userState)
	{
		base.CancelAsync(userState);
	}
}

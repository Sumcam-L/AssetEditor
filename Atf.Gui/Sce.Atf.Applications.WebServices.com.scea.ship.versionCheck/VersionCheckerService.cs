using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Sce.Atf.Applications.WebServices.com.scea.ship.versionCheck;

[GeneratedCode("wsdl", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[WebServiceBinding(Name = "VersionCheckerSoapBinding", Namespace = "http://scea.com/tnt/versioncheck")]
[SoapInclude(typeof(TNTWebServiceFault))]
public class VersionCheckerService : SoapHttpClientProtocol
{
	private SendOrPostCallback getPackageIdOperationCompleted;

	private SendOrPostCallback getHomePageOperationCompleted;

	private SendOrPostCallback getLatestVersionInfoOperationCompleted;

	private SendOrPostCallback getMappingPropertiesOperationCompleted;

	public event getPackageIdCompletedEventHandler getPackageIdCompleted;

	public event getHomePageCompletedEventHandler getHomePageCompleted;

	public event getLatestVersionInfoCompletedEventHandler getLatestVersionInfoCompleted;

	public event getMappingPropertiesCompletedEventHandler getMappingPropertiesCompleted;

	public VersionCheckerService()
	{
		base.Url = "http://www.ship.scea.com/portal/services/VersionChecker";
	}

	[SoapRpcMethod("", RequestNamespace = "http://scea.com/tnt/versioncheck", ResponseNamespace = "http://scea.com/tnt/versioncheck")]
	[return: SoapElement("getPackageIdReturn")]
	public string getPackageId(string shortcutName)
	{
		object[] array = Invoke("getPackageId", new object[1] { shortcutName });
		return (string)array[0];
	}

	public IAsyncResult BegingetPackageId(string shortcutName, AsyncCallback callback, object asyncState)
	{
		return BeginInvoke("getPackageId", new object[1] { shortcutName }, callback, asyncState);
	}

	public string EndgetPackageId(IAsyncResult asyncResult)
	{
		object[] array = EndInvoke(asyncResult);
		return (string)array[0];
	}

	public void getPackageIdAsync(string shortcutName)
	{
		getPackageIdAsync(shortcutName, null);
	}

	public void getPackageIdAsync(string shortcutName, object userState)
	{
		if (getPackageIdOperationCompleted == null)
		{
			getPackageIdOperationCompleted = OngetPackageIdOperationCompleted;
		}
		InvokeAsync("getPackageId", new object[1] { shortcutName }, getPackageIdOperationCompleted, userState);
	}

	private void OngetPackageIdOperationCompleted(object arg)
	{
		if (this.getPackageIdCompleted != null)
		{
			InvokeCompletedEventArgs e = (InvokeCompletedEventArgs)arg;
			this.getPackageIdCompleted(this, new getPackageIdCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	[SoapRpcMethod("", RequestNamespace = "http://scea.com/tnt/versioncheck", ResponseNamespace = "http://scea.com/tnt/versioncheck")]
	[return: SoapElement("getHomePageReturn")]
	public string getHomePage(string shortcutName)
	{
		object[] array = Invoke("getHomePage", new object[1] { shortcutName });
		return (string)array[0];
	}

	public IAsyncResult BegingetHomePage(string shortcutName, AsyncCallback callback, object asyncState)
	{
		return BeginInvoke("getHomePage", new object[1] { shortcutName }, callback, asyncState);
	}

	public string EndgetHomePage(IAsyncResult asyncResult)
	{
		object[] array = EndInvoke(asyncResult);
		return (string)array[0];
	}

	public void getHomePageAsync(string shortcutName)
	{
		getHomePageAsync(shortcutName, null);
	}

	public void getHomePageAsync(string shortcutName, object userState)
	{
		if (getHomePageOperationCompleted == null)
		{
			getHomePageOperationCompleted = OngetHomePageOperationCompleted;
		}
		InvokeAsync("getHomePage", new object[1] { shortcutName }, getHomePageOperationCompleted, userState);
	}

	private void OngetHomePageOperationCompleted(object arg)
	{
		if (this.getHomePageCompleted != null)
		{
			InvokeCompletedEventArgs e = (InvokeCompletedEventArgs)arg;
			this.getHomePageCompleted(this, new getHomePageCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	[SoapRpcMethod("", RequestNamespace = "http://scea.com/tnt/versioncheck", ResponseNamespace = "http://scea.com/tnt/versioncheck")]
	[return: SoapElement("getLatestVersionInfoReturn")]
	public object[] getLatestVersionInfo(string shortcutName)
	{
		object[] array = Invoke("getLatestVersionInfo", new object[1] { shortcutName });
		return (object[])array[0];
	}

	public IAsyncResult BegingetLatestVersionInfo(string shortcutName, AsyncCallback callback, object asyncState)
	{
		return BeginInvoke("getLatestVersionInfo", new object[1] { shortcutName }, callback, asyncState);
	}

	public object[] EndgetLatestVersionInfo(IAsyncResult asyncResult)
	{
		object[] array = EndInvoke(asyncResult);
		return (object[])array[0];
	}

	public void getLatestVersionInfoAsync(string shortcutName)
	{
		getLatestVersionInfoAsync(shortcutName, null);
	}

	public void getLatestVersionInfoAsync(string shortcutName, object userState)
	{
		if (getLatestVersionInfoOperationCompleted == null)
		{
			getLatestVersionInfoOperationCompleted = OngetLatestVersionInfoOperationCompleted;
		}
		InvokeAsync("getLatestVersionInfo", new object[1] { shortcutName }, getLatestVersionInfoOperationCompleted, userState);
	}

	private void OngetLatestVersionInfoOperationCompleted(object arg)
	{
		if (this.getLatestVersionInfoCompleted != null)
		{
			InvokeCompletedEventArgs e = (InvokeCompletedEventArgs)arg;
			this.getLatestVersionInfoCompleted(this, new getLatestVersionInfoCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	[SoapRpcMethod("", RequestNamespace = "http://scea.com/tnt/versioncheck", ResponseNamespace = "http://scea.com/tnt/versioncheck")]
	[return: SoapElement("getMappingPropertiesReturn")]
	public object[] getMappingProperties(string shortcutName)
	{
		object[] array = Invoke("getMappingProperties", new object[1] { shortcutName });
		return (object[])array[0];
	}

	public IAsyncResult BegingetMappingProperties(string shortcutName, AsyncCallback callback, object asyncState)
	{
		return BeginInvoke("getMappingProperties", new object[1] { shortcutName }, callback, asyncState);
	}

	public object[] EndgetMappingProperties(IAsyncResult asyncResult)
	{
		object[] array = EndInvoke(asyncResult);
		return (object[])array[0];
	}

	public void getMappingPropertiesAsync(string shortcutName)
	{
		getMappingPropertiesAsync(shortcutName, null);
	}

	public void getMappingPropertiesAsync(string shortcutName, object userState)
	{
		if (getMappingPropertiesOperationCompleted == null)
		{
			getMappingPropertiesOperationCompleted = OngetMappingPropertiesOperationCompleted;
		}
		InvokeAsync("getMappingProperties", new object[1] { shortcutName }, getMappingPropertiesOperationCompleted, userState);
	}

	private void OngetMappingPropertiesOperationCompleted(object arg)
	{
		if (this.getMappingPropertiesCompleted != null)
		{
			InvokeCompletedEventArgs e = (InvokeCompletedEventArgs)arg;
			this.getMappingPropertiesCompleted(this, new getMappingPropertiesCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
		}
	}

	public new void CancelAsync(object userState)
	{
		base.CancelAsync(userState);
	}
}

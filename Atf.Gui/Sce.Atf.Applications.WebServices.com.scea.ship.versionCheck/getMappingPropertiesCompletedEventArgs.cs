using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace Sce.Atf.Applications.WebServices.com.scea.ship.versionCheck;

[GeneratedCode("wsdl", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
public class getMappingPropertiesCompletedEventArgs : AsyncCompletedEventArgs
{
	private readonly object[] results;

	public object[] Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return (object[])results[0];
		}
	}

	internal getMappingPropertiesCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState)
		: base(exception, cancelled, userState)
	{
		this.results = results;
	}
}

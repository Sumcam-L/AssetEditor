using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sce.Atf.Applications.WebServices.com.scea.ship.versionCheck;

[Serializable]
[SoapInclude(typeof(GeneralSystemFault))]
[SoapInclude(typeof(LoginFailedFault))]
[SoapInclude(typeof(PermissionFault))]
[SoapInclude(typeof(ObjectNotFoundFault))]
[GeneratedCode("wsdl", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[SoapType(Namespace = "http://faults.webservices.portal.ship.scea.com")]
public abstract class TNTWebServiceFault
{
}

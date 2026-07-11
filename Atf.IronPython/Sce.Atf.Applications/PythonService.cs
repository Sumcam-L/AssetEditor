using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IScriptingService))]
[Export(typeof(ScriptingService))]
[Export(typeof(PythonService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PythonService : BasicPythonService, IInitializable
{
	[Import(AllowDefault = true)]
	private ScriptConsole m_scriptConsole;

	[Import(AllowDefault = true)]
	private IControlHostService m_controlHostService;

	protected Control Control => (m_scriptConsole != null) ? m_scriptConsole.Control : null;

	void IInitializable.Initialize()
	{
		if (m_controlHostService != null && m_scriptConsole == null)
		{
			m_scriptConsole = new ScriptConsole(this, m_controlHostService);
		}
	}
}

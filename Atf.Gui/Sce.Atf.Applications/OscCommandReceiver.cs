using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(OscCommandReceiver))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OscCommandReceiver : IInitializable
{
	private readonly CommandServiceBase m_commandService;

	private readonly IOscService m_oscService;

	private readonly Dictionary<string, CommandInfo> m_addressesToCommands = new Dictionary<string, CommandInfo>();

	[ImportingConstructor]
	public OscCommandReceiver(ICommandService commandService, IOscService oscService)
	{
		m_commandService = (CommandServiceBase)commandService;
		m_oscService = oscService;
	}

	public virtual void Initialize()
	{
		foreach (CommandInfo commandInfo in m_commandService.GetCommandInfos())
		{
			string oscAddress = "/" + m_commandService.GetCommandPath(commandInfo);
			oscAddress = OscServices.FixPropertyAddress(oscAddress);
			m_addressesToCommands.Add(oscAddress, commandInfo);
		}
		m_oscService.MessageReceived += OscServiceMessageReceived;
	}

	public IEnumerable<string> GetOscAddresses()
	{
		return m_addressesToCommands.Keys;
	}

	private void OscServiceMessageReceived(object sender, OscMessageReceivedArgs e)
	{
		if (m_addressesToCommands.TryGetValue(e.Address, out var value))
		{
			ICommandClient client = m_commandService.GetClient(value.CommandTag);
			if (client != null && client.CanDoCommand(value.CommandTag))
			{
				client.DoCommand(value.CommandTag);
			}
			e.Handled = true;
		}
	}
}

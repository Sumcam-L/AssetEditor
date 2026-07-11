using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IProtocol))]
[Export(typeof(TcpIpProtocol))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TcpIpProtocol : IProtocol
{
	[ImportMany]
	private IEnumerable<ITargetDiscovery> m_targetDiscovery = null;

	private static Guid ms_sId = new Guid(2858279633u, 17463, 19024, 129, 187, 208, 169, 92, 241, 110, 138);

	private static readonly Version SVersion = new Version(1, 0, 0);

	public uint DefaultPortNumber { get; set; }

	public string Name => "TCP/IP";

	public string Id => ms_sId.ToString();

	public Version Version => SVersion;

	public bool CanFindTargets => m_targetDiscovery != null && m_targetDiscovery.Any((ITargetDiscovery x) => x.ProtocolName == Name);

	public bool CanCreateUserTarget => true;

	public TcpIpProtocol()
	{
		DefaultPortNumber = 4001u;
	}

	public IEnumerable<ITarget> FindTargets()
	{
		return m_targetDiscovery.SelectMany((ITargetDiscovery x) => x.Targets);
	}

	public ITransportLayer CreateTransportLayer(ITarget target)
	{
		return new TcpIpTransport(target as TcpIpTarget);
	}

	public ITarget CreateUserTarget(string[] args = null)
	{
		string ip = "255.255.255.255";
		uint port = DefaultPortNumber;
		if (args != null)
		{
			if (args.Length != 0)
			{
				ip = args[0];
			}
			if (args.Length > 1)
			{
				port = uint.Parse(args[1]);
			}
		}
		return new TcpIpTarget("New Target", Id, Name, ip, port);
	}

	public bool EditUserTarget(ITarget target)
	{
		if (!(target is TcpIpTarget target2))
		{
			throw new ArgumentException("Can only edit TCP/IP targets");
		}
		bool? flag = DialogUtils.ShowDialogWithViewModel<TcpIpTargetEditDialog>(new TcpIpTargetEditDialogViewModel(target2));
		return flag.HasValue && flag.Value;
	}
}

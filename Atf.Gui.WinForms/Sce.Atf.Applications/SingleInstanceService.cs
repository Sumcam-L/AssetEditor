using System;
using System.ComponentModel.Composition;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(ISingleInstanceService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SingleInstanceService : MarshalByRefObject, IInitializable, ISingleInstanceService
{
	private Mutex m_mutex;

	private string[] m_commandLine;

	public string[] CommandLine
	{
		get
		{
			return m_commandLine;
		}
		set
		{
			if (value != null && value != m_commandLine)
			{
				m_commandLine = value;
				this.CommandLineChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler CommandLineChanged;

	public void RestartApplication()
	{
		m_mutex.Dispose();
		Application.Restart();
		Application.ExitThread();
	}

	[ImportingConstructor]
	public SingleInstanceService()
		: this(Application.ProductName + "-" + Application.ProductVersion + "-" + Environment.UserName)
	{
	}

	public SingleInstanceService(string applicationId)
	{
		if (applicationId.Length > 250)
		{
			applicationId = applicationId.Substring(0, 250);
		}
		applicationId = applicationId.Replace('/', '-');
		applicationId = applicationId.Replace('\\', '-');
		m_mutex = new Mutex(initiallyOwned: true, applicationId, out var createdNew);
		if (createdNew)
		{
			IpcChannel ipcChannel = null;
			int num = 20;
			while (ipcChannel == null && num > 0)
			{
				try
				{
					ipcChannel = new IpcChannel(applicationId);
				}
				catch (Exception)
				{
					num--;
					Thread.Sleep(250);
				}
			}
			if (ipcChannel == null)
			{
				MessageBoxes.Show("Failed to setup IPC for AssetCloud. You must restart the application manually.");
				Environment.Exit(-1);
			}
			ChannelServices.RegisterChannel(ipcChannel, ensureSecurity: false);
			RemotingServices.Marshal(this, "SingleInstanceService");
		}
		else
		{
			string url = "ipc://" + applicationId + "/SingleInstanceService";
			try
			{
				SingleInstanceService singleInstanceService = (SingleInstanceService)RemotingServices.Connect(typeof(SingleInstanceService), url);
				singleInstanceService.CommandLine = Environment.GetCommandLineArgs();
			}
			catch
			{
			}
			Environment.Exit(-1);
		}
	}

	public void Initialize()
	{
		CommandLine = Environment.GetCommandLineArgs();
	}

	public override object InitializeLifetimeService()
	{
		ILease lease = (ILease)base.InitializeLifetimeService();
		if (lease.CurrentState == LeaseState.Initial)
		{
			lease.InitialLeaseTime = TimeSpan.Zero;
		}
		return lease;
	}
}

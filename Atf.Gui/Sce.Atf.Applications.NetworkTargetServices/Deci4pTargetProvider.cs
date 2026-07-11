using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetProvider))]
[Export(typeof(Deci4pTargetProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Deci4pTargetProvider : ITargetProvider, IInitializable, IDisposable
{
	private BackgroundWorker m_worker;

	private IPsp2TmApi m_tmApi;

	private bool m_initialized;

	private List<TargetInfo> m_targets = new List<TargetInfo>();

	private Timer m_timer;

	private bool m_enabled = true;

	private static readonly object s_lock = new object();

	public static bool SdkInstalled => Type.GetTypeFromProgID("psp2tmapi.PSP2TMAPI") != null;

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	public string Name => "Vita Target".Localize();

	public bool CanCreateNew => false;

	[ImportMany(typeof(ITargetConsumer))]
	protected IEnumerable<ITargetConsumer> TargetConsumers { get; set; }

	void IInitializable.Initialize()
	{
		m_worker = new BackgroundWorker();
		m_worker.WorkerSupportsCancellation = true;
		m_worker.DoWork += BgwDoWork;
		m_worker.RunWorkerCompleted += BgwRunWorkerCompleted;
		m_worker.RunWorkerAsync(this);
		m_timer = new Timer(TimerTick, "Deci4pTicker", 1000, 1000);
	}

	private void BgwDoWork(object sender, DoWorkEventArgs e)
	{
		List<TargetInfo> list = new List<TargetInfo>();
		list.AddRange(FindTargets());
		e.Result = list;
	}

	private void BgwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Cancelled || e.Error != null || !(e.Result is List<TargetInfo>))
		{
			return;
		}
		List<TargetInfo> list = e.Result as List<TargetInfo>;
		bool flag = m_targets.Count != list.Count;
		if (!flag)
		{
			foreach (TargetInfo newTarget in list)
			{
				TargetInfo targetInfo = m_targets.Find((TargetInfo n) => n.Endpoint == newTarget.Endpoint && n.Name == newTarget.Name && n.Protocol == newTarget.Protocol && n.Scope == newTarget.Scope && n.Platform == newTarget.Platform);
				if (targetInfo == null)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		m_targets.Clear();
		m_targets.AddRange(list);
		foreach (ITargetConsumer targetConsumer in TargetConsumers)
		{
			targetConsumer.TargetsChanged(this, list);
		}
	}

	public TargetInfo CreateNew()
	{
		throw new InvalidOperationException("Vita targets can only be discovered!");
	}

	public bool AddTarget(TargetInfo target)
	{
		return false;
	}

	public bool Remove(TargetInfo target)
	{
		return false;
	}

	public IEnumerable<TargetInfo> GetTargets(ITargetConsumer targetConsumer)
	{
		foreach (TargetInfo target in m_targets)
		{
			yield return target;
		}
	}

	public void Dispose()
	{
		try
		{
			lock (s_lock)
			{
				if (m_tmApi != null)
				{
					m_tmApi.Dispose();
					m_tmApi = null;
				}
				if (m_worker != null)
				{
					m_worker.CancelAsync();
					m_worker.DoWork -= BgwDoWork;
					m_worker.RunWorkerCompleted -= BgwRunWorkerCompleted;
					m_worker = null;
				}
				if (m_timer != null)
				{
					m_timer.Dispose();
					m_timer = null;
				}
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
	}

	private IEnumerable<TargetInfo> FindTargets()
	{
		if (!m_initialized)
		{
			Type psp2TmType = Type.GetTypeFromProgID("psp2tmapi.PSP2TMAPI");
			if (psp2TmType != null)
			{
				object tmInstance = Activator.CreateInstance(psp2TmType);
				m_tmApi = (IPsp2TmApi)tmInstance;
				m_tmApi.CheckCompatibility(18u);
			}
			m_initialized = true;
			if (m_tmApi == null)
			{
				Dispose();
			}
		}
		if (!Enabled)
		{
			yield break;
		}
		lock (s_lock)
		{
			if (m_tmApi == null)
			{
				yield break;
			}
			foreach (ITarget target in m_tmApi.Targets)
			{
				yield return new Deci4pTargetInfo
				{
					Name = target.Name,
					Platform = "Vita",
					Endpoint = target.HardwareId,
					Protocol = "Deci4p",
					Scope = TargetScope.PerUser
				};
			}
		}
	}

	private void TimerTick(object data)
	{
		if (m_worker == null || !Monitor.TryEnter(m_worker))
		{
			return;
		}
		try
		{
			if (!m_worker.IsBusy)
			{
				m_worker.RunWorkerAsync(this);
			}
		}
		finally
		{
			Monitor.Exit(m_worker);
		}
	}
}

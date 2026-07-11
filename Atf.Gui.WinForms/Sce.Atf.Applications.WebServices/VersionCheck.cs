using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Sce.Atf.Applications.WebServices.com.scea.ship.versionCheck;

namespace Sce.Atf.Applications.WebServices;

public class VersionCheck
{
	private readonly string m_appMappingName;

	private readonly Version m_appVersion;

	private Version m_serverVersion;

	private static bool s_checkInProgress;

	private readonly Control m_dummyControl = new Control();

	public Version AppVersion => m_appVersion;

	public Version ServerVersion => m_serverVersion;

	public event CheckCompletedHandler CheckComplete;

	public VersionCheck()
	{
		Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(assembly, typeof(ProjectMappingAttribute));
		if (projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0)
		{
			m_appMappingName = projectMappingAttribute.Mapping.Trim();
		}
		else
		{
			m_appMappingName = null;
		}
		m_appVersion = assembly.GetName().Version;
		IntPtr handle = m_dummyControl.Handle;
	}

	public void Check(bool async)
	{
		try
		{
			if (m_appMappingName == null || m_appMappingName.Length == 0)
			{
				throw new Exception("Cannot check for update.\nAssembly mapping name not found");
			}
			if (m_appVersion == null)
			{
				throw new Exception("App version not defined");
			}
			if (async)
			{
				Thread thread = new Thread(DoCheck);
				thread.Priority = ThreadPriority.Lowest;
				thread.SetApartmentState(ApartmentState.STA);
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				thread.Name = "thread: Checking for update";
				thread.IsBackground = true;
				thread.Start();
			}
			else
			{
				DoCheck();
			}
		}
		catch (Exception ex)
		{
			NotifyClients("Version Check Failed".Localize() + ex.Message, error: true);
		}
	}

	private void DoCheck()
	{
		try
		{
			if (!s_checkInProgress)
			{
				s_checkInProgress = true;
				VersionCheckerService versionCheckerService = new VersionCheckerService();
				object[] latestVersionInfo = versionCheckerService.getLatestVersionInfo(m_appMappingName);
				string text = ((string)latestVersionInfo[0]).Trim();
				string[] array = text.Split(' ');
				m_serverVersion = new Version(array[array.Length - 1]);
				string msg = null;
				if (m_serverVersion > m_appVersion)
				{
					msg = ((string)latestVersionInfo[2]).Trim();
				}
				NotifyClients(msg, error: false);
			}
		}
		catch (Exception ex)
		{
			NotifyClients("Version check failed.\nError: " + ex.Message, error: true);
		}
		finally
		{
			s_checkInProgress = false;
		}
	}

	private void NotifyClients(string msg, bool error)
	{
		if (m_dummyControl.InvokeRequired)
		{
			m_dummyControl.Invoke(new Action<string, bool>(NotifyClients), msg, error);
		}
		else if (this.CheckComplete != null)
		{
			CheckCompletedHandler checkCompletedHandler = this.CheckComplete;
			checkCompletedHandler(msg, error);
		}
	}
}

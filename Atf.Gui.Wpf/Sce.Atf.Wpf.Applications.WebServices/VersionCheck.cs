using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using Sce.Atf.Applications.WebServices.com.scea.ship.versionCheck;

namespace Sce.Atf.Wpf.Applications.WebServices;

public class VersionCheck
{
	private readonly string m_appMappingName;

	private static bool ms_checkInProgress;

	public Version AppVersion { get; private set; }

	public Version ServerVersion { get; private set; }

	public event CheckCompletedHandler CheckComplete;

	public VersionCheck()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(ProjectMappingAttribute));
		if (projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0)
		{
			m_appMappingName = projectMappingAttribute.Mapping.Trim();
		}
		else
		{
			m_appMappingName = null;
		}
		AppVersion = entryAssembly.GetName().Version;
	}

	public void Check(bool async)
	{
		try
		{
			Requires.NotNullOrEmpty(m_appMappingName, "Cannot check for update.\nAssembly mapping name not found");
			Requires.NotNull(AppVersion, "App version not defined");
			if (async)
			{
				Thread thread = new Thread(DoCheck);
				thread.Name = "thread: Checking for update";
				thread.IsBackground = true;
				thread.SetApartmentState(ApartmentState.STA);
				thread.Priority = ThreadPriority.Lowest;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
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
			if (!ms_checkInProgress)
			{
				ms_checkInProgress = true;
				VersionCheckerService versionCheckerService = new VersionCheckerService();
				object[] latestVersionInfo = versionCheckerService.getLatestVersionInfo(m_appMappingName);
				string text = ((string)latestVersionInfo[0]).Trim();
				string[] array = text.Split(' ');
				ServerVersion = new Version(array[array.Length - 1]);
				string msg = null;
				if (ServerVersion > AppVersion)
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
			ms_checkInProgress = false;
		}
	}

	private void NotifyClients(string msg, bool error)
	{
		Application.Current.Dispatcher.InvokeIfRequired(delegate
		{
			if (this.CheckComplete != null)
			{
				this.CheckComplete?.Invoke(msg, error);
			}
		});
	}
}

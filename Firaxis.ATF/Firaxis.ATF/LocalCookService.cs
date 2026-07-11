using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Firaxis.AssetCloudFramework;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.CookerInterface;
using Firaxis.Controls;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(ICookService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalCookService : ICookService
{
	private readonly CivTechContext m_civTechContext;

	private readonly ICivTechService m_civTechService;

	private readonly ConcurrentQueue<string> m_cookErrorLog = new ConcurrentQueue<string>();

	private readonly ConcurrentQueue<string> m_cookLog = new ConcurrentQueue<string>();

	private readonly string m_cookLogPath;

	private readonly IDictionary<ICookerResult, ResultCode> m_resultMap;

	private static readonly object m_cookLocker = new object();

	public event EventHandler<DocumentEventArgs> DocumentCooked;

	public event EventHandler<DocumentEventArgs> DocumentCookFailed;

	[ImportingConstructor]
	public LocalCookService(ICivTechService civTechSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_civTechService = civTechSvc;
			m_civTechContext = Context.EnsureCreated<CivTechContext>();
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "AssetCloud");
			m_cookLogPath = Path.Combine(path, "Asset_Editor_Cook_Log.txt");
			m_resultMap = CreateResultMap();
		}
	}

	public CookResult Cook(IDocument document)
	{
		CookResult cookResult = Cook(document.Uri);
		if ((bool)cookResult.Result)
		{
			EventHandler<DocumentEventArgs> eventHandler = this.DocumentCooked;
			if (eventHandler != null)
			{
				eventHandler(this, new DocumentEventArgs(document));
				return cookResult;
			}
			return cookResult;
		}
		EventHandler<DocumentEventArgs> eventHandler2 = this.DocumentCookFailed;
		if (eventHandler2 != null)
		{
			eventHandler2(this, new DocumentEventArgs(document));
			return cookResult;
		}
		return cookResult;
	}

	public CookResult Cook(Uri uriToCook)
	{
		ICookerOptions cookerOptions = GenerateCookerOptions(uriToCook);
		if (cookerOptions == null)
		{
			return null;
		}
		return CookCustom(cookerOptions);
	}

	public CookResult CookCustom(ICookerOptions options)
	{
		return DoCook(options);
	}

	private LogEventHandler CreateLogEventHandler()
	{
		return delegate(string c, LogLevel level, string s)
		{
			OutputMessageType type = OutputMessageType.Info;
			bool flag = false;
			bool flag2 = false;
			switch (level)
			{
			case LogLevel.Info:
				type = OutputMessageType.Info;
				break;
			case LogLevel.Warning:
				type = OutputMessageType.Warning;
				flag = true;
				break;
			case LogLevel.Error:
				type = OutputMessageType.Error;
				flag2 = true;
				break;
			}
			if (s.EndsWith("\n") && !flag)
			{
				Outputs.Write(type, "{0}: {1}", c, s);
			}
			else if (!flag)
			{
				Outputs.WriteLine(type, "{0}: {1}", c, s);
			}
			m_cookLog.Enqueue(s);
			if (flag2)
			{
				m_cookErrorLog.Enqueue(s);
			}
		};
	}

	private IDictionary<ICookerResult, ResultCode> CreateResultMap()
	{
		return new Dictionary<ICookerResult, ResultCode>
		{
			{
				ICookerResult.COOK_SUCCESS,
				ResultCode.Success
			},
			{
				ICookerResult.COOK_FAILURE,
				new ResultCode("Cook failed.")
			},
			{
				ICookerResult.COOK_PARTIAL_SUCCESS,
				new ResultCode("Cook had a partial success.")
			},
			{
				ICookerResult.COOK_FAILURE_NO_PROJECT_CONFIG,
				new ResultCode("Unable to load the project config.")
			},
			{
				ICookerResult.COOK_FAILURE_CREATE_DIR,
				new ResultCode("Unable to create the output directory.")
			}
		};
	}

	private CookResult DoCook(ICookerOptions options)
	{
		Outputs.WriteLine(OutputMessageType.Info, "\n***Cooker Output!***\n");
		ICookerResult result = ICookerResult.COOK_FAILURE;
		LogEventHandler value = CreateLogEventHandler();
		Outputs.WriteLine(OutputMessageType.Diagnostic, "*************************************");
		Outputs.WriteLine(OutputMessageType.Diagnostic, "");
		Outputs.WriteLine(OutputMessageType.Diagnostic, "About to cook the following RELATIVE FILE PATHS:");
		foreach (string item in options.FilesToCook)
		{
			Outputs.WriteLine(OutputMessageType.Diagnostic, item);
		}
		Outputs.WriteLine(OutputMessageType.Diagnostic, "*************************************");
		try
		{
			lock (m_cookLocker)
			{
				ICookerIntf cookerIntf = m_civTechContext.CreateInstance<ICookerIntf>();
				cookerIntf.CookerLog += value;
				cookerIntf.Startup(m_civTechService.ToolHostLoader.ToolHostInterface);
				cookerIntf.Configure(options);
				result = cookerIntf.Cook(options);
				cookerIntf.CookerLog -= value;
				cookerIntf.Shutdown();
			}
		}
		catch (NullReferenceException)
		{
			m_civTechContext.CivTechLogger.AddLogItem(LogEventType.Error, "Cooker", "Error: Unable to get the appropriate ToolHost version from the DLL!");
		}
		catch (System.Exception ex2)
		{
			m_civTechContext.CivTechLogger.AddLogItem(LogEventType.Error, "Cooker", "Error: {0}", ex2.ToString());
		}
		FlushCookLog(options.AppendLogging);
		return new CookResult(GenerateResultCollection(result, options.FilesToCook));
	}

	private void FlushCookLog(bool AppendLogging)
	{
		try
		{
			using FileStream stream = File.Open(m_cookLogPath, AppendLogging ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None);
			using StreamWriter streamWriter = new StreamWriter(stream);
			string result = string.Empty;
			while (m_cookLog.TryDequeue(out result))
			{
				streamWriter.Write(result);
			}
			streamWriter.Flush();
		}
		catch (System.Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		if (m_cookErrorLog.Count > 0)
		{
			string text = "Cook Errors";
			string message = "The following cook errors occurred during this cook:\r\n\r\n" + string.Join("\r\n", m_cookErrorLog);
			using (DetailMessageBox detailMessageBox = new DetailMessageBox())
			{
				detailMessageBox.Text = text;
				detailMessageBox.Message = message;
				detailMessageBox.TopMost = true;
				detailMessageBox.ShowDialog();
			}
			string result2;
			while (m_cookErrorLog.TryDequeue(out result2))
			{
			}
		}
	}

	private ICookerOptions GenerateCookerOptions(Uri uri)
	{
		ICookerOptions cookerOptions = CookHelpers.CreateCookerArgs(m_civTechService, useAbsolutePaths: false);
		ProjectPaths paths = m_civTechService.PrimaryProject.Paths;
		string extension = Path.GetExtension(uri.LocalPath);
		if (extension.Equals(".xlp", StringComparison.CurrentCultureIgnoreCase))
		{
			string root = Path.Combine(paths.GamePantry, paths.XLPRoot);
			string text = RemoveRoot(root, uri.LocalPath);
			text = text.TrimStart(Path.DirectorySeparatorChar);
			cookerOptions.Mode = CookerMode.XLP;
			cookerOptions.XLPs.Add(text);
		}
		else
		{
			if (!extension.Equals(".artdef", StringComparison.CurrentCultureIgnoreCase))
			{
				Outputs.WriteLine(OutputMessageType.Error, "Tried to cook an unknown file extension.  This is not supported.  Path:  {0}", uri.LocalPath);
				return null;
			}
			string root2 = Path.Combine(paths.GamePantry, paths.ArtDefRoot);
			string text2 = RemoveRoot(root2, uri.LocalPath);
			text2 = text2.TrimStart(Path.DirectorySeparatorChar);
			cookerOptions.Mode = CookerMode.ArtDef;
			cookerOptions.ArtDefs.Add(text2);
		}
		return cookerOptions;
	}

	private IEnumerable<CookItemResultCode> GenerateResultCollection(ICookerResult result, IEnumerable<string> filesToCook)
	{
		ICollection<CookItemResultCode> collection = new List<CookItemResultCode>();
		if (m_resultMap.TryGetValue(result, out var value))
		{
			foreach (string item2 in filesToCook)
			{
				CookItemResultCode item = new CookItemResultCode(value, item2);
				collection.Add(item);
			}
		}
		return collection;
	}

	private string RemoveRoot(string root, string fullPath)
	{
		int length = root.Length;
		if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
		{
			return fullPath;
		}
		return fullPath.Substring(length);
	}
}

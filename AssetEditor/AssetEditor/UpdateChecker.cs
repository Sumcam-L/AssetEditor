using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AssetEditor;

internal static class UpdateChecker
{
	private const string RepoApiUrl = "https://api.github.com/repos/Sumcam-L/AssetEditor/releases/latest";

	private const int HttpTimeoutMs = 10000;

	public static void CheckForUpdatesAsync(Form owner)
	{
		Thread thread = new Thread(() => CheckForUpdates(owner));
		thread.IsBackground = true;
		thread.Name = "AssetEditor Update Check";
		thread.Start();
	}

	private static void CheckForUpdates(Form owner)
	{
		try
		{
			string json = FetchString(RepoApiUrl);
			string tagName = ParseJsonString(json, "tag_name");
			string downloadUrl = ParseFirstAssetUrl(json);
			if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(downloadUrl))
				return;

			if (!TryParseVersion(tagName, out Version latestVersion))
				return;

			Version currentVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
			if (latestVersion <= currentVersion)
				return;

			owner.BeginInvoke((Action)(() => PromptUpdate(owner, latestVersion, currentVersion, downloadUrl)));
		}
		catch
		{
			// Fetch failures (no network, rate limit, no releases) are silent by design.
		}
	}

	private static void PromptUpdate(Form owner, Version latest, Version current, string downloadUrl)
	{
		string message = string.Format("发现新版本 {0}（当前版本 {1}）。是否下载并更新？", latest, current);
		if (MessageBox.Show(owner, message, "Asset Editor 更新", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
			return;

		PerformUpdate(owner, downloadUrl);
	}

	private static void PerformUpdate(Form owner, string downloadUrl)
	{
		Exception error = null;
		bool staged = false;
		using (UpdateProgressDialog dialog = new UpdateProgressDialog())
		{
			Thread worker = new Thread(() =>
			{
				try
				{
					staged = DownloadAndStageUpdate(downloadUrl);
				}
				catch (Exception ex)
				{
					error = ex;
				}
				finally
				{
					dialog.BeginInvoke((Action)(() => dialog.Close()));
				}
			});
			worker.IsBackground = true;
			worker.Start();
			dialog.ShowDialog(owner);
		}

		if (error != null)
		{
			MessageBox.Show(owner, "更新失败：" + error.Message, "Asset Editor 更新", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		if (!staged)
			return;

		Application.Exit();
	}

	private static bool DownloadAndStageUpdate(string downloadUrl)
	{
		string tempRoot = Path.Combine(Path.GetTempPath(), "AssetEditorUpdate");
		if (Directory.Exists(tempRoot))
			Directory.Delete(tempRoot, true);
		Directory.CreateDirectory(tempRoot);

		string zipPath = Path.Combine(tempRoot, "update.zip");
		DownloadFile(downloadUrl, zipPath);

		string extractDir = Path.Combine(tempRoot, "extract");
		ZipFile.ExtractToDirectory(zipPath, extractDir);

		string exeDir = Path.GetDirectoryName(Application.ExecutablePath);
		string sourceDir = FindSourceDir(extractDir);
		if (exeDir == null || sourceDir == null)
			throw new InvalidOperationException("更新包中未找到 AssetEditor.exe");

		string updaterPath = Path.Combine(tempRoot, "updater.cmd");
		File.WriteAllText(updaterPath, BuildUpdaterScript(), Encoding.ASCII);

		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = "cmd.exe",
			UseShellExecute = false,
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = "/c \"" + updaterPath + "\" \"" + sourceDir + "\" \"" + exeDir + "\""
		};
		Process.Start(psi);
		return true;
	}

	private static string BuildUpdaterScript()
	{
		return "@echo off\r\n" +
			":wait\r\n" +
			"tasklist /fi \"imagename eq AssetEditor.exe\" | find /i \"AssetEditor.exe\" >nul\r\n" +
			"if %errorlevel%==0 (\r\n" +
			"  ping -n 2 127.0.0.1 >nul\r\n" +
			"  goto wait\r\n" +
			")\r\n" +
			"xcopy /y /e /i /q \"%~1\\*\" \"%~2\\\" >nul\r\n" +
			"start \"\" \"%~2\\AssetEditor.exe\"\r\n" +
			"del \"%~f0\"\r\n" +
			"rmdir /s /q \"%~dp0\" >nul 2>nul\r\n";
	}

	private static string FindSourceDir(string root)
	{
		if (File.Exists(Path.Combine(root, "AssetEditor.exe")))
			return root;
		foreach (string dir in Directory.GetDirectories(root))
		{
			string found = FindSourceDir(dir);
			if (found != null)
				return found;
		}
		return null;
	}

	private static string FetchString(string url)
	{
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.UserAgent = "AssetEditor-Updater";
		request.Timeout = HttpTimeoutMs;
		request.ReadWriteTimeout = HttpTimeoutMs;
		request.Method = "GET";
		using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
		using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
			return reader.ReadToEnd();
	}

	private static void DownloadFile(string url, string destPath)
	{
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.UserAgent = "AssetEditor-Updater";
		request.Timeout = HttpTimeoutMs;
		request.ReadWriteTimeout = HttpTimeoutMs;
		request.Method = "GET";
		using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
		using (Stream stream = response.GetResponseStream())
		using (FileStream file = File.Create(destPath))
			stream.CopyTo(file);
	}

	private static string ParseJsonString(string json, string key)
	{
		Match match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"([^\"]*)\"");
		return match.Success ? match.Groups[1].Value : null;
	}

	private static string ParseFirstAssetUrl(string json)
	{
		Match match = Regex.Match(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]*)\"");
		return match.Success ? match.Groups[1].Value : null;
	}

	private static bool TryParseVersion(string tag, out Version version)
	{
		version = null;
		tag = tag.Trim();
		if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
			tag = tag.Substring(1);
		return Version.TryParse(tag, out version);
	}

	private sealed class UpdateProgressDialog : Form
	{
		public UpdateProgressDialog()
		{
			Text = "Asset Editor 更新";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ControlBox = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			ClientSize = new Size(340, 90);
			Label label = new Label
			{
				Text = "正在下载并准备更新...",
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				Location = new Point(12, 12),
				Size = new Size(316, 20)
			};
			ProgressBar bar = new ProgressBar
			{
				Style = ProgressBarStyle.Marquee,
				MarqueeAnimationSpeed = 30,
				Location = new Point(12, 40),
				Size = new Size(316, 18)
			};
			Controls.Add(label);
			Controls.Add(bar);
		}
	}
}

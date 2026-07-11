using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;

namespace Sce.Atf;

public class FindFileResolver : XmlUrlResolver
{
	private static readonly object s_lockObject = new object();

	private static Thread s_finderThread;

	private static bool s_ignoreAll;

	private static bool s_acceptAll;

	private static bool s_searchAll;

	private static string s_searchRoot = string.Empty;

	private static readonly Dictionary<string, Uri> s_localPathMap = new Dictionary<string, Uri>();

	public static bool UIEnabled
	{
		get
		{
			return !s_ignoreAll;
		}
		set
		{
			s_ignoreAll = !value;
		}
	}

	public override object GetEntity(Uri absoluteUri, string role, Type returnType)
	{
		if (absoluteUri != null && absoluteUri.IsFile && !File.Exists(absoluteUri.LocalPath))
		{
			if (!Find(absoluteUri, out var newUri))
			{
				return null;
			}
			absoluteUri = newUri;
		}
		return base.GetEntity(absoluteUri, role, returnType);
	}

	public static bool Find(Uri uri, out Uri newUri)
	{
		bool result = false;
		lock (s_lockObject)
		{
			newUri = uri;
			if (s_finderThread != null)
			{
				s_finderThread.Join();
			}
			if (s_localPathMap.TryGetValue(uri.LocalPath, out var value))
			{
				newUri = value;
				return true;
			}
			if (File.Exists(uri.LocalPath))
			{
				return true;
			}
			if (s_ignoreAll)
			{
				return false;
			}
			Uri suggestedUri = FindSuggestion(uri);
			if (suggestedUri != null && (s_acceptAll || s_searchAll))
			{
				newUri = suggestedUri;
				return true;
			}
			if (s_searchAll && SearchForFile(uri, out newUri, askUser: false))
			{
				return true;
			}
			try
			{
				Uri newUriFromThread = uri;
				s_finderThread = new Thread((ThreadStart)delegate
				{
					result = QueryUser(uri, suggestedUri, out newUriFromThread);
				});
				s_finderThread.Name = "FindFileService";
				s_finderThread.IsBackground = true;
				s_finderThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				s_finderThread.SetApartmentState(ApartmentState.STA);
				s_finderThread.Start();
				s_finderThread.Join();
				newUri = newUriFromThread;
			}
			finally
			{
				s_finderThread = null;
			}
		}
		return result;
	}

	public static void Reset()
	{
		s_ignoreAll = false;
		s_acceptAll = false;
		s_searchAll = false;
		s_searchRoot = string.Empty;
		s_localPathMap.Clear();
	}

	private static bool QueryUser(Uri uri, Uri suggestedUri, out Uri newUri)
	{
		while (true)
		{
			FindFileAction findFileAction;
			if (suggestedUri == null)
			{
				FindFileDialog findFileDialog = new FindFileDialog(uri.LocalPath);
				findFileAction = ((findFileDialog.ShowDialog() != DialogResult.Cancel) ? findFileDialog.Action : FindFileAction.Ignore);
			}
			else
			{
				FindFileWithSuggestionDialog findFileWithSuggestionDialog = new FindFileWithSuggestionDialog(uri.LocalPath, suggestedUri.LocalPath);
				findFileAction = ((findFileWithSuggestionDialog.ShowDialog() != DialogResult.Cancel) ? findFileWithSuggestionDialog.Action : FindFileAction.Ignore);
			}
			switch (findFileAction)
			{
			case FindFileAction.AcceptSuggestion:
				newUri = suggestedUri;
				return true;
			case FindFileAction.AcceptAllSuggestions:
				s_acceptAll = true;
				newUri = suggestedUri;
				return true;
			case FindFileAction.SearchDirectory:
				if (SearchForFile(uri, out newUri, askUser: true))
				{
					return true;
				}
				break;
			case FindFileAction.SearchDirectoryForAll:
				s_searchAll = true;
				if (SearchForFile(uri, out newUri, askUser: false))
				{
					return true;
				}
				break;
			case FindFileAction.UserSpecify:
				if (UserFindFile(uri, out newUri))
				{
					return true;
				}
				break;
			case FindFileAction.Ignore:
				newUri = uri;
				return false;
			case FindFileAction.IgnoreAll:
				s_ignoreAll = true;
				newUri = uri;
				return false;
			default:
				throw new InvalidOperationException("unhandled FindFileAction enum");
			}
		}
	}

	private static bool SearchForFile(Uri uri, out Uri newUri, bool askUser)
	{
		newUri = uri;
		bool flag = true;
		if (askUser || s_searchRoot == string.Empty)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			if (s_searchRoot != string.Empty)
			{
				folderBrowserDialog.SelectedPath = s_searchRoot;
			}
			else
			{
				folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
			}
			DialogResult dialogResult = folderBrowserDialog.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				s_searchRoot = folderBrowserDialog.SelectedPath;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			string fileName = Path.GetFileName(uri.LocalPath);
			using IEnumerator<string> enumerator = DirectoryUtil.GetFilesIteratively(s_searchRoot, fileName).GetEnumerator();
			if (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				newUri = new Uri(current);
				s_localPathMap.Add(uri.LocalPath, newUri);
				return true;
			}
		}
		return false;
	}

	private static Uri FindSuggestion(Uri uri)
	{
		string directoryName = Path.GetDirectoryName(uri.LocalPath);
		string fileName = Path.GetFileName(uri.LocalPath);
		foreach (KeyValuePair<string, Uri> item in s_localPathMap)
		{
			string directoryName2 = Path.GetDirectoryName(item.Key);
			string directoryName3 = Path.GetDirectoryName(item.Value.LocalPath);
			string relativePath = PathUtil.GetRelativePath(directoryName, directoryName2);
			string absolutePath = PathUtil.GetAbsolutePath(relativePath, directoryName3);
			string text = Path.Combine(absolutePath, fileName);
			if (File.Exists(text))
			{
				Uri uri2 = new Uri(text);
				s_localPathMap.Add(uri.LocalPath, uri2);
				return uri2;
			}
			text = Path.Combine(directoryName3, fileName);
			if (File.Exists(text))
			{
				Uri uri3 = new Uri(text);
				s_localPathMap.Add(uri.LocalPath, uri3);
				return uri3;
			}
		}
		return null;
	}

	private static bool UserFindFile(Uri uri, out Uri newUri)
	{
		newUri = uri;
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.CheckFileExists = true;
		openFileDialog.CheckPathExists = true;
		openFileDialog.Multiselect = false;
		openFileDialog.Title = $"Find or Replace {Path.GetFileName(uri.LocalPath)}";
		DialogResult dialogResult = openFileDialog.ShowDialog();
		if (dialogResult == DialogResult.OK)
		{
			newUri = new Uri(openFileDialog.FileName);
			s_localPathMap.Add(uri.LocalPath, newUri);
			return true;
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;
using Microsoft.Win32;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf;

public class FindFileResolver : XmlUrlResolver
{
	private static readonly object s_lockObject = new object();

	private static string s_searchRoot = string.Empty;

	private static FindFileAction s_lastAction = FindFileAction.UserSpecify;

	private static readonly Dictionary<string, Uri> s_localPathMap = new Dictionary<string, Uri>();

	public static bool UIEnabled
	{
		get
		{
			return s_lastAction == FindFileAction.IgnoreAll;
		}
		set
		{
			s_lastAction = (value ? FindFileAction.IgnoreAll : FindFileAction.UserSpecify);
		}
	}

	public override object GetEntity(Uri absoluteUri, string role, Type returnType)
	{
		if (absoluteUri != null && absoluteUri.IsFile && !File.Exists(absoluteUri.LocalPath))
		{
			if (Find(absoluteUri, out var newUri, SelectFileFilterOptions.Any) != true)
			{
				return null;
			}
			absoluteUri = newUri;
		}
		return base.GetEntity(absoluteUri, role, returnType);
	}

	public static bool? Find(Uri uri, out Uri newUri, SelectFileFilterOptions options)
	{
		bool? result = false;
		lock (s_lockObject)
		{
			newUri = uri;
			if (s_localPathMap.TryGetValue(uri.LocalPath, out var value))
			{
				newUri = value;
				return true;
			}
			if (File.Exists(uri.LocalPath))
			{
				return true;
			}
			Uri suggestedUri = FindSuggestion(uri);
			bool flag = s_lastAction == FindFileAction.AcceptAllSuggestions || s_lastAction == FindFileAction.SearchDirectoryForAll || s_lastAction == FindFileAction.IgnoreAll;
			if (suggestedUri != null && flag)
			{
				newUri = suggestedUri;
				return true;
			}
			if (s_lastAction == FindFileAction.SearchDirectoryForAll || s_lastAction == FindFileAction.IgnoreAll)
			{
				bool fileFound = false;
				Uri foundUri = null;
				Application.Current.Dispatcher.Invoke(delegate
				{
					fileFound = SearchForFile(uri, out foundUri, askUser: false);
				});
				if (fileFound)
				{
					newUri = foundUri;
					return true;
				}
			}
			if (s_lastAction != FindFileAction.IgnoreAll)
			{
				Uri tempUri = null;
				Application.Current.Dispatcher.Invoke(delegate
				{
					result = QueryUser(uri, suggestedUri, out tempUri, options);
				});
				newUri = tempUri;
			}
		}
		return result;
	}

	public static void Reset()
	{
		s_lastAction = FindFileAction.UserSpecify;
		s_searchRoot = string.Empty;
		s_localPathMap.Clear();
	}

	private static bool? QueryUser(Uri uri, Uri suggestedUri, out Uri newUri, SelectFileFilterOptions options)
	{
		while (true)
		{
			FindFileDialogViewModel findFileDialogViewModel = new FindFileDialogViewModel
			{
				Action = s_lastAction,
				OriginalPath = uri.LocalPath,
				SuggestedPath = ((suggestedUri != null) ? suggestedUri.LocalPath : null)
			};
			s_lastAction = ((DialogUtils.ShowDialogWithViewModel<FindFileDialog>(findFileDialogViewModel) == false) ? FindFileAction.Quit : findFileDialogViewModel.Action);
			switch (s_lastAction)
			{
			case FindFileAction.AcceptSuggestion:
				newUri = suggestedUri;
				return true;
			case FindFileAction.AcceptAllSuggestions:
				newUri = suggestedUri;
				return true;
			case FindFileAction.SearchDirectory:
				if (SearchForFile(uri, out newUri, askUser: true))
				{
					return true;
				}
				break;
			case FindFileAction.SearchDirectoryForAll:
				if (SearchForFile(uri, out newUri, askUser: false))
				{
					return true;
				}
				break;
			case FindFileAction.UserSpecify:
				if (UserFindFile(uri, out newUri, options))
				{
					return true;
				}
				break;
			case FindFileAction.Ignore:
				newUri = uri;
				return false;
			case FindFileAction.IgnoreAll:
				newUri = uri;
				return false;
			case FindFileAction.Quit:
				newUri = uri;
				return null;
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
			BrowseForFolderDialog browseForFolderDialog = new BrowseForFolderDialog
			{
				InitialFolder = ((s_searchRoot != string.Empty) ? s_searchRoot : Directory.GetCurrentDirectory())
			};
			if (browseForFolderDialog.ShowDialog(Application.Current.MainWindow) == true)
			{
				s_searchRoot = browseForFolderDialog.SelectedFolder;
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

	private static bool UserFindFile(Uri uri, out Uri newUri, SelectFileFilterOptions options)
	{
		newUri = uri;
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			CheckFileExists = true,
			CheckPathExists = true,
			Multiselect = false,
			Title = $"Find or Replace {Path.GetFileName(uri.LocalPath)}"
		};
		switch (options)
		{
		case SelectFileFilterOptions.ExactMatch:
		{
			string fileName = Path.GetFileName(uri.LocalPath);
			openFileDialog.Filter = fileName + "|" + fileName;
			break;
		}
		case SelectFileFilterOptions.ExtensionMatch:
		{
			string extension = Path.GetExtension(uri.LocalPath);
			openFileDialog.Filter = extension + "|" + extension;
			break;
		}
		}
		if (openFileDialog.ShowDialog(DialogUtils.GetActiveWindow()) == true)
		{
			newUri = new Uri(openFileDialog.FileName);
			s_localPathMap.Add(uri.LocalPath, newUri);
			return true;
		}
		return false;
	}
}

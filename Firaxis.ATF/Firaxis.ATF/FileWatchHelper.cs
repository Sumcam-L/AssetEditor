using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Controls;

namespace Firaxis.ATF;

public static class FileWatchHelper
{
	private static readonly ConcurrentDictionary<string, bool> m_checkedStates;

	private static readonly IDictionary<string, ICollection<IFileWatchNode>> m_registeredNodes;

	private static readonly ISet<string> m_unwatchedFileExtensions;

	static FileWatchHelper()
	{
		m_checkedStates = new ConcurrentDictionary<string, bool>();
		m_registeredNodes = new Dictionary<string, ICollection<IFileWatchNode>>();
		m_unwatchedFileExtensions = new HashSet<string>();
		foreach (string supportedSourceFileExtension in ExporterService.GetSupportedSourceFileExtensions(InstanceType.IT_GEOMETRY))
		{
			m_unwatchedFileExtensions.Add(supportedSourceFileExtension);
		}
		foreach (string supportedSourceFileExtension2 in ExporterService.GetSupportedSourceFileExtensions(InstanceType.IT_ANIMATION))
		{
			m_unwatchedFileExtensions.Add(supportedSourceFileExtension2);
		}
	}

	public static TriStateCheckboxTreeView.CheckState GetCheckState(string filePath)
	{
		TriStateCheckboxTreeView.CheckState result = TriStateCheckboxTreeView.CheckState.Checked;
		string extension = Path.GetExtension(filePath);
		if (m_unwatchedFileExtensions.Contains(extension))
		{
			result = TriStateCheckboxTreeView.CheckState.Unchecked;
		}
		return result;
	}

	public static IEnumerable<IFileWatchNode> GetNodesByURI(Uri nodeUri)
	{
		string localPath = nodeUri.LocalPath;
		ICollection<IFileWatchNode> value;
		lock (m_registeredNodes)
		{
			m_registeredNodes.TryGetValue(localPath, out value);
		}
		if (value != null)
		{
			lock (value)
			{
				return value.ToArray();
			}
		}
		return Enumerable.Empty<IFileWatchNode>();
	}

	public static bool GetStartingCheckedState(IFileWatchNode node)
	{
		string localPath = node.FileUri.LocalPath;
		bool value = GetCheckState(localPath) == TriStateCheckboxTreeView.CheckState.Checked;
		m_checkedStates.TryAdd(localPath, value);
		return m_checkedStates[localPath];
	}

	public static void RegisterNode(IFileWatchNode node)
	{
		string localPath = node.FileUri.LocalPath;
		ICollection<IFileWatchNode> collection;
		lock (m_registeredNodes)
		{
			if (!m_registeredNodes.ContainsKey(localPath))
			{
				m_registeredNodes.Add(localPath, new HashSet<IFileWatchNode>());
			}
			collection = m_registeredNodes[localPath];
		}
		lock (collection)
		{
			collection.Add(node);
		}
	}

	public static void UnregisterNode(IFileWatchNode node)
	{
		string localPath = node.FileUri.LocalPath;
		ICollection<IFileWatchNode> value;
		lock (m_registeredNodes)
		{
			m_registeredNodes.TryGetValue(localPath, out value);
		}
		if (value != null)
		{
			lock (value)
			{
				value.Remove(node);
			}
		}
	}

	public static void UpdateCheckedState(Uri nodeUri, bool newCheckedState)
	{
		string localPath = nodeUri.LocalPath;
		m_checkedStates[localPath] = newCheckedState;
		ICollection<IFileWatchNode> value;
		lock (m_registeredNodes)
		{
			m_registeredNodes.TryGetValue(localPath, out value);
		}
		if (value != null)
		{
			IFileWatchNode[] array;
			lock (value)
			{
				array = value.ToArray();
			}
			IFileWatchNode[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].IsChecked = newCheckedState;
			}
		}
	}
}

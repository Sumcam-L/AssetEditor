using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.ATF;

public static class DockPanelExtensions
{
	public static string GetLayoutState(this DockPanel dockPanel)
	{
		using MemoryStream memoryStream = new MemoryStream();
		dockPanel.SaveAsXml(memoryStream, Encoding.UTF8);
		using MemoryStream stream = new MemoryStream(memoryStream.GetBuffer());
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	public static void SetLayoutState(this DockPanel dockPanel, string value, IDictionary<Control, DockContent> dockContentDictionary)
	{
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using StreamWriter streamWriter = new StreamWriter(memoryStream);
		int length = Math.Min(value.Length, 20);
		if (!StringUtil.RemoveAllWhiteSpace(value.Substring(0, length)).StartsWith("<?xmlversion=", StringComparison.OrdinalIgnoreCase))
		{
			streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
		}
		streamWriter.Write(value);
		streamWriter.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		int count = dockPanel.Contents.Count;
		foreach (KeyValuePair<Control, DockContent> item in dockContentDictionary)
		{
			DockContent value2 = item.Value;
			value2.DockState = DockState.Unknown;
			value2.DockPanel = null;
			value2.FloatPane = null;
			value2.Pane = null;
		}
		DeserializeDockContent deserializeContent = delegate(string id)
		{
			foreach (DockContent value4 in dockContentDictionary.Values)
			{
				if (value4.Name == id)
				{
					return value4;
				}
			}
			return (IDockContent)null;
		};
		try
		{
			dockPanel.LoadFromXml(memoryStream, deserializeContent, closeStream: true);
		}
		catch (System.Exception ex)
		{
			string text = null;
			foreach (DockContent content in dockPanel.Contents)
			{
				text = text + content.AccessibilityObject.Name + " ";
			}
			BugSubmitter.SilentReport($"DockPanel extensions failed to load from Xml:{count} {dockContentDictionary.Values.Count} {text} {ex.Message} @assign nicholai.wojtowycz @summary DockPanel extensions failed to load from Xml");
		}
		foreach (KeyValuePair<Control, DockContent> item2 in dockContentDictionary)
		{
			DockContent value3 = item2.Value;
			if (value3.DockState == DockState.Unknown)
			{
				value3.Show(dockPanel, DockState.Document);
			}
		}
	}
}

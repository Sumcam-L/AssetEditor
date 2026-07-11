using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech.Properties;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace AssetEditor;

[Export(typeof(IInitializable))]
[Export(typeof(HelpAboutCommand))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetEditorHelpAboutCommand : HelpAboutCommand
{
	protected override void ShowHelpAbout()
	{
		RichTextBox richTextBox = new RichTextBox();
		richTextBox.BorderStyle = BorderStyle.None;
		richTextBox.ReadOnly = true;
		richTextBox.Text = "Sid Meier's Civilization 6 Content Creation Tools\r\n\r\nBrought to you by Firaxis Games and the Civilization Team\r\n\r\n© 1991-2018 Take-Two Interactive Software, Inc.\r\nSid Meier's Civilization VI, Sid Meier's Civilization, Civilization, Civ, and their respective logos are trademarks of Take-Two Interactive Software, Inc.";
		List<string> list = new List<string>();
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		ToolVersionConverter.GetVersion(Application.ProductVersion, version);
		list.Add("Icons courtesy of Axialis Software http://www.axialis.com");
		list.Add("Uses Granny Animation. Copyright © 1999-2018 by RAD Game Tools Inc.");
		list.Add("Uses Fork Particle. Copyright © 2018 Fork Particle, Inc.");
		list.Add("Uses Havok® Script. © Copyright 1999 – 2018 Havok.com, Inc. (and its Licensors). All Rights Reserved. See www.havok.com for details.");
		list.Add("AMD, the AMD Arrow logo, Radeon, Crossfire, and combinations thereof are either registered trademarks or trademarks of Advanced Micro Devices, Inc. in the United States and/or other countries.");
		list.Add("Microsoft, DirectX, Visual Studio, and Windows are either registered trademarks or trademarks of Microsoft Corporation in the United States and/or other countries.");
		list.Add("Lua Copyright © 1994-2018 Lua.org, PUC-Rio.");
		list.Add("All other marks and trademarks are the property of their respective owners. All rights reserved. The content of this videogame is fictional and is not intended to represent or depict an actual record of the events, persons or entities in the game's historical setting.");
		string url = (Firaxis.CivTech.Properties.Resources.ModTools ? "https://forums.civfanatics.com/forums/civ6-creation-customization.541/" : "https://hub.2k.com/display/FXSMadrid/Asset+Editor");
		new AboutDialog("About AssetEditor".Localize(), url, richTextBox, ResourceUtil.GetImage(Resources.AssetEditorIcon), list, addAtfInfo: true).ShowDialog();
	}
}

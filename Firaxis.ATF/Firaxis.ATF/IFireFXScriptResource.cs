using System;
using System.Collections.Generic;
using Firaxis.CivTech.FireFX;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IFireFXScriptResource : IResource
{
	string Text { get; set; }

	IList<CompileIssue> Issues { get; set; }

	IFireFXEffect Effect { get; set; }

	event EventHandler TextChanged;

	event EventHandler EffectChanged;
}

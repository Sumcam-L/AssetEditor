using System;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking;

[AttributeUsage(AttributeTargets.All)]
internal sealed class LocalizedCategoryAttribute : CategoryAttribute
{
	public LocalizedCategoryAttribute(string key)
		: base(key)
	{
	}

	protected override string GetLocalizedString(string value)
	{
		return ResourceHelper.GetString(value);
	}
}

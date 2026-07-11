using System;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TextElementAdapter : DomNodeAdapter
{
	public string Text
	{
		get
		{
			return GetAttribute<string>(BaseSchema.TextElementType.TextAttribute);
		}
		set
		{
			SetAttribute(BaseSchema.TextElementType.TextAttribute, value);
		}
	}

	public event EventHandler<ItemChangedEventArgs<string>> TextChanged;

	protected virtual void HandleDomNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == BaseSchema.TextElementType.TextAttribute)
		{
			RaiseTextChanged(Text);
		}
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += HandleDomNodeAttributeChanged;
		base.OnNodeSet();
	}

	protected virtual void RaiseTextChanged(string text)
	{
		this.TextChanged?.Invoke(this, new ItemChangedEventArgs<string>(text));
	}
}

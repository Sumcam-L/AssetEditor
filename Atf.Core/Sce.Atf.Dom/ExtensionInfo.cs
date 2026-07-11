using System;

namespace Sce.Atf.Dom;

public abstract class ExtensionInfo : FieldMetadata
{
	public abstract Type Type { get; }

	public ExtensionInfo(string name)
		: base(name)
	{
	}

	public abstract object Create(DomNode node);

	public bool IsEquivalent(ExtensionInfo other)
	{
		return other != null && base.Index == other.Index && base.DefiningType == other.DefiningType;
	}

	protected override NamedMetadata GetParent()
	{
		if (base.OwningType != null)
		{
			return base.OwningType.BaseType.GetExtensionInfo(base.Index);
		}
		return null;
	}
}
public class ExtensionInfo<T> : ExtensionInfo where T : new()
{
	public override Type Type => typeof(T);

	public ExtensionInfo()
		: base(typeof(T).FullName)
	{
	}

	public ExtensionInfo(string name)
		: base(name)
	{
	}

	public override object Create(DomNode node)
	{
		return new T();
	}
}

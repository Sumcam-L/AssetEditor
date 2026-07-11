using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Pin : DomNodeAdapter, ICircuitPin, IEdgeRoute
{
	private int m_index;

	protected abstract AttributeInfo TypeAttribute { get; }

	protected abstract AttributeInfo NameAttribute { get; }

	public virtual string Name
	{
		get
		{
			return GetAttribute<string>(NameAttribute);
		}
		set
		{
			SetAttribute(NameAttribute, value);
		}
	}

	public virtual string TypeName
	{
		get
		{
			return GetAttribute<string>(TypeAttribute);
		}
		set
		{
			SetAttribute(TypeAttribute, value);
		}
	}

	public virtual int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public virtual bool AllowFanIn => false;

	public virtual bool AllowFanOut => true;
}

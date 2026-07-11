namespace Sce.Atf.Dom;

public abstract class FieldMetadata : NamedMetadata
{
	private DomNodeType m_owningType;

	private DomNodeType m_definingType;

	private int m_index;

	public DomNodeType OwningType
	{
		get
		{
			return m_owningType;
		}
		internal set
		{
			m_owningType = value;
		}
	}

	public DomNodeType DefiningType
	{
		get
		{
			return m_definingType;
		}
		internal set
		{
			m_definingType = value;
		}
	}

	internal int Index
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

	protected FieldMetadata(string name)
		: base(name)
	{
	}

	public bool Equivalent(FieldMetadata other)
	{
		return other != null && Index == other.Index && DefiningType == other.DefiningType;
	}

	public int GetEquivalentHashCode()
	{
		return base.Name.GetHashCode();
	}
}

using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Adaptable;

public class DiagramHitRecord
{
	private object m_item;

	private object m_subItem;

	private object m_part;

	private object m_subPart;

	private object m_defaultPart;

	private AdaptablePath<object> m_hitPath;

	public object Item
	{
		get
		{
			return m_item;
		}
		protected set
		{
			m_item = value;
		}
	}

	public object SubItem
	{
		get
		{
			return m_subItem;
		}
		set
		{
			m_subItem = value;
		}
	}

	public object Part
	{
		get
		{
			return m_part;
		}
		set
		{
			m_part = value;
		}
	}

	public object SubPart
	{
		get
		{
			return m_subPart;
		}
		set
		{
			m_subPart = value;
		}
	}

	public object DefaultPart
	{
		get
		{
			return m_defaultPart;
		}
		set
		{
			m_defaultPart = value;
		}
	}

	public AdaptablePath<object> HitPath
	{
		get
		{
			return m_hitPath;
		}
		set
		{
			m_hitPath = value;
		}
	}

	public DiagramHitRecord()
	{
	}

	public DiagramHitRecord(object item)
	{
		m_item = item;
	}

	public DiagramHitRecord(object item, object part)
	{
		m_item = item;
		m_part = part;
	}
}

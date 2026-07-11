using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Timelines;

public class TimelineLayout : IEnumerable<KeyValuePair<TimelinePath, RectangleF>>, IEnumerable
{
	private readonly Dictionary<TimelinePath, RectangleF> m_paths = new Dictionary<TimelinePath, RectangleF>();

	public RectangleF this[TimelinePath path]
	{
		get
		{
			return m_paths[path];
		}
		set
		{
			m_paths[path] = value;
		}
	}

	public RectangleF this[ITimelineObject obj]
	{
		get
		{
			return m_paths[new TimelinePath(obj)];
		}
		set
		{
			m_paths[new TimelinePath(obj)] = value;
		}
	}

	public void Add(TimelinePath path, RectangleF bounds)
	{
		m_paths.Add(path, bounds);
	}

	public RectangleF GetBounds(TimelinePath path)
	{
		return m_paths[path];
	}

	public bool TryGetBounds(TimelinePath path, out RectangleF bounds)
	{
		return m_paths.TryGetValue(path, out bounds);
	}

	public bool ContainsPath(TimelinePath path)
	{
		return m_paths.ContainsKey(path);
	}

	public IEnumerator<KeyValuePair<TimelinePath, RectangleF>> GetEnumerator()
	{
		return m_paths.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

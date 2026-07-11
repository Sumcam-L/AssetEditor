using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public class D2dSnapManipulator
{
	private class SnapOffsetInfo
	{
		private readonly float m_snapFromPoint;

		private float m_dist;

		private float m_snapToPoint;

		public float Offset => m_snapToPoint - m_snapFromPoint;

		public float SnapToPoint => m_snapToPoint;

		public SnapOffsetInfo(float snapFromPoint)
		{
			m_snapFromPoint = snapFromPoint;
			m_dist = float.MaxValue;
		}

		public void Update(float snapToPoint, IEvent snapToEvent, float tolerance)
		{
			float num = Math.Abs(snapToPoint - m_snapFromPoint);
			if (num < tolerance && num < m_dist)
			{
				m_snapToPoint = snapToPoint;
				m_dist = num;
			}
		}

		public static void RemoveInvalid(List<SnapOffsetInfo> infos)
		{
			float num = float.MaxValue;
			for (int i = 0; i < infos.Count; i++)
			{
				SnapOffsetInfo snapOffsetInfo = infos[i];
				if (snapOffsetInfo.m_dist < num)
				{
					num = snapOffsetInfo.m_dist;
				}
			}
			int num2 = 0;
			for (int j = 0; j < infos.Count; j++)
			{
				SnapOffsetInfo snapOffsetInfo2 = infos[j];
				if (snapOffsetInfo2.m_dist < num + 0.0001f)
				{
					infos[num2++] = snapOffsetInfo2;
				}
			}
			infos.RemoveRange(num2, infos.Count - num2);
		}
	}

	private readonly D2dTimelineControl m_owner;

	private IEvent m_scrubber;

	private readonly List<SnapOffsetInfo> m_snapInfo = new List<SnapOffsetInfo>(2);

	private static float s_snapTolerance = 10f;

	private static Color s_color = Color.DarkRed;

	private static Keys s_deactivatorKeys = Keys.Shift;

	private static Keys s_activatorKeys = Keys.None;

	public IEvent Scrubber
	{
		get
		{
			return m_scrubber;
		}
		set
		{
			m_scrubber = value;
		}
	}

	[DefaultValue(10f)]
	public static float SnapTolerance
	{
		get
		{
			return s_snapTolerance;
		}
		set
		{
			s_snapTolerance = value;
		}
	}

	[DefaultValue(typeof(Color), "DarkRed")]
	public static Color Color
	{
		get
		{
			return s_color;
		}
		set
		{
			s_color = value;
		}
	}

	[DefaultValue(Keys.None)]
	public static Keys ActivatorKeys
	{
		get
		{
			return s_activatorKeys;
		}
		set
		{
			s_activatorKeys = value;
		}
	}

	[DefaultValue(Keys.Shift)]
	public static Keys DeactivatorKeys
	{
		get
		{
			return s_deactivatorKeys;
		}
		set
		{
			s_deactivatorKeys = value;
		}
	}

	public D2dSnapManipulator(D2dTimelineControl owner)
	{
		m_owner = owner;
		m_owner.GetSnapOffset = GetSnapOffset;
		m_owner.DrawingD2d += owner_DrawingD2d;
		m_owner.MouseUp += owner_MouseUp;
		m_owner.MouseDown += owner_MouseDown;
		m_owner.KeyDown += owner_KeyDown;
	}

	public void CancelSnap()
	{
		if (m_snapInfo.Count > 0)
		{
			m_snapInfo.Clear();
			m_owner.Invalidate();
		}
	}

	private float GetSnapOffset(IEnumerable<float> movingPoints, D2dTimelineControl.SnapOptions options)
	{
		m_snapInfo.Clear();
		m_owner.Invalidate();
		if (options == null)
		{
			options = new D2dTimelineControl.SnapOptions();
		}
		if (options.CheckModifierKeys)
		{
			Keys modifierKeys = Control.ModifierKeys;
			if (s_deactivatorKeys != Keys.None && (modifierKeys & s_deactivatorKeys) == s_deactivatorKeys)
			{
				return 0f;
			}
			if (s_activatorKeys != Keys.None && (modifierKeys & s_activatorKeys) != s_activatorKeys)
			{
				return 0f;
			}
		}
		foreach (float movingPoint in movingPoints)
		{
			m_snapInfo.Add(new SnapOffsetInfo(movingPoint));
		}
		if (m_snapInfo.Count == 0)
		{
			return 0f;
		}
		float tolerance = GdiUtil.InverseTransformVector(m_owner.Transform, s_snapTolerance);
		List<TimelinePath> list = new List<TimelinePath>(D2dTimelineControl.GetObjects<IEvent>(m_owner.Timeline));
		if (m_scrubber != null && options.IncludeScrubber)
		{
			list.Add(new TimelinePath(m_scrubber));
		}
		foreach (TimelinePath item in list)
		{
			if (!options.IncludeSelected && m_owner.Selection.SelectionContains(item))
			{
				continue;
			}
			IEvent obj = (IEvent)item.Last;
			if (options.Filter != null && !options.Filter(obj, options))
			{
				continue;
			}
			Matrix localToWorld = D2dTimelineControl.CalculateLocalToWorld(item);
			GetEventDimensions(obj, localToWorld, out var start, out var length);
			foreach (SnapOffsetInfo item2 in m_snapInfo)
			{
				item2.Update(start, obj, tolerance);
				if (length > 0f)
				{
					item2.Update(start + length, obj, tolerance);
				}
			}
		}
		SnapOffsetInfo.RemoveInvalid(m_snapInfo);
		if (m_snapInfo.Count == 0)
		{
			return 0f;
		}
		SnapOffsetInfo snapOffsetInfo = m_snapInfo[0];
		return snapOffsetInfo.Offset;
	}

	private void GetEventDimensions(IEvent snapToEvent, Matrix localToWorld, out float start, out float length)
	{
		start = GdiUtil.Transform(localToWorld, snapToEvent.Start);
		length = GdiUtil.TransformVector(localToWorld, snapToEvent.Length);
	}

	private void owner_DrawingD2d(object sender, EventArgs e)
	{
		if (m_snapInfo.Count == 0)
		{
			return;
		}
		D2dGraphics d2dGraphics = m_owner.D2dGraphics;
		Rectangle visibleClientRectangle = m_owner.VisibleClientRectangle;
		try
		{
			d2dGraphics.PushAxisAlignedClip(visibleClientRectangle);
			Matrix transform = m_owner.Transform;
			foreach (SnapOffsetInfo item in m_snapInfo)
			{
				float num = GdiUtil.Transform(transform, item.SnapToPoint);
				d2dGraphics.DrawLine(num, visibleClientRectangle.Top, num, visibleClientRectangle.Bottom, s_color, 3f);
			}
		}
		finally
		{
			d2dGraphics.PopAxisAlignedClip();
		}
	}

	private void owner_MouseUp(object sender, MouseEventArgs e)
	{
		CancelSnap();
	}

	private void owner_MouseDown(object sender, MouseEventArgs e)
	{
		CancelSnap();
	}

	private void owner_KeyDown(object sender, KeyEventArgs e)
	{
		if (s_deactivatorKeys != Keys.None && (e.Modifiers & s_deactivatorKeys) == s_deactivatorKeys)
		{
			CancelSnap();
		}
	}
}

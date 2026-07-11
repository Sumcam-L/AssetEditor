using System;

namespace Sce.Atf.VectorMath;

public struct EulerAngleLimits
{
	public const float MaxAngle = (float)Math.PI;

	private Vec3F m_angleLimits;

	private EulerAngleChannels m_channels;

	public Vec3F Angles
	{
		get
		{
			return m_angleLimits;
		}
		set
		{
			m_angleLimits = value;
		}
	}

	public EulerAngleChannels Channels
	{
		get
		{
			return m_channels;
		}
		set
		{
			m_channels = value;
		}
	}

	public float X
	{
		get
		{
			if ((m_channels & EulerAngleChannels.X) != 0)
			{
				return m_angleLimits.X;
			}
			return (float)Math.PI;
		}
		set
		{
			m_angleLimits.X = value;
			if (m_angleLimits.X > (float)Math.PI)
			{
				m_angleLimits.X = (float)Math.PI;
			}
			m_channels |= EulerAngleChannels.X;
		}
	}

	public float Y
	{
		get
		{
			if ((m_channels & EulerAngleChannels.Y) != 0)
			{
				return m_angleLimits.Y;
			}
			return (float)Math.PI;
		}
		set
		{
			m_angleLimits.Y = value;
			if (m_angleLimits.Y > (float)Math.PI)
			{
				m_angleLimits.Y = (float)Math.PI;
			}
			m_channels |= EulerAngleChannels.Y;
		}
	}

	public float Z
	{
		get
		{
			if ((m_channels & EulerAngleChannels.Z) != 0)
			{
				return m_angleLimits.Z;
			}
			return (float)Math.PI;
		}
		set
		{
			m_angleLimits.Z = value;
			if (m_angleLimits.Z > (float)Math.PI)
			{
				m_angleLimits.Z = (float)Math.PI;
			}
			m_channels |= EulerAngleChannels.Z;
		}
	}

	public bool HasRotationX => (m_channels & EulerAngleChannels.X) != 0;

	public bool HasRotationY => (m_channels & EulerAngleChannels.Y) != 0;

	public bool HasRotationZ => (m_channels & EulerAngleChannels.Z) != 0;

	public EulerAngleLimits(Vec3F angleLimits, EulerAngleChannels channels)
	{
		m_angleLimits = angleLimits;
		m_channels = channels;
	}

	public EulerAngleLimits(float[] angles, EulerAngleChannels channels)
	{
		m_angleLimits = new Vec3F(angles);
		m_channels = channels;
	}
}

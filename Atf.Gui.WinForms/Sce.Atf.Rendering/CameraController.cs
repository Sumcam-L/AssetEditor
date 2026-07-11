using System;

namespace Sce.Atf.Rendering;

public abstract class CameraController : Controller, IDisposable
{
	private Camera m_camera;

	private bool m_updating;

	private static bool s_lockOrthographic;

	public Camera Camera
	{
		get
		{
			return m_camera;
		}
		set
		{
			if (m_camera != null)
			{
				m_camera.CameraChanged -= CameraChanged;
			}
			m_camera = value;
			if (m_camera != null)
			{
				Setup(m_camera);
				CameraToController(m_camera);
				ControllerToCamera(m_camera);
				m_camera.CameraChanged += CameraChanged;
			}
		}
	}

	public virtual bool HandlesWASD => false;

	public static bool LockOrthographic
	{
		get
		{
			return s_lockOrthographic;
		}
		set
		{
			s_lockOrthographic = value;
		}
	}

	public void Dispose()
	{
		Camera = null;
	}

	public virtual bool CanHandleCamera(Camera camera)
	{
		return true;
	}

	protected void ControllerToCamera()
	{
		try
		{
			m_updating = true;
			ControllerToCamera(m_camera);
		}
		finally
		{
			m_updating = false;
		}
	}

	protected virtual void Setup(Camera camera)
	{
	}

	protected virtual void CameraToController(Camera camera)
	{
	}

	protected virtual void ControllerToCamera(Camera camera)
	{
	}

	private void CameraChanged(object sender, EventArgs e)
	{
		if (!m_updating)
		{
			CameraToController(m_camera);
		}
	}
}

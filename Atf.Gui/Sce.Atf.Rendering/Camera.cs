using System;
using System.Globalization;
using System.Xml;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public class Camera
{
	private ViewTypes m_viewType = ViewTypes.Perspective;

	private Vec3F m_eye;

	private Vec3F m_lookAtPoint;

	private Vec3F m_lookAt;

	private Vec3F m_up;

	private Vec3F m_right;

	private float m_lookAtDistance;

	private float m_focusRadius = 1f;

	private Matrix4F m_axisSystem = new Matrix4F();

	private float m_aspectRatio;

	private float m_yFov;

	private float m_perspectiveNearZ = 0.01f;

	private float m_orthographicNearZ = -10000f;

	private ProjectionType m_projectionType;

	private readonly Frustum m_frustum = new Frustum();

	private bool m_changingCamera;

	public Matrix4F AxisSystem
	{
		get
		{
			return m_axisSystem;
		}
		set
		{
			m_axisSystem = value;
			OnCameraChanged(EventArgs.Empty);
		}
	}

	public Matrix4F InverseAxisSystem
	{
		get
		{
			Matrix4F matrix4F = new Matrix4F();
			matrix4F.Transpose(m_axisSystem);
			return matrix4F;
		}
	}

	public ProjectionType ProjectionType => m_projectionType;

	public ViewTypes ViewType
	{
		get
		{
			return m_viewType;
		}
		set
		{
			if (m_viewType != value)
			{
				m_viewType = value;
				if (m_viewType == ViewTypes.Perspective)
				{
					SetPerspective(m_yFov, m_aspectRatio, m_perspectiveNearZ, m_frustum.FarZ);
					return;
				}
				GetViewVectors(out var lookAt, out var up);
				float distanceFromLookAt = DistanceFromLookAt;
				Set(m_lookAtPoint - lookAt * distanceFromLookAt, m_lookAtPoint, up);
				SetOrthographic(distanceFromLookAt);
			}
		}
	}

	public string ViewTypeName => m_viewType switch
	{
		ViewTypes.Back => "Back".Localize("the back side of the model that is being viewed"), 
		ViewTypes.Bottom => "Bottom".Localize("the bottom side of the model that is being viewed"), 
		ViewTypes.Front => "Front".Localize("the front side of the model that is being viewed"), 
		ViewTypes.Left => "Left".Localize("the left side of the model that is being viewed"), 
		ViewTypes.Right => "Right".Localize("the right side of the model that is being viewed"), 
		ViewTypes.Top => "Top".Localize("the top side of the model that is being viewed"), 
		_ => "Perspective".Localize("a 3D perspective view of the model"), 
	};

	public Frustum Frustum => m_frustum;

	public Matrix4F ProjectionMatrix
	{
		get
		{
			if (Frustum.IsOrtho)
			{
				float right = Frustum.Right;
				float left = Frustum.Left;
				float top = Frustum.Top;
				float bottom = Frustum.Bottom;
				float near = Frustum.Near;
				float far = Frustum.Far;
				float m = (0f - (right + left)) / (right - left);
				float m2 = (0f - (top + bottom)) / (top - bottom);
				float m3 = (0f - (far + near)) / (far - near);
				return new Matrix4F(2f / (right - left), 0f, 0f, 0f, 0f, 2f / (top - bottom), 0f, 0f, 0f, 0f, -2f / (far - near), 0f, m, m2, m3, 1f);
			}
			float num = 1f / (float)Math.Tan(0.5f * Frustum.FovY);
			float near2 = Frustum.Near;
			float far2 = Frustum.Far;
			return new Matrix4F(num / m_aspectRatio, 0f, 0f, 0f, 0f, num, 0f, 0f, 0f, 0f, (far2 + near2) / (near2 - far2), -1f, 0f, 0f, 2f * far2 * near2 / (near2 - far2), 0f);
		}
	}

	public Matrix4F ViewMatrix
	{
		get
		{
			float m = 0f - Vec3F.Dot(m_right, m_eye);
			float m2 = 0f - Vec3F.Dot(m_up, m_eye);
			float m3 = Vec3F.Dot(m_lookAt, m_eye);
			Matrix4F matrix4F = new Matrix4F(m_right.X, m_up.X, 0f - m_lookAt.X, 0f, m_right.Y, m_up.Y, 0f - m_lookAt.Y, 0f, m_right.Z, m_up.Z, 0f - m_lookAt.Z, 0f, m, m2, m3, 1f);
			matrix4F.Mul(AxisSystem, matrix4F);
			return matrix4F;
		}
	}

	public Vec3F Eye => m_eye;

	public Vec3F WorldEye
	{
		get
		{
			InverseAxisSystem.Transform(m_eye, out var result);
			return result;
		}
	}

	public Vec3F LookAtPoint => m_lookAtPoint;

	public Vec3F WorldLookAtPoint
	{
		get
		{
			InverseAxisSystem.Transform(m_lookAtPoint, out var result);
			return result;
		}
	}

	public Vec3F LookAt => m_lookAt;

	public Vec3F WorldLookAt
	{
		get
		{
			InverseAxisSystem.TransformVector(m_lookAt, out var result);
			return result;
		}
	}

	public Vec3F Up => m_up;

	public Vec3F WorldUp
	{
		get
		{
			InverseAxisSystem.TransformVector(m_up, out var result);
			return result;
		}
	}

	public Vec3F Right => m_right;

	public Vec3F WorldRight
	{
		get
		{
			InverseAxisSystem.TransformVector(m_right, out var result);
			return result;
		}
	}

	public float DistanceFromLookAt => m_lookAtDistance;

	public float FocusRadius
	{
		get
		{
			return m_focusRadius;
		}
		set
		{
			m_focusRadius = value;
		}
	}

	public float FarZ
	{
		get
		{
			return m_frustum.FarZ;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_frustum.FarZ = value;
			OnCameraChanged(EventArgs.Empty);
		}
	}

	public float NearZ => m_frustum.NearZ;

	public float OrthographicNearZ
	{
		get
		{
			return m_orthographicNearZ;
		}
		set
		{
			m_orthographicNearZ = value;
			if (m_projectionType != ProjectionType.Perspective)
			{
				m_frustum.NearZ = value;
				OnCameraChanged(EventArgs.Empty);
			}
		}
	}

	public float PerspectiveNearZ
	{
		get
		{
			return m_perspectiveNearZ;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_perspectiveNearZ = value;
			if (m_projectionType == ProjectionType.Perspective)
			{
				m_frustum.NearZ = value;
				OnCameraChanged(EventArgs.Empty);
			}
		}
	}

	public float YFov => m_yFov;

	public float Aspect
	{
		get
		{
			return m_aspectRatio;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_aspectRatio = value;
			if (m_projectionType == ProjectionType.Orthographic)
			{
				float num = m_frustum.Top - m_frustum.Bottom;
				m_frustum.SetOrtho(num * m_aspectRatio / 2f, (0f - num) * m_aspectRatio / 2f, num / 2f, (0f - num) / 2f, m_frustum.NearZ, m_frustum.FarZ);
			}
			else
			{
				m_frustum.SetPerspective(m_yFov, m_aspectRatio, m_frustum.NearZ, m_frustum.FarZ);
			}
			OnCameraChanged(EventArgs.Empty);
		}
	}

	public event EventHandler CameraChanged;

	public Camera()
	{
		Set(new Vec3F(1f, 1f, 1f), new Vec3F(0f, 0f, 0f), new Vec3F(0f, 1f, 0f));
		SetPerspective((float)Math.PI / 4f, 1f, 0.01f, 2048f);
	}

	public void Init(Camera source)
	{
		source.GetState(out var perspective, out var eye, out var lookAtPoint, out var upVector, out var yFov, out var nearZ, out var farZ, out var focusRadius);
		SetState(perspective, eye, lookAtPoint, upVector, yFov, nearZ, farZ, focusRadius);
	}

	public void GetState(out ViewTypes perspective, out Vec3F eye, out Vec3F lookAtPoint, out Vec3F upVector, out float yFov, out float nearZ, out float farZ, out float focusRadius)
	{
		perspective = m_viewType;
		eye = Eye;
		lookAtPoint = LookAtPoint;
		upVector = Up;
		yFov = YFov;
		nearZ = NearZ;
		farZ = FarZ;
		focusRadius = FocusRadius;
	}

	public void SetState(ViewTypes viewType, Vec3F eye, Vec3F lookAtPoint, Vec3F upVector, float yFov, float nearZ, float farZ, float focusRadius)
	{
		m_changingCamera = true;
		try
		{
			m_viewType = viewType;
			Set(eye, lookAtPoint, upVector);
			if (viewType == ViewTypes.Perspective)
			{
				SetPerspective(yFov, m_aspectRatio, nearZ, farZ);
			}
			else
			{
				float distanceFromLookAt = DistanceFromLookAt;
				SetOrthographic(distanceFromLookAt * m_aspectRatio / 2f, (0f - distanceFromLookAt) * m_aspectRatio / 2f, distanceFromLookAt / 2f, (0f - distanceFromLookAt) / 2f, nearZ, farZ);
			}
		}
		finally
		{
			m_changingCamera = false;
		}
		OnCameraChanged(EventArgs.Empty);
	}

	public void GetState(XmlElement root, XmlDocument xmlDoc)
	{
		GetState(out var perspective, out var eye, out var lookAtPoint, out var upVector, out var yFov, out var nearZ, out var farZ, out var focusRadius);
		root.SetAttribute("viewType", $"{(int)perspective:x}");
		root.SetAttribute("eye", eye.ToString());
		root.SetAttribute("lookAtPoint", lookAtPoint.ToString());
		root.SetAttribute("upVector", upVector.ToString());
		root.SetAttribute("yFov", yFov.ToString());
		root.SetAttribute("nearZ", nearZ.ToString());
		root.SetAttribute("farZ", farZ.ToString());
		root.SetAttribute("focusRadius", focusRadius.ToString());
	}

	public void SetState(XmlElement root, XmlDocument xmlDoc)
	{
		string attribute = root.GetAttribute("viewType");
		ViewTypes viewType = (ViewTypes)int.Parse(attribute, NumberStyles.AllowHexSpecifier);
		attribute = root.GetAttribute("eye");
		Vec3F eye = Vec3F.Parse(attribute);
		attribute = root.GetAttribute("lookAtPoint");
		Vec3F lookAtPoint = Vec3F.Parse(attribute);
		attribute = root.GetAttribute("upVector");
		Vec3F upVector = Vec3F.Parse(attribute);
		attribute = root.GetAttribute("yFov");
		float yFov = float.Parse(attribute);
		attribute = root.GetAttribute("nearZ");
		float nearZ = float.Parse(attribute);
		attribute = root.GetAttribute("farZ");
		float farZ = float.Parse(attribute);
		attribute = root.GetAttribute("focusRadius");
		float focusRadius = float.Parse(attribute);
		SetState(viewType, eye, lookAtPoint, upVector, yFov, nearZ, farZ, focusRadius);
	}

	public void SetPerspective(float yFov, float aspectRatio, float nearZ, float farZ)
	{
		if (yFov <= 0f || aspectRatio <= 0f || nearZ <= 0f || farZ <= 0f)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_perspectiveNearZ = nearZ;
		m_projectionType = ProjectionType.Perspective;
		m_yFov = yFov;
		m_aspectRatio = aspectRatio;
		m_frustum.SetPerspective(yFov, aspectRatio, nearZ, farZ);
		OnCameraChanged(EventArgs.Empty);
	}

	public void SetOrthographic(float right, float left, float top, float bottom, float near, float far)
	{
		m_orthographicNearZ = near;
		m_frustum.SetOrtho(right, left, top, bottom, near, far);
		m_projectionType = ProjectionType.Orthographic;
		OnCameraChanged(EventArgs.Empty);
	}

	public void SetOrthographic(float d)
	{
		SetOrthographic(d * m_aspectRatio / 2f, (0f - d) * m_aspectRatio / 2f, d / 2f, (0f - d) / 2f, m_orthographicNearZ, m_frustum.FarZ);
	}

	public void Set(Vec3F eye, Vec3F lookAtPoint, Vec3F up)
	{
		m_eye = eye;
		m_lookAtPoint = lookAtPoint;
		m_up = up;
		UpdateGeometry();
	}

	public void Set(Vec3F eye)
	{
		Vec3F vec3F = eye - m_eye;
		m_eye = eye;
		m_lookAtPoint += vec3F;
		UpdateGeometry();
	}

	public void ZoomOnSphere(Sphere3F sphere)
	{
		float num = sphere.Radius;
		if (num == 0f)
		{
			num = 1f;
		}
		if (num > FarZ)
		{
			num = FarZ;
		}
		else if (num < NearZ)
		{
			num = NearZ;
		}
		m_focusRadius = num;
		GetViewVectors(out var lookAt, out var up);
		AxisSystem.Transform(sphere.Center, out var result);
		Vec3F eye = result - lookAt * num;
		Set(eye, result, up);
		if (ViewType == ViewTypes.Perspective)
		{
			float val = Math.Abs(sphere.Radius) * 0.1f;
			val = Math.Max(val, 0.001f);
			PerspectiveNearZ = val;
		}
	}

	public Ray3F CreateRay(float x, float y)
	{
		Ray3F result = default(Ray3F);
		if (Frustum.IsOrtho)
		{
			float num = Frustum.Right - Frustum.Left;
			float num2 = Frustum.Top - Frustum.Bottom;
			result.Origin = new Vec3F(x * num, y * num2, 0f - NearZ);
			result.Direction = new Vec3F(0f, 0f, -1f);
		}
		else
		{
			float num2 = Frustum.Far * (float)Math.Tan(Frustum.FovY / 2f) * 2f;
			float num = Frustum.Far * (float)Math.Tan(Frustum.FovX / 2f) * 2f;
			result.Origin = new Vec3F(0f, 0f, 0f);
			result.Direction = new Vec3F(x * num, y * num2, 0f - Frustum.Far);
			float length = result.Direction.Length;
			result.Direction /= length;
		}
		return result;
	}

	protected void OnCameraChanged(EventArgs e)
	{
		if (!m_changingCamera)
		{
			this.CameraChanged?.Invoke(this, e);
		}
	}

	private void UpdateGeometry()
	{
		m_lookAt = m_lookAtPoint - m_eye;
		m_lookAt.Normalize();
		m_right = Vec3F.Cross(m_lookAt, m_up);
		m_right.Normalize();
		m_up = Vec3F.Cross(m_right, m_lookAt);
		m_lookAtDistance = Vec3F.Distance(m_eye, m_lookAtPoint);
		if (ProjectionType == ProjectionType.Orthographic)
		{
			SetOrthographic(m_lookAtDistance);
		}
		else
		{
			OnCameraChanged(EventArgs.Empty);
		}
	}

	private void GetViewVectors(out Vec3F lookAt, out Vec3F up)
	{
		switch (m_viewType)
		{
		case ViewTypes.Front:
			lookAt = new Vec3F(0f, 0f, -1f);
			up = new Vec3F(0f, 1f, 0f);
			break;
		case ViewTypes.Back:
			lookAt = new Vec3F(0f, 0f, 1f);
			up = new Vec3F(0f, 1f, 0f);
			break;
		case ViewTypes.Top:
			lookAt = new Vec3F(0f, -1f, 0f);
			up = new Vec3F(0f, 0f, -1f);
			break;
		case ViewTypes.Bottom:
			lookAt = new Vec3F(0f, 1f, 0f);
			up = new Vec3F(0f, 0f, -1f);
			break;
		case ViewTypes.Left:
			lookAt = new Vec3F(1f, 0f, 0f);
			up = new Vec3F(0f, 1f, 0f);
			break;
		case ViewTypes.Right:
			lookAt = new Vec3F(-1f, 0f, 0f);
			up = new Vec3F(0f, 1f, 0f);
			break;
		default:
			lookAt = m_lookAt;
			up = m_up;
			break;
		}
	}
}

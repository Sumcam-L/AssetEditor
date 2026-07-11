using System;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public class RenderState : ICloneable
{
	private RenderMode m_renderMode;

	private RenderMode m_inheritState;

	private RenderMode m_overrideChildState;

	private Vec4F m_wireframeColor = new Vec4F(0f, 0f, 0f, 1f);

	private Vec4F m_diffuseColor = new Vec4F(0f, 0f, 0f, 1f);

	private Vec4F m_emissionColor = new Vec4F(0f, 0f, 0f, 1f);

	private Vec4F m_ambientColor = new Vec4F(0f, 0f, 0f, 1f);

	private Vec4F m_specularColor = new Vec4F(0f, 0f, 0f, 1f);

	private float m_shininess;

	private int m_textureName;

	private int m_lineThickness = 1;

	public RenderMode OverrideChildState
	{
		get
		{
			return m_overrideChildState;
		}
		set
		{
			m_overrideChildState = value;
		}
	}

	public RenderMode RenderMode
	{
		get
		{
			return m_renderMode;
		}
		set
		{
			m_renderMode = value;
		}
	}

	public RenderMode InheritState
	{
		get
		{
			return m_inheritState;
		}
		set
		{
			m_inheritState = value;
		}
	}

	public Vec4F SolidColor
	{
		get
		{
			return m_diffuseColor;
		}
		set
		{
			m_diffuseColor = value;
		}
	}

	public Vec4F WireframeColor
	{
		get
		{
			return m_wireframeColor;
		}
		set
		{
			m_wireframeColor = value;
		}
	}

	public Vec4F DiffuseColor
	{
		get
		{
			return m_diffuseColor;
		}
		set
		{
			m_diffuseColor = value;
		}
	}

	public Vec4F EmissionColor
	{
		get
		{
			return m_emissionColor;
		}
		set
		{
			m_emissionColor = value;
		}
	}

	public Vec4F AmbientColor
	{
		get
		{
			return m_ambientColor;
		}
		set
		{
			m_ambientColor = value;
		}
	}

	public Vec4F SpecularColor
	{
		get
		{
			return m_specularColor;
		}
		set
		{
			m_specularColor = value;
		}
	}

	public float Shininess
	{
		get
		{
			return m_shininess;
		}
		set
		{
			m_shininess = value;
		}
	}

	public int TextureName
	{
		get
		{
			return m_textureName;
		}
		set
		{
			m_textureName = value;
		}
	}

	public int LineThickness
	{
		get
		{
			return m_lineThickness;
		}
		set
		{
			m_lineThickness = value;
		}
	}

	public RenderState()
	{
	}

	public RenderState(RenderState other)
	{
		Init(other);
	}

	public void Init(RenderState other)
	{
		m_overrideChildState = other.m_overrideChildState;
		m_renderMode = other.m_renderMode;
		m_inheritState = other.m_inheritState;
		m_wireframeColor = other.m_wireframeColor;
		m_diffuseColor = other.m_diffuseColor;
		m_emissionColor = other.m_emissionColor;
		m_ambientColor = other.m_ambientColor;
		m_specularColor = other.m_specularColor;
		m_shininess = other.m_shininess;
		m_textureName = other.m_textureName;
		m_lineThickness = other.m_lineThickness;
	}

	public virtual void ComposeFrom(RenderState parent)
	{
		RenderMode renderMode = ((~parent.OverrideChildState & ~InheritState) | OverrideChildState) & RenderMode;
		RenderMode renderMode2 = parent.OverrideChildState & ~OverrideChildState;
		RenderMode renderMode3 = (renderMode2 | InheritState) & parent.RenderMode;
		RenderMode = renderMode | renderMode3;
		bool flag = (renderMode2 & RenderMode.SolidColor) != 0 || (InheritState & RenderMode.SolidColor) != 0;
		bool flag2 = (renderMode2 & RenderMode.WireframeColor) != 0 || (InheritState & RenderMode.WireframeColor) != 0;
		bool flag3 = (renderMode2 & RenderMode.WireframeThickness) != 0 || (InheritState & RenderMode.WireframeThickness) != 0;
		bool flag4 = (parent.RenderMode & RenderMode.Textured) != 0;
		if (flag)
		{
			SolidColor = parent.SolidColor;
		}
		if (flag)
		{
			SpecularColor = parent.SpecularColor;
		}
		if (flag2)
		{
			WireframeColor = parent.WireframeColor;
		}
		if (flag3)
		{
			LineThickness = parent.LineThickness;
		}
		if (flag4 && m_textureName == 0)
		{
			m_textureName = parent.TextureName;
		}
	}

	public virtual void CommitAllBitsToGuardian(RenderStateGuardian rsg)
	{
		for (int i = 0; 1 << i <= 1024; i++)
		{
			rsg.SetRenderStateByIndex(i, this);
		}
	}

	public virtual object Clone()
	{
		return new RenderState(this);
	}
}

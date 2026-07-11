using System.ComponentModel.Composition;
using System.Drawing;

namespace Sce.Atf.Rendering.Dom;

[Export(typeof(Scene))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Scene : SceneNode
{
	private Color m_backgroundColor = Color.LightGray;

	public Color BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			m_backgroundColor = value;
		}
	}

	public Scene()
		: base(null)
	{
	}
}

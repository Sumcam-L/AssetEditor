using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class ElementType : ICircuitElementType
{
	public class Pin : ICircuitPin, IEdgeRoute
	{
		private string m_name;

		private string m_typeName;

		private int m_index;

		public string Name => m_name;

		public string TypeName => m_typeName;

		public int Index => m_index;

		public bool AllowFanIn => false;

		public bool AllowFanOut => true;

		public Pin(string name, string typeName, int index)
		{
			m_name = name;
			m_typeName = typeName;
			m_index = index;
		}
	}

	private string m_name;

	private ICircuitPin[] m_inputPins;

	private ICircuitPin[] m_outputPins;

	private Image m_image;

	private bool m_isConnector;

	public bool IsConnector => m_isConnector;

	public string Name => m_name;

	public Size InteriorSize => (m_image != null) ? new Size(32, 32) : default(Size);

	public Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			m_image = value;
		}
	}

	public IList<ICircuitPin> Inputs => m_inputPins;

	public IList<ICircuitPin> Outputs => m_outputPins;

	public ElementType()
	{
	}

	public ElementType(string name, bool isConnector, Size size, Image image, ICircuitPin[] inputPins, ICircuitPin[] outputPins)
	{
		Set(name, isConnector, size, image, inputPins, outputPins);
	}

	public void Set(string name, bool isConnector, Size size, Image image, ICircuitPin[] inputPins, ICircuitPin[] outputPins)
	{
		m_name = name;
		m_isConnector = isConnector;
		m_image = image;
		m_inputPins = inputPins.OrderBy((ICircuitPin n) => n.Index).ToArray();
		m_outputPins = outputPins.OrderBy((ICircuitPin n) => n.Index).ToArray();
	}
}

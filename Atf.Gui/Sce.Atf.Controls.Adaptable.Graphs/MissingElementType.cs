using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class MissingElementType : ICircuitElementType
{
	private class MissingPinList : IList<ICircuitPin>, ICollection<ICircuitPin>, IEnumerable<ICircuitPin>, IEnumerable
	{
		private MissingPin m_missingPin;

		private int m_count;

		public int Count => m_count;

		public bool IsReadOnly { get; private set; }

		public ICircuitPin this[int index]
		{
			get
			{
				if (index + 1 > m_count)
				{
					m_count = index + 1;
				}
				return m_missingPin;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public MissingPinList(string name)
		{
			m_missingPin = new MissingPin(name, "MissingPin", 0);
		}

		public IEnumerator<ICircuitPin> GetEnumerator()
		{
			int i = 0;
			while (i < Count)
			{
				yield return m_missingPin;
				int num = i + 1;
				i = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ICircuitPin item)
		{
		}

		public void Clear()
		{
		}

		public bool Contains(ICircuitPin item)
		{
			return false;
		}

		public void CopyTo(ICircuitPin[] array, int arrayIndex)
		{
		}

		public bool Remove(ICircuitPin item)
		{
			return false;
		}

		public int IndexOf(ICircuitPin item)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, ICircuitPin item)
		{
		}

		public void RemoveAt(int index)
		{
		}
	}

	public class MissingPin : ICircuitPin, IEdgeRoute
	{
		private string m_name;

		private string m_typeName;

		private int m_index;

		public string Name => m_name;

		public string TypeName => m_typeName;

		public int Index => m_index;

		public bool AllowFanIn => true;

		public bool AllowFanOut => true;

		public MissingPin(string name, string typeName, int index)
		{
			m_name = name;
			m_typeName = typeName;
			m_index = index;
		}
	}

	private MissingPinList m_inputs = new MissingPinList("In");

	private MissingPinList m_outputs = new MissingPinList("Out");

	public string Name { get; private set; }

	public Size InteriorSize => (Image != null) ? new Size(Image.Width + 18, Image.Height + 18) : default(Size);

	public Image Image { get; set; }

	public IList<ICircuitPin> Inputs => m_inputs;

	public IList<ICircuitPin> Outputs => m_outputs;

	public MissingElementType(string name)
	{
		Name = name;
	}
}

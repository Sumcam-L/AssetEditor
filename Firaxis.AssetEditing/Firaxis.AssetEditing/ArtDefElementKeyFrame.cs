using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefElementKeyFrame : IKeyFrame, IFieldContainerAdapter
{
	private ArtDefElementAdapter m_adapter;

	private FloatFieldValueAdapter m_fieldAdapter;

	private const string m_timeFieldName = "Time";

	public IArtDefElement ArtDefElement => m_adapter.ArtDefElement;

	public ArtDefElementAdapter ArtDefElementAdapter => m_adapter;

	public float Time
	{
		get
		{
			BindToTimeField();
			return m_fieldAdapter.ParameterValue;
		}
		set
		{
			BindToTimeField();
			if (m_fieldAdapter.ParameterValue != value)
			{
				m_fieldAdapter.ParameterValue = value;
				RaiseTimeChangedEvent();
			}
		}
	}

	public IList<IFieldValueAdapter> Fields => m_adapter.Fields;

	public event EventHandler TimeChanged;

	public event EventHandler<DomNode> ValueChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public ArtDefElementKeyFrame(ArtDefElementAdapter nodeAdapter)
	{
		m_adapter = nodeAdapter;
	}

	protected void RaiseTimeChangedEvent()
	{
		this.TimeChanged?.Invoke(this, new EventArgs());
	}

	protected void RaiseValueChangedEvent(DomNode dn)
	{
		this.ValueChanged?.Invoke(this, dn);
	}

	private void BindToTimeField()
	{
		if (m_fieldAdapter == null)
		{
			m_fieldAdapter = m_adapter.Fields.FirstOrDefault((IFieldValueAdapter fa) => fa.Name == "Time") as FloatFieldValueAdapter;
			if (m_fieldAdapter == null || !typeof(IFloatValue).IsAssignableFrom(m_fieldAdapter.Parameter.DefaultValue.GetType()))
			{
				throw new InvalidOperationException("KeyFrame editor requires elements with a \"Time\" float value");
			}
			m_fieldAdapter.DomNode.AttributeChanged += TimeDomNode_AttributeChanged;
			RegisterValueChangeHandlers();
		}
	}

	private void RegisterValueChangeHandlers()
	{
		foreach (IFieldValueAdapter field in m_adapter.Fields)
		{
			if (field != m_fieldAdapter)
			{
				field.DomNode.AttributeChanged += ValueDomNode_AttributeChanged;
			}
		}
	}

	private void TimeDomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.FloatFieldValueType.ValueAttribute)
		{
			RaiseTimeChangedEvent();
		}
	}

	private void ValueDomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		RaiseValueChangedEvent(e.DomNode);
	}
}

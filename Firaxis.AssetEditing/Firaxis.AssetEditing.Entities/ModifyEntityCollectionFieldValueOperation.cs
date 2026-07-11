using System.Linq;
using Firaxis.ATF;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing.Entities;

public class ModifyEntityCollectionFieldValueOperation : TransactionContext.Operation
{
	private readonly BaseInstanceEntityDocument m_document;

	private readonly string m_fieldName;

	private readonly int m_index;

	private readonly object m_newValue;

	private readonly object m_oldValue;

	public ModifyEntityCollectionFieldValueOperation(BaseInstanceEntityDocument doc, string fieldName, int index, object oldValue, object newValue)
	{
		m_document = doc;
		m_fieldName = fieldName;
		m_index = index;
		m_oldValue = oldValue;
		m_newValue = newValue;
	}

	public override void Do()
	{
		(m_document.As<InstanceEntityAdapter>().CookParameterSet.Fields.First((IFieldValueAdapter fld) => fld.Name == m_fieldName) as CollectionFieldValueAdapter).Values[m_index].ValueDataAsObject = m_newValue;
	}

	public override void Undo()
	{
		(m_document.As<InstanceEntityAdapter>().CookParameterSet.Fields.First((IFieldValueAdapter fld) => fld.Name == m_fieldName) as CollectionFieldValueAdapter).Values[m_index].ValueDataAsObject = m_oldValue;
	}
}

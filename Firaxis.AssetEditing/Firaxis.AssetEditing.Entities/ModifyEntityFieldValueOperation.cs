using System.Linq;
using Firaxis.ATF;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing.Entities;

public class ModifyEntityFieldValueOperation : TransactionContext.Operation
{
	private readonly BaseInstanceEntityDocument m_document;

	private readonly string m_fieldName;

	private readonly object m_newValue;

	private readonly object m_oldValue;

	public ModifyEntityFieldValueOperation(BaseInstanceEntityDocument doc, string fieldName, object oldValue, object newValue)
	{
		m_document = doc;
		m_fieldName = fieldName;
		m_oldValue = oldValue;
		m_newValue = newValue;
	}

	public override void Do()
	{
		m_document.As<InstanceEntityAdapter>().CookParameterSet.Fields.First((IFieldValueAdapter fld) => fld.Name == m_fieldName).ValueDataAsObject = m_newValue;
	}

	public override void Undo()
	{
		m_document.As<InstanceEntityAdapter>().CookParameterSet.Fields.First((IFieldValueAdapter fld) => fld.Name == m_fieldName).ValueDataAsObject = m_oldValue;
	}
}

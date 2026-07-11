using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public interface IFieldValueAdapter : INamedAdapter
{
	object DefaultDataAsObject { get; }

	DomNode DomNode { get; }

	IParameter Parameter { get; }

	IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors { get; }

	IValue Value { get; }

	object ValueDataAsObject { get; set; }

	void AddNativeField(IValueSet valSet, IParameter valParam);

	void AssignDefaultValue();

	void InitializeEditor(Func<bool> readOnlyFunctor);

	bool RequiresUpdate(IValue newValue);

	void UpdateDomFromNative(IValue val);

	void UpdateNativeFromDom();

	void CopyValue(IValue val);
}

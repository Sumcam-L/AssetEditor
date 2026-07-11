using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IOscService))]
[Export(typeof(LemurOscService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LemurOscService : OscService
{
	private class ChildListFloatingPointArrayDesc : ChildAttributePropertyDescriptor
	{
		private readonly int m_coordinateIndex;

		private readonly AttributePropertyDescriptor m_childAttributeDesc;

		private readonly ChildInfo m_childInfo;

		public ChildListFloatingPointArrayDesc(ChildInfo childInfo, AttributePropertyDescriptor childAttributeDesc, int coordinateIndex)
			: base(childAttributeDesc.Name, childAttributeDesc.AttributeInfo, childInfo, childAttributeDesc.Category, childAttributeDesc.Description, childAttributeDesc.IsReadOnly, null, childAttributeDesc.Converter)
		{
			m_childInfo = childInfo;
			m_childAttributeDesc = childAttributeDesc;
			m_coordinateIndex = coordinateIndex;
		}

		public override object GetValue(object component)
		{
			DomNode domNode = component.As<DomNode>();
			if (domNode != null)
			{
				IList<DomNode> childList = domNode.GetChildList(m_childInfo);
				float[] array = new float[childList.Count];
				for (int i = 0; i < childList.Count; i++)
				{
					DomNode component2 = childList[i];
					float[] array2 = (float[])m_childAttributeDesc.GetValue(component2);
					array[i] = array2[m_coordinateIndex];
				}
				return array;
			}
			return null;
		}

		public override void SetValue(object component, object value)
		{
			DomNode domNode = component.As<DomNode>();
			if (domNode == null)
			{
				return;
			}
			IList<DomNode> childList = domNode.GetChildList(m_childInfo);
			float[] array = (float[])value;
			for (int i = 0; i < childList.Count; i++)
			{
				DomNode component2 = childList[i];
				float[] array2 = (float[])m_childAttributeDesc.GetValue(component2);
				if (i >= array.Length)
				{
					break;
				}
				float[] array3 = (float[])array2.Clone();
				array3[m_coordinateIndex] = array[i];
				m_childAttributeDesc.SetValue(component2, array3);
			}
		}
	}

	public string Add2DPointProperty(ChildInfo childInfo, AttributePropertyDescriptor childAttributeDesc, string oscAddress)
	{
		oscAddress = OscServices.FixPropertyAddress(oscAddress);
		ChildListFloatingPointArrayDesc descriptor = new ChildListFloatingPointArrayDesc(childInfo, childAttributeDesc, 0);
		AddPropertyAddress(descriptor, oscAddress + "/x");
		ChildListFloatingPointArrayDesc descriptor2 = new ChildListFloatingPointArrayDesc(childInfo, childAttributeDesc, 1);
		AddPropertyAddress(descriptor2, oscAddress + "/y");
		return oscAddress;
	}

	protected override object PrepareDataForSending(object data, object common, OscAddressInfo info)
	{
		if (data is bool)
		{
			return ((bool)data) ? 1f : 0f;
		}
		return base.PrepareDataForSending(data, common, info);
	}

	protected override IEnumerable<Tuple<string, object>> GetCustomDataToSend(object common)
	{
		string interfaceName = GetLemurInterfaceName(common);
		if (interfaceName != null)
		{
			yield return new Tuple<string, object>("/interface", interfaceName);
		}
	}

	protected virtual string GetLemurInterfaceName(object common)
	{
		return null;
	}

	protected override void SendSynchronously(IList<Tuple<string, object>> addressesAndData)
	{
		int num = addressesAndData.Count;
		while (num > 0)
		{
			int num2 = ((num <= 16) ? num : 16);
			SendPacket(addressesAndData, addressesAndData.Count - num, num2);
			num -= num2;
		}
	}
}

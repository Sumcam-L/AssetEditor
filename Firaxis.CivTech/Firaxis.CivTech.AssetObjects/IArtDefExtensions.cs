using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.Collections;

namespace Firaxis.CivTech.AssetObjects;

public static class IArtDefExtensions
{
	public static void PruneEmptyElements(this IArtDefElement artDefElement)
	{
		artDefElement.Children.ForEach(delegate(IArtDefCollection childCollection)
		{
			childCollection.PruneEmptyElements();
		});
	}

	public static void PruneEmptyElements(this IArtDefCollection artDefCollection)
	{
		while (artDefCollection.Elements.FirstOrDefault((IArtDefElement element) => string.IsNullOrEmpty(element.Name)) != null)
		{
			artDefCollection.RemoveElement(string.Empty);
		}
		artDefCollection.Elements.ForEach(delegate(IArtDefElement element)
		{
			element.PruneEmptyElements();
		});
	}

	public static void PruneEmptyElements(this IArtDef artDef)
	{
		artDef.RootCollections.ForEach(delegate(IArtDefCollection collection)
		{
			collection.PruneEmptyElements();
		});
	}

	public static IEnumerable<IArtDefElement> GetAllElements(this IArtDef artDef)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<IArtDefElement>();
		}
		List<IArtDefElement> artDefElements = new List<IArtDefElement>();
		artDef.RootCollections.ForEach(delegate(IArtDefCollection collection)
		{
			artDefElements.AddRange(collection.GetAllElements());
		});
		return artDefElements;
	}

	public static IEnumerable<IArtDefElement> GetAllElements(this IArtDefCollection collection)
	{
		List<IArtDefElement> artDefElements = new List<IArtDefElement>();
		collection?.Elements.ForEach(delegate(IArtDefElement element)
		{
			artDefElements.AddRange(element.GetAllElements());
		});
		return artDefElements;
	}

	public static IEnumerable<IArtDefElement> GetAllElements(this IArtDefElement element)
	{
		List<IArtDefElement> artDefElements = new List<IArtDefElement>();
		if (element != null)
		{
			artDefElements.Add(element);
			element.Children.ForEach(delegate(IArtDefCollection child)
			{
				artDefElements.AddRange(child.GetAllElements());
			});
		}
		return artDefElements;
	}

	public static void VisitAllElements(this IArtDef artDef, Action<IArtDefElement> act)
	{
		if (artDef == null)
		{
			return;
		}
		foreach (IArtDefCollection rootCollection in artDef.RootCollections)
		{
			rootCollection.VisitAllElements(act);
		}
	}

	public static void VisitAllElements(this IArtDefCollection artDefCollection, Action<IArtDefElement> act)
	{
		if (artDefCollection == null)
		{
			return;
		}
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			element.VisitAllElements(act);
		}
	}

	public static void VisitAllElements(this IArtDefElement artDefElement, Action<IArtDefElement> act)
	{
		if (artDefElement == null)
		{
			return;
		}
		act(artDefElement);
		foreach (IArtDefCollection child in artDefElement.Children)
		{
			child.VisitAllElements(act);
		}
	}

	public static void VisitAllCollections(this IArtDef artDef, Action<IArtDefCollection> act)
	{
		if (artDef == null)
		{
			return;
		}
		foreach (IArtDefCollection rootCollection in artDef.RootCollections)
		{
			rootCollection.VisitAllCollections(act);
		}
	}

	public static void VisitAllCollections(this IArtDefCollection artDefCollection, Action<IArtDefCollection> act)
	{
		if (artDefCollection == null)
		{
			return;
		}
		act(artDefCollection);
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			element.VisitAllCollections(act);
		}
	}

	public static void VisitAllCollections(this IArtDefElement artDefElement, Action<IArtDefCollection> act)
	{
		if (artDefElement == null)
		{
			return;
		}
		foreach (IArtDefCollection child in artDefElement.Children)
		{
			child.VisitAllCollections(act);
		}
	}

	public static IEnumerable<IArtDefCollection> GetAllCollections(this IArtDef artDef)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<IArtDefCollection>();
		}
		List<IArtDefCollection> collections = new List<IArtDefCollection>();
		artDef.RootCollections.ForEach(delegate(IArtDefCollection col)
		{
			collections.AddRange(col.GetAllCollections());
		});
		return collections;
	}

	public static IEnumerable<IArtDefCollection> GetAllCollections(this IArtDefCollection collection)
	{
		List<IArtDefCollection> collections = new List<IArtDefCollection>();
		if (collection != null)
		{
			collections.Add(collection);
			collection.Elements.ForEach(delegate(IArtDefElement ele)
			{
				collections.AddRange(ele.GetAllCollections());
			});
		}
		return collections;
	}

	public static IEnumerable<IArtDefCollection> GetAllCollections(this IArtDefElement element)
	{
		List<IArtDefCollection> collections = new List<IArtDefCollection>();
		element?.Children.ForEach(delegate(IArtDefCollection col)
		{
			collections.AddRange(col.GetAllCollections());
		});
		return collections;
	}

	public static IArtDefCollection FindArtDefCollectionRootFirst(this IArtDef artDef, string collectionName)
	{
		if (artDef == null)
		{
			return null;
		}
		IArtDefCollection artDefCollection = null;
		IEnumerator<IArtDefCollection> enumerator = artDef.RootCollections.GetEnumerator();
		while (artDefCollection == null && enumerator.MoveNext())
		{
			if (enumerator.Current.CollectionName == collectionName)
			{
				artDefCollection = enumerator.Current;
			}
		}
		if (artDefCollection == null)
		{
			enumerator = artDef.RootCollections.GetEnumerator();
			while (artDefCollection == null && enumerator.MoveNext())
			{
				artDefCollection = enumerator.Current.FindArtDefCollection(collectionName);
			}
		}
		return artDefCollection;
	}

	public static IArtDefCollection FindArtDefCollection(this IArtDefElement element, string collectionName)
	{
		if (element == null)
		{
			return null;
		}
		IArtDefCollection artDefCollection = null;
		IEnumerator<IArtDefCollection> enumerator = element.Children.GetEnumerator();
		while (artDefCollection == null && enumerator.MoveNext())
		{
			if (enumerator.Current.CollectionName == collectionName)
			{
				artDefCollection = enumerator.Current;
			}
		}
		if (artDefCollection == null)
		{
			enumerator = element.Children.GetEnumerator();
			while (artDefCollection == null && enumerator.MoveNext())
			{
				artDefCollection = enumerator.Current.FindArtDefCollection(collectionName);
			}
		}
		return artDefCollection;
	}

	public static IArtDefCollection FindArtDefCollection(this IArtDefCollection collection, string collectionName)
	{
		if (collection == null)
		{
			return null;
		}
		if (collection.CollectionName == collectionName)
		{
			return collection;
		}
		IArtDefCollection artDefCollection = null;
		IEnumerator<IArtDefElement> enumerator = collection.Elements.GetEnumerator();
		while (artDefCollection == null && enumerator.MoveNext())
		{
			artDefCollection = enumerator.Current.FindArtDefCollection(collectionName);
		}
		return artDefCollection;
	}

	public static void VisitAllValues(this IArtDef artDef, Action<IValue> act)
	{
		if (artDef == null)
		{
			return;
		}
		foreach (IArtDefCollection rootCollection in artDef.RootCollections)
		{
			rootCollection.VisitAllValues(act);
		}
	}

	public static void VisitAllValues(this IArtDefCollection artDefCollection, Action<IValue> act)
	{
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			element.VisitAllValues(act);
		}
	}

	public static void VisitAllValues(this IArtDefElement artDefElement, Action<IValue> act)
	{
		foreach (IValue item in artDefElement.Fields.Items)
		{
			act(item);
		}
		foreach (IArtDefCollection child in artDefElement.Children)
		{
			child.VisitAllValues(act);
		}
	}

	public static IEnumerable<IValue> GetAllValues(this IArtDef artDef)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<IValue>();
		}
		List<IValue> list = new List<IValue>();
		foreach (IArtDefCollection rootCollection in artDef.RootCollections)
		{
			list.AddRange(rootCollection.GetAllValues());
		}
		return list;
	}

	public static IEnumerable<IValue> GetAllValues(this IArtDefCollection artDefCollection)
	{
		List<IValue> list = new List<IValue>();
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			list.AddRange(element.GetAllValues());
		}
		return list;
	}

	public static IEnumerable<IValue> GetAllValues(this IArtDefElement artDefElement)
	{
		List<IValue> list = new List<IValue>();
		list.AddRange(artDefElement.Fields.Items);
		foreach (IArtDefCollection child in artDefElement.Children)
		{
			list.AddRange(child.GetAllValues());
		}
		return list;
	}

	public static IEnumerable<IObjectValue> GetAllObjectValues(this IArtDef artDef)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<IObjectValue>();
		}
		List<IObjectValue> list = new List<IObjectValue>();
		IEnumerable<IValue> allValues = artDef.GetAllValues();
		list.AddRange(allValues.OfType<IObjectValue>());
		IEnumerable<ICollectionValue> enumerable = from col in allValues.OfType<ICollectionValue>()
			where col.EntryValueType == ValueType.VT_OBJECT
			select col;
		foreach (ICollectionValue item in enumerable)
		{
			list.AddRange(item.Items.OfType<IObjectValue>());
		}
		return list;
	}

	public static IEnumerable<EntityID> GetAllReferencedEntities(this IArtDef artDef)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<EntityID>();
		}
		ICollection<EntityID> collection = new HashSet<EntityID>();
		IEnumerable<IObjectValue> boundObjectValues = artDef.GetAllObjectValues().GetBoundObjectValues();
		foreach (IObjectValue item2 in boundObjectValues)
		{
			EntityID item = new EntityID(item2);
			if (!collection.Contains(item))
			{
				collection.Add(item);
			}
		}
		return collection;
	}

	public static IEnumerable<IBLPEntryValue> GetAllBLPValues(this IArtDef artDef)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<IBLPEntryValue>();
		}
		List<IBLPEntryValue> list = new List<IBLPEntryValue>();
		IEnumerable<IValue> allValues = artDef.GetAllValues();
		list.AddRange(allValues.OfType<IBLPEntryValue>());
		IEnumerable<ICollectionValue> enumerable = from col in allValues.OfType<ICollectionValue>()
			where col.EntryValueType == ValueType.VT_BLP_ENTRY
			select col;
		foreach (ICollectionValue item in enumerable)
		{
			list.AddRange(item.Items.OfType<IBLPEntryValue>());
		}
		return list;
	}

	public static IEnumerable<Uri> GetAllReferencedXLPs(this IArtDef artDef, string XLPRoot)
	{
		if (artDef == null)
		{
			return Enumerable.Empty<Uri>();
		}
		ICollection<Uri> collection = new HashSet<Uri>();
		IEnumerable<IBLPEntryValue> enumerable = from val in artDef.GetAllBLPValues()
			where !string.IsNullOrEmpty(val.XLPPath)
			select val;
		foreach (IBLPEntryValue item2 in enumerable)
		{
			string uriString = Path.Combine(XLPRoot, item2.XLPPath);
			Uri item = new Uri(uriString);
			if (!collection.Contains(item))
			{
				collection.Add(item);
			}
		}
		return collection;
	}

	public static void UpdateBLPReferences(this IArtDef artDef, IArtDefTemplate template)
	{
		if (artDef == null || template == null)
		{
			return;
		}
		foreach (IArtDefElementTemplate collection in template.Collections)
		{
			artDef.FindArtDefCollectionRootFirst(collection.Name)?.UpdateBLPReferences(collection);
		}
	}

	public static void UpdateBLPReferences(this IArtDefCollection artDefCollection, IArtDefElementTemplate template)
	{
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			element.UpdateBLPReferences(template);
		}
	}

	public static void RemoveDuplicateEntries(this IArtDef artDef)
	{
		foreach (IArtDefCollection rootCollection in artDef.RootCollections)
		{
			rootCollection.RemoveDuplicateElementsFromCollection();
		}
	}

	public static void RemoveDuplicateElementsFromCollection(this IArtDefCollection artDefCollection)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			if (!list.Contains(element.Name))
			{
				list.Add(element.Name);
			}
			else
			{
				list2.Add(element.Name);
			}
			foreach (IArtDefCollection child in element.Children)
			{
				child.RemoveDuplicateElementsFromCollection();
			}
		}
		foreach (string item in list2)
		{
			artDefCollection.RemoveElement(item);
		}
	}

	public static void UpdateBLPReferences(this IArtDefElement artDefElement, IArtDefElementTemplate template)
	{
		foreach (IParameter item in template.Parameters.Items)
		{
			if (item.ParameterType == ParameterType.PT_BLP_ENTRY)
			{
				IBLPEntryValue iBLPEntryValue = artDefElement.Fields.FindValue(item.Name) as IBLPEntryValue;
				IBLPEntryParameter iBLPEntryParameter = (IBLPEntryParameter)item;
				if (iBLPEntryValue != null)
				{
					iBLPEntryValue.XLPClass = iBLPEntryParameter.XLPClassName;
					iBLPEntryValue.LibraryName = iBLPEntryParameter.LibraryName;
				}
			}
			else
			{
				if (item.ParameterType != ParameterType.PT_COLLECTION)
				{
					continue;
				}
				ICollectionParameter collectionParameter = (ICollectionParameter)item;
				if (collectionParameter.EntryParameterType != ParameterType.PT_BLP_ENTRY)
				{
					continue;
				}
				IBLPEntryParameter blpColParam = (IBLPEntryParameter)collectionParameter.EntryParameter;
				if (artDefElement.Fields.FindValue(item.Name) is ICollectionValue collectionValue)
				{
					collectionValue.Items.OfType<IBLPEntryValue>().ForEach(delegate(IBLPEntryValue blpVal)
					{
						blpVal.XLPClass = blpColParam.XLPClassName;
						blpVal.LibraryName = blpColParam.LibraryName;
					});
				}
			}
		}
		foreach (IArtDefElementTemplate child in template.Children)
		{
			artDefElement.FindArtDefCollection(child.Name)?.UpdateBLPReferences(child);
		}
	}

	public static void UpdateAppendMergedParameterCollection(this IArtDef artDef, IArtDefTemplate template)
	{
		if (artDef == null || template == null)
		{
			return;
		}
		foreach (IArtDefElementTemplate collection in template.Collections)
		{
			IArtDefCollection artDefCollection = artDef.FindArtDefCollectionRootFirst(collection.Name);
			if (artDefCollection != null)
			{
				artDefCollection.UpdateAppendMergedParameterCollection(collection);
			}
		}
	}

	private static void UpdateAppendMergedParameterCollection(this IArtDefCollection artDefCollection, IArtDefElementTemplate template)
	{
		foreach (IArtDefElement element in artDefCollection.Elements)
		{
			element.UpdateAppendMergedParameterCollection(template);
		}
	}

	private static void UpdateAppendMergedParameterCollection(this IArtDefElement artDefElement, IArtDefElementTemplate template)
	{
		artDefElement.AppendMergedParameterCollections = template.AppendMergedParameterCollections;
		foreach (IArtDefElementTemplate child in template.Children)
		{
			IArtDefCollection artDefCollection = artDefElement.FindArtDefCollection(child.Name);
			if (artDefCollection != null)
			{
				artDefCollection.UpdateAppendMergedParameterCollection(child);
			}
		}
	}
}

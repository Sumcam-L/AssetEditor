using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.Error;

namespace Firaxis.AssetEditing;

public class RemoveElementOperation : ArtDefCollectionOperation
{
	private class ArtDefMemento
	{
		public IList<KeyValuePair<string, IList<ArtDefMemento>>> Collections = new List<KeyValuePair<string, IList<ArtDefMemento>>>();

		public string ElementName;

		public IList<KeyValuePair<string, object>> Fields = new List<KeyValuePair<string, object>>();

		public int Index;

		public ArtDefMemento(ArtDefCollectionAdapter colAdapter, ArtDefElementAdapter elemAdapter)
		{
			BuildElement(colAdapter, elemAdapter);
		}

		public void RestoreFromMemento(ArtDefCollectionAdapter colAdapter, ArtDefElementAdapter elemAdapter)
		{
			PlatformAssert.If(ElementName != elemAdapter.Name);
			foreach (KeyValuePair<string, object> field in Fields)
			{
				elemAdapter.FindField(field.Key).ValueDataAsObject = field.Value;
			}
			foreach (KeyValuePair<string, IList<ArtDefMemento>> collection in Collections)
			{
				ArtDefCollectionAdapter artDefCollectionAdapter = elemAdapter.FindCollection(collection.Key);
				foreach (ArtDefMemento item in collection.Value)
				{
					artDefCollectionAdapter.AddElement(item.ElementName, item.Index);
					item.RestoreFromMemento(artDefCollectionAdapter, artDefCollectionAdapter.FindElement(item.ElementName));
				}
			}
		}

		private void BuildElement(ArtDefCollectionAdapter colAdapter, ArtDefElementAdapter elemAdapter)
		{
			Index = colAdapter.Elements.IndexOf(elemAdapter);
			ElementName = elemAdapter.Name;
			foreach (IFieldValueAdapter field in elemAdapter.Fields)
			{
				Fields.Add(new KeyValuePair<string, object>(field.Name, field.ValueDataAsObject));
			}
			foreach (ArtDefCollectionAdapter collection in elemAdapter.Collections)
			{
				if (collection.Elements.Count <= 0)
				{
					continue;
				}
				IList<ArtDefMemento> list = new List<ArtDefMemento>();
				foreach (ArtDefElementAdapter element in collection.Elements)
				{
					list.Add(new ArtDefMemento(collection, element));
				}
				Collections.Add(new KeyValuePair<string, IList<ArtDefMemento>>(collection.Name, list));
			}
		}
	}

	private ArtDefMemento Memento { get; set; }

	public RemoveElementOperation(ArtDefDocument doc, IList<string> pathToParent, string elementName)
		: base(doc, pathToParent)
	{
		ArtDefCollectionAdapter collection = GetCollection();
		ArtDefElementAdapter elemAdapter = collection.FindElement(elementName);
		Memento = new ArtDefMemento(collection, elemAdapter);
		Do();
	}

	public override void Do()
	{
		GetCollection().RemoveElement(Memento.ElementName);
	}

	public override void Undo()
	{
		ArtDefCollectionAdapter collection = GetCollection();
		collection.AddElement(Memento.ElementName, Memento.Index);
		Memento.RestoreFromMemento(collection, collection.FindElement(Memento.ElementName));
	}
}

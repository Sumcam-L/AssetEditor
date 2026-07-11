using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameArtIDAdapter : DomNodeAdapter
{
	public string GameName
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.GameArtIDType.GameNameAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.GameArtIDType.GameNameAttribute, value);
		}
	}

	public string GameArtID
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.GameArtIDType.GameArtIDAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.GameArtIDType.GameArtIDAttribute, value);
		}
	}

	public static GameArtIDAdapter Create(IGameArtID gameArtID)
	{
		return Create(gameArtID.Name, gameArtID.ID);
	}

	public static GameArtIDAdapter Create(string name, string ID)
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.GameArtIDType.Type);
		domNode.InitializeExtensions();
		GameArtIDAdapter gameArtIDAdapter = domNode.As<GameArtIDAdapter>();
		gameArtIDAdapter.GameName = name;
		gameArtIDAdapter.GameArtID = ID;
		return gameArtIDAdapter;
	}

	public override string ToString()
	{
		return GameName + " (" + GameArtID + ")";
	}
}

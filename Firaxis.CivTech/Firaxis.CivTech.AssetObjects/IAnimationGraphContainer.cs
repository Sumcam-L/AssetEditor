namespace Firaxis.CivTech.AssetObjects;

public interface IAnimationGraphContainer
{
	void AddRoot(int iNodeID, int nAnimationItems, int nTimelineItems, int nMaterialItems, int nStateItems);

	void AddOrphanRoot(int iNodeID, int nAnimationItems, int nTimelineItems, int nMaterialItems, int nStateItems);

	void AddAnimation(int iParentID, int iNodeID, int iItemInParent);

	void AddTimeline(int iParentID, int iNodeID, int iItemInParent);

	void AddMaterial(int iParentID, int iNodeID, int iItemInParent);

	void AddState(int iParentID, int iNodeID, int iItemInParent);

	void AddAnimationSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eSelectorType, int nItems);

	void AddTimelineSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eSelectorType, int nItems);

	void AddMaterialSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eSelectorType, int nItems);

	void AddStateSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eSelectorType, int nItems);

	void SetAnimationID(int iNodeID, string sAnimationName, int iAnimationID);

	void SetAnimationText(int iNodeID, string sText);

	void SetAnimationLocation(int iNodeID, int iPosX, int iPosY);

	void SetTimelineID(int iNodeID, string sTimelineName, int iTimelineID);

	void SetTimelineText(int iNodeID, string sText);

	void SetTimelineLocation(int iNodeID, int iPosX, int iPosY);

	void SetStateID(int iNodeID, string sStateName, int iStateID);

	void SetStateText(int iNodeID, string sText);

	void SetStateLocation(int iNodeID, int iPosX, int iPosY);

	void SetMaterialID(int iNodeID, string sMaterialName, int iMaterialID);

	void SetMaterialText(int iNodeID, string sText);

	void SetMaterialLocation(int iNodeID, int iPosX, int iPosY);

	void SetAnimationSelectorText(int iNodeID, string sText);

	void SetAnimationSelectorLocation(int iNodeID, int iPosX, int iPosY);

	void SetTimelineSelectorText(int iNodeID, string sText);

	void SetTimelineSelectorLocation(int iNodeID, int iPosX, int iPosY);

	void SetMaterialSelectorText(int iNodeID, string sText);

	void SetMaterialSelectorLocation(int iNodeID, int iPosX, int iPosY);

	void SetStateSelectorText(int iNodeID, string sText);

	void SetStateSelectorLocation(int iNodeID, int iPosX, int iPosY);
}

namespace Firaxis.CivTech.AssetObjects;

public interface IStateGraphContainer
{
	void AddRoot(int iNodeID, string sText, int iPosX, int iPosY);

	void AddOrphanRoot(int iNodeID);

	void AddData(int iParentID, int iNodeID, string sTypeName, string sText, int iPosX, int iPosY);

	void AddSource(int iParentID, int iNodeID, string sText, int iPosX, int iPosY);

	void AddDestination(int iParentID, int iNodeID, string sText, int iPosX, int iPosY);

	void AddAnimationGraph(int iParentID, int iNodeID, string sText, int iPosX, int iPosY);

	void SetSourceStateName(int iNodeID, string sStateName);

	void SetDestinationStateName(int iNodeID, string sStateName);

	void SetDestinationLoop(int iNodeID, bool bLoop);

	void SetDestinationBlendDuration(int iNodeID, float fBlendDuration);

	void SetDestinationRandomOffset(int iNodeID, bool bRandomOffset);

	void SetDestinationContinueOffset(int iNodeID, bool bContinueOffset);

	void SetAnimationGraphPercentChance(int iNodeID, float fPercentChance);

	IAnimationGraphContainer GetAnimationGraph(int iNodeID);
}

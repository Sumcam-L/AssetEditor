using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IAudioPreviewer : IAssemblyInstance, IDisposable
{
	void Startup(ICivTechService civTech, bool bColdStart);

	void Shutdown(bool bStopWwise);

	void ReloadSoundBanks();

	int GetBankID(string bankName);

	int GetNumBankEventNames(int iBankID);

	string GetBankEventName(int iBankID, int iEventIndex);

	bool GetBankCategory(string sBankName, int iCategoryToCheck);

	void PlaySoundEvent(string sEventName);

	void StopAllSounds();

	int GetNumSoundBanks();

	string GetSoundBankName(int iBank);

	uint GetNumSwitchGroups();

	uint GetNumSwitchSettings(string sGroupName);

	string GetSwitchGroupName(uint uSwitch);

	string GetSwitchSettingName(string sGroupName, uint uSwitch);

	void SetPlaybackSwitch(string sGroupName, string sSwitchName);

	void UnloadProjectData();

	void LoadProjectData();
}

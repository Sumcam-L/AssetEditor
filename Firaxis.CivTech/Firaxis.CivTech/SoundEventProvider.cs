using System.Collections.Generic;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.CivTech;

public class SoundEventProvider
{
	public readonly IAudioPreviewer AudioPreviewer;

	private readonly List<SoundEvent> AudioEvents = new List<SoundEvent>();

	public IEnumerable<SoundEvent> SoundEvents => AudioEvents;

	public SoundEventProvider(IAudioPreviewer pmAudioPreviewer)
	{
		AudioPreviewer = pmAudioPreviewer;
		AudioEvents.Clear();
		ISet<string> set = new HashSet<string>();
		for (int i = 0; i < AudioPreviewer.GetNumSoundBanks(); i++)
		{
			string soundBankName = AudioPreviewer.GetSoundBankName(i);
			int bankID = AudioPreviewer.GetBankID(soundBankName);
			int numBankEventNames = AudioPreviewer.GetNumBankEventNames(bankID);
			for (int j = 0; j < numBankEventNames; j++)
			{
				string bankEventName = AudioPreviewer.GetBankEventName(bankID, j);
				if (set.Add(bankEventName))
				{
					if (AudioPreviewer.GetBankCategory(soundBankName, 4))
					{
						AudioEvents.Add(new SoundEvent(bankEventName, AudioScriptType.Sound3d));
					}
					else if (AudioPreviewer.GetBankCategory(soundBankName, 3))
					{
						AudioEvents.Add(new SoundEvent(bankEventName, AudioScriptType.Sound2d));
					}
					else
					{
						AudioEvents.Add(new SoundEvent(bankEventName, AudioScriptType.All));
					}
				}
			}
		}
		AudioEvents.Sort();
	}

	public void PlayScriptSound(string pmName)
	{
		AudioPreviewer?.PlaySoundEvent(pmName);
	}

	public void StopAllSounds()
	{
		AudioPreviewer?.StopAllSounds();
	}
}

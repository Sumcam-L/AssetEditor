namespace Firaxis.ATF;

public interface ITunerService
{
	bool Enabled { get; set; }

	bool IsConnected { get; }

	void SendTunerCommand(string tunerCommand);
}

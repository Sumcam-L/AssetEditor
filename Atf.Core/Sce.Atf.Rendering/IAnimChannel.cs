namespace Sce.Atf.Rendering;

public interface IAnimChannel
{
	IAnimData Data { get; }

	string Channel { get; set; }

	string InputObject { get; }

	string InputChannel { get; }

	object Target { get; set; }

	int ValueIndex { get; set; }

	bool Enabled { get; set; }
}

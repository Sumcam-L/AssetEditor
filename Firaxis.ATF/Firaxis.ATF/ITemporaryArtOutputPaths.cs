namespace Firaxis.ATF;

public interface ITemporaryArtOutputPaths
{
	string ArtDefOutputDirectory { get; }

	string BLPOutputDirectory { get; }

	string TemporaryCookLocation { get; }

	string TemporaryPantryDirectory { get; }
}

namespace ComponentAce.Compression.Libs.ZLib;

internal enum InflateCodesMode
{
	START,
	LEN,
	LENEXT,
	DIST,
	DISTEXT,
	COPY,
	LIT,
	WASH,
	END,
	BADCODE
}

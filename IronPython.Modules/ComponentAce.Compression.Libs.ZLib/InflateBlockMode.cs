namespace ComponentAce.Compression.Libs.ZLib;

internal enum InflateBlockMode
{
	TYPE,
	LENS,
	STORED,
	TABLE,
	BTREE,
	DTREE,
	CODES,
	DRY,
	DONE,
	BAD
}

namespace Firaxis.MathEx;

public class CRC32
{
	private const uint DefaultPolynomial = 3988292384u;

	private const uint DefaultCRCInit = uint.MaxValue;

	private static uint[] m_table;

	private static uint m_magic;

	public uint CRCInit { get; set; }

	static CRC32()
	{
		m_table = new uint[256];
		CalcTable(3988292384u);
	}

	public CRC32()
		: this(uint.MaxValue)
	{
	}

	public CRC32(uint uiCrcInit)
	{
		CRCInit = uiCrcInit;
	}

	public uint Calc(byte[] abBuffer)
	{
		return Calc(abBuffer, CRCInit);
	}

	public static uint Calc(byte[] abBuffer, uint uiCRCInit)
	{
		uint num = uiCRCInit;
		foreach (byte b in abBuffer)
		{
			num = (num >> 8) ^ m_table[(b ^ num) & 0xFF];
		}
		return num;
	}

	private static void CalcTable(uint polynomial)
	{
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = num;
			for (uint num3 = 8u; num3 != 0; num3--)
			{
				uint num4 = num2 & 1;
				num2 >>= 1;
				if (num4 != 0)
				{
					num2 ^= polynomial;
				}
			}
			m_table[num] = num2;
		}
		byte[] abBuffer = new byte[4];
		m_magic = Calc(abBuffer, uint.MaxValue);
	}
}

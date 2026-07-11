namespace SharpDX.Multimedia;

public static class SpeakersExtensions
{
	public static int ToChannelCount(Speakers speakers)
	{
		int num = (int)speakers;
		int num2 = 0;
		while (num != 0)
		{
			if ((num & 1) != 0)
			{
				num2++;
			}
			num >>= 1;
		}
		return num2;
	}
}

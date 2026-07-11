using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonRandom
{
	[PythonType]
	public class Random
	{
		private System.Random _rnd;

		public Random()
		{
			seed();
		}

		public Random(object seed)
		{
			this.seed(seed);
		}

		public object getrandbits(int bits)
		{
			if (bits <= 0)
			{
				throw PythonOps.ValueError("number of bits must be greater than zero");
			}
			lock (this)
			{
				return _rnd.GetRandBits(bits);
			}
		}

		public object getstate()
		{
			return _rnd;
		}

		public void jumpahead(int count)
		{
			lock (this)
			{
				_rnd.NextBytes(new byte[4096]);
			}
		}

		public void jumpahead(double count)
		{
			throw PythonOps.TypeError("jumpahead requires an integer, not 'float'");
		}

		public object random()
		{
			lock (this)
			{
				return _rnd.NextDouble();
			}
		}

		public void seed()
		{
			seed(DateTime.Now);
		}

		public void seed(object s)
		{
			int num = ((!(s is int)) ? s.GetHashCode() : ((int)s));
			lock (this)
			{
				_rnd = new System.Random(num);
			}
		}

		public void setstate(object state)
		{
			System.Random random = state as System.Random;
			lock (this)
			{
				if (random != null)
				{
					_rnd = random;
					return;
				}
				throw PythonOps.TypeError("setstate: argument must be value returned from getstate()");
			}
		}
	}

	public const string __doc__ = "implements a random number generator";
}

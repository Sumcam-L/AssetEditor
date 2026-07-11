using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime;

public sealed class Profiler
{
	private sealed class InnerMethodProfiler : DynamicExpressionVisitor
	{
		private readonly Profiler _profiler;

		private readonly ParameterExpression _tick;

		private readonly int _profileIndex;

		public InnerMethodProfiler(Profiler profiler, ParameterExpression tick, int profileIndex)
		{
			_profiler = profiler;
			_tick = tick;
			_profileIndex = profileIndex;
		}

		protected override Expression VisitDynamic(DynamicExpression node)
		{
			return _profiler.AddInnerProfiling(node, _tick, _profileIndex);
		}

		protected override Expression VisitExtension(Expression node)
		{
			if (node is ReducableDynamicExpression)
			{
				return _profiler.AddInnerProfiling(node, _tick, _profileIndex);
			}
			return base.VisitExtension(node);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			Expression result = base.VisitMethodCall(node);
			if (IgnoreMethod(node.Method))
			{
				return _profiler.AddInnerProfiling(node, _tick, _profileIndex);
			}
			return result;
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return node;
		}
	}

	public struct Data
	{
		public string Name;

		public long InclusiveTime;

		public long ExclusiveTime;

		public int Calls;

		public Data(string _name, long _inclusive, long _exclusive, int _calls)
		{
			Name = _name;
			InclusiveTime = _inclusive;
			ExclusiveTime = _exclusive;
			Calls = _calls;
		}
	}

	private const int _initialSize = 100;

	private const int TimeInBody = 0;

	private const int TimeInChildMethods = 1;

	private const int NumberOfCalls = 2;

	private readonly Dictionary<MethodBase, int> _methods;

	private readonly Dictionary<string, int> _names;

	private readonly List<string> _counters;

	private readonly List<long[,]> _profiles;

	private long[,] _profileData;

	private static readonly object _profileKey = new object();

	public static Profiler GetProfiler(PythonContext context)
	{
		return context.GetOrCreateModuleState(_profileKey, () => new Profiler());
	}

	private Profiler()
	{
		_methods = new Dictionary<MethodBase, int>();
		_names = new Dictionary<string, int>();
		_counters = new List<string>();
		_profiles = new List<long[,]>();
		_profileData = new long[100, 3];
	}

	private static string FormatMethodName(MethodBase method)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (method.DeclaringType != null)
		{
			stringBuilder.Append("type ");
			stringBuilder.Append(method.DeclaringType.Name);
			stringBuilder.Append(": ");
		}
		stringBuilder.Append("method: ");
		stringBuilder.Append(method.Name);
		stringBuilder.Append('(');
		bool flag = false;
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				flag = true;
			}
			stringBuilder.Append(parameterInfo.ParameterType.Name);
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	private int GetProfilerIndex(MethodBase method)
	{
		lock (_methods)
		{
			if (!_methods.TryGetValue(method, out var value))
			{
				value = GetNewProfilerIndex(FormatMethodName(method));
				_methods[method] = value;
			}
			return value;
		}
	}

	private int GetProfilerIndex(string name)
	{
		lock (_methods)
		{
			if (!_names.TryGetValue(name, out var value))
			{
				value = GetNewProfilerIndex(name);
				_names[name] = value;
			}
			return value;
		}
	}

	private int GetNewProfilerIndex(string name)
	{
		int count;
		lock (_counters)
		{
			count = _counters.Count;
			_counters.Add(name);
			if (count >= _profileData.Length / 3)
			{
				long[,] value = new long[count * 2, 3];
				_profiles.Add(Interlocked.Exchange(ref _profileData, value));
			}
		}
		return count;
	}

	public List<Data> GetProfile(bool includeUnused)
	{
		List<Data> list = new List<Data>(_counters.Count);
		lock (_counters)
		{
			int num = _profileData.Length / 3;
			long[,] value = new long[num, 3];
			long[,] array = Interlocked.Exchange(ref _profileData, value);
			for (int i = 0; i < _profiles.Count; i++)
			{
				for (int j = 0; j < num; j++)
				{
					if (j < _profiles[i].Length / 3)
					{
						array[j, 0] += _profiles[i][j, 0];
						array[j, 1] += _profiles[i][j, 1];
						array[j, 2] += _profiles[i][j, 2];
					}
				}
			}
			_profiles.Clear();
			_profiles.Add(array);
			for (int k = 0; k < _counters.Count; k++)
			{
				if (includeUnused || array[k, 2] > 0)
				{
					list.Add(new Data(_counters[k], DateTimeTicksFromTimeData(array[k, 0] + array[k, 1]), DateTimeTicksFromTimeData(array[k, 0]), (int)array[k, 2]));
				}
			}
			return list;
		}
	}

	public void Reset()
	{
		lock (_counters)
		{
			int num = _profileData.Length / 3;
			long[,] value = new long[num, 3];
			Interlocked.Exchange(ref _profileData, value);
			_profiles.Clear();
		}
	}

	private static long DateTimeTicksFromTimeData(long elapsedStopwatchTicks)
	{
		if (Stopwatch.IsHighResolution)
		{
			return (long)((double)elapsedStopwatchTicks * 10000000.0 / (double)Stopwatch.Frequency);
		}
		return elapsedStopwatchTicks;
	}

	public long StartCall(int index)
	{
		Interlocked.Increment(ref _profileData[index, 2]);
		return Stopwatch.GetTimestamp();
	}

	public long StartNestedCall(int index, long timestamp)
	{
		long timestamp2 = Stopwatch.GetTimestamp();
		Interlocked.Add(ref _profileData[index, 0], timestamp2 - timestamp);
		return timestamp2;
	}

	public long FinishNestedCall(int index, long timestamp)
	{
		long timestamp2 = Stopwatch.GetTimestamp();
		Interlocked.Add(ref _profileData[index, 1], timestamp2 - timestamp);
		return timestamp2;
	}

	public void FinishCall(int index, long timestamp)
	{
		long timestamp2 = Stopwatch.GetTimestamp();
		Interlocked.Add(ref _profileData[index, 0], timestamp2 - timestamp);
	}

	internal Expression AddOuterProfiling(Expression body, ParameterExpression tick, int profileIndex)
	{
		return Expression.Block(Expression.Assign(tick, Expression.Call(Expression.Constant(this, typeof(Profiler)), typeof(Profiler).GetMethod("StartCall"), Utils.Constant(profileIndex))), Utils.Try(body).Finally(Expression.Call(Expression.Constant(this, typeof(Profiler)), typeof(Profiler).GetMethod("FinishCall"), Utils.Constant(profileIndex), tick)));
	}

	internal Expression AddInnerProfiling(Expression body, ParameterExpression tick, int profileIndex)
	{
		return Expression.Block(Expression.Assign(tick, Expression.Call(Expression.Constant(this, typeof(Profiler)), typeof(Profiler).GetMethod("StartNestedCall"), Utils.Constant(profileIndex), tick)), Utils.Try(body).Finally(Expression.Assign(tick, Expression.Call(Expression.Constant(this, typeof(Profiler)), typeof(Profiler).GetMethod("FinishNestedCall"), Utils.Constant(profileIndex), tick))));
	}

	private static bool IgnoreMethod(MethodBase method)
	{
		object[] customAttributes = method.GetCustomAttributes(typeof(ProfilerTreatsAsExternalAttribute), inherit: false);
		return customAttributes.Length > 0;
	}

	internal Expression AddProfiling(Expression body, ParameterExpression tick, string name, bool unique)
	{
		int profilerIndex = GetProfilerIndex(name);
		return AddOuterProfiling(new InnerMethodProfiler(this, tick, profilerIndex).Visit(body), tick, profilerIndex);
	}

	internal Expression AddProfiling(Expression body, MethodBase method)
	{
		if (method is DynamicMethod || IgnoreMethod(method))
		{
			return body;
		}
		int profilerIndex = GetProfilerIndex(method);
		ParameterExpression parameterExpression = Expression.Variable(typeof(long), "$tick");
		return Expression.Block(new ParameterExpression[1] { parameterExpression }, AddOuterProfiling(body, parameterExpression, profilerIndex));
	}
}

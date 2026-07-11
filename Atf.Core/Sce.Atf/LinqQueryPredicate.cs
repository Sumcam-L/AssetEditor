using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Sce.Atf;

public abstract class LinqQueryPredicate : IQueryPredicate
{
	public class StringReplaceQueryPattern : IReplacingQueryPattern
	{
		private readonly string m_pattern;

		private StringReplaceQueryPattern()
			: this(null)
		{
		}

		public StringReplaceQueryPattern(string pattern)
		{
			m_pattern = pattern;
		}

		public bool Matches(IQueryMatch itemToMatch)
		{
			string input = itemToMatch.GetValue().ToString();
			return Regex.Match(input, m_pattern, RegexOptions.IgnoreCase).Success;
		}

		public void Replace(IQueryMatch itemToReplace, object replaceWith)
		{
			string input = itemToReplace.GetValue().ToString();
			itemToReplace.SetValue(Regex.Replace(input, m_pattern, replaceWith.ToString(), RegexOptions.IgnoreCase));
		}
	}

	public class NumberReplaceQueryPattern : IReplacingQueryPattern
	{
		private NumberReplaceQueryPattern()
			: this(0.0, 0.0)
		{
		}

		public NumberReplaceQueryPattern(double pattern1)
			: this(pattern1, 0.0)
		{
		}

		public NumberReplaceQueryPattern(double pattern1, double pattern2)
		{
		}

		public bool Matches(IQueryMatch itemToMatch)
		{
			return IsConvertibleToDouble(itemToMatch.GetValue());
		}

		public void Replace(IQueryMatch itemToMatch, object replaceWith)
		{
			if (IsConvertibleToDouble(itemToMatch.GetValue()) && IsConvertibleToDouble(replaceWith))
			{
				double num = Convert.ToDouble(replaceWith);
				itemToMatch.SetValue(num);
			}
		}
	}

	private readonly List<Expression> m_expressionList;

	protected LambdaExpression m_lambdaExpression;

	private IReplacingQueryPattern m_matchPattern;

	protected ParameterExpression m_queryableData;

	public MethodCallExpression QueryableValue => Expression.Call((Expression)m_queryableData, "GetValue", (Type[])null, (Expression[])null);

	public MethodCallExpression QueryableValueString => Expression.Call((Expression)QueryableValue, "ToString", (Type[])null, (Expression[])null);

	protected Expression LambdaExpression
	{
		get
		{
			if (m_queryableData == null)
			{
				throw new InvalidOperationException("Attempting to construct a Lambda expression when the expression on which to iterate hasn't been created.");
			}
			if (m_lambdaExpression == null)
			{
				Expression expression = null;
				foreach (Expression expression2 in m_expressionList)
				{
					expression = ((expression != null) ? Expression.AndAlso(expression, expression2) : expression2);
				}
				m_lambdaExpression = Expression.Lambda(expression, m_queryableData);
			}
			return m_lambdaExpression;
		}
	}

	public IReplacingQueryPattern MatchPattern
	{
		get
		{
			return m_matchPattern;
		}
		set
		{
			if (m_matchPattern != null)
			{
				throw new InvalidOperationException("Search predicate has been assigned more than one match pattern");
			}
			m_matchPattern = value;
		}
	}

	public LinqQueryPredicate()
	{
		m_lambdaExpression = null;
		m_expressionList = new List<Expression>();
		m_queryableData = null;
	}

	public bool Test(object searchItem, out IList<IQueryMatch> matchList)
	{
		matchList = null;
		IQueryable queryable = GetQueryable(searchItem);
		if (queryable != null)
		{
			foreach (object item in queryable)
			{
				IQueryMatch queryMatch = CreatePredicateMatch(searchItem, item);
				if (MatchPattern == null || MatchPattern.Matches(queryMatch))
				{
					if (matchList == null)
					{
						matchList = new List<IQueryMatch>();
					}
					matchList.Add(queryMatch);
				}
			}
		}
		return matchList != null;
	}

	public void Replace(IList<IQueryMatch> matchList, object replaceValue)
	{
		foreach (IQueryMatch match in matchList)
		{
			MatchPattern.Replace(match, replaceValue);
		}
	}

	protected abstract IQueryable GetQueryableElements(object queryItem);

	protected IQueryable GetQueryable(object queryItem)
	{
		IQueryable result = null;
		IQueryable queryableElements = GetQueryableElements(queryItem);
		if (queryableElements != null)
		{
			MethodCallExpression expression = Expression.Call(typeof(Queryable), "Where", new Type[1] { queryableElements.ElementType }, queryableElements.Expression, LambdaExpression);
			result = queryableElements.Provider.CreateQuery(expression);
		}
		return result;
	}

	public abstract IQueryMatch CreatePredicateMatch(object searchItem, object queryMatch);

	protected void AddExpression(Expression expression)
	{
		m_expressionList.Add(expression);
	}

	protected BinaryExpression GetNullOrEmptyExpression(string matchString)
	{
		return Expression.Equal(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Constant(matchString)), Expression.Constant(true));
	}

	public MethodCallExpression GetStringIndexOfExpression(Expression sourceStringExpression, string matchString)
	{
		ConstantExpression constantExpression = Expression.Constant(StringComparison.InvariantCultureIgnoreCase);
		return Expression.Call(sourceStringExpression, "IndexOf", null, Expression.Constant(matchString), constantExpression);
	}

	public string CreateRegularExpressionPattern(string matchString, ulong searchType)
	{
		string text = "";
		ulong num = searchType - 1;
		if (num <= 3)
		{
			switch ((uint)num)
			{
			case 0u:
				return "^" + Regex.Escape(matchString) + "$";
			case 3u:
				return "^" + Regex.Escape(matchString);
			case 1u:
			case 2u:
				goto IL_00a4;
			}
		}
		switch (searchType)
		{
		case 8uL:
			return Regex.Escape(matchString) + "$";
		case 16uL:
		{
			bool flag = true;
			try
			{
				Regex.Match(string.Empty, matchString);
			}
			catch (ArgumentException)
			{
				flag = false;
			}
			return flag ? matchString : Regex.Escape(matchString);
		}
		}
		goto IL_00a4;
		IL_00a4:
		return Regex.Escape(matchString);
	}

	public void AddValueStringSearchExpression(string matchString, ulong searchType, bool isReplacePattern)
	{
		AddStringSearchExpression(QueryableValueString, matchString, searchType, isReplacePattern);
	}

	public void AddStringSearchExpression(Expression sourceStringExp, string matchString, ulong searchType, bool isReplacePattern)
	{
		string text = CreateRegularExpressionPattern(matchString, searchType);
		Expression expression = Expression.Constant(text);
		if (isReplacePattern)
		{
			MatchPattern = new StringReplaceQueryPattern(text);
		}
		Expression expression2 = Expression.Constant(RegexOptions.IgnoreCase, typeof(RegexOptions));
		MethodCallExpression expression3 = Expression.Call(typeof(Regex), "Match", null, sourceStringExp, expression, expression2);
		MemberExpression left = Expression.Property(expression3, "Success");
		AddExpression(Expression.OrElse(GetNullOrEmptyExpression(matchString), Expression.Equal(left, Expression.Constant(true))));
	}

	public BinaryExpression GetValueIsConvertibleToDoubleExpression()
	{
		MethodCallExpression left = Expression.Call(typeof(LinqQueryPredicate), "IsConvertibleToDouble", null, QueryableValue);
		return Expression.Equal(left, Expression.Constant(true));
	}

	public MethodCallExpression GetConvertToDoubleExpression()
	{
		return Expression.Call(typeof(Convert), "ToDouble", null, QueryableValue);
	}

	public void AddNumberValueEqualsExpression(double patternNumber, bool isReplacePattern)
	{
		if (isReplacePattern)
		{
			MatchPattern = new NumberReplaceQueryPattern(patternNumber);
		}
		AddExpression(Expression.AndAlso(GetValueIsConvertibleToDoubleExpression(), Expression.Equal(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
	}

	public void AddNumberValueLesserExpression(double patternNumber, bool isReplacePattern)
	{
		if (isReplacePattern)
		{
			MatchPattern = new NumberReplaceQueryPattern(patternNumber);
		}
		AddExpression(Expression.AndAlso(GetValueIsConvertibleToDoubleExpression(), Expression.LessThan(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
	}

	public void AddNumberValueLesserEqualExpression(double patternNumber, bool isReplacePattern)
	{
		if (isReplacePattern)
		{
			MatchPattern = new NumberReplaceQueryPattern(patternNumber);
		}
		AddExpression(Expression.AndAlso(GetValueIsConvertibleToDoubleExpression(), Expression.LessThanOrEqual(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
	}

	public void AddNumberValueGreaterEqualExpression(double patternNumber, bool isReplacePattern)
	{
		if (isReplacePattern)
		{
			MatchPattern = new NumberReplaceQueryPattern(patternNumber);
		}
		AddExpression(Expression.AndAlso(GetValueIsConvertibleToDoubleExpression(), Expression.GreaterThanOrEqual(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
	}

	public void AddNumberValueGreaterExpression(double patternNumber, bool isReplacePattern)
	{
		if (isReplacePattern)
		{
			MatchPattern = new NumberReplaceQueryPattern(patternNumber);
		}
		AddExpression(Expression.AndAlso(GetValueIsConvertibleToDoubleExpression(), Expression.GreaterThan(GetConvertToDoubleExpression(), Expression.Constant(patternNumber))));
	}

	public void AddNumberValueBetweenExpression(double patternNumber1, double patternNumber2, bool isReplacePattern)
	{
		if (isReplacePattern)
		{
			MatchPattern = new NumberReplaceQueryPattern(patternNumber1, patternNumber2);
		}
		MethodCallExpression convertToDoubleExpression = GetConvertToDoubleExpression();
		ConstantExpression constantExpression = Expression.Constant(patternNumber1);
		ConstantExpression constantExpression2 = Expression.Constant(patternNumber2);
		AddExpression(Expression.AndAlso(GetValueIsConvertibleToDoubleExpression(), Expression.Or(Expression.AndAlso(Expression.LessThanOrEqual(constantExpression, constantExpression2), Expression.AndAlso(Expression.LessThanOrEqual(constantExpression, convertToDoubleExpression), Expression.LessThanOrEqual(convertToDoubleExpression, constantExpression2))), Expression.AndAlso(Expression.LessThanOrEqual(constantExpression2, constantExpression), Expression.AndAlso(Expression.LessThanOrEqual(constantExpression2, convertToDoubleExpression), Expression.LessThanOrEqual(convertToDoubleExpression, constantExpression))))));
	}

	public static bool IsConvertibleToDouble(object candidate)
	{
		double result;
		if (candidate is string)
		{
			return double.TryParse((string)candidate, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}
		return candidate != null && (candidate is short || candidate is int || candidate is long || candidate is decimal || candidate is float || candidate is double || candidate is bool);
	}
}

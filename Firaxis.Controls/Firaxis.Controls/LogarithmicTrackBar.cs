using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class LogarithmicTrackBar : TrackBar
{
	private int _midpoint;

	private double _transformedValue;

	private IContainer components = null;

	[Description("Point to display as the middle between the maximum and minimum.  This is used to determine the logarithmic scaling.")]
	[Category("Behavior")]
	public int Midpoint
	{
		get
		{
			return _midpoint;
		}
		set
		{
			if (value < base.Minimum)
			{
				value = (int)Math.Ceiling((double)base.Minimum * 1.01);
			}
			if (value > base.Maximum)
			{
				value = (int)Math.Floor((double)base.Maximum * 0.99);
			}
			_midpoint = value;
		}
	}

	public double TransformedValue
	{
		get
		{
			return _transformedValue;
		}
		set
		{
			_transformedValue = value;
		}
	}

	public LogarithmicTrackBar()
	{
		InitializeComponent();
		base.ValueChanged += LogarithmicTrackBar_ValueChanged;
	}

	private void LogarithmicTrackBar_ValueChanged(object sender, EventArgs e)
	{
		TransformedValue = ConvertValue((double)base.Value);
	}

	private int ConvertValue(double rawValue)
	{
		if (base.Minimum != 0)
		{
			return (int)rawValue;
		}
		if (rawValue <= 0.0)
		{
			return base.Minimum;
		}
		if (rawValue >= 1.0)
		{
			return base.Maximum;
		}
		double num = (double)base.Maximum * Math.Pow(Midpoint, 2.0);
		double num2 = Math.Pow(base.Maximum, 2.0) - (double)(2 * Midpoint * base.Maximum);
		double num3 = num / num2;
		double num4 = num3 * -1.0;
		double num5 = Math.Log(Math.Pow(base.Maximum - Midpoint, 2.0) / Math.Pow(Midpoint, 2.0));
		return (int)(num4 + num3 + Math.Exp(num5 * rawValue));
	}

	private double ConvertValue(int convertedValue)
	{
		if (base.Minimum != 0)
		{
			return convertedValue;
		}
		double num = (double)base.Maximum * Math.Pow(Midpoint, 2.0);
		double num2 = Math.Pow(base.Maximum, 2.0) - (double)(2 * Midpoint * base.Maximum);
		double num3 = num / num2;
		double num4 = num3 * -1.0;
		double num5 = Math.Log(Math.Pow(base.Maximum - Midpoint, 2.0) / Math.Pow(Midpoint, 2.0));
		return Math.Log(((double)convertedValue - num4) / num3) / num5;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}
}

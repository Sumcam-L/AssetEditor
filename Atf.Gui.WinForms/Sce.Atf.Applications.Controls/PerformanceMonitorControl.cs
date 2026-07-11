using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Applications.Controls;

public class PerformanceMonitorControl : UserControl
{
	public class ControlAdapter : IPerformanceTarget
	{
		private readonly Control m_control;

		public event EventHandler EventOccurred
		{
			add
			{
				if (this.m_eventOccurred == null)
				{
					m_control.Paint += PaintEventHandler;
				}
				m_eventOccurred += value;
			}
			remove
			{
				m_eventOccurred -= value;
				if (this.m_eventOccurred == null)
				{
					m_control.Paint -= PaintEventHandler;
				}
			}
		}

		private event EventHandler m_eventOccurred;

		public ControlAdapter(Control control)
		{
			m_control = control;
		}

		public void DoEvent()
		{
			m_control.Refresh();
		}

		private void PaintEventHandler(object sender, PaintEventArgs e)
		{
			this.m_eventOccurred.Raise(this, EventArgs.Empty);
		}
	}

	private IPerformanceTarget m_target;

	private string m_targetName = string.Empty;

	private int m_intervalFrameCount;

	private int m_frameCountSinceReset;

	private TimeSpan m_lastIntervalStart;

	private float m_fps;

	private float m_maxFps;

	private long m_managedBytes;

	private long m_unmanagedBytes;

	private readonly Stopwatch m_stopWatch = new Stopwatch();

	private readonly Timer m_timer = new Timer();

	private const int Interval = 2500;

	private const int StressTestDuration = 5000;

	private IContainer components = null;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label FpsLabel;

	private Label ManagedMemoryLabel;

	private Label MaxFpsLabel;

	private Button ResetBtn;

	private Button ClipboardBtn;

	private Label controlNameLabel;

	private Button GarbageCollectionBtn;

	private Label NumPaintsLabel;

	private Label label6;

	private Button StressTestBtn;

	private Label label5;

	private Label UnmanagedMemoryLabel;

	public PerformanceMonitorControl()
	{
		InitializeComponent();
		m_stopWatch.Start();
		m_lastIntervalStart = m_stopWatch.Elapsed;
		m_timer.Interval = 2500;
		m_timer.Tick += TimerTick;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		m_timer.Enabled = base.Visible;
	}

	public void Bind(Control target, string name)
	{
		ControlAdapter target2 = new ControlAdapter(target);
		Bind(target2, name);
	}

	public void Bind(IPerformanceTarget target, string name)
	{
		if (m_target != target || !m_targetName.Equals(name))
		{
			if (m_target != null)
			{
				m_target.EventOccurred -= TargetEventHandler;
			}
			m_target = target;
			m_targetName = name;
			if (m_target != null)
			{
				m_target.EventOccurred += TargetEventHandler;
			}
			controlNameLabel.Text = m_targetName;
		}
	}

	private void TargetEventHandler(object sender, EventArgs e)
	{
		if (m_timer.Enabled)
		{
			m_intervalFrameCount++;
			m_frameCountSinceReset++;
		}
	}

	private void TimerTick(object sender, EventArgs e)
	{
		TimeSpan elapsed = m_stopWatch.Elapsed;
		TimeSpan timeSpan = elapsed - m_lastIntervalStart;
		m_lastIntervalStart = elapsed;
		m_fps = (float)((double)(m_intervalFrameCount * 1000) / timeSpan.TotalMilliseconds);
		m_intervalFrameCount = 0;
		if (m_fps > m_maxFps)
		{
			m_maxFps = m_fps;
		}
		m_managedBytes = GC.GetTotalMemory(forceFullCollection: false);
		Process currentProcess = Process.GetCurrentProcess();
		m_unmanagedBytes = currentProcess.WorkingSet64;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		FpsLabel.Text = m_fps.ToString();
		MaxFpsLabel.Text = m_maxFps.ToString();
		NumPaintsLabel.Text = m_frameCountSinceReset.ToString();
		ManagedMemoryLabel.Text = (m_managedBytes / 1024).ToString("N0") + " K";
		UnmanagedMemoryLabel.Text = (m_unmanagedBytes / 1024).ToString("N0") + " K";
		Refresh();
	}

	private void ResetBtn_Click(object sender, EventArgs e)
	{
		m_fps = 0f;
		m_maxFps = 0f;
		m_frameCountSinceReset = 0;
		UpdateDisplay();
	}

	private void ClipboardBtn_Click(object sender, EventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Control's Name: {0}".Localize(), controlNameLabel.Text);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Max frames per second: {0}".Localize(), m_maxFps);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Total frame count: {0}".Localize(), m_frameCountSinceReset);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Managed memory (KB): {0}".Localize(), m_managedBytes / 1024);
		stringBuilder.AppendLine();
		stringBuilder.AppendFormat("Unmanaged memory (KB): {0}".Localize(), m_unmanagedBytes / 1024);
		stringBuilder.AppendLine();
		Clipboard.SetText(stringBuilder.ToString());
	}

	private void GarbageCollectionBtn_Click(object sender, EventArgs e)
	{
		GC.Collect();
		UpdateDisplay();
	}

	private void StressTestBtn_Click(object sender, EventArgs e)
	{
		ResetBtn_Click(sender, e);
		m_target.EventOccurred -= TargetEventHandler;
		string text = StressTestBtn.Text;
		StressTestBtn.Enabled = false;
		m_timer.Tick -= TimerTick;
		TimeSpan elapsed = m_stopWatch.Elapsed;
		List<long> list = new List<long>(1000);
		try
		{
			while (true)
			{
				long elapsedTicks = m_stopWatch.ElapsedTicks;
				m_target.DoEvent();
				long elapsedTicks2 = m_stopWatch.ElapsedTicks;
				list.Add(elapsedTicks2 - elapsedTicks);
				TargetEventHandler(sender, e);
				TimeSpan elapsed2 = m_stopWatch.Elapsed;
				TimeSpan timeSpan = elapsed2 - elapsed;
				if (timeSpan.TotalMilliseconds >= 5000.0)
				{
					break;
				}
				string value = (5 - (int)timeSpan.TotalSeconds).ToString();
				if (!StressTestBtn.Text.Equals(value))
				{
					StressTestBtn.Text = value;
					StressTestBtn.Refresh();
				}
			}
		}
		finally
		{
			StressTestBtn.Text = text;
			StressTestBtn.Enabled = true;
			m_target.EventOccurred += TargetEventHandler;
			m_timer.Interval = 2500;
			m_timer.Tick += TimerTick;
			TimerTick(sender, e);
			if (list.Count > 0)
			{
				list.Sort();
				long num = list[list.Count / 2];
				long num2 = list[0];
				long num3 = list[list.Count - 1];
				long num4 = num * 1000 / Stopwatch.Frequency;
				long num5 = num2 * 1000 / Stopwatch.Frequency;
				long num6 = num3 * 1000 / Stopwatch.Frequency;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("Target: {0}".Localize("'target' is the Windows Control that is being analyzed"), m_targetName);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("Number of rendered frames: {0}".Localize(), list.Count);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("Mean rendering time: {0}ms or {1} ticks".Localize(), num4, num);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("Fastest rendering time: {0}ms or {1} ticks".Localize(), num5, num2);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("Slowest rendering time: {0}ms or {1} ticks".Localize(), num6, num3);
				stringBuilder.AppendLine();
				Clipboard.SetText(stringBuilder.ToString());
				MessageBox.Show(stringBuilder.ToString(), "The performance report is in the clipboard".Localize());
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			m_timer.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.Controls.PerformanceMonitorControl));
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.FpsLabel = new System.Windows.Forms.Label();
		this.ManagedMemoryLabel = new System.Windows.Forms.Label();
		this.MaxFpsLabel = new System.Windows.Forms.Label();
		this.ResetBtn = new System.Windows.Forms.Button();
		this.ClipboardBtn = new System.Windows.Forms.Button();
		this.controlNameLabel = new System.Windows.Forms.Label();
		this.GarbageCollectionBtn = new System.Windows.Forms.Button();
		this.NumPaintsLabel = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.StressTestBtn = new System.Windows.Forms.Button();
		this.label5 = new System.Windows.Forms.Label();
		this.UnmanagedMemoryLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.label3, "label3");
		this.label3.Name = "label3";
		resources.ApplyResources(this.FpsLabel, "FpsLabel");
		this.FpsLabel.Name = "FpsLabel";
		resources.ApplyResources(this.ManagedMemoryLabel, "ManagedMemoryLabel");
		this.ManagedMemoryLabel.Name = "ManagedMemoryLabel";
		resources.ApplyResources(this.MaxFpsLabel, "MaxFpsLabel");
		this.MaxFpsLabel.Name = "MaxFpsLabel";
		this.ResetBtn.AutoEllipsis = true;
		resources.ApplyResources(this.ResetBtn, "ResetBtn");
		this.ResetBtn.Name = "ResetBtn";
		this.ResetBtn.UseVisualStyleBackColor = true;
		this.ResetBtn.Click += new System.EventHandler(ResetBtn_Click);
		this.ClipboardBtn.AutoEllipsis = true;
		resources.ApplyResources(this.ClipboardBtn, "ClipboardBtn");
		this.ClipboardBtn.Name = "ClipboardBtn";
		this.ClipboardBtn.UseVisualStyleBackColor = true;
		this.ClipboardBtn.Click += new System.EventHandler(ClipboardBtn_Click);
		this.controlNameLabel.AutoEllipsis = true;
		resources.ApplyResources(this.controlNameLabel, "controlNameLabel");
		this.controlNameLabel.Name = "controlNameLabel";
		resources.ApplyResources(this.GarbageCollectionBtn, "GarbageCollectionBtn");
		this.GarbageCollectionBtn.Name = "GarbageCollectionBtn";
		this.GarbageCollectionBtn.UseVisualStyleBackColor = true;
		this.GarbageCollectionBtn.Click += new System.EventHandler(GarbageCollectionBtn_Click);
		resources.ApplyResources(this.NumPaintsLabel, "NumPaintsLabel");
		this.NumPaintsLabel.Name = "NumPaintsLabel";
		resources.ApplyResources(this.label6, "label6");
		this.label6.Name = "label6";
		resources.ApplyResources(this.StressTestBtn, "StressTestBtn");
		this.StressTestBtn.Name = "StressTestBtn";
		this.StressTestBtn.UseVisualStyleBackColor = true;
		this.StressTestBtn.Click += new System.EventHandler(StressTestBtn_Click);
		resources.ApplyResources(this.label5, "label5");
		this.label5.Name = "label5";
		resources.ApplyResources(this.UnmanagedMemoryLabel, "UnmanagedMemoryLabel");
		this.UnmanagedMemoryLabel.Name = "UnmanagedMemoryLabel";
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this.UnmanagedMemoryLabel);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.StressTestBtn);
		base.Controls.Add(this.NumPaintsLabel);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.GarbageCollectionBtn);
		base.Controls.Add(this.controlNameLabel);
		base.Controls.Add(this.ClipboardBtn);
		base.Controls.Add(this.ResetBtn);
		base.Controls.Add(this.MaxFpsLabel);
		base.Controls.Add(this.ManagedMemoryLabel);
		base.Controls.Add(this.FpsLabel);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Name = "PerformanceMonitorControl";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

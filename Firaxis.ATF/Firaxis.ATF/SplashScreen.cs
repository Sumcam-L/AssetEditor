using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Firaxis.ATF.Properties;
using Firaxis.CivTech;
using Firaxis.Threading;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

public class SplashScreen : Form
{
	private struct LogMsg
	{
		public LogEventType eventType;

		public string source;

		public string text;

		public override string ToString()
		{
			if (text.EndsWith("\n"))
			{
				return $"{eventType}: {text}";
			}
			return $"{eventType}: {text}\n";
		}
	}

	private bool m_showingOutput = true;

	private const int kMaxLogItemsPerOutputUpdate = 50;

	private const uint kMinimumOutputQuietPeriodMS = 100u;

	private uint m_nextOutputTick;

	private ReaderWriterLockSlim m_logQueueLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private List<LogMsg> m_logQueue = new List<LogMsg>();

	private IQuietTimeAction m_logFlusher;

	private Size m_sizeWithOutput = new Size(720, 640);

	private Size m_sizeNoOutput = new Size(720, 500);

	private ISplashScreenOutputWriter m_toolOutputWriter;

	private IContainer components;

	private Label m_caption;

	private SplitContainer m_splitter;

	private RichTextBox m_textBox;

	private PictureBox pictureBox1;

	public Icon CaptionImage
	{
		set
		{
			BeginInvokeIfPossible(delegate
			{
				SetIconImpl(value);
			});
		}
	}

	public string Message
	{
		set
		{
			m_caption.BeginInvokeIfPossible(delegate
			{
				SetMessageImpl(value);
			});
		}
	}

	public SplashScreen(string app)
	{
		InitializeComponent();
		Text = app;
		m_textBox.Multiline = true;
		m_textBox.WordWrap = false;
		m_textBox.ReadOnly = true;
		m_logFlusher = new QuietTimeAction(125, delegate
		{
			WriteQueueOutput();
		});
		base.HandleCreated += SplashScreen_HandleCreated;
		Cursor.Current = Cursors.AppStarting;
	}

	public void ClearOutput()
	{
		InvokeIfRequired(ClearOutputImpl);
	}

	public void HookOutputWriter(ISplashScreenOutputWriter writer)
	{
		InvokeIfRequired(delegate
		{
			HookOutputWriterImpl(writer);
		});
	}

	public void HideOutputWindow()
	{
		if (m_showingOutput)
		{
			InvokeIfRequired(HideOutputWindowImpl);
		}
	}

	public void ShowOutputWindow()
	{
		if (!m_showingOutput)
		{
			InvokeIfRequired(ShowOutputWindowImpl);
		}
	}

	public new void Show()
	{
		InvokeIfRequired(ShowImpl);
	}

	public new void Close()
	{
		InvokeIfRequired(CloseImpl);
	}

	protected override void Dispose(bool disposing)
	{
		InvokeIfRequired(delegate
		{
			DisposeImpl(disposing);
		});
	}

	private void SplashScreen_HandleCreated(object sender, EventArgs e)
	{
		HideOutputWindow();
	}

	private void Writer_Logger(string context, OutputMessageType msgType, OutputMessageVerbosity msgVerb, string text)
	{
		ShowOutputWindow();
		QueueLogOutput(new LogMsg
		{
			eventType = CategorizedOutputs.ConvertEventType(msgType),
			source = context,
			text = text
		});
		if (TimeForMoreOutput())
		{
			WriteQueueOutput();
		}
	}

	private bool TimeForMoreOutput()
	{
		return NativeMethods.GetTickCount() >= m_nextOutputTick;
	}

	private void UpdateNextOutputTime()
	{
		m_nextOutputTick = NativeMethods.GetTickCount() + 100;
	}

	private void WriteQueueOutput()
	{
		LogMsg[] messages = new LogMsg[0];
		using (ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_logQueueLock))
		{
			int num = Math.Min(m_logQueue.Count, 50);
			if (num > 0)
			{
				messages = m_logQueue.GetRange(0, num).ToArray();
				using (new ScopedWriterLock(upgrableReadLock))
				{
					m_logQueue.RemoveRange(0, num);
				}
			}
		}
		WriteLogMessages(messages);
		UpdateNextOutputTime();
		EnqueueQuietTimeFlush();
	}

	private void EnqueueQuietTimeFlush()
	{
		m_logFlusher?.UpdateLastChangeTime();
	}

	private void QueueLogOutput(LogMsg message)
	{
		using (new ScopedWriterLock(m_logQueueLock))
		{
			m_logQueue.Add(message);
		}
	}

	private void WriteLogMessages(LogMsg[] messages)
	{
		if (messages.Length != 0 && !m_textBox.IsDisposed)
		{
			m_textBox.BeginInvokeIfPossible(delegate
			{
				WriteLogMessagesImpl(messages);
			});
		}
	}

	private void WriteLogMessagesImpl(LogMsg[] messages)
	{
		if (!m_textBox.IsDisposed)
		{
			foreach (LogMsg message in messages)
			{
				WriteLogMessage(message);
			}
			m_textBox.ScrollToCaret();
		}
	}

	private void WriteLogMessage(LogMsg message)
	{
		Color color = Color.White;
		if (message.eventType == LogEventType.Error || message.text.StartsWith("error", StringComparison.CurrentCultureIgnoreCase))
		{
			color = Color.Red;
		}
		else if (message.eventType == LogEventType.Warning || message.text.StartsWith("warning", StringComparison.CurrentCultureIgnoreCase))
		{
			color = Color.Orange;
		}
		if (!m_textBox.IsDisposed)
		{
			if (m_textBox.SelectionColor != color)
			{
				m_textBox.SelectionColor = color;
			}
			m_textBox.AppendText(message.ToString());
		}
	}

	private void SetIconImpl(Icon icon)
	{
		if (!base.IsDisposed)
		{
			base.Icon = icon;
		}
	}

	private void SetMessageImpl(string msg)
	{
		if (!base.IsDisposed && !(m_caption.Text == msg))
		{
			m_caption.Text = msg;
			if (base.IsHandleCreated && !base.IsDisposed)
			{
				Update();
			}
			m_caption.Update();
		}
	}

	private void ClearOutputImpl()
	{
		if (!base.IsDisposed && !m_textBox.IsDisposed)
		{
			m_textBox.Clear();
		}
	}

	private void DisposeImpl()
	{
		if (!base.IsDisposed)
		{
			m_logFlusher?.Dispose();
			Dispose();
		}
	}

	private void ShowImpl()
	{
		if (!base.IsDisposed)
		{
			base.Show();
		}
	}

	private void CloseImpl()
	{
		if (!base.IsDisposed)
		{
			base.Close();
		}
	}

	private void DisposeImpl(bool disposing)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (disposing)
		{
			if (m_toolOutputWriter != null)
			{
				m_toolOutputWriter.Logger -= Writer_Logger;
				m_toolOutputWriter = null;
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void HookOutputWriterImpl(ISplashScreenOutputWriter writer)
	{
		if (!base.IsDisposed)
		{
			if (m_toolOutputWriter != null)
			{
				m_toolOutputWriter.Logger -= Writer_Logger;
				m_toolOutputWriter = null;
			}
			if (writer != null)
			{
				m_toolOutputWriter = writer;
				m_toolOutputWriter.Logger += Writer_Logger;
			}
		}
	}

	private void HideOutputWindowImpl()
	{
		if (m_showingOutput)
		{
			m_showingOutput = false;
			base.Size = m_sizeNoOutput;
			m_splitter.Panel2Collapsed = true;
			m_splitter.Panel2.Hide();
		}
	}

	private void ShowOutputWindowImpl()
	{
		if (!m_showingOutput && !m_textBox.IsDisposed)
		{
			m_showingOutput = true;
			m_textBox.Clear();
			base.Size = m_sizeWithOutput;
			m_splitter.Panel2Collapsed = false;
			m_splitter.Panel2.Show();
		}
	}

	private void BeginInvokeIfPossible(Action action)
	{
		ThreadedFormHelpers.BeginInvokeIfPossible(this, action);
	}

	private void InvokeIfRequired(Action action)
	{
		ThreadedFormHelpers.InvokeIfRequired(this, action);
	}

	private void InitializeComponent()
	{
		this.m_caption = new System.Windows.Forms.Label();
		this.m_splitter = new System.Windows.Forms.SplitContainer();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.m_textBox = new System.Windows.Forms.RichTextBox();
		((System.ComponentModel.ISupportInitialize)this.m_splitter).BeginInit();
		this.m_splitter.Panel1.SuspendLayout();
		this.m_splitter.Panel2.SuspendLayout();
		this.m_splitter.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.m_caption.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.m_caption.Font = new System.Drawing.Font("Tahoma", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_caption.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.m_caption.Location = new System.Drawing.Point(0, 432);
		this.m_caption.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
		this.m_caption.Name = "m_caption";
		this.m_caption.Size = new System.Drawing.Size(704, 23);
		this.m_caption.TabIndex = 0;
		this.m_caption.Text = "Starting...";
		this.m_caption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_splitter.BackColor = System.Drawing.Color.Black;
		this.m_splitter.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.m_splitter.IsSplitterFixed = true;
		this.m_splitter.Location = new System.Drawing.Point(0, 0);
		this.m_splitter.Name = "m_splitter";
		this.m_splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.m_splitter.Panel1.Controls.Add(this.pictureBox1);
		this.m_splitter.Panel1.Controls.Add(this.m_caption);
		this.m_splitter.Panel2.BackColor = System.Drawing.Color.Black;
		this.m_splitter.Panel2.Controls.Add(this.m_textBox);
		this.m_splitter.Size = new System.Drawing.Size(704, 601);
		this.m_splitter.SplitterDistance = 455;
		this.m_splitter.TabIndex = 1;
		this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
		this.pictureBox1.Image = Firaxis.ATF.Properties.Resources.Splashscreenvert;
		this.pictureBox1.Location = new System.Drawing.Point(0, 0);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(704, 427);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		this.m_textBox.BackColor = System.Drawing.Color.FromArgb(53, 53, 53);
		this.m_textBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_textBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.m_textBox.Location = new System.Drawing.Point(0, 0);
		this.m_textBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.Size = new System.Drawing.Size(704, 142);
		this.m_textBox.TabIndex = 0;
		this.m_textBox.Text = "";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.ClientSize = new System.Drawing.Size(704, 601);
		base.ControlBox = false;
		base.Controls.Add(this.m_splitter);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(720, 500);
		base.Name = "SplashScreen";
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Asset Creation Tool";
		this.m_splitter.Panel1.ResumeLayout(false);
		this.m_splitter.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_splitter).EndInit();
		this.m_splitter.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
	}
}

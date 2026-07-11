using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Applications.NetworkTargetServices;

namespace Sce.Atf.Applications;

internal class OscDialog : Form
{
	private readonly OscService m_oscService;

	private OscCommandReceiver m_commandReceiver;

	private IContainer components = null;

	private Label label2;

	private Label label3;

	private ListView m_listView;

	private ColumnHeader oscAddress;

	private ColumnHeader className;

	private ColumnHeader propertyName;

	private ColumnHeader propertyType;

	private Button m_okButton;

	private Button m_cancelButton;

	private Button m_toClipboardButton;

	private TextBox m_statusTextBox;

	private Label label5;

	private Panel panel1;

	private TableLayoutPanel tableLayoutPanel2;

	private TextBox m_destinationIPAddress;

	private Label label8;

	private Label label12;

	private TextBox m_destinationPortNumber;

	private Label label13;

	private Panel panel2;

	private TableLayoutPanel tableLayoutPanel1;

	private Label m_hostName;

	private Label label4;

	private TextBox m_receivingPortNumber;

	private Label label6;

	private Label label1;

	private Label label7;

	private Button m_noDestinationButton;

	private ComboBox m_receivingIPAddresses;

	public OscDialog(OscService oscService, OscCommandReceiver commandReceiver)
	{
		InitializeComponent();
		SuspendLayout();
		m_oscService = oscService;
		m_commandReceiver = commandReceiver;
		string text;
		try
		{
			text = Dns.GetHostName();
		}
		catch (SocketException)
		{
			text = "<not available>";
		}
		m_hostName.Text = text;
		int num = 0;
		int selectedIndex = 0;
		foreach (IPAddress localIPAddress in OscService.GetLocalIPAddresses())
		{
			m_receivingIPAddresses.Items.Add(localIPAddress.ToString());
			if (localIPAddress.Equals(m_oscService.ReceivingIPAddress))
			{
				selectedIndex = num;
			}
			num++;
		}
		m_receivingIPAddresses.SelectedIndex = selectedIndex;
		m_receivingPortNumber.Text = m_oscService.ReceivingPort.ToString(CultureInfo.InvariantCulture);
		m_statusTextBox.Text = m_oscService.StatusMessage;
		m_destinationIPAddress.Text = m_oscService.DestinationEndpoint.Address.ToString();
		m_destinationPortNumber.Text = m_oscService.DestinationEndpoint.Port.ToString(CultureInfo.InvariantCulture);
		List<OscService.OscAddressInfo> list = new List<OscService.OscAddressInfo>(m_oscService.AddressInfos);
		list.Sort((OscService.OscAddressInfo info1, OscService.OscAddressInfo info2) => info1.Address.CompareTo(info2.Address));
		ListView.ListViewItemCollection items = m_listView.Items;
		foreach (OscService.OscAddressInfo item in list)
		{
			ListViewItem value = new ListViewItem(new string[4]
			{
				item.Address,
				item.PropertyName,
				PropertyTypeToReadableString(item.PropertyType),
				item.CompatibleType.ToString()
			});
			items.Add(value);
		}
		if (m_commandReceiver != null)
		{
			foreach (string oscAddress in m_commandReceiver.GetOscAddresses())
			{
				ListViewItem value2 = new ListViewItem(new string[4] { oscAddress, "n/a", "n/a", "n/a" });
				items.Add(value2);
			}
		}
		ResumeLayout();
	}

	private void toClipboardButton_Click(object sender, EventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Open Sound Control (OSC) configuration information");
		stringBuilder.AppendLine("--------------------------------------------------");
		stringBuilder.AppendLine("This computer's host name: " + m_hostName.Text);
		stringBuilder.AppendLine("This app's local IP address: " + m_oscService.ReceivingIPAddress);
		stringBuilder.AppendLine("This app's port # for receiving OSC messages: " + m_oscService.ReceivingPort);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Destination IP address: " + m_destinationIPAddress.Text);
		stringBuilder.AppendLine("Destination port #: " + m_destinationPortNumber.Text);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Status:");
		stringBuilder.AppendLine(m_oscService.StatusMessage);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("OSC Address\tproperty name\tproperty type\tC# class name");
		foreach (OscService.OscAddressInfo addressInfo in m_oscService.AddressInfos)
		{
			stringBuilder.AppendLine(addressInfo.Address + "\t" + addressInfo.PropertyName + "\t" + PropertyTypeToReadableString(addressInfo.PropertyType) + "\t" + addressInfo.CompatibleType.ToString());
		}
		foreach (string oscAddress in m_commandReceiver.GetOscAddresses())
		{
			stringBuilder.AppendLine(oscAddress + "\tn/a\tn/a\tn/a\t");
		}
		Clipboard.SetText(stringBuilder.ToString());
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		IPEndPoint iPEndPoint = TcpIpTargetInfo.TryParseIPEndPoint(m_receivingIPAddresses.Items[m_receivingIPAddresses.SelectedIndex].ToString() + ":" + m_receivingPortNumber.Text);
		if (iPEndPoint == null)
		{
			MessageBox.Show("The receiving port number or IP address are not correctly formatted.".Localize());
			base.DialogResult = DialogResult.None;
			return;
		}
		IPEndPoint iPEndPoint2 = TcpIpTargetInfo.TryParseIPEndPoint(m_destinationIPAddress.Text + ":" + m_destinationPortNumber.Text);
		if (iPEndPoint2 == null)
		{
			MessageBox.Show("The destination port number or IP address are not correctly formatted.".Localize());
			base.DialogResult = DialogResult.None;
		}
		else
		{
			m_oscService.DestinationEndpoint = iPEndPoint2;
			m_oscService.ReceivingEndpoint = iPEndPoint;
		}
	}

	private string PropertyTypeToReadableString(Type propertyType)
	{
		if (propertyType == typeof(int))
		{
			return "int";
		}
		if (propertyType == typeof(float))
		{
			return "float";
		}
		if (propertyType == typeof(string))
		{
			return "string";
		}
		if (propertyType == typeof(bool))
		{
			return "bool";
		}
		return propertyType.ToString();
	}

	private void disableButton_Click(object sender, EventArgs e)
	{
		m_destinationIPAddress.Text = "255.255.255.255";
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.OscDialog));
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.m_listView = new System.Windows.Forms.ListView();
		this.oscAddress = new System.Windows.Forms.ColumnHeader();
		this.propertyName = new System.Windows.Forms.ColumnHeader();
		this.propertyType = new System.Windows.Forms.ColumnHeader();
		this.className = new System.Windows.Forms.ColumnHeader();
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_toClipboardButton = new System.Windows.Forms.Button();
		this.m_statusTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.label8 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.m_destinationPortNumber = new System.Windows.Forms.TextBox();
		this.m_destinationIPAddress = new System.Windows.Forms.TextBox();
		this.m_noDestinationButton = new System.Windows.Forms.Button();
		this.label13 = new System.Windows.Forms.Label();
		this.panel2 = new System.Windows.Forms.Panel();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.m_hostName = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_receivingPortNumber = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.m_receivingIPAddresses = new System.Windows.Forms.ComboBox();
		this.label7 = new System.Windows.Forms.Label();
		this.panel1.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		this.panel2.SuspendLayout();
		this.tableLayoutPanel1.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.label3, "label3");
		this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label3.Name = "label3";
		resources.ApplyResources(this.m_listView, "m_listView");
		this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.oscAddress, this.propertyName, this.propertyType, this.className });
		this.m_listView.Name = "m_listView";
		this.m_listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
		this.m_listView.UseCompatibleStateImageBehavior = false;
		this.m_listView.View = System.Windows.Forms.View.Details;
		resources.ApplyResources(this.oscAddress, "oscAddress");
		resources.ApplyResources(this.propertyName, "propertyName");
		resources.ApplyResources(this.propertyType, "propertyType");
		resources.ApplyResources(this.className, "className");
		resources.ApplyResources(this.m_okButton, "m_okButton");
		this.m_okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(okButton_Click);
		resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.m_toClipboardButton, "m_toClipboardButton");
		this.m_toClipboardButton.Name = "m_toClipboardButton";
		this.m_toClipboardButton.UseVisualStyleBackColor = true;
		this.m_toClipboardButton.Click += new System.EventHandler(toClipboardButton_Click);
		resources.ApplyResources(this.m_statusTextBox, "m_statusTextBox");
		this.m_statusTextBox.Name = "m_statusTextBox";
		this.m_statusTextBox.ReadOnly = true;
		resources.ApplyResources(this.label5, "label5");
		this.label5.Name = "label5";
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel1.Controls.Add(this.tableLayoutPanel2);
		resources.ApplyResources(this.panel1, "panel1");
		this.panel1.Name = "panel1";
		resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
		this.tableLayoutPanel2.Controls.Add(this.label8, 0, 0);
		this.tableLayoutPanel2.Controls.Add(this.label12, 0, 1);
		this.tableLayoutPanel2.Controls.Add(this.m_destinationPortNumber, 1, 1);
		this.tableLayoutPanel2.Controls.Add(this.m_destinationIPAddress, 1, 0);
		this.tableLayoutPanel2.Controls.Add(this.m_noDestinationButton, 1, 2);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		resources.ApplyResources(this.label8, "label8");
		this.label8.Name = "label8";
		resources.ApplyResources(this.label12, "label12");
		this.label12.Name = "label12";
		resources.ApplyResources(this.m_destinationPortNumber, "m_destinationPortNumber");
		this.m_destinationPortNumber.Name = "m_destinationPortNumber";
		resources.ApplyResources(this.m_destinationIPAddress, "m_destinationIPAddress");
		this.m_destinationIPAddress.Name = "m_destinationIPAddress";
		resources.ApplyResources(this.m_noDestinationButton, "m_noDestinationButton");
		this.m_noDestinationButton.Name = "m_noDestinationButton";
		this.m_noDestinationButton.UseVisualStyleBackColor = true;
		this.m_noDestinationButton.Click += new System.EventHandler(disableButton_Click);
		resources.ApplyResources(this.label13, "label13");
		this.label13.Name = "label13";
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel2.Controls.Add(this.tableLayoutPanel1);
		resources.ApplyResources(this.panel2, "panel2");
		this.panel2.Name = "panel2";
		resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
		this.tableLayoutPanel1.Controls.Add(this.m_hostName, 1, 2);
		this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
		this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
		this.tableLayoutPanel1.Controls.Add(this.m_receivingPortNumber, 1, 1);
		this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
		this.tableLayoutPanel1.Controls.Add(this.m_receivingIPAddresses, 1, 0);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.m_hostName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		resources.ApplyResources(this.m_hostName, "m_hostName");
		this.m_hostName.Name = "m_hostName";
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label4, "label4");
		this.label4.Name = "label4";
		resources.ApplyResources(this.m_receivingPortNumber, "m_receivingPortNumber");
		this.m_receivingPortNumber.Name = "m_receivingPortNumber";
		resources.ApplyResources(this.label6, "label6");
		this.label6.Name = "label6";
		this.m_receivingIPAddresses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_receivingIPAddresses.FormattingEnabled = true;
		resources.ApplyResources(this.m_receivingIPAddresses, "m_receivingIPAddresses");
		this.m_receivingIPAddresses.Name = "m_receivingIPAddresses";
		resources.ApplyResources(this.label7, "label7");
		this.label7.Name = "label7";
		base.AcceptButton = this.m_okButton;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_cancelButton;
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_statusTextBox);
		base.Controls.Add(this.label13);
		base.Controls.Add(this.m_toClipboardButton);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Controls.Add(this.m_listView);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.panel2);
		base.Name = "OscDialog";
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		this.panel1.ResumeLayout(false);
		this.tableLayoutPanel2.ResumeLayout(false);
		this.tableLayoutPanel2.PerformLayout();
		this.panel2.ResumeLayout(false);
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

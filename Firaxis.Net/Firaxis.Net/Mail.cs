using System;
using System.Collections.Generic;
using System.Net.Mail;
using Firaxis.Error;
using Firaxis.Net.Properties;

namespace Firaxis.Net;

public class Mail
{
	private static string sm_sSmtpClientName = Resources.SMTPClientName;

	private static List<string> sm_vctAttachmentList = new List<string>();

	public static string SmptClientName
	{
		get
		{
			return sm_sSmtpClientName;
		}
		set
		{
			sm_sSmtpClientName = value;
		}
	}

	public static List<string> Attachments => sm_vctAttachmentList;

	public static bool SendMessage(string sTo, string sSubject, string sMessageBody)
	{
		return SendMessage(sTo, sSubject, sMessageBody, MailPriority.Normal);
	}

	public static bool SendMessage(string sTo, string sSubject, string sMessageBody, MailPriority priority)
	{
		return SendMessage(GetDefaultSender(), sTo, sSubject, sMessageBody, priority);
	}

	public static bool SendMessage(MailAddress from, string sTo, string sSubject, string sMessageBody, MailPriority priority)
	{
		List<string> list = new List<string>();
		list.Add(sTo);
		return SendMessage(from, list, sSubject, sMessageBody, priority);
	}

	public static bool SendMessage(List<string> to, string sSubject, string sMessageBody, MailPriority priority)
	{
		return SendMessage(GetDefaultSender(), to, sSubject, sMessageBody, priority);
	}

	public static bool SendMessage(MailAddress from, List<string> to, string sSubject, string sMessageBody, MailPriority priority)
	{
		try
		{
			MailMessage mailMessage = new MailMessage();
			mailMessage.From = from;
			foreach (string item2 in to)
			{
				mailMessage.To.Add(new MailAddress(item2));
			}
			mailMessage.Subject = sSubject;
			mailMessage.Body = sMessageBody;
			mailMessage.Priority = priority;
			foreach (string sm_vctAttachment in sm_vctAttachmentList)
			{
				Attachment item = new Attachment(sm_vctAttachment);
				mailMessage.Attachments.Add(item);
			}
			SmtpClient smtpClient = new SmtpClient(sm_sSmtpClientName);
			smtpClient.Send(mailMessage);
			return true;
		}
		catch (Exception ex)
		{
			ExceptionLogger.Log(ex);
			Console.WriteLine("E-Mail Send Failed: " + ex.Message);
			return false;
		}
	}

	private static MailAddress GetDefaultSender()
	{
		UserInfo current = UserInfo.GetCurrent();
		return new MailAddress(current.FirstName + "." + current.LastName + "@firaxis.com", current.DisplayName);
	}
}

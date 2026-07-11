using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Firaxis.AssetCloudFramework.Properties;
using Firaxis.CivTech;
using Firaxis.Error;

namespace Firaxis.AssetCloudFramework;

public static class ClientHelper
{
	private static Uri CreateURI(string service, params string[] parameters)
	{
		string text = string.Empty;
		if (parameters != null)
		{
			List<string> list = parameters.Where((string item) => !string.IsNullOrEmpty(item)).ToList();
			if (list.Count > 0)
			{
				for (int num = 0; num < list.Count; num++)
				{
					int num2 = list[num].IndexOf("=") + 1;
					string text2 = list[num].Substring(0, num2);
					string stringToEscape = list[num].Substring(num2);
					stringToEscape = Uri.EscapeDataString(stringToEscape);
					list[num] = text2 + stringToEscape;
				}
				text = "?" + string.Join("&", list);
			}
		}
		try
		{
			string processName = Process.GetCurrentProcess().ProcessName;
			text = (string.IsNullOrEmpty(text) ? $"?processName={processName}" : $"{text}&processName={processName}");
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
		}
		return new Uri(Resources.CloudServerAddress + service + text);
	}

	private static string SendRequest<TService>(string method, TService request, params string[] parameters) where TService : IServiceNameProvider
	{
		string result = string.Empty;
		try
		{
			if (request != null)
			{
				if (request is IValidateService validateService && !validateService.Validate())
				{
					ExceptionLogger.Log(string.Empty, "ERROR - Failed validation");
					return "ERROR - Failed validation";
				}
			}
			else
			{
				request = Activator.CreateInstance<TService>();
			}
			Uri requestUri = CreateURI(request.GetServiceName(), parameters);
			WebRequest webRequest = WebRequest.Create(requestUri);
			webRequest.ContentType = "text/xml";
			webRequest.Method = method;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				SerializationHelper.Serialize(memoryStream, request);
				long num = (webRequest.ContentLength = memoryStream.Length);
				Stream requestStream = webRequest.GetRequestStream();
				requestStream.Write(memoryStream.ToArray(), 0, (int)num);
				requestStream.Close();
				memoryStream.Close();
			}
			using WebResponse webResponse = webRequest.GetResponse();
			using Stream stream = webResponse.GetResponseStream();
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			result = streamReader.ReadToEnd();
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
			result = string.Empty;
		}
		return result;
	}

	private static TService SendRequestObjectResponse<TService>(string method, TService request, params string[] parameters) where TService : IServiceNameProvider
	{
		TService result = default(TService);
		try
		{
			if (request != null)
			{
				if (request is IValidateService validateService && !validateService.Validate())
				{
					ExceptionLogger.Log(string.Empty, "ERROR - Failed validation");
					return result;
				}
			}
			else
			{
				request = Activator.CreateInstance<TService>();
			}
			Uri requestUri = CreateURI(request.GetServiceName(), parameters);
			WebRequest webRequest = WebRequest.Create(requestUri);
			webRequest.ContentType = "text/xml";
			webRequest.Method = method;
			using (Stream stream = webRequest.GetRequestStream())
			{
				SerializationHelper.Serialize(stream, request);
			}
			using WebResponse webResponse = webRequest.GetResponse();
			using Stream stream2 = webResponse.GetResponseStream();
			result = SerializationHelper.Deserialize<TService>(stream2);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
		}
		return result;
	}

	public static TService SendGetRequest<TService>(params string[] parameters) where TService : IServiceNameProvider
	{
		TService result = Activator.CreateInstance<TService>();
		try
		{
			Uri requestUri = CreateURI(result.GetServiceName(), parameters);
			WebRequest webRequest = WebRequest.Create(requestUri);
			webRequest.ContentType = string.Empty;
			webRequest.Method = "GET";
			using WebResponse webResponse = webRequest.GetResponse();
			using Stream stream = webResponse.GetResponseStream();
			result = SerializationHelper.Deserialize<TService>(stream);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
			result = default(TService);
		}
		return result;
	}

	public static TServiceResult SendGetRequest<TService, TServiceResult>(params string[] parameters) where TService : IServiceNameProvider
	{
		TService val = Activator.CreateInstance<TService>();
		TServiceResult result = Activator.CreateInstance<TServiceResult>();
		try
		{
			Uri requestUri = CreateURI(val.GetServiceName(), parameters);
			WebRequest webRequest = WebRequest.Create(requestUri);
			webRequest.ContentType = string.Empty;
			webRequest.Method = "GET";
			using WebResponse webResponse = webRequest.GetResponse();
			using Stream stream = webResponse.GetResponseStream();
			result = SerializationHelper.Deserialize<TServiceResult>(stream);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e);
			result = default(TServiceResult);
		}
		return result;
	}

	public static string SendPostRequest<TService>(TService request, params string[] parameters) where TService : IServiceNameProvider
	{
		return SendRequest("POST", request, parameters);
	}

	public static TService SendPostRequestObjectResponse<TService>(TService request, params string[] parameters) where TService : IServiceNameProvider
	{
		return SendRequestObjectResponse("POST", request, parameters);
	}

	public static string SendDeleteRequest<TService>(TService request, params string[] parameters) where TService : IServiceNameProvider
	{
		return SendRequest("DELETE", request, parameters);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class PerforceVersionControlServer : VersionControlServerBase
{
	private struct DepotInfo
	{
		public string Name;

		public DateTime Created;

		public DepotType Type;

		public string Spec;

		public string Description;
	}

	public static readonly int DefaultTimeout = 30000;

	private readonly string[] OperationsOfInterest = new string[4] { "sync", "revert", "unshelve", "revert" };

	private Regex m_workspaceDepotSplit = new Regex("^\\s+\"?([-+]?//.*)/(.*)\"?\\s+\"?(//.*)/(.*)\"?$", RegexOptions.Compiled);

	public override bool IsConnected
	{
		get
		{
			ResultCode resultCode = UpdateDepots();
			return resultCode == ResultCode.Success;
		}
	}

	public PerforceVersionControlServer(VersionControlContext context)
		: base(context)
	{
		ResultCode resultCode = UpdateUsers();
		if (resultCode != ResultCode.Success)
		{
			throw new ResultCodeException(resultCode, "Failed to enumerate users");
		}
		ResultCode resultCode2 = UpdateDepots();
		if (resultCode2 != ResultCode.Success)
		{
			throw new ResultCodeException(resultCode2, "Failed to enumerate depots");
		}
		ResultCode resultCode3 = UpdateWorkspaces();
		if (resultCode3 != ResultCode.Success)
		{
			throw new ResultCodeException(resultCode3, "Failed to enumerate workspaces");
		}
	}

	public override bool Reconnect(string password)
	{
		string request = "login";
		string stdinData = $"{password}\n";
		bool success = true;
		PerforceRequestHelper.Run(base.Context, request, stdinData, delegate(string res, string err)
		{
			success = string.IsNullOrEmpty(err);
		});
		return success;
	}

	public override void GetNumPendingOperations(Action<ResultCode, int> result)
	{
		Regex monitorParser = new Regex("\\s+(\\d+)\\s([RTPBFI])\\s([^\\s]+)\\s+(\\d\\d+:\\d\\d:\\d\\d)\\s([^\\s]+)", RegexOptions.Compiled);
		PerforceRequestHelper.Run(base.Context, "monitor show -s R", delegate(string res, string err)
		{
			ResultCode res2 = ResultCode.Success;
			if (PerforceRequestHelper.IsGlobalError(err, ref res2))
			{
				result(res2, 0);
			}
			else
			{
				int num = 0;
				using (StringReader stringReader = new StringReader(res))
				{
					while (res2 == ResultCode.Success)
					{
						string text = stringReader.ReadLine();
						if (string.IsNullOrEmpty(text))
						{
							break;
						}
						MatchCollection matchCollection = monitorParser.Matches(text);
						if (matchCollection.Count > 0 && matchCollection[0].Groups.Count == 6)
						{
							if (base.Context.Username.StartsWith(matchCollection[0].Value) && OperationsOfInterest.Contains(matchCollection[4].Value))
							{
								num++;
							}
						}
						else
						{
							res2 = new ResultCode("Failed to parse monitor show result");
						}
					}
				}
				result(res2, num);
			}
		});
	}

	private bool ParseDepotInfo(string depotStr, ref DepotInfo info)
	{
		Regex regex = new Regex("(?:^|\\s)('(?:[^']+|'')*'|[^\\s]*)", RegexOptions.Compiled);
		MatchCollection matchCollection = regex.Matches(depotStr);
		if (matchCollection.Count != 6 && matchCollection.Count != 7)
		{
			return false;
		}
		if (matchCollection[0].Value.ToLower() != "depot")
		{
			return false;
		}
		info.Name = matchCollection[1].Value.TrimStart(' ');
		info.Created = DateTime.Parse(matchCollection[2].Value.TrimStart(' '));
		if (!Enum.TryParse<DepotType>(matchCollection[3].Value.TrimStart(' '), ignoreCase: true, out info.Type))
		{
			info.Type = DepotType.Invalid;
		}
		info.Spec = matchCollection[matchCollection.Count - 2].Value.TrimStart(' ');
		info.Description = matchCollection[matchCollection.Count - 1].Value.TrimStart(' ');
		return true;
	}

	private ResultCode UpdateUsers()
	{
		ResultCode result = ResultCode.Success;
		Regex userSplit = new Regex("(\\S+)\\s<.*", RegexOptions.Compiled);
		PerforceRequestHelper.Run(base.Context, "users", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, ref result))
			{
				using (StringReader stringReader = new StringReader(res))
				{
					while (result == ResultCode.Success)
					{
						string text = stringReader.ReadLine();
						if (string.IsNullOrEmpty(text))
						{
							break;
						}
						MatchCollection matchCollection = userSplit.Matches(text);
						if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 2)
						{
							string value = matchCollection[0].Groups[1].Value;
							m_users.Add(value);
						}
						else
						{
							result = new ResultCode("Failed to parse user results");
						}
					}
				}
			}
		});
		return result;
	}

	private ResultCode UpdateDepots()
	{
		ResultCode result = ResultCode.Success;
		PerforceRequestHelper.Run(base.Context, "depots", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, ref result))
			{
				using (StringReader stringReader = new StringReader(res))
				{
					while (result == ResultCode.Success)
					{
						string text = stringReader.ReadLine();
						if (string.IsNullOrEmpty(text))
						{
							break;
						}
						DepotInfo info = default(DepotInfo);
						if (ParseDepotInfo(text, ref info))
						{
							try
							{
								m_depots[info.Name] = new PerforceVersionControlDepot(info.Name, info.Type);
							}
							catch (Exception ex)
							{
								result = new ResultCode($"Failed to create local depot cache. Error=\"{ex.Message}\"");
							}
						}
						else
						{
							result = new ResultCode("Failed to parse depot results");
						}
					}
				}
				using StringReader stringReader2 = new StringReader(err);
				while (result == ResultCode.Success)
				{
					string text2 = stringReader2.ReadLine();
					if (string.IsNullOrEmpty(text2))
					{
						break;
					}
					if (text2.Contains("PASSWD"))
					{
						result = PerforceResultCode.NotSignedIn;
					}
				}
			}
		});
		return result;
	}

	private ResultCode UpdateWorkspaces()
	{
		ResultCode result = ResultCode.Success;
		Regex sdsSplit = new Regex("(?:^|\\s)('(?:[^']+|'')*'|[^\\s]*)", RegexOptions.Compiled);
		PerforceRequestHelper.Run(base.Context, $"clients -u {m_context.Username}", delegate(string res, string err)
		{
			if (!PerforceRequestHelper.IsGlobalError(err, ref result))
			{
				using (StringReader stringReader = new StringReader(res))
				{
					while (result == ResultCode.Success)
					{
						string text = stringReader.ReadLine();
						if (string.IsNullOrEmpty(text))
						{
							break;
						}
						MatchCollection matchCollection = sdsSplit.Matches(text);
						if (matchCollection.Count == 6)
						{
							if (matchCollection[0].Value.ToLower() == "client")
							{
								string name = matchCollection[1].Value.TrimStart(' ');
								DateTime dateTime = DateTime.Parse(matchCollection[2].Value.TrimStart(' '));
								string text2 = matchCollection[3].Value.TrimStart(' ');
								string rootFolder = matchCollection[4].Value.TrimStart(' ').TrimEnd('\\');
								string text3 = matchCollection[5].Value.TrimStart(' ');
								IList<VersionControlPath> mappings = new List<VersionControlPath>();
								PerforceRequestHelper.Run(base.Context, $"client -o {name}", delegate(string s, string err2)
								{
									if (!PerforceRequestHelper.IsGlobalError(err2, ref result))
									{
										using (StringReader stringReader2 = new StringReader(s))
										{
											while (true)
											{
												string text4 = stringReader2.ReadLine();
												if (text4 == null)
												{
													break;
												}
												if (!text4.StartsWith("#") && text4.StartsWith("View:"))
												{
													while (true)
													{
														string text5 = stringReader2.ReadLine();
														if (string.IsNullOrEmpty(text5))
														{
															break;
														}
														MatchCollection matchCollection2 = m_workspaceDepotSplit.Matches(text5);
														if (matchCollection2.Count == 1 && matchCollection2[0].Groups.Count == 5)
														{
															if (!matchCollection2[0].Groups[1].Value.StartsWith("-") && !matchCollection2[0].Groups[1].Value.StartsWith("+"))
															{
																string value = matchCollection2[0].Groups[1].Value;
																value = value.TrimEnd('/');
																string value2 = matchCollection2[0].Groups[3].Value;
																value2 = value2.Replace(name, rootFolder);
																value2 = value2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
																value2 = value2.TrimStart('\\');
																value2 = value2.TrimEnd('\\');
																mappings.Add(new VersionControlPath(value, value2));
															}
														}
														else
														{
															result = new ResultCode($"Failed to parse workspace mapping\n\n{text5}");
														}
													}
												}
											}
										}
									}
								});
								try
								{
									m_workspaces[name] = new PerforceVersionControlWorkspace(m_context, name, rootFolder, mappings);
								}
								catch (Exception ex)
								{
									result = new ResultCode($"Failed to create local workspace cache. Error=\"{ex.Message}\"");
								}
							}
							else
							{
								result = new ResultCode("Failed to parse workspace results");
							}
						}
						else
						{
							result = new ResultCode($"Failed to parse workspace results\n\n{text}");
						}
					}
				}
			}
		});
		return result;
	}
}

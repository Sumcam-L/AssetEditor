namespace Firaxis.VersionControl;

public struct VersionControlContext
{
	private int _timeout;

	public readonly string ConnectionURI;

	public readonly string Host;

	public readonly string Username;

	public int Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_timeout = value;
		}
	}

	public VersionControlContext(string uri, string host, string user, int timeout)
	{
		ConnectionURI = uri;
		Host = host;
		Username = user;
		_timeout = timeout;
	}
}

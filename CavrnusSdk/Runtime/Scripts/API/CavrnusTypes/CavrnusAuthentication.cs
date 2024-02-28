using Collab.Proxy.Comm.RestApi;

namespace CavrnusSdk.API
{
	public class CavrnusAuthentication
	{
		internal RestApiEndpoint Endpoint;
		public string Token;

		internal CavrnusAuthentication(RestApiEndpoint endpoint, string token)
		{
			Endpoint = endpoint;
			Token = token;
		}
	}
}
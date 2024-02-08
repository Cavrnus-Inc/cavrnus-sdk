using Collab.Proxy.Comm.RestApi;

namespace CavrnusSdk.API
{
	public class CavrnusAuthentication
	{
		internal RestApiEndpoint Endpoint;

		internal CavrnusAuthentication(RestApiEndpoint endpoint)
		{
			Endpoint = endpoint;
		}
	}
}
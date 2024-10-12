using Collab.Proxy.Comm.RestApi;

namespace CavrnusSdk.API
{
	public class CavrnusAuthentication
	{
		internal RestUserCommunication RestUserComm;
		internal RestApiEndpoint Endpoint;
		public string Token;

		internal CavrnusAuthentication(RestUserCommunication ruc, RestApiEndpoint endpoint, string token)
		{
			RestUserComm = ruc;
			Endpoint = endpoint;
			Token = token;
		}
	}
}
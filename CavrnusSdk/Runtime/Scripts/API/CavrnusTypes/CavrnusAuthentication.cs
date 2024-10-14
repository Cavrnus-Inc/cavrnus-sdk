using Collab.Proxy.Comm.RestApi;

namespace CavrnusSdk.API
{
	public class CavrnusAuthentication
	{
		internal readonly RestUserCommunication RestUserComm;
		internal readonly RestApiEndpoint Endpoint;
		public readonly string Token;
		
		internal CavrnusAuthentication(RestUserCommunication ruc, RestApiEndpoint endpoint, string token)
		{
			RestUserComm = ruc;
			Endpoint = endpoint;
			Token = token;
		}
	}
}
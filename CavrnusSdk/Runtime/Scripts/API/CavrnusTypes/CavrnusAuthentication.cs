using Collab.Proxy.Comm.RestApi;

namespace CavrnusSdk.API
{
	public class CavrnusAuthentication
	{
		public readonly string Token;
		
		internal readonly RestUserCommunication RestUserComm;
		internal readonly RestApiEndpoint Endpoint;
		
		internal CavrnusAuthentication(RestUserCommunication ruc, RestApiEndpoint endpoint, string token)
		{
			RestUserComm = ruc;
			Endpoint = endpoint;
			Token = token;
		}
	}
}
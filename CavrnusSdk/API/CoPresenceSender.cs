using Collab.Proxy.Comm.LiveTypes;
using Collab.Proxy.Comm.LocalTypes;
using UnityBase;
using UnityEngine;

namespace CavrnusSdk
{
	//Attach this to your player camera!
	public class CoPresenceSender : MonoBehaviour
	{
		private CavrnusSpaceConnection cavrnusSpaceConnection;

		// Start is called before the first frame update
		void Start() { CavrnusSpaceJoinEvent.OnAnySpaceConnection(OnSpaceConnection); }

		private void OnSpaceConnection(CavrnusSpaceConnection obj) { this.cavrnusSpaceConnection = obj; }

		private const float CoPresenceSendTimegap = .2f;
		private float lastCoPresenceSendTime;

		private void Update()
		{
			if (cavrnusSpaceConnection == null) return;

			//TODO: Make smarter about when to send!

			if (Time.time - lastCoPresenceSendTime > CoPresenceSendTimegap) {
				lastCoPresenceSendTime = Time.time;

				CoPresenceLive cpLive = new CoPresenceLive();
				cpLive.Scale = transform.localScale.x;
				cpLive.BodyRoot = new AvatarRootLive() {
					transform = new AvatarTransformLive() {
						Position = transform.position.ToFloat3(), Rotation = transform.eulerAngles.ToFloat3(),
					}
				};

				cavrnusSpaceConnection.RoomSystem.Comm.SendTransientEvent(
					new TransientEvent() {Copresence = cpLive.ToPb()},
					false, false);
			}
		}
	}
}
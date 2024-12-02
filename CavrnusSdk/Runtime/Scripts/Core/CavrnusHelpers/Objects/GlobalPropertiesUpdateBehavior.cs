using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.LiveRoomSystem;
using Collab.Proxy.Prop;
using Collab.Proxy.Prop.ScalarProp;
using Collab.Proxy.Prop.StringProp;
using Collab.Proxy.Prop.SystemProperties;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UnityBase.Objects
{
	public class GlobalPropertiesUpdateBehavior : MonoBehaviour
	{
		private RoomSystem roomSystem;

		private void Update()
		{
			if (this.roomSystem != null)
			{
				roomSystem.DateTimeProperties.Update(Time.realtimeSinceStartupAsDouble);
			}
		}

		public void Setup(RoomSystem rs)
		{
			this.roomSystem = rs;
		}

		public void Shutdown()
		{
		}
	}
}

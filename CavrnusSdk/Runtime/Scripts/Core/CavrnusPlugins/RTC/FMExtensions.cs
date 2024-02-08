﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.RtcCommon;
using FM.LiveSwitch;

namespace Collab.RtcWrapper
{
	public static class FMExtensions
	{
		public static RtcConnectionStatus ToStatus(this ConnectionState s)
		{
			switch (s)
			{
				case ConnectionState.Closed:
					return RtcConnectionStatus.Closed;
				case ConnectionState.Closing:
					return RtcConnectionStatus.Closing;
				case ConnectionState.Connected:
					return RtcConnectionStatus.Connected;
				case ConnectionState.Connecting:
					return RtcConnectionStatus.Connecting;
				case ConnectionState.Failed:
					return RtcConnectionStatus.Failed;
				case ConnectionState.Failing:
					return RtcConnectionStatus.Failing;
				case ConnectionState.Initializing:
					return RtcConnectionStatus.Initializing;
				case ConnectionState.New:
					return RtcConnectionStatus.New;
				default:
					throw new NotImplementedException("Unhandled ConnectionState enum value.");
			}
		}
	}
}

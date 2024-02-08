using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collab.RtcCommon;
using FM.LiveSwitch;

namespace Collab.RtcWrapper
{
	public static class RtcHelpers
	{
		public static RtcInputSource FromFM(this SourceInput i) => i != null ? new RtcInputSource() { Id = i.Id, Name = i.Name } : null;
		public static SourceInput ToFM(this RtcInputSource i) => i != null ? new SourceInput(i.Id, i.Name) : null;

		public static RtcOutputSink FromFM(this SinkOutput i) => i != null ? new RtcOutputSink() { Id = i.Id, Name = i.Name } : null;
		public static SinkOutput ToFM(this RtcOutputSink i) => i != null ? new SinkOutput(i.Id, i.Name) : null;
	}
}

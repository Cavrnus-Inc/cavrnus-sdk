using System;
using Collab.Base.Collections;
using Collab.RtcCommon;

namespace CavrnusCore
{
    public class FakeRtcLocalUserContext : IRtcLocalUserContext
    {
        public void ChangeAudioSourceEnabled(bool enabled, Action<bool> onSuccess, Action<string> onFail) {  }
        public void NotifyVideoImageProcessed(int frame) {  }
        public void Reestablish() {  }
        public void Shutdown() {  }
        public string LocalConnectionId => "";
        public string LocalUserId => "";
        
        public ISetting<bool> Muted => new Setting<bool>();
        public IReadonlySetting<bool> Enabled => new Setting<bool>();
        public ISetting<float> CurrentGain => new Setting<float>();

        public LocalUserReportInfo LastReport => new LocalUserReportInfo();

        public IReadonlySetting<float> CurrentTransmittingVolume => new Setting<float>();
        public IReadonlySetting<string> CurrentLiveVideoSource => new Setting<string>();

        public IVideoProvider VideoProvider => new DirectObjectVideoProvider();
        
    }
}
using System;
using Collab.Base.Collections;
using Collab.Base.Graphics;
using Collab.RtcCommon;

namespace CavrnusCore
{
    public class FakeRtcContext : IRtcContext
    {
        public string GetDeviceId() => "";
        public void FetchAudioInputOptions(Action<RtcInputSource[]> inputsCb) {  }
        public void ChangeAudioInputDevice(RtcInputSource inputSource, Action<RtcInputSource> success, Action<string> failure) {  }
        public void FetchAudioOutputOptions(Action<RtcOutputSink[]> outputsCb) {  }
        public void ChangeAudioOutputDevice(RtcOutputSink outputSink, Action<RtcOutputSink> success, Action<string> failure) {  }
        public void FetchVideoInputOptions(Action<RtcInputSource[]> outputsCb) {  }
        public void ChangeVideoInputDevice(RtcInputSource inputSource, Action<RtcInputSource> success, Action<string> failure) {  }
        public void NotifyLocalVideoImageProcessed(int frame) {  }

        public IRtcLocalUserContext BuildLocalUserContext(string localConnectionId, string localUserId,
                                                          RequestRtcAccessInfoDelegate requestRtcAuthCb, IReadonlySetting<RtcModeEnum> allowedSend,
                                                          IReadonlySetting<RtcModeEnum> allowedRecv)
        {
            return new FakeRtcLocalUserContext();
        }

        public void DestroyLocalUserContext() {  }

        public IRtcRemoteUserContext FindOrBuildRemoteUserContext(string connectionId)
        {
            return null;
        }

        public void Initialize(RtcInputSource audioInput, RtcOutputSink audioOutput, RtcInputSource videoInput, RtcModeEnum sendMode, RtcModeEnum recvMode)
        {
            
        }

        public void Shutdown() {  }
        public void ForceRestartSystem() {  }
        public void ProvideImageToImageVideoSource(IImage2D image) {  }
        public void OverrideDirectLocalVideoSource(object dvs) {  }
        public IReadonlySetting<RtcInputSource> CurrentAudioInputSource{ get; }
        public IReadonlySetting<RtcOutputSink> CurrentAudioOutputSink{ get; }
        public IReadonlySetting<RtcInputSource> CurrentVideoInputSource{ get; }
        public IReadonlySetting<float> CurrentInputVolume{ get; }
        public IVideoProvider LocalVideoProvider{ get; }
        public IRtcSystem RtcSystem{ get; }
    }
}
using System;
using System.Collections;
using Collab.Base.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CavrnusSdk.XR.Widgets
{
    public class WidgetVoiceVolume : MonoBehaviour
    {
        [SerializeField] private Image volumeIndicator;
        [SerializeField] private Color quietColor = Color.blue;
        [SerializeField] private Color speakingColor = Color.green;

        [Space]
        [SerializeField] private float transitionSpeed;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0,1,1);
        
        private IDisposable disposable;
        private Coroutine currentRoutine;

        public void Setup(CavrnusUser user)
        {
            //Set the volume component to always match the user's data
            disposable = user.IsSpeaking.Bind(speaking => {
                if (currentRoutine != null)
                    StopCoroutine(currentRoutine);

                StartCoroutine(LerpColorRoutine(speaking));
            });
        }

        private IEnumerator LerpColorRoutine(bool speaking)
        {
            var elapsedTime = 0f;
            var startColor = volumeIndicator.color;
            var targetColor = speaking ? quietColor : speakingColor;

            var colorDifference = Vector4.Distance(startColor, targetColor);
            var duration = colorDifference / transitionSpeed;

            while (elapsedTime < duration)
            {
                var progress = elapsedTime / transitionSpeed;
                var curvePercentage = animationCurve.Evaluate(progress);

                // Use Color.Lerp to interpolate between startColor and targetColor
                volumeIndicator.color = Color.Lerp(startColor, targetColor, curvePercentage);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            volumeIndicator.color = targetColor;
        }

        private void OnDestroy() => disposable.Dispose();
    }
}
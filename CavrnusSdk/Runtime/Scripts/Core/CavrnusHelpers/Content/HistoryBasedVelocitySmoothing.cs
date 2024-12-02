using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityBase.Content
{
	public class HistoryBasedVelocitySmoothing
	{
		private List<Tuple<float, Vector3>> positionHistory = new List<Tuple<float, Vector3>>();
		private const double VELOCITY_SMOOTHING_DURATION = .4;

		public Vector3 PresentPosition(Vector3 pos, float curTime)
		{
			Vector3 averagedVelocity = Vector3.zero;

			// truncate history
			for (int i = 0; i < positionHistory.Count;)
			{
				if (positionHistory[0].Item1 < curTime - VELOCITY_SMOOTHING_DURATION)
					positionHistory.RemoveAt(0);
				else
					i++;
			}

			// push position history
			positionHistory.Add(Tuple.Create(curTime, pos));

			// if this is a jump/teleport/discontinuity, then flush the history and start averaging anew, from zero.
			if (positionHistory.Count >= 2)
			{
				var cur = positionHistory[positionHistory.Count - 1];
				var prev = positionHistory[positionHistory.Count - 2];
				Vector3 instVel = (cur.Item2 - prev.Item2) * (1f / (cur.Item1 - prev.Item1));
				if (instVel.magnitude > 40f)
				{
					// JUMP! FLUSH
					positionHistory.Clear();
					positionHistory.Add(cur);
				}
			}

			// compute velocity
			if (positionHistory.Count > 2) // wait till we have 2 data points to estimate velocity
			{
				Vector3 summedVelocityEstimates = Vector3.zero;
				float samplesCount = 0f;
				for (int i = 0; i < positionHistory.Count - 1; i++) // Why -1? Don't include the current value, or we'll NaN out since the time diff will be zero.
				{
					Vector3 instVel = (pos - positionHistory[i].Item2) * (1f / (curTime - positionHistory[i].Item1));
					summedVelocityEstimates += instVel;
					samplesCount += 1f;
				}

				if (samplesCount > 0f)
					averagedVelocity = summedVelocityEstimates * (1f / samplesCount);
				else
					averagedVelocity = Vector3.zero;
			}

			return averagedVelocity;
		}
	}
}

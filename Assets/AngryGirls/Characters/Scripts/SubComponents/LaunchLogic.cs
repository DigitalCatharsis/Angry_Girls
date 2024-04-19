using System.Collections;
using UnityEngine;

namespace Angry_Girls
{

    public class LaunchLogic : SubComponent
    {
        public bool HasBeenLaunched = false;
        public bool HasFinishedLaunch = false;

        public IEnumerator ProcessLaunch()
        {
            yield return StartCoroutine(OnLaunch_Routine());
        }

        private IEnumerator OnLaunch_Routine()
        {
            HasBeenLaunched = true;
            while (Control.SubComponentProcessor.GroundDetector.IsAirboned)
            {

                yield return null;
            }
            HasFinishedLaunch = true;
        }
    }
}
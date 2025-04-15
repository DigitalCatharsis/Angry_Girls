using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public class AppController : MonoBehaviour
    {
        public int Sum(int a, int b, int c)
        {
            return a + b + c;
        }

        public void DoSomething(Action<int> onComplete)
        {
            var result = Sum(5, 4, 11);
            onComplete(result);
        }

        public string GetVersion()
        {
            return "1.0.2";
        }
    }
}
using NUnit.Framework;
using UnityEngine;

namespace Angry_Girls
{
    public class AppControllerTest
    {
        [Test]
        public void AppVersionShouldReturnCurrentValue()
        {
            AppController controller = new GameObject().AddComponent<AppController>();
            
            var actualValue = controller.GetVersion();

            Assert.That(actualValue, Is.EqualTo("1.0.2"));           
        }
    }
}

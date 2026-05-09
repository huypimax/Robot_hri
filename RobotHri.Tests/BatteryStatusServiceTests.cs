using NUnit.Framework;
using RobotHri.Services;

namespace RobotHri.Tests
{
    [TestFixture]
    public class BatteryStatusServiceTests
    {
        [Test]
        public void SetPercent_StoresValueAndDisplayText()
        {
            var s = new BatteryStatusService();
            s.SetPercent(82.4);

            Assert.That(s.Percent, Is.EqualTo(82.4));
            Assert.That(s.DisplayText, Is.EqualTo("82%"));
        }

        [Test]
        public void SetPercent_ClampOver100()
        {
            var s = new BatteryStatusService();
            s.SetPercent(150);

            Assert.That(s.Percent, Is.EqualTo(100));
            Assert.That(s.DisplayText, Is.EqualTo("100%"));
        }

        [Test]
        public void SetPercent_ClampUnder0()
        {
            var s = new BatteryStatusService();
            s.SetPercent(-5);

            Assert.That(s.Percent, Is.EqualTo(0));
            Assert.That(s.DisplayText, Is.EqualTo("0%"));
        }

        [Test]
        public void Initial_DisplayText_IsDashUntilFirstUpdate()
        {
            var s = new BatteryStatusService();
            Assert.That(s.Percent, Is.Null);
            Assert.That(s.DisplayText, Is.EqualTo("—"));
        }
    }
}

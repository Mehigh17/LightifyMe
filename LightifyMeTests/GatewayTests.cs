using FluentAssertions;
using LightifyMeCore;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LightifyMeTests
{
	public class GatewayTests
    {
		
		private const string GatewayIp = "192.168.0.15";
		
	    [Fact]
	    public async Task AllBulbs_TurnedOn_ShouldBeOn()
        {
            var gateway = new GatewayController();
            await gateway.Init(GatewayIp);
            gateway.TurnAllOn();

            await gateway.Shutdown();
        }

        [Fact]
        public async Task AllBulbs_TurnedOff_ShouldBeOff()
        {
            var gateway = new GatewayController();
            await gateway.Init(GatewayIp);
            gateway.TurnAllOff();

            await gateway.Shutdown();
        }

        [Fact]
        public async Task FirstBulb_TurnedOn_ShouldBeOn()
        {
            var gateway = new GatewayController();
            await gateway.Init(GatewayIp);
            var bulb = gateway.GetBulbs().FirstOrDefault();

            bulb.Should().NotBeNull();

            gateway.TurnOn(bulb);

            bulb.IsOn.Should().BeTrue();

            await gateway.Shutdown();
        }

        [Fact]
        public async Task FirstBulb_TurnedOff_ShouldBeOff()
        {
            var gateway = new GatewayController();
            await gateway.Init(GatewayIp);
            var bulb = gateway.GetBulbs().FirstOrDefault();

            bulb.Should().NotBeNull();

            gateway.TurnOff(bulb);

            bulb.IsOn.Should().BeFalse();

            await gateway.Shutdown();
        }

        [Fact]
        public async Task FirstBulb_TurnedOn_BrightnessShouldBe100()
        {
            byte brightness = 100;

            var gateway = new GatewayController();
            await gateway.Init(GatewayIp);
            var bulb = gateway.GetBulbs().FirstOrDefault();

            bulb.Should().NotBeNull();

            gateway.TurnOn(bulb);

            bulb.IsOn.Should().BeTrue();

            gateway.SetBrightness(bulb, brightness);

            bulb.Brightness.Should().Be(brightness);

            await gateway.Shutdown();
        }

        [Fact]
        public async Task FirstBulb_TurnedOn_ColorShouldBeBlue()
        {
            var color = Color.Red;

            var gateway = new GatewayController();
            await gateway.Init(GatewayIp);
            var bulb = gateway.GetBulbs().FirstOrDefault();

            bulb.Should().NotBeNull();

            gateway.TurnOn(bulb);

            bulb.IsOn.Should().BeTrue();

            gateway.SetColor(bulb, color);
            
            bulb.Color.Should().BeEquivalentTo(color);

            await gateway.Shutdown();
        }

    }
}

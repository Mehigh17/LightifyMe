using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LightifyMe.Core.Builders
{
    public class BulbBuilder : IBulbBuilder
    {

        private readonly List<byte> _bytes;

        public BulbBuilder()
        {
            _bytes = new List<byte>();
        }

        public IBulbBuilder AddBytes(byte[] bytes)
        {
            _bytes.AddRange(bytes);
            return this;
        }

        // Do not use simplified object initialization to ease debugging and exception catching
        public Bulb Build()
        {
            var bulbData = _bytes.ToArray();
            var bulb = new Bulb();
            
            bulb.Id = BitConverter.ToUInt16(bulbData, 0);
            bulb.MacAddress = BitConverter.ToUInt64(bulbData, 2);
            bulb.Type = bulbData[9];
            bulb.FirmwareVersion = BitConverter.ToUInt32(bulbData, 10);
            bulb.IsAvailable = BitConverter.ToBoolean(bulbData, 14);
            bulb.GroupId = BitConverter.ToUInt16(bulbData, 15);
            bulb.IsOn = BitConverter.ToBoolean(bulbData, 17);
            bulb.Brightness = bulbData[18];
            bulb.Temperature = BitConverter.ToInt16(bulbData, 19);
            bulb.Color = Color.FromArgb(bulbData[24], bulbData[21], bulbData[22], bulbData[23]);
            bulb.Name = Encoding.Default.GetString(bulbData, 25, bulbData.Length - 25);

            _bytes.Clear();

            if (bulb.MacAddress == 0) // There's clearly something wrong if the mac address is equal to 0
                return null;

            return bulb;
        }
    }
}

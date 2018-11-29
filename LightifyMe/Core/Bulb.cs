using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LightifyMe.Core
{
    public class Bulb
    {

        public ushort Id { get; set; }
        public ulong MacAddress { get; set; }
        public byte Type { get; set; }
        public uint FirmwareVersion { get; set; }
        public bool IsAvailable { get; set; }
        public ushort GroupId { get; set; }
        public bool IsOn { get; set; }
        public byte Brightness { get; set; }
        public short Temperature { get; set; }
        public Color Color { get; set; }
        public string Name { get; set; }

    }
}

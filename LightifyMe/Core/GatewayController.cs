using LightifyMe.Core.Builders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LightifyMe.Core
{
    public class GatewayController
    {
        private const int RequestBufferHeaderSize = 11;
        private const int BulbBufferSize = 50;
        private const int ResponseBufferSize = 20;

        private readonly Socket _socket;
        private readonly IBulbBuilder _bulbBuilder;

        private int _sessionId = 0;

        public GatewayController(IBulbBuilder bulbBuilder)
        {
            _bulbBuilder = bulbBuilder;
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 5000,
                ReceiveTimeout = 5000,
            };
        }

        public GatewayController() : this(new BulbBuilder())
        { }

        public async Task Init(string localIp, int port = 4000)
        {
            if (!IPAddress.TryParse(localIp, out var localIpAddress))
                throw new ArgumentException($"Please make sure your local ip '{localIp}:{port}' is valid.");

            await _socket.ConnectAsync(localIpAddress, port);

            if (!_socket.Connected)
                throw new Exception($"The socket couldn't connect to '{localIp}:{port}'.");
        }

        public Task Shutdown()
        {
            _socket.Disconnect(false);
            return Task.CompletedTask;
        }

        public List<Bulb> GetBulbs()
        {
            if(!_socket.Connected)
                throw new InvalidOperationException("The controller must be connected to the gateway.");

            var bulbs = new List<Bulb>();
            var receivedHeader = new byte[RequestBufferHeaderSize];

            // Build the first request buffer to know how many bulbs are connected
            var requestBuffer = new List<byte>
            {
                0x0B, 0x00, 0x00, 0x13,
            };
            requestBuffer.AddRange(BitConverter.GetBytes(_sessionId));
            requestBuffer.AddRange(new byte[] {0x01, 0x00, 0x00, 0x00, 0x00});

            _socket.Send(requestBuffer.ToArray());
            _socket.Receive(receivedHeader);

            var bulbCount = receivedHeader[9];
            var receivedBuffer = new byte[bulbCount * BulbBufferSize]; // Create the bulb data buffer

            // Build the second request buffer which is going to request all the bulb information
            requestBuffer.Clear();
            requestBuffer = new List<byte>
            {
                0x0B, 0x00, 0x00, 0x13,
            };
            requestBuffer.AddRange(BitConverter.GetBytes(++_sessionId));
            requestBuffer.AddRange(new byte[] {0x01, 0x00, 0x00, 0x00, 0x00});
            
            _socket.Send(requestBuffer.ToArray());
            _socket.Receive(receivedBuffer);

            for (var i = 0; i < bulbCount; i++)
            {
                var bulbData = receivedBuffer.Skip(BulbBufferSize * i).Take(BulbBufferSize)
                    .ToArray();

                var bulb = _bulbBuilder
                    .AddBytes(bulbData)
                    .Build();

                if(bulb != null)
                    bulbs.Add(bulb);
            }

            return bulbs;
        }

        #region Bulb Handling Methods

        public void TurnAllOn()
        {
            var requestBuffer = new byte[]
            {
                0x0f, 0x00, 0x00, 0x32, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01
            };
            var responseBuffer = new byte[ResponseBufferSize];

            _socket.Send(requestBuffer);
            _socket.Receive(responseBuffer);
        }

        public void TurnAllOff()
        {
            var requestBuffer = new byte[]
            {
                0x0f, 0x00, 0x00, 0x32, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00
            };
            var responseBuffer = new byte[ResponseBufferSize];

            _socket.Send(requestBuffer);
            _socket.Receive(responseBuffer);
        }

        public void TurnOn(Bulb bulb)
        {
            var responseBuffer = new byte[ResponseBufferSize];

            _sessionId++;
            var requestBuffer = new List<byte>
            {
                0x0F, 0x00, 0x00, 0x32
            };
            requestBuffer.AddRange(BitConverter.GetBytes(_sessionId));
            requestBuffer.AddRange(BitConverter.GetBytes(bulb.MacAddress));
            requestBuffer.Add(0x01);

            _socket.Send(requestBuffer.ToArray());
            _socket.Receive(responseBuffer);

            bulb.IsOn = true;
        }

        public void TurnOff(Bulb bulb)
        {
            var responseBuffer = new byte[ResponseBufferSize];

            _sessionId++;
            var requestBuffer = new List<byte>
            {
                0x0F, 0x00, 0x00, 0x32
            };
            requestBuffer.AddRange(BitConverter.GetBytes(_sessionId));
            requestBuffer.AddRange(BitConverter.GetBytes(bulb.MacAddress));
            requestBuffer.Add(0x00);

            _socket.Send(requestBuffer.ToArray());
            _socket.Receive(responseBuffer);

            bulb.IsOn = false;
        }

        public void SetBrightness(Bulb bulb, byte brightnessPercentage)
        {
            if(brightnessPercentage > 100)
                throw new InvalidOperationException("The bulb brightness percentage can not be smaller than 0 or greater than 100.");

            var responseBuffer = new byte[ResponseBufferSize];

            _sessionId++;
            var requestBuffer = new List<byte>
            {
                0x11, 0x00, 0x00, 0x31
            };
            requestBuffer.AddRange(BitConverter.GetBytes(_sessionId));
            requestBuffer.AddRange(BitConverter.GetBytes(bulb.MacAddress));
            requestBuffer.Add(brightnessPercentage);
            requestBuffer.AddRange(new byte[] {0x0, 0x0});

            _socket.Send(requestBuffer.ToArray());
            _socket.Receive(responseBuffer);

            bulb.Brightness = brightnessPercentage;
        }

        public void SetColor(Bulb bulb, Color color)
        {
            var responseBuffer = new byte[ResponseBufferSize];

            _sessionId++;
            var requestBuffer = new List<byte>
            {
                0x14, 0x00, 0x00, 0x36
            };
            requestBuffer.AddRange(BitConverter.GetBytes(_sessionId));
            requestBuffer.AddRange(BitConverter.GetBytes(bulb.MacAddress));
            requestBuffer.AddRange(new[] {color.A, color.B, color.G, color.R});
            requestBuffer.AddRange(new byte[] { 0x0, 0x0 });

            _socket.Send(requestBuffer.ToArray());
            _socket.Receive(responseBuffer);

            bulb.Color = color;
        }

        #endregion

    }
}

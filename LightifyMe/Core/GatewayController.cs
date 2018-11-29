using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LightifyMe.Core.Builders;

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
            var receivedBuffer = new byte[RequestBufferHeaderSize + BulbBufferSize]; // TODO: Support more than 1 bulb per gateway

            var requestBuffer = new byte[]
                {0x0B, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00};
            _socket.Send(requestBuffer);
            _socket.Receive(receivedBuffer);

            var bulb = _bulbBuilder
                .AddBytes(receivedBuffer.Skip(RequestBufferHeaderSize).Take(BulbBufferSize).ToArray())
                .Build();

            bulbs.Add(bulb);

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

        #endregion

    }
}

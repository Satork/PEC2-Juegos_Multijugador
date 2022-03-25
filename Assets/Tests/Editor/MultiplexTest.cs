using System;
using NSubstitute;
using NUnit.Framework;

namespace Mirror.Tests
{
    public class MultiplexTest : MirrorTest
    {
	    private Transport m_Transport1;
        private Transport m_Transport2;
        private MultiplexTransport m_Transport;

        [SetUp]
        public void Setup()
        {
            base.SetUp();

            m_Transport = holder.AddComponent<MultiplexTransport>();

            m_Transport1 = Substitute.For<Transport>();
            m_Transport2 = Substitute.For<Transport>();
            m_Transport.transports = new[] { m_Transport1, m_Transport2 };

            m_Transport.Awake();
        }

        [TearDown]
        public override void TearDown() => base.TearDown();

        // A Test behaves as an ordinary method
        [Test]
        public void TestAvailable()
        {
            m_Transport1.Available().Returns(true);
            m_Transport2.Available().Returns(false);
            Assert.That(m_Transport.Available());
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestNotAvailable()
        {
            m_Transport1.Available().Returns(false);
            m_Transport2.Available().Returns(false);
            Assert.That(m_Transport.Available(), Is.False);
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestConnect()
        {
            m_Transport1.Available().Returns(false);
            m_Transport2.Available().Returns(true);
            m_Transport.ClientConnect("some.server.com");

            m_Transport1.DidNotReceive().ClientConnect(Arg.Any<string>());
            m_Transport2.Received().ClientConnect("some.server.com");
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestConnectFirstUri()
        {
            Uri uri = new Uri("tcp://some.server.com");

            m_Transport1.Available().Returns(true);
            m_Transport2.Available().Returns(true);

            m_Transport.ClientConnect(uri);
            m_Transport1.Received().ClientConnect(uri);
            m_Transport2.DidNotReceive().ClientConnect(uri);
        }


        // A Test behaves as an ordinary method
        [Test]
        public void TestConnectSecondUri()
        {
            Uri uri = new Uri("ws://some.server.com");

            m_Transport1.Available().Returns(true);

            // first transport does not support websocket
            m_Transport1
                .When(x => x.ClientConnect(uri))
                .Do(x => { throw new ArgumentException("Scheme not supported"); });

            m_Transport2.Available().Returns(true);

            m_Transport.ClientConnect(uri);
            m_Transport2.Received().ClientConnect(uri);
        }

        [Test]
        public void TestConnected()
        {
            m_Transport1.Available().Returns(true);
            m_Transport.ClientConnect("some.server.com");

            m_Transport1.ClientConnected().Returns(true);

            Assert.That(m_Transport.ClientConnected());
        }

        [Test]
        public void TestDisconnect()
        {
            m_Transport1.Available().Returns(true);
            m_Transport.ClientConnect("some.server.com");

            m_Transport.ClientDisconnect();

            m_Transport1.Received().ClientDisconnect();
        }

        [Test]
        public void TestClientSend()
        {
            m_Transport1.Available().Returns(true);
            m_Transport.ClientConnect("some.server.com");

            byte[] data = { 1, 2, 3 };
            ArraySegment<byte> segment = new ArraySegment<byte>(data);

            m_Transport.ClientSend(segment, 3);

            m_Transport1.Received().ClientSend(segment, 3);
        }

        [Test]
        public void TestClient1Connected()
        {
            m_Transport1.Available().Returns(true);
            m_Transport2.Available().Returns(true);

            Action callback = Substitute.For<Action>();
            // find available
            m_Transport.Awake();
            // set event and connect to give event to inner
            m_Transport.OnClientConnected = callback;
            m_Transport.ClientConnect("localhost");
            m_Transport1.OnClientConnected.Invoke();
            callback.Received().Invoke();
        }

        [Test]
        public void TestClient2Connected()
        {
            m_Transport1.Available().Returns(false);
            m_Transport2.Available().Returns(true);

            Action callback = Substitute.For<Action>();
            // find available
            m_Transport.Awake();
            // set event and connect to give event to inner
            m_Transport.OnClientConnected = callback;
            m_Transport.ClientConnect("localhost");
            m_Transport2.OnClientConnected.Invoke();
            callback.Received().Invoke();
        }

        [Test]
        public void TestServerConnected()
        {
            byte[] data = { 1, 2, 3 };
            ArraySegment<byte> segment = new ArraySegment<byte>(data);

            // on connect, send a message back
            void SendMessage(int connectionId)
            {
                m_Transport.ServerSend(connectionId, segment, 5);
            }

            // set event and Start to give event to inner
            m_Transport.OnServerConnected = SendMessage;
            m_Transport.ServerStart();

            m_Transport1.OnServerConnected.Invoke(1);

            m_Transport1.Received().ServerSend(1, segment, 5);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicBeePlugin.Ampache;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class AmpacheClientTests
    {
        private string testAmpache = "";
        private string testUsername = "";
        private string testPassword = "";

        private AmpacheClient ampacheClient;

        [TestInitialize]
        public void Setup()
        {
            ampacheClient = new AmpacheClient(testAmpache, testUsername, testPassword);
        }

        [TestMethod]
        public void Do_Handshake()
        {
            ManualResetEvent e = new ManualResetEvent(false);

            HandshakeResult result = null;

            ampacheClient.HandshakeCompleted += (sender, eventArgs) =>
            {
                result = eventArgs.Result;

                e.Set();
            };

            ampacheClient.StartHandshake();

            e.WaitOne();

            Assert.IsNotNull(result);
            Assert.AreNotEqual(DateTimeOffset.MinValue, result.add);
        }
    }
}

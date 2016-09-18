using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicBeePlugin.Ampache;
using System.Threading;
using System.Configuration;

namespace Tests
{
    [TestClass]
    public class AmpacheClientTests
    {
        private AmpacheClient ampacheClient;

        [TestInitialize]
        public void Setup()
        {
            var testServer = ConfigurationManager.AppSettings["server"];

            ampacheClient = new AmpacheClient(testServer);
        }

        [TestMethod]
        public void Can_Do_Handshake()
        {
            var testUsername = ConfigurationManager.AppSettings["username"];
            var testPassword = ConfigurationManager.AppSettings["password"];

            ManualResetEvent e = new ManualResetEvent(false);

            HandshakeResponse result = null;

            ampacheClient.HandshakeCompleted += (sender, eventArgs) =>
            {
                result = eventArgs.Response;

                e.Set();
            };

            ampacheClient.StartHandshake(testUsername, testPassword);

            e.WaitOne();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AuthToken);
        }
    }
}

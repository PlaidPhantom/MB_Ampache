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
            var testUsername = ConfigurationManager.AppSettings["username"];
            var testPassword = ConfigurationManager.AppSettings["password"];

            ampacheClient = new AmpacheClient(testServer, testUsername, testPassword);
        }

        [TestMethod]
        public void Can_Do_Handshake()
        {
            ManualResetEvent e = new ManualResetEvent(false);

            HandshakeResponse result = null;

            ampacheClient.Connected += (sender, eventArgs) =>
            {
                result = eventArgs.Response;

                e.Set();
            };

            ampacheClient.Connect();

            e.WaitOne();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AuthToken);
        }
    }
}

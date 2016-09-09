using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicBeePlugin.Ampache;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class AmpacheClientTests
    {
        private AmpacheClient ampacheClient;

        [ClassInitialize]
        public void Setup()
        {
            ampacheClient = new AmpacheClient(new Uri(""), "", "");
        }

        [TestMethod]
        public void Do_Handshake()
        {
            ManualResetEvent e = new ManualResetEvent(false);

            ampacheClient.HandshakeCompleted += (sender, eventArgs) =>
            {
                var result = eventArgs.Result;

                e.Set();
            };

            ampacheClient.StartHandshake();

            e.WaitOne(5000);
        }
    }
}

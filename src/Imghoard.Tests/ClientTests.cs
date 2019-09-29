using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Imghoard.Tests
{
    public class ClientTests
    {
        [Fact]
        public void ConstructClientTest()
        {
            var i = new ImghoardClient(ImghoardClient.Config.Default());

            Assert.NotNull(i);
        }
    }
}

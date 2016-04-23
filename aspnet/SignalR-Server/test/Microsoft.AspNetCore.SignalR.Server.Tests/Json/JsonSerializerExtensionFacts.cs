using System.IO;
using Microsoft.AspNetCore.SignalR.Json;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.SignalR.Tests.Json
{
    public class JsonSerializerExtensionFacts
    {
        [Fact]
        public void SerializeInterceptsIJsonWritable()
        {
            // Arrange
            var serializer = JsonUtility.CreateDefaultSerializer();
            var sw = new StringWriter();
            var value = new Mock<IJsonWritable>();
            value.Setup(m => m.WriteJson(It.IsAny<TextWriter>()))
                 .Callback<TextWriter>(_ => sw.Write("Custom"));

            // Act
            serializer.Serialize(value.Object, sw);

            // Assert
            Assert.Equal("Custom", sw.ToString());
        }
    }
}

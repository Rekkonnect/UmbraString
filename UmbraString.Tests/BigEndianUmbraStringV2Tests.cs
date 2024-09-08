namespace Rekkon.UmbraString.Tests;

#pragma warning disable CS0618 // Type or member is obsolete
// The type is marked as obsolete since the tests have not been run

public class BigEndianUmbraStringV2Tests : BaseUmbraStringTests<BigEndianUmbraStringV2>
{
    private const string _ignoreMessage = "This platform does not support big endian.";
    private readonly bool _canBeRun = !BitConverter.IsLittleEndian;

    [SetUp]
    public void SetupFixture()
    {
        if (!_canBeRun)
        {
            Assert.Ignore(_ignoreMessage);
        }
    }
}

#pragma warning restore CS0618 // Type or member is obsolete

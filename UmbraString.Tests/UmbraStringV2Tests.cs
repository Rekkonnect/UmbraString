namespace Rekkon.UmbraString.Tests;

public class UmbraStringV2Tests : BaseUmbraStringTests<UmbraStringV2>
{
    private const string _ignoreMessage = "This platform does not support little endian.";
    private readonly bool _canBeRun = BitConverter.IsLittleEndian;

    [SetUp]
    public void SetupFixture()
    {
        if (!_canBeRun)
        {
            Assert.Ignore(_ignoreMessage);
        }
    }
}

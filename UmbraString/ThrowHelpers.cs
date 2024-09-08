namespace Rekkon.UmbraString;

internal static class ThrowHelpers
{
    public static void ThrowUnsupportedLittleEndian()
    {
        throw new PlatformNotSupportedException(
            "This type is only supported on big-endian architectures.");
    }

    public static void ThrowUnsupportedBigEndian()
    {
        throw new PlatformNotSupportedException(
            "This type is only supported on little-endian architectures.");
    }

    public static void ThrowV2LengthLimitExceedance()
    {
        throw new ArgumentOutOfRangeException(
            "The maximum length of the Umbra string v2 cannot exceed 3.5 GiB.");
    }
}

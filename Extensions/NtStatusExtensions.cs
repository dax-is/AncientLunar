using AncientLunar.Native.Enums;

namespace AncientLunar.Extensions
{
    internal static class NtStatusExtensions
    {
        internal static bool IsSuccess(this NtStatus status)
        {
            return status.Status == 0;
        }
    }
}

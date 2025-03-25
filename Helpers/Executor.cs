using System;

namespace AncientLunar.Helpers
{
    internal static class Executor
    {
        internal static void IgnoreExceptions(Action operation)
        {
            try
            {
                operation.Invoke();
            } catch
            {
                // Ignore
            }
        }
    }
}

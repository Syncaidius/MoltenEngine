using Silk.NET.Core.Native;

namespace Molten.Graphics.DX12
{
    internal static class LoggerExtensions
    {
        internal static bool CheckResult(this Logger log, HResult r, Func<string> msg = null)
        {
            if (r.IsFailure)
            {
                string message = msg?.Invoke() ?? "N/A";
                log.Error($" - [HRESULT {r.Value}] Code: {r.Code} -- Severity: {r.Severity} -- Facility: {r.Value} -- Message: {message}");
                return false;
            }

            return true;
        }
    }
}

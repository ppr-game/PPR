using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PPR.Main {
    public static class Helper {
        public static void OpenUrl(string url) {
            try { Process.Start(url); }
            catch {
                // https://github.com/dotnet/corefx/issues/10361
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", url);
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url);
                else throw;
            }
        }
    }
}

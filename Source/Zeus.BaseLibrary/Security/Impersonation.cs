using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Zeus.BaseLibrary.Security
{
	public static class Impersonation
	{
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern bool CloseHandle(IntPtr handle);
 
		public static WindowsIdentity CreateIdentity(string user, string domain, string password)
		{
			var phToken = IntPtr.Zero;
			if (!LogonUser(user, domain, password, 3, 0, ref phToken))
			{
				var num = Marshal.GetLastWin32Error();
				throw new Exception("LogonUser failed with error code: " + num);
			}
			var identity = new WindowsIdentity(phToken);
			CloseHandle(phToken);
			return identity;
		}

		public static void RunImpersonatedCode(string user, string domain, string password, Action callback)
		{
			using (var wi = CreateIdentity(user, domain, password))
			{
				using (var wic = wi.Impersonate())
				{
					try
					{
						callback();
					}
					finally
					{
						wic.Undo();
					}
				}
			}
		}
	}
}
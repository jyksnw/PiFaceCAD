using System;
using System.Runtime.InteropServices;

namespace PiFacceCAD
{
	internal static class LinuxWrapper
	{
		[DllImport ("MonoPosixHelper", SetLastError = true)]
		internal static extern bool poll_serial (int fd, out int error, int timeout);

		[DllImport ("libc")]
		internal static extern IntPtr strerror (int errnum);

		[DllImport ("libc")]
		internal static extern int close (int filedes);
	}
}


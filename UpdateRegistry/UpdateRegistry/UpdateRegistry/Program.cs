using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateRegistry {
	class Program {
		private static readonly string registryKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options";

		static void Main(string[] args) {
			string exeName = args[0];
			bool enable = args[1] == "true";

			RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(registryKey, true);
			RegistryKey newKey = baseKey.CreateSubKey(exeName, RegistryKeyPermissionCheck.ReadWriteSubTree);
			newKey.SetValue("Debugger", enable ? "" : "GameControl.exe run " + exeName);
		}
	}
}

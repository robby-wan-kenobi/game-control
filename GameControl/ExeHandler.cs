using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameControl {
	public class ExeHandler {

		private readonly string steamBase = "D:\\steam\\games\\steamapps\\common";
		private readonly string thisApp = "GameControl";

		private readonly string[] otherNames = { "vcredist_x64.exe", "vcredist_x86.exe", "dxsetup.exe", "DXSETUP.exe",
												"eadm-installer.exe", "PhysX_SystemSoftware.exe", "vcredist_x86_en.exe", "vcredist_x86_de.exe",
												"vcredist_x86_fr.exe", "vcredist_x86_it.exe", "vcredist_x86_es.exe", "vc_redist.x64.exe",
												"vc_redist.x86.exe", "CaptiveAppEntry.exe", "RegVideoDLL.exe", "monolinker.exe",
												"ilasm.exe", "sqlmetal.exe", "smcs.exe", "installutil.exe",
												"sgen.exe", "monop.exe", "lc.exe", "httpcfg.exe",
												"resgen.exe", "mdoc.exe", "mono-xmltool.exe", "al.exe",
												"svcutil.exe", "mono-shlib-cop.exe", "gmcs.exe", "sqlsharp.exe",
												"pdb2mdb.exe", "nunit-console.exe", "mkbundle.exe", "booc.exe",
												"mono-api-info.exe", "xsd.exe", "xbuild.exe", "us.exe",
												"RabbitMQ.Client.Apigen.exe", "csharp.exe", "wsdl.exe", "mono-service.exe",
												"mconfig.exe", "gacutil.exe", "mono.exe", "eauninstall.exe",
												"dotnetfx35.exe", "dotnetfx35setup.exe", "Helper.exe", "LegacyFirewallAdd.exe",
												"LegacyFirewallDel.exe", "GDFTool.exe", "unins000.exe", "Editor.exe",
												"gu.exe", "UplayInstaller.exe"};
		private string[] exes;

		public ExeHandler() {
			exes = listExes();
		}

		private string[] listExes() {
			string[] exeArray = Directory.GetFiles(steamBase, "*.exe", SearchOption.AllDirectories);
			HashSet<string> exesSet = new HashSet<string>();
			foreach(string exe in exeArray){
				string name = exe.Split('\\').Last();
				if(!otherFile(name))
					exesSet.Add(name);
			}
			return exesSet.ToArray();
		}

		private bool otherFile(string name) {
			return otherNames.Contains(name.ToLower());
		}

		public void StartExe(string exeName) {
			string[] exeArray = Directory.GetFiles(steamBase, exeName, SearchOption.AllDirectories);
			if(exeArray.Length == 0) {
				NotificationHandler.NotifyWindows("Executable (" + exeName + ") does not exist in the steam directory.");
				Enable(exeName);
				return;
			}

			try {
				Enable(exeArray[0]);
				Process.Start(exeArray[0]);
			} catch(Exception e) {
				Debug.WriteLine(e.Message);
			}
		}

		public void DisableAll() {
			disableEnable(true);
		}

		public void EnableAll() {
			disableEnable(false);
		}

		private void Disable(string exe) {
			remoteCallEnableDisable(exe, false);
		}

		private void Enable(string exe) {
			remoteCallEnableDisable(exe, true);
		}

		private void remoteCallEnableDisable(string exeName, bool enable) {
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.Arguments = exeName + (enable ? " true" : " false");
			startInfo.FileName = "UpdateRegistry.exe";
			startInfo.Verb = "runas";
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.UseShellExecute = true;

			Process process = Process.Start(startInfo);
		}

		private void disableEnable(bool disable) {
			foreach(string exe in exes)
				remoteCallEnableDisable(exe, !disable);
		}

		public void AddExecutables() {
			disableEnable(true);
		}

		public bool IsThisAppRunning() {
			return Process.GetProcessesByName(thisApp).Length > 1;
		}

		public string[] ExesRunning() {
			List<string> running = new List<string>();
			foreach(string exe in exes) {
				string processName = exe.Split('.').First();
				if(Process.GetProcessesByName(processName).Length > 0)
					running.Add(processName);
			}
			return running.ToArray();
		}

		public void CloseExes() {
			foreach(string exe in exes) {
				string processName = exe.Split('.').First();
				closeExe(processName);
			}
		}

		private void closeExe(string exe) {
			Process[] pList = Process.GetProcessesByName(exe);
			foreach(Process p in pList) {
				p.CloseMainWindow();
			}
		}

	}
}

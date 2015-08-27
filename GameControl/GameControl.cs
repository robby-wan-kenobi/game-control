using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameControl {
	public class GameControl {

		/*
		 * 
		 * How it will work
		 *	- Maybe have it run every so often to check for new exes.
		 *		- If it finds that one is running, it starts the timer.
		 *		- Otherwise, it just adds the registry key.
		 *	- All of these exes will go through this app (put this in the bin).
		 *		- After checking, it will start a timer.
		 *			- It will check if the process is running every 5 minutes.
		 *				- If it has stopped, the timer will stop and it will be stored (encrypted) in a file.
		 *				- Else it will keep going.
		 *			- It will warn on the hour, 1.5 hours, 1.75 hours, 5 minutes before, and 1 minute before.
		 *			- At 0, it will shut down the program.
		 *				- Will store the times in a file.
		 *					- Maybe make the file write protected with a password that's generated?
		 *					- Or store in a file name that's randomly generated and encrypted?
		 *					- Or store the results in an encrypted file that if it's not there or empty, assumes it's been tampered with.
		 *				- The times will be checked the next time it's run to see if enough time has passed.
		 * 
		 * Rules
		 *	- 2 hours at a time
		 *	- No more than 2 hours on a week day
		 *	- No more than 5 hours on Saturday
		 *	- Not allowed on Sunday
		 *	- Prerequisites
		 *	
		 * How
		 *	- Redirect to this app with argument of process name
		 *	- If criteria is met
		 *		- Find the executable from the process name and run it
		 *	- Else
		 *		- Show error and why they can't play
		 *		
		 * When starting, add timestamp. Times are checked periodically.
		 * 
		 */

		public static void Main(string[] args) {
			if(args.Length == 0) {
				Debug.WriteLine("USAGE: gameControl <args>\n\t" +
								"Args\n\t\tcheck\t\t\t: Checks for new and already running executables.\n\t\t" +
								"run <exe_name>\t: Runs the specified executable.");
				return;
			}

			if(args[0] == "check") {
				handleCheck();
			} else if(args[0] == "run" && args.Length > 1) {
				handleRun(args[1]);
			} else if(args[0] == "run" && args.Length == 1) {
				Debug.WriteLine("Must specify executable to run.");
			} else {
				Debug.WriteLine("Unsupported argument.");
			}

			//CriteriaHandler criteriaHandler = new CriteriaHandler();
			//bool preReqs = criteriaHandler.CheckCriteria();
		}

		private static void handleCheck() {
			ExeHandler exeHandler = new ExeHandler();

			if(exeHandler.IsThisAppRunning())
				return;

			exeHandler.AddExecutables();
			exeHandler.DisableAll();
			if(exeHandler.ExesRunning().Length > 0) {
				handleRunning(exeHandler:exeHandler);
			}
		}

		private static void handleRun(string exeName) {
			TimeHandler timeHandler = new TimeHandler();
			bool time = timeHandler.CheckTime();
			if(time) {
				ExeHandler exeHandler = new ExeHandler();
				exeHandler.AddExecutables();
				exeHandler.EnableAll();
				exeHandler.StartExe(exeName);
				exeHandler.DisableAll();
				timeHandler.StartTimer();
				NotificationHandler.NotifyWindows("Starting timer.");
				DateTime start = DateTime.Now;
				while(true) {
					DateTime now = DateTime.Now;
					handleRunning(timeHandler, exeHandler);
					Thread.Sleep(60000);
				}
			}
		}

		private static void handleRunning(TimeHandler timeHandler=null, ExeHandler exeHandler=null) {
			if(timeHandler == null)
				timeHandler = new TimeHandler();
			if(exeHandler == null)
				exeHandler = new ExeHandler();

			if(exeHandler.ExesRunning().Length > 0) {
				if(!timeHandler.CheckTime()) {
					NotificationHandler.NotifyWindows("You've played enough for today. Shutting down.", () => {
						Thread.Sleep(5000);
						exeHandler.CloseExes();
						exeHandler.DisableAll();
						timeHandler.StopTimer();
						Environment.Exit(0);
					});
				}
			} else {
				NotificationHandler.NotifyWindows("Stopping timer.", () => {
					exeHandler.DisableAll();
					timeHandler.StopTimer();
					Environment.Exit(0);
				});
			}
		}

	}
}

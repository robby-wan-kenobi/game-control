using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

// TODO : Still need to wipe the file from the old day. Also need to account for starting after it's too late (time span).

namespace GameControl {
	public class TimeHandler {
		private bool timerRunning;
		private ExeHandler exeHandler;

		private DateTime fridayEnd;
		private DateTime otherEnd;
		private DateTime dayStart;

		private bool warningHour;
		private bool warning30;
		private bool warning10;
		private bool warning5;
		private bool warning1;

		private string tempDir;

		private List<DateTime> starts;
		private List<DateTime> ends;

		public TimeHandler() {
			starts = new List<DateTime>();
			ends = new List<DateTime>();
			tempDir = Environment.GetEnvironmentVariable("TEMP");
			exeHandler = new ExeHandler();
			timerRunning = false;
			DateTime now = DateTime.Now;
			DateTime tomorrow = DateTime.Now.AddDays(1f);
			dayStart = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
			otherEnd = new DateTime(now.Year, now.Month, now.Day, 23, 30, 0);
			fridayEnd = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 1, 0, 0);

			warningHour = false;
			warning30 = false;
			warning10 = false;
			warning5 = false;
			warning1 = false;

			readXML();
		}

		private void writeXML() {
			string filePath = getPath();

			XmlDocument xml = new XmlDocument();
			XmlNode root = xml.CreateElement("root");
			xml.AppendChild(root);

			XmlNode start = xml.CreateElement("start");
			root.AppendChild(start);
			foreach(DateTime d in starts) {
				XmlNode timeElement = xml.CreateElement("time");
				XmlNode time = xml.CreateTextNode(d.ToString());
				timeElement.AppendChild(time);
				start.AppendChild(timeElement);
			}

			XmlNode end = xml.CreateElement("end");
			root.AppendChild(end);
			foreach(DateTime d in ends) {
				XmlNode timeElement = xml.CreateElement("time");
				XmlNode time = xml.CreateTextNode(d.ToString());
				timeElement.AppendChild(time);
				end.AppendChild(timeElement);
			}

			xml.Save(filePath);
		}

		private void readXML() {
			string filePath = getPath();
			if(!File.Exists(filePath))
				return;

			XmlDocument xml = new XmlDocument();
			xml.Load(filePath);

			XmlNode startNode = xml.GetElementsByTagName("start")[0];
			foreach(XmlNode child in startNode.ChildNodes)
				starts.Add(DateTime.Parse(child.ChildNodes[0].Value));
			
			XmlNode endNode = xml.GetElementsByTagName("end")[0];
			foreach(XmlNode child in endNode.ChildNodes)
				ends.Add(DateTime.Parse(child.ChildNodes[0].Value));

			DateTime lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 7, 0, 0);
			if(ends[ends.Count() - 1] < lastDay) {
				starts = new List<DateTime>();
				ends = new List<DateTime>();
			}
		}

		private string getPath() {
			if(Properties.Settings.Default.file == "") {
				Properties.Settings.Default.file = Guid.NewGuid().ToString().Substring(0, 4);
				Properties.Settings.Default.Save();
			}
			return tempDir + "\\tmp" + Properties.Settings.Default.file.ToUpper() + ".tmp";
		}

		public bool CheckTime() {
			switch(DateTime.Now.DayOfWeek) {
				case DayOfWeek.Sunday:
					NotificationHandler.NotifyWindows("Can't play on Sunday.");
					return false;
				case DayOfWeek.Saturday:
					return handleDay(5.0 * 60.0, 120.0, 60.0, dayStart, otherEnd);
				case DayOfWeek.Friday:
					return handleDay(4.0 * 60.0, 120.0, 60.0, dayStart, fridayEnd);
				default:
					return handleDay(2.0 * 60.0, 120.0, 60.0, dayStart, otherEnd);
			}
		}

		private bool handleDay(double totalTimeForDay, double maxTimeForSession, double minTimeBetweenSessions, DateTime startOfTheDay, DateTime endOfTheDay) {
			bool withinSpan = beforeEndAfterStart(startOfTheDay, endOfTheDay);
			if(!withinSpan) {
				NotificationHandler.NotifyWindows("You should be sleeping.");
				return false;
			}
			double totalBeforeCurrentSession = totalNonSessionTime();
			if(totalBeforeCurrentSession >= totalTimeForDay) {
				return false;
			}
				
			bool liveSession = exeHandler.ExesRunning().Length > 0;
			if(liveSession) {
				double totalOfCurrentSession = totalSessionTime();
				double timeLeft = maxTimeForSession - totalOfCurrentSession;
				if(totalOfCurrentSession >= maxTimeForSession) {
					return false;
				} else if(!warningHour && Math.Abs(timeLeft - 60.0) <= 1.5) {
					NotificationHandler.NotifyWindows("Hour warning.");
					warningHour = true;
				} else if(!warning30 && Math.Abs(timeLeft - 30.0) <= 1.5) {
					NotificationHandler.NotifyWindows("Half-hour warning.");
					warning30 = true;
				} else if(!warning10 && Math.Abs(timeLeft - 10.0) <= 1.5) {
					NotificationHandler.NotifyWindows("Ten-minute warning.\nFind a save point.");
					warning10 = true;
				} else if(!warning5 && Math.Abs(timeLeft - 5.0) <= 1.5) {
					NotificationHandler.NotifyWindows("Five-minute warning.\nFind a save point.");
					warning5 = true;
				} else if(!warning1 && Math.Abs(timeLeft - 1.0) <= 1.5) {
					NotificationHandler.NotifyWindows("One-minute warning.");
					warning1 = true;
				}
			} else {
				double breakTime = timeSinceLastSession();
				double timeLeft = minTimeBetweenSessions - breakTime;
				if(breakTime < minTimeBetweenSessions) {
					NotificationHandler.NotifyWindows("Not yet. Try again in " + (int)(timeLeft) + " minutes.");
					return false;
				}
			}
			return true;
		}

		private bool beforeEndAfterStart(DateTime tooEarly, DateTime tooLate) {
			bool beforeBedtime = DateTime.Now < tooLate;
			bool afterEarly = DateTime.Now >= tooEarly;
			return beforeBedtime && afterEarly;
		}

		private double totalSessionTime() {
			if(starts.Count == ends.Count)
				return 0.0;
			return (DateTime.Now - starts[starts.Count() - 1]).TotalMinutes;
		}

		private double totalNonSessionTime() {
			int count = Math.Min(starts.Count(), ends.Count());
			double runningTotal = 0.0;
			for(int i = 0; i < count; i++) {
				runningTotal += (ends[i] - starts[i]).TotalMinutes;
			}
			return runningTotal;
		}

		private double timeSinceLastSession() {
			if(ends.Count() == 0)
				return 61.0;
			return (DateTime.Now - ends[ends.Count() - 1]).TotalMinutes;
		}

		public bool IsTimerRunning() {
			return timerRunning;
		}

		public void StartTimer() {
			timerRunning = true;
			starts.Add(DateTime.Now);
			writeXML();
		}

		public void StopTimer() {
			timerRunning = false;
			ends.Add(DateTime.Now);
			writeXML();
		}
	}
}

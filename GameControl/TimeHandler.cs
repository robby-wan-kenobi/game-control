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
		private ExeHandler exeHandler;

		private DateTime fridayEnd;
		private DateTime otherEnd;
		private DateTime dayStart;

		private bool warningHour;
		private bool warning30;
		private bool warning10;
		private bool warning5;
		private bool warning1;

		public TimeHandler() {
			exeHandler = new ExeHandler();
			DateTime now = DateTime.Now;
			DateTime tomorrow = DateTime.Now.AddDays(1f);
			dayStart = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
			otherEnd = new DateTime(now.Year, now.Month, now.Day, 23, 00, 0);
			fridayEnd = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 1, 0, 0);

			warningHour = false;
			warning30 = false;
			warning10 = false;
			warning5 = false;
			warning1 = false;
		}

		private DateTime getTime() {
			return Properties.Settings.Default.PlayTime;
		}

		private float getDayTotal() {
			float total = 0f;
			if(Properties.Settings.Default.Playing)
				total = getCurrentTotal();
			return total + Properties.Settings.Default.DayTotal;
		}

		private float getCurrentTotal() {
			return DateTime.Now.Subtract(Properties.Settings.Default.PlayTime).Minutes;
		}

		public bool CheckTime() {
			switch(DateTime.Now.DayOfWeek) {
				case DayOfWeek.Sunday:
					NotificationHandler.NotifyWindows("Can't play on Sunday.");
					return false;
				case DayOfWeek.Saturday:
					return handleDay(5.0f * 60.0f, 120.0f, 60.0f, dayStart, otherEnd);
				case DayOfWeek.Friday:
					return handleDay(4.0f * 60.0f, 120.0f, 60.0f, dayStart, fridayEnd);
				default:
					return handleDay(2.0f * 60.0f, 120.0f, 60.0f, dayStart, otherEnd);
			}
		}

		private bool handleDay(float totalTimeForDay, float maxTimeForSession, float minTimeBetweenSessions, DateTime startOfTheDay, DateTime endOfTheDay) {
			if(!beforeEndAfterStart(startOfTheDay, endOfTheDay)) {
				NotificationHandler.NotifyWindows("You should be sleeping.");
				return false;
			}
			if(getDayTotal() >= totalTimeForDay) {
				return false;
			}
				
			bool liveSession = exeHandler.ExesRunning().Length > 0;
			if(liveSession) {
				float totalOfCurrentSession = getCurrentTotal();
				float timeLeft = maxTimeForSession - totalOfCurrentSession;
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
				float breakTime = timeSinceLastSession();
				float timeLeft = minTimeBetweenSessions - breakTime;
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

		private float timeSinceLastSession() {
			return DateTime.Now.Subtract(Properties.Settings.Default.PlayTime).Minutes;
		}

		public bool IsTimerRunning() {
			return Properties.Settings.Default.Playing;
		}

		public void StartTimer() {
			Properties.Settings.Default.PlayTime = DateTime.Now;
			Properties.Settings.Default.Playing = true;
			Properties.Settings.Default.Save();
		}

		public void StopTimer() {
			Properties.Settings.Default.PlayTime = DateTime.Now;
			Properties.Settings.Default.Playing = true;
			Properties.Settings.Default.Save();
		}
	}
}

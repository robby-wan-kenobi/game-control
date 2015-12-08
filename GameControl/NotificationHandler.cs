using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.System.Profile;
using System.Threading;

namespace GameControl {
	public class NotificationHandler {
		public static void NotifyWindows(string message, Action callback=null) {
			ShowToast(message);
			if(callback != null)
				callback();
		}

		private static void ShowToast(string message) {
			ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
			XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

			XmlNodeList toastText = toastXml.GetElementsByTagName("text");
			toastText[0].AppendChild(toastXml.CreateTextNode(message));

			IXmlNode toastNode = toastXml.SelectSingleNode("/toast");

			/*
			XmlElement audio = toastXml.CreateElement("audio");
			audio.SetAttribute("silent", "true");

			toastNode.AppendChild(audio);
			*/

			((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\",\"param1\":\"12345\",\"param2\":\"67890\"}");

			ToastNotification toast = new ToastNotification(toastXml);
			ToastNotificationManager.CreateToastNotifier("GameControl").Show(toast);
		}
	}
}

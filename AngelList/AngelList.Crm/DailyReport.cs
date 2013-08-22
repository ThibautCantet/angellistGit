using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Crm
{
    public class DailyReport
    {
		private static CrmSettings _crmSettings = new CrmSettings();
		public static void Send(string info)
		{
			MailMessage m = new MailMessage();
			SmtpClient sc = new SmtpClient();
			m.To.Add(new MailAddress(_crmSettings.ThibautEmail, _crmSettings.Thibaut));
		//	m.To.Add(new MailAddress(_crmSettings.ClementEmail, _crmSettings.Clement));

			m.Subject = "daily angellist report";

			try
			{
				using (StreamReader streamReader = new StreamReader(@"D:\Perso\AngelList\AngelList.Crm\Templates\DailyReport.html", Encoding.UTF8))
				{
					m.Body = string.Format(streamReader.ReadToEnd(), DateTime.Today.Date, info);

					streamReader.Close();
				}
				m.IsBodyHtml = true;
				sc.EnableSsl = true;
				sc.Send(m);
			}
			catch (Exception e)
			{
			}
		}
    }
}

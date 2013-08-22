using AngelList.DataAccess;
using AngelList.Entities.Startup;
using Newtonsoft.Json;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public class StartupRoleStream
	{
		public static Dictionary<Startup, Dictionary<string, List<Role>>> GetStartupRoles()
		{
			var res = new Dictionary<Startup, Dictionary<string, List<Role>>>();
			var startups = StartupStream.GetStartups();
			foreach (Startup startup in startups)
			{
				var roles = GetStartupRoles(startup.id);
				Dictionary<string, List<Role>> gp = roles.GroupBy(r => r.role).ToDictionary(k => k.Key, v => v.ToList());
				res.Add(startup, gp);
			}

			return res;
		}

		private static List<Role> GetStartupRoles(int id)
		{
			return GetStartups(id).startup_roles;
		}

		private static StartupRole GetStartups(int id)
		{
			try
			{
				string res = WebAccess.GetRequestResult(@"https://api.angel.co/1/startup_roles?v=1&startup_id=" +id);
				var startupRole = JsonConvert.DeserializeObject<StartupRole>(res);
				return startupRole;
			}
			catch (Exception ex)
			{
				Log.Error("GetStartups", "error", id.ToString());
				throw;
			}
		}
	}
}

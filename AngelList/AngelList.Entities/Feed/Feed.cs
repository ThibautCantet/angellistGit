using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Feed
{
    public class Feed
    {
		public string id { get; set; }
		public string timestamp { get; set; }
		public Item item { get; set; }
		public string description { get; set; }
		/// <summary>
		/// Ex : fond qui a fait l'investissement
		/// </summary>
		public Actor actor { get; set; }
		/// <summary>
		/// Ex : startup dans laquelle a investi le fond
		/// </summary>
		public Target target { get; set; }

		private string Action
		{
			get
			{
				if (description.Contains("incubate"))
				{
					return "incubated";
				}
				else if (description.Contains("invest"))
				{
					return "invested";
				}
				else
				{
					return "did something";
				}
			}
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3} {4} {5}", id, actor.id, actor.name, Action, target.id, target.name);
		}
    }
}

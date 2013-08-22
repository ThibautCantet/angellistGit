﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Feed
{
	public class Feeds
	{
		public List<Feed> feed { get; set; }
		public int total { get; set; }
		public int per_page { get; set; }
		public int page { get; set; }
		public int last_page { get; set; }
	}
}

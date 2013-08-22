using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Feed
{
	public class Item
	{
		private FeedTypes? _typeValue;

		public string type { get; set; }

		public FeedTypes typeValue
		{
			get
			{
				FeedTypes current = FeedTypes.Unknown;
				if (_typeValue == null && Enum.TryParse(type, out current))
				{
					_typeValue = current;
				}
				else
				{
					_typeValue = FeedTypes.Unknown;
				}
				return _typeValue.Value;
			}
		}
	//	public List<int> ids { get; set; }
	}
}

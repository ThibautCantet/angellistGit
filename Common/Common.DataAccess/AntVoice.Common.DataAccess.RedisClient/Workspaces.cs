using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient
{
	public enum Workspaces
	{
		StandardAlgorithmResults,
		AnalyticsBadgeQueueProcessor,
		AnalyticsActionQueueProcessor,
		RecommendationLogging
	}
}

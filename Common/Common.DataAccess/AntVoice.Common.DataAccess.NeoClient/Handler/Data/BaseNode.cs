// <copyright file="BaseNode.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

using System.Collections.Specialized;

namespace AntVoice.Common.DataAccess.NeoClient.Handler.Data
{
	using AntVoice.Common.DataAccess.NeoClient.Attribute;
	using AntVoice.Common.DataAccess.NeoClient.Contracts;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public abstract class BaseNode
	{
        #region Inherited from INode

		public abstract long GetBusinessKey();

		public abstract string GetBusinessKeyName();

		public string GetIndex()
		{
			return GetBusinessKeyName() + ":" + GetBusinessKey();
		}

		#endregion
	}
}

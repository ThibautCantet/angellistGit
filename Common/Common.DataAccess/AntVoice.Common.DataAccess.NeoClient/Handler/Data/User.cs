// <copyright file="User.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler.Data
{
    using System;
    using AntVoice.Common.Tools.Extensions;
    using AntVoice.Common.DataAccess.NeoClient.Attribute;

	[Serializable]
	public class User : BaseNode
	{
        private string _label;

		[Graphable]
		public string Label { get; set; }

		/// <summary>
		/// The unique member id across all platform
		/// </summary>
		[Graphable]
		public int UserId { get; set; }

        public User(int userId)
        {
            this.UserId = userId;
        }

		public User(int userId, string firstName, string lastName)
		{
			this.UserId = userId;
		    this._label = firstName + " " + lastName;
		}

	    public const string USER_INDEX_KEY = "UserId";

        public override long GetBusinessKey()
		{
			return this.UserId;
		}

		public override string GetBusinessKeyName()
		{
			return USER_INDEX_KEY;
		}
	}
}

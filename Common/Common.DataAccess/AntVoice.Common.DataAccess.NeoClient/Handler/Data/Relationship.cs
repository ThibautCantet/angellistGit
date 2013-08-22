// <copyright file="Relationship.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler.Data
{
	using AntVoice.Common.DataAccess.NeoClient.Contracts;
	using AntVoice.Common.Tools.Extensions;
	using System;

	/// <summary>
	/// Tells which type is an end node
	/// </summary>
	public enum RelationshipEnd
	{
		USER = 1,
		ITEM = 2,
		INTEREST = 4,
		TAG = 8
	}

	public class RelationshipData
	{
		public string Label { get; set; }

		public long CreationDate { get; set; }
	}

	public class Relationship
	{
		#region IGraphObject implementation

		public BaseNode Source { get; set; }

		public BaseNode Target { get; set; }

		public string Label { get; set; }

		public DateTimeOffset CreationDate { get; set; }

		public long? GraphId { get; set; }

		public bool IsDirectional { get; set; }

		#endregion

		private RelationshipEnd _sourceType;

		private RelationshipEnd _targetType;

		public Relationship(BaseNode source, BaseNode target, string label, DateTimeOffset creationDate)
		{
			Source = source;
			Target = target;
			Label = label;
			CreationDate = creationDate;

			_sourceType = RelationshipEnd.USER;
			if (Source is User) _sourceType = RelationshipEnd.USER;
			else if (Source is Product) _sourceType = RelationshipEnd.ITEM;
			else if (Source is Interest) _sourceType = RelationshipEnd.INTEREST;
			else if (Source is Tag) _sourceType = RelationshipEnd.TAG;

			_targetType = RelationshipEnd.USER;
			if (Target is User) _targetType = RelationshipEnd.USER;
			else if (Target is Product) _targetType = RelationshipEnd.ITEM;
			else if (Target is Interest) _targetType = RelationshipEnd.INTEREST;
			else if (Target is Tag) _targetType = RelationshipEnd.TAG;
		}


		public Relationship(BaseNode source, BaseNode target, string label, DateTimeOffset creationDate, bool isDirectional)
			: this(source, target, label, creationDate)
		{
			IsDirectional = isDirectional;
		}

		public RelationshipData GetRelationshipDetail()
		{
			return new RelationshipData
			{
				Label = Label,
				CreationDate = CreationDate.ToTimestamp()
			};
		}

		public string GetRelationType()
		{
			return GetRelationLabel(_sourceType, _targetType);
		}

		public static string GetRelationLabel(RelationshipEnd source, RelationshipEnd target)
		{
			string label = GetRelationLabel((int)source | (int)target);
			if (label == null)
			{
				throw new InvalidOperationException(string.Format("Relationship between {0} and {1} are invalid", source.ToString(), target.ToString()));
			}
			return label;
		}

		private static string GetRelationLabel(int relationship)
		{
			switch (relationship)
			{
				case 1:
					return "KNOWS"; // User + User
				case 2:
				case 4:
				case 8:
					return "LINKEDTO"; // Tag + Tag
				case 3:
					return "INTERACTS"; //  User + Item
				case 5:
					return "INTERESTEDIN"; // User + Interest
				case 6:
				case 12:
					return "ABOUT"; // Interest + Tag
				case 10:
					return "TAGGED"; // Item + Tag
				default:
					return null;
			}
		}
	}
}

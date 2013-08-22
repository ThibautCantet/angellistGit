// <copyright file="UpdateBehavior.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler
{
	using System;

	/// <summary>
	/// Tells if the node should be updated
	/// </summary>
	public enum UpdateNodeBehavior
	{
		Update,
		None
	}

	/// <summary>
	/// Falg to tell which behavior we should have regarding update
	/// </summary>
	public enum UpdateRelationshipBehavior
	{
		UpdateSourceNode = 1,
		UpdateTargetNode = 2,
		UpdateRelationship = 4
	}
}

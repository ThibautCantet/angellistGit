// <copyright file="ScriptBuilder.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Contracts.Gremlin
{
	using System;
	using System.Text;

	public class ScriptBuilder
	{
		private string[] _imports = new string[] { 
			"import org.neo4j.graphdb.index.*;", 
			"import org.neo4j.graphdb.*;", 
			"import org.neo4j.index.lucene.*;", 
			"import org.apache.lucene.search.*;" 
		};

		private StringBuilder _builder;

		public ScriptBuilder()
		{
		}

		public ScriptBuilder(string[] imports)
		{
			_imports = imports;
		}

		public void StartNewScript()
		{
			_builder = new StringBuilder();
			_builder.Append(string.Join("", _imports))
				.Append("neo4j = g.getRawGraph();")
				.Append("idx = neo4j.index().getNodeAutoIndexer().getAutoIndex();")
				.Append("tx = neo4j.beginTx();");
		}

		public ScriptBuilder AppendStatement(string statement)
		{
			_builder.AppendLine(statement);
			return this;
		}

		public ScriptBuilder AppendFormatStatement(string statement, params object[] parameters)
		{
			_builder.AppendLine(string.Format(statement, parameters));
			return this;
		}

		public string GetScript()
		{
			return _builder.ToString();
		}
	}
}

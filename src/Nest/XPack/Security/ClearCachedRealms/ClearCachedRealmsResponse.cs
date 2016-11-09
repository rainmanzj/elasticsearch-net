﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Nest
{
	public interface IClearCachedRealmsResponse : IResponse
	{
		[JsonProperty("cluster_name")]
		string ClusterName { get; }

		[JsonProperty("nodes")]
		IReadOnlyDictionary<string, ShieldNode>  Nodes { get; }
	}

	public class ClearCachedRealmsResponse : ResponseBase, IClearCachedRealmsResponse
	{
		public string ClusterName { get; internal set; }
		public IReadOnlyDictionary<string, ShieldNode> Nodes { get; internal set; } = EmptyReadOnly<string, ShieldNode>.Dictionary;
	}
}

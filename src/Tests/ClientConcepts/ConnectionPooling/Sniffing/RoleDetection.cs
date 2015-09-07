﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.ConnectionPool;
using Elasticsearch.Net.Providers;
using FluentAssertions;
using Nest;
using Tests.Framework;
using Tests.Framework.MockResponses;

namespace Tests.ClientConcepts.ConnectionPooling.Sniffing
{
	public class RoleDetection
	{
		/** == Sniffing role detection
		* 
		* When we sniff the custer state we detect the role of the node whether its master eligable and holds data
		* We use this information when selecting a node to perform an API call on.
		*/
		[U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
		public async Task DetectsMasterNodes()
		{
			var virtualWorld = new AuditTrailTester();
			virtualWorld.Cluster = () => Cluster
				.Nodes(10)
				.Sniff(s => s.FailAlways())
				.Sniff(s => s.OnPort(9202)
					.SucceedAlways(Cluster.Nodes(8).MasterEligable(9200, 9201, 9202))
				)
				.SniffingConnectionPool()
				.AllDefaults();

			virtualWorld.AssertPoolBeforeCall = (pool) =>
			{
				pool.Should().NotBeNull();
				pool.Nodes.Should().HaveCount(10);
				pool.Nodes.Where(n=>n.MasterEligable).Should().HaveCount(10);
			};

			virtualWorld.AssertPoolAfterCall = (pool) =>
			{
				pool.Should().NotBeNull();
				pool.Nodes.Should().HaveCount(8);
				pool.Nodes.Where(n=>n.MasterEligable).Should().HaveCount(3);
			};

			await virtualWorld.TraceStartup();
		}

		[U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
		public async Task DetectsDataNodes()
		{
			var virtualWorld = new AuditTrailTester();
			virtualWorld.Cluster = () => Cluster
				.Nodes(10)
				.Sniff(s => s.FailAlways())
				.Sniff(s => s.OnPort(9202)
					.SucceedAlways(Cluster.Nodes(8).StoresNoData(9200, 9201, 9202))
				)
				.SniffingConnectionPool()
				.AllDefaults();

			virtualWorld.AssertPoolBeforeCall = (pool) =>
			{
				pool.Should().NotBeNull();
				pool.Nodes.Should().HaveCount(10);
				pool.Nodes.Where(n=>n.HoldsData).Should().HaveCount(10);
			};

			virtualWorld.AssertPoolAfterCall = (pool) =>
			{
				pool.Should().NotBeNull();
				pool.Nodes.Should().HaveCount(8);
				pool.Nodes.Where(n=>n.HoldsData).Should().HaveCount(5);
			};

			await virtualWorld.TraceStartup();
		}
	}
}
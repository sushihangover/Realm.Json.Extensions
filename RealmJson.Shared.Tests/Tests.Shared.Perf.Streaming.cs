using System;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using NUnit.Framework;
using RealmJson.Extensions;
using Realms;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

#if __ANDROID__
using Android.App;
using Android.Util;
using Java.IO;
using System.Linq;
#endif
#if __IOS__
#endif

namespace RealmJson.Test
{
	[TestFixture]
	public partial class Tests
	{
#if true
		[Test]
		public void PerformanceProgressive()
		{
			var perfResults = new List<string>();
			uint readDelay = 500;
			var sw = new Stopwatch();
			var totalReconds = 0;
			var totalTime = 0.0;
			// Warmup
			CreateStreamBasedRecords(1 << 2, 500);
			GC.Collect();

			try
			{
				for (int i = (1 << 12); i <= (1 << 16); i++)
				{
					GC.Collect();
					sw.Restart();
					CreateStreamBasedRecords(1, readDelay);
					sw.Stop();
					var s = $"Records: {i}\t| {(sw.Elapsed.TotalMilliseconds - readDelay) / (i)} ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay} ms";
					perfResults.Add(s);
					totalReconds += i;
					totalTime += sw.Elapsed.TotalMilliseconds;
				}
				var a = $"Avg: {totalTime / totalReconds} ms/rec";
				perfResults.Add(a);
			}
			finally
			{
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
			}
		}

		[Test]
		public void Performance4KBoundary()
		{
			var perfResults = new List<string>();
			uint readDelay = 500;
			var sw = new Stopwatch();

			// Warmup
			CreateStreamBasedRecords(1 << 2, 500);
			GC.Collect();

			try
			{
				for (int i = (1 << 12) - (1 << 4); i <= (1 << 12) + (1 << 4); i++)
				{
					GC.Collect();
					sw.Restart();
					CreateStreamBasedRecords(1, readDelay);
					sw.Stop();
					var s = $"Records: {i}\t| {(sw.Elapsed.TotalMilliseconds - readDelay) / (i)} ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay} ms";
					perfResults.Add(s);
				}
			}
			finally
			{
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
			}
		}

		[Test]
		public void PerformanceOfJsonTextReader()
		{
			var perfResults = new List<string>();
			uint readDelay = 250;
			int max = 21;
			var sw = new Stopwatch();

			// Warmup
			CreateStreamBasedRecords(1 << 2, 100);
			GC.Collect();

			try
			{
				for (int i = 8; i <= max; i++)
				{
					GC.Collect();
					sw.Restart();
					CreateStreamBasedRecords(1 << i, readDelay);
					sw.Stop();
					var s = $"Records: {1 << i}\t| {(sw.Elapsed.TotalMilliseconds - readDelay)/(1 << i)}ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay}ms";
					perfResults.Add(s);
				}
			}
			finally
			{
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
			}
		}

		void CreateStreamBasedRecords(int recordCountRequest, uint readDelay)
		{
			const char jsonBegin = '[';
			const char jsonEnd = ']';
			const char jsonMore = ',';
			string jsonData = $"{{\"name\":\"1\",\"abbreviation\":\"1\"}}";
			var bufferLast = Encoding.UTF8.GetBytes(jsonData); 
			var bufferMore = Encoding.UTF8.GetBytes(jsonData + jsonMore);

			using (var stream = new SlidingStream())
			{
				Parallel.Invoke(() =>
				{
					stream.WriteByte((byte)jsonBegin);
					for (int i = 0; i < recordCountRequest; i++)
					{
						if (i < recordCountRequest - 1)
						{
							stream.Write(bufferMore, 0, bufferMore.Length);
						}
						else
						{
							stream.Write(bufferLast, 0, bufferLast.Length);
						}
					}
					stream.WriteByte((byte)jsonEnd);
				},
				() =>
				{
					Task.Delay((int)readDelay).Wait();
					using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
					{
						stream.ReadTimeout = 10 * 1000;
						theRealm.CreateAllFromJson<State>(stream);
						Assert.AreEqual(recordCountRequest,theRealm.All<State>().Count());
						Assert.IsTrue(theRealm.All<State>().Last().abbreviation == (1.ToString()), $"Last Record does not = {1.ToString()}");
					}
				});
			}
		}
#endif
	}
}

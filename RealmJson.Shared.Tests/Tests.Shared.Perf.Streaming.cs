using System;
using System.Text;
using NUnit.Framework;
using Realms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using SushiHangover.RealmJson;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace RealmJson.Test
{
	[TestFixture]
	public partial class Tests
	{

		//TODO: Refactor / There is lot of cut/paste code...

#if PERF
		static ulong availMemory;

		//[Test] // Testing memeory conditions of performing multi bulk loads...
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
					var s = $"Records: {1 << i}\t| {((sw.Elapsed.TotalMilliseconds - readDelay) / (1 << i)).ToString("0.####")}ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay}ms";
					perfResults.Add(s);
					totalReconds += i;
					totalTime += sw.Elapsed.TotalMilliseconds - readDelay;
				}
			}
			finally
			{
				Log("");
				Log(GetCurrentMethod());
				var a = $"Avg: {(totalTime / totalReconds).ToString("0.####")} ms/rec ({totalReconds}/{totalTime})";
				perfResults.Add(a);
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
				Log("");
			}
		}

		//[Test] // Testing weird 4k time spike... 
		public void Performance4KBoundary()
		{
			var perfResults = new List<string>();
			uint readDelay = 500;
			var sw = new Stopwatch();
			var totalReconds = 0;
			var totalTime = 0.0;
			// Warmup
			CreateStreamBasedRecords(1 << 2, 500);
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(0, GCCollectionMode.Forced, true);

			try
			{
				for (int i = (1 << 12) - (1 << 4); i <= (1 << 12) + (1 << 4); i++)
				{
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(0, GCCollectionMode.Forced, true);
					sw.Restart();
					CreateStreamBasedRecords(1, readDelay);
					sw.Stop();
					var s = $"Records: {1 << i}\t| {((sw.Elapsed.TotalMilliseconds - readDelay) / (1 << i)).ToString("0.####")}ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay}ms";
					perfResults.Add(s);
					totalReconds += i;
					totalTime += sw.Elapsed.TotalMilliseconds - readDelay;
				}
			}
			finally
			{
				Log("");
				Log(GetCurrentMethod());
				var a = $"Avg: {(totalTime / totalReconds).ToString("0.####")} ms/rec ({totalReconds}/{totalTime})";
				perfResults.Add(a);
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
				Log("");
			}
		}

		[Test]
		public void PerformanceOfJsonTextReaderUsingRealmManage()
		{
			var perfResults = new List<string>();
			uint readDelay = 250;
			int max = 21;
			var totalReconds = 0;
			var totalTime = 0.0;
			var sw = new Stopwatch();
			// Warmup
			CreateStreamBasedRecords(1 << 2, 100);
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(0, GCCollectionMode.Forced, true);

			try
			{
				for (int i = 8; i <= max; i++)
				{
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(0, GCCollectionMode.Forced, true);
					sw.Restart();
					CreateStreamBasedRecords(1 << i, readDelay, true);
					sw.Stop();
					var s = $"Records: {1 << i}\t| {((sw.Elapsed.TotalMilliseconds - readDelay) / (1 << i)).ToString("0.####")}ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay}ms\t| Mem: {availMemory} ";
					perfResults.Add(s);
					totalReconds += 1 << i;
					totalTime += sw.Elapsed.TotalMilliseconds - readDelay;
				}
			}
			finally
			{
				Log("");
				Log(GetCurrentMethod());
				var a = $"Avg: {(totalTime / totalReconds).ToString("0.####")} ms/rec ({totalReconds}/{totalTime})";
				perfResults.Add(a);
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
				Log("");
			}
		}

		[Test]
		public void PerformanceOfJsonTextReaderUsingAutoMapper()
		{
			var perfResults = new List<string>();
			uint readDelay = 250;
			int max = 21;
			var totalReconds = 0;
			var totalTime = 0.0;
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
					CreateStreamBasedRecords(1 << i, readDelay, updateExistingRecords: false, autoMapper: true);
					sw.Stop();
					var s = $"Records: {1 << i}\t| {((sw.Elapsed.TotalMilliseconds - readDelay) / (1 << i)).ToString("0.####")}ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay}ms\t| Mem: {availMemory} ";
					perfResults.Add(s);
					totalReconds += 1 << i;
					totalTime += sw.Elapsed.TotalMilliseconds - readDelay;
				}
			}
			finally
			{
				Log("");
				Log(GetCurrentMethod());
				var a = $"Avg: {(totalTime / totalReconds).ToString("0.####")} ms/rec ({totalReconds}/{totalTime})";
				perfResults.Add(a);
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
				Log("");
			}
		}


		//[Test]
		public void PerformanceOfJsonTextReaderNoUpdateRecords()
		{
			var perfResults = new List<string>();
			uint readDelay = 250;
			int max = 21;
			var totalReconds = 0;
			var totalTime = 0.0;
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
					CreateStreamBasedRecords(1 << i, readDelay, false);
					sw.Stop();
					var s = $"Records: {1 << i}\t| {((sw.Elapsed.TotalMilliseconds - readDelay) / (1 << i)).ToString("0.####")}ms/rec\t| Time: {sw.Elapsed.TotalMilliseconds - readDelay}ms";
					perfResults.Add(s);
					totalReconds += 1 << i;
					totalTime += sw.Elapsed.TotalMilliseconds - readDelay;
				}
			}
			finally
			{
				Log("");
				Log(GetCurrentMethod());
				var a = $"Avg: {(totalTime / totalReconds).ToString("0.####")} ms/rec ({totalReconds}/{totalTime})";
				perfResults.Add(a);
				foreach (var perfItem in perfResults)
				{
					Log(perfItem);
				}
				Log("");
			}
		}

		void CreateStreamBasedRecords(int recordCountRequest, uint readDelay, bool updateExistingRecords = true, bool autoMapper = false)
		{
			const char jsonBegin = '[';
			const char jsonEnd = ']';
			const char jsonMore = ',';
			string jsonData = $"{{\"name\":\"1\",\"abbreviation\":\"1\"}}";
			// Byte level arrays, no string creation in write to steam due to object creation and GC overhead in benchmarking
			var bufferLast = Encoding.UTF8.GetBytes(jsonData); 
			var bufferMore = Encoding.UTF8.GetBytes(jsonData + jsonMore);

			// TODO: Replace SlidingStream with a Circular single Byte buffer to avoid the memory allocs of `new ArraySegment<byte>` that it does...
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
						using (var transaction = theRealm.BeginWrite())
						{

							if (autoMapper)
							{
								theRealm.CreateAllFromJsonViaAutoMapper<State>(stream, inTransaction: true);
							}
							else
							{
								theRealm.CreateAllFromJson<State>(stream, updateExistingRecords, inTransaction: true);
							}
							availMemory = ConsumedMemory();
							transaction.Commit();
						}
						Assert.AreEqual(recordCountRequest,theRealm.All<State>().Count());
						Assert.IsTrue(theRealm.All<State>().Last().abbreviation == (1.ToString()), $"Last Record does not = {1.ToString()}");
					}
				});
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public string GetCurrentMethod()
		{
			var st = new StackTrace();
			var sf = st.GetFrame(1);
			return sf.GetMethod().Name;
		}

#endif

	}
}

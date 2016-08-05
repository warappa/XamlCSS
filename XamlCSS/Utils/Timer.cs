using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XamlCSS.Utils
{
	internal sealed class Timer : CancellationTokenSource, IDisposable
	{
		public Timer(TimeSpan period, Action<object> callback, object state = null, TaskScheduler taskScheduler = null)
		{
			Task.Delay(1, Token).ContinueWith(async (t, s) =>
			{
				var tuple = (Tuple<Action<object>, object>)s;

				while (!IsCancellationRequested)
				{
					await Task.Run(() => tuple.Item1(tuple.Item2)).ConfigureAwait(true);
					await Task.Delay(period).ConfigureAwait(true);
				}
			}, Tuple.Create(callback, state), CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
				taskScheduler ?? TaskScheduler.FromCurrentSynchronizationContext());
		}

		public new void Dispose() { base.Cancel(); }
	}
}

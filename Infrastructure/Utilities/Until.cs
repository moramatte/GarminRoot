using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public static class Until
    {
        public static async Task Changed(IList list, CancellationToken token)
        {
            var startCount = list.Count;
           
            while (startCount == list.Count)
            {
                await Task.Delay(20);
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
            }
        }
    }
}

using System.Threading;

namespace CoffeeMachineAPI.Services
{
    public class RequestCounter : IRequestCounter
    {

        private int _count = 0;

        public int Increment()
        {
            return Interlocked.Increment(ref _count);
        }

    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleObjectPool {
    public class Program
    {
        public static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'c' || Console.ReadKey().KeyChar == 'C')
                    cts.Cancel();
            }, cts.Token);

            ObjectPool<TestClass> pool = new TestPool();

            Parallel.For(0, 1000000, (i, loopState) =>
            {
                TestClass mc = pool.CheckOut();
                Console.CursorLeft = 0;
                Console.WriteLine("{0:####.####}", mc.GetValue(i));

                pool.CheckIn(mc);
                if (cts.Token.IsCancellationRequested)
                    loopState.Stop();

            });
            
            Console.WriteLine("Press the Enter key to exit.");
            Console.ReadLine();
            cts.Dispose();
        }
    }
}

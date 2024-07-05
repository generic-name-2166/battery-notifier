using System.Runtime.Versioning;
using WmiLight;

namespace BatteryNotifier
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static ushort GetBattery()
        {
            // https://stackoverflow.com/a/8946096
            // https://github.com/MartinKuschnik/WmiLight//README.md

            using WmiConnection con = new();
            var collection = con.CreateQuery("SELECT * FROM Win32_Battery");
            foreach (WmiObject mo in collection)
            {
                var estimated = mo.GetPropertyValue("EstimatedChargeRemaining");
                if (estimated is ushort v)
                    return v;
            }
            return ushort.MaxValue;
        }

        static bool CheckSanity()
        {
            var maybe = GetBattery();
            if (maybe == ushort.MaxValue)
            {
                return false;
            }
            else
            {
                Notify(maybe);
                return true;
            }
        }

        static void Notify(ushort percentage)
        {
            const ushort LOWER = 48;
            const ushort UPPER = 58;

            if (percentage < LOWER)
                Console.WriteLine($"Battery percent {percentage} too low. Connect a charger");
            else if (percentage > UPPER)
                Console.WriteLine($"Battery percent {percentage} too high. Disconnect charger");
            else
                Console.WriteLine($"Battery percent {percentage}");

            Notifier.notify(percentage);
        }

        static void Handle(object? source, System.Timers.ElapsedEventArgs? args)
        {
            Notify(GetBattery());
        }

        static void Setup()
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer.interval?view=net-8.0
            // 'Timer' is an ambiguous reference between 'System.Timers.Timer' and 'System.Threading.Timer'
            System.Timers.Timer timer = new();

            const int POLL_TIME = 1000 * 60;
            timer.Interval = POLL_TIME;
            timer.Elapsed += Handle;
            timer.AutoReset = true;
            // Start the timer
            timer.Enabled = true;

            Console.WriteLine("Press the Enter key to exit the program at any time... ");
            Console.ReadLine();
        }

        static int Main()
        {
            if (!CheckSanity())
            {
                Console.WriteLine("Could not find a battery on this decvice");
                return 1;
            }

            Setup();
            return 0;
        }
    }
}

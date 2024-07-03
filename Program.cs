using System.Management;
using System.Runtime.Versioning;

namespace BatteryNotifier
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static ushort GetBattery()
        {
            // https://stackoverflow.com/a/8946096
            ObjectQuery query = new("SELECT * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new(query);

            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject mo in collection.Cast<ManagementObject>())
            {
                var estimated = mo.GetPropertyValue("EstimatedChargeRemaining");
                if (estimated is ushort v)
                    return v;
            }
            return ushort.MaxValue;
        }

        static bool CheckSanity(out ushort percentage)
        {
            var maybe = GetBattery();
            if (maybe == ushort.MaxValue)
            {
                percentage = 0;
                return false;
            }
            else
            {
                percentage = maybe;
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

            const int TWO_MINUTES = 1000 * 60 * 2;
            timer.Interval = TWO_MINUTES;
            timer.Elapsed += Handle;
            timer.AutoReset = true;
            // Start the timer
            timer.Enabled = true;

            // First event manual
            Handle(null, null);

            Console.WriteLine("Press the Enter key to exit the program at any time... ");
            Console.ReadLine();
        }

        static int Main()
        {
            if (!CheckSanity(out ushort percentage))
            {
                Console.WriteLine("Could not find a battery on this decvice");
                return 1;
            }

            Setup();
            return 0;
        }
    }
}

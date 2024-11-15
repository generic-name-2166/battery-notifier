using System.Runtime.Versioning;
using WmiLight;

namespace BatteryNotifier
{
    enum Battery
    {
        // The battery is below the lower bound and is not charging
        Low,

        // The battery is above the upper bound and is not in power saving mode
        High,

        // The battery is either within the bounds or doesn't fall in either of the above
        Good,
    }

    [SupportedOSPlatform("windows")]
    class Program
    {
        static Battery GetStatus(ushort percentage, bool saving, bool charging)
        {
            const ushort LOWER = 48;
            const ushort UPPER = 58;

            if (percentage < LOWER && !charging)
                return Battery.Low;
            else if (percentage > UPPER && !saving)
                return Battery.High;
            return Battery.Good;
        }

        static (ushort, Battery) GetBattery()
        {
            // https://stackoverflow.com/a/8946096
            // https://github.com/MartinKuschnik/WmiLight//README.md
            // https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-battery
            const string CHARGE_COLUMN = "EstimatedChargeRemaining";
            const string AVAILABILITY_COLUMN = "Availability";
            const string STATUS_COLUMN = "BatteryStatus";
            const string QUERY =
                $"SELECT {CHARGE_COLUMN}, {AVAILABILITY_COLUMN}, {STATUS_COLUMN} FROM Win32_Battery";

            using WmiConnection con = new();
            var collection = con.CreateQuery(QUERY);
            ushort percentage = ushort.MaxValue;
            bool saving = false;
            bool charging = false;

            foreach (WmiObject mo in collection)
            {
                try
                {
                    var estimated = mo.GetPropertyValue<ushort>(CHARGE_COLUMN);
                    // charge value in percentage
                    percentage = estimated;

                    var availability = mo.GetPropertyValue<ushort>(AVAILABILITY_COLUMN);
                    // values that mean power saving mode is on
                    saving = availability == 13 || availability == 14 || availability == 17;

                    var status = mo.GetPropertyValue<ushort>(STATUS_COLUMN);
                    // values that mean that the device is charging
                    charging = status >= 6 && status <= 9;
                }
                catch (InvalidCastException)
                { /* No battery data on device, continue */
                }
            }

            return (percentage, GetStatus(percentage, saving, charging));
        }

        static bool CheckSanity()
        {
            var (percentage, battery) = GetBattery();
            if (percentage == ushort.MaxValue)
            {
                return false;
            }
            else
            {
                Notify(percentage, battery);
                return true;
            }
        }

        static void Notify(ushort percentage, Battery battery)
        {
            var output = $"Battery percent {percentage}";

            switch (battery)
            {
                case Battery.Low:
                {
                    Console.WriteLine(output + " too low. Connect a charger");
                    Notifier.notify(percentage);
                    break;
                }
                case Battery.High:
                {
                    Console.WriteLine(output + " too high. Disconnect charger");
                    Notifier.notify(percentage);
                    break;
                }
                case Battery.Good:
                    Console.WriteLine(output);
                    break;
            }
        }

        static void Handle(object? source, System.Timers.ElapsedEventArgs? args)
        {
            var (percentage, battery) = GetBattery();
            Notify(percentage, battery);
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

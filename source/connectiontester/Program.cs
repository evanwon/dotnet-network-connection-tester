using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace connectiontester
{
    /// <summary>
    /// Based on: http://stackoverflow.com/a/8345173/606910 with additional diagnostics
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Checking network availability...");

            IsNetworkAvailable();

            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }

        /// <summary>
        /// Indicates whether any network connection is available.
        /// Filter connections below a specified speed, as well as virtual network cards.
        /// </summary>
        /// <param name="minimumSpeed">The minimum speed required. Passing 0 will not filter connection using speed.</param>
        /// <returns>
        ///     <c>true</c> if a network connection is available; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNetworkAvailable(long minimumSpeed = 0)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Console.WriteLine(".NET Framework reported that no network was available.");
                return false;
            }

            var adapters = NetworkInterface.GetAllNetworkInterfaces();

            Console.WriteLine(string.Format("Found {0} available network adapters...", adapters.Count()));

            foreach (var ni in adapters)
            {
                // discard because of standard reasons
                if ((ni.OperationalStatus != OperationalStatus.Up)
                    || (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    || (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                {
                    Console.WriteLine(string.Format("Skipping adapter ({0}): Status: {1} Type: {2}", ni.Name, ni.OperationalStatus, ni.NetworkInterfaceType));
                    continue;
                }

                // this allow to filter modems, serial, etc.
                // I use 10000000 as a minimum speed for most cases
                if (ni.Speed < minimumSpeed)
                {
                    Console.WriteLine("Skipping adapter, speed lower than minimum: " + ni.Speed);
                    continue;
                }

                // discard virtual cards (virtual box, virtual pc, etc.)
                if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0)
                    || (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    Console.WriteLine(
                        string.Format(
                            "Skipping adapter ({0}), virtual detected in name or descrption. Name: {1}, Description: {2}.",
                            ni.Name,
                            ni.Name,
                            ni.Description));

                    continue;
                }

                // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        string.Format("Skipping adapter ({0}), loopback detected: {1}", ni.Name, ni.Description));

                    continue;
                }

                Console.WriteLine("Network available is true!");
                return true;
            }

            Console.WriteLine("Network available is false!");
            return false;
        }
    }


}

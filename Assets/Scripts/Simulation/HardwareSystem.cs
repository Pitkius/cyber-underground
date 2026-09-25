using System.Collections.Generic;

namespace CyberUnderground.Simulation
{
    public sealed class HardwareSystem
    {
        readonly HashSet<string> _owned;

        public string Name { get; private set; }
        public int RamGb { get; private set; }
        public int CpuTier { get; private set; }
        public int StorageGb { get; private set; }
        public int NetTier { get; private set; }
        public int MaxWindows { get; private set; }

        public HardwareSystem()
        {
            _owned = new HashSet<string>();
            ApplyBaseLaptop();
        }

        public bool Owns(string upgradeId)
        {
            return _owned.Contains(upgradeId);
        }

        public IEnumerable<string> OwnedIds()
        {
            return _owned;
        }

        public void MarkOwned(string upgradeId)
        {
            _owned.Add(upgradeId);
        }

        public void ApplyBaseLaptop()
        {
            Name = "Old Campus Laptop";
            RamGb = 4;
            CpuTier = 0;
            StorageGb = 128;
            NetTier = 0;
            MaxWindows = 3;
            _owned.Clear();
        }

        public void ApplyUpgrade(string upgradeId)
        {
            if (upgradeId == "ram_8")
            {
                RamGb = 8;
                MaxWindows = 5;
            }
            else if (upgradeId == "net_share")
            {
                NetTier = 1;
            }
            else if (upgradeId == "cpu_refurb")
            {
                CpuTier = 1;
                Name = "Refurbished Laptop";
            }

            _owned.Add(upgradeId);
        }

        public void Restore(int ramGb, int cpuTier, int storageGb, int netTier, int maxWindows, IEnumerable<string> owned)
        {
            RamGb = ramGb;
            CpuTier = cpuTier;
            StorageGb = storageGb;
            NetTier = netTier;
            MaxWindows = maxWindows;
            Name = cpuTier >= 1 ? "Refurbished Laptop" : "Old Campus Laptop";
            _owned.Clear();
            if (owned == null)
                return;
            foreach (var id in owned)
            {
                if (!string.IsNullOrEmpty(id))
                    _owned.Add(id);
            }
        }

        public string CpuLabel
        {
            get { return CpuTier >= 1 ? "refurbished quad-core" : "weak dual-core"; }
        }

        public string NetLabel
        {
            get { return NetTier >= 1 ? "shared fiber" : "slow campus wi-fi"; }
        }

        public string Summary()
        {
            return Name
                + "\nRAM: " + RamGb + " GB"
                + "\nCPU: " + CpuLabel
                + "\nStorage: " + StorageGb + " GB"
                + "\nNetwork: " + NetLabel
                + "\nWindows at once: " + MaxWindows;
        }
    }
}

namespace CyberUnderground.Simulation
{
    public sealed class ReputationSystem
    {
        readonly int[] _values;

        public ReputationSystem()
        {
            _values = new int[4];
        }

        public int Total
        {
            get { return Get(RepKind.Hacker) + Get(RepKind.Underground) + Get(RepKind.Corporate) + Get(RepKind.Intelligence); }
        }

        public int Get(RepKind kind)
        {
            return _values[(int)kind];
        }

        public void Add(RepKind kind, int delta)
        {
            int next = _values[(int)kind] + delta;
            if (next < 0)
                next = 0;
            _values[(int)kind] = next;
        }

        public void Restore(int hacker, int underground, int corporate, int intelligence)
        {
            _values[0] = Clamp(hacker);
            _values[1] = Clamp(underground);
            _values[2] = Clamp(corporate);
            _values[3] = Clamp(intelligence);
        }

        static int Clamp(int value)
        {
            return value < 0 ? 0 : value;
        }

        public string Summary()
        {
            return "Reputation " + Total
                + "   hacker " + Get(RepKind.Hacker)
                + "   underground " + Get(RepKind.Underground)
                + "   corporate " + Get(RepKind.Corporate)
                + "   intelligence " + Get(RepKind.Intelligence);
        }
    }
}

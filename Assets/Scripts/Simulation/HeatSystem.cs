namespace CyberUnderground.Simulation
{
    public sealed class HeatSystem
    {
        public int Value { get; private set; }

        public void Add(int delta)
        {
            int next = Value + delta;
            if (next < 0)
                next = 0;
            if (next > 100)
                next = 100;
            Value = next;
        }

        public void Restore(int value)
        {
            if (value < 0)
                value = 0;
            if (value > 100)
                value = 100;
            Value = value;
        }
    }
}

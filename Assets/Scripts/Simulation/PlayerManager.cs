using System.Collections.Generic;

namespace CyberUnderground.Simulation
{
    public sealed class LedgerLine
    {
        public string Label;
        public int Amount;
    }

    public sealed class PlayerManager
    {
        public const int StartingMoney = 17;
        public const int SemesterGoal = 500;

        readonly List<LedgerLine> _ledger = new List<LedgerLine>();

        public int MoneyEuros { get; private set; }

        public PlayerManager()
        {
            MoneyEuros = StartingMoney;
            _ledger.Add(new LedgerLine { Label = "Opening balance", Amount = StartingMoney });
        }

        public IReadOnlyList<LedgerLine> Ledger
        {
            get { return _ledger; }
        }

        public void Earn(int amount, string label)
        {
            if (amount <= 0)
                return;
            MoneyEuros += amount;
            _ledger.Add(new LedgerLine { Label = label, Amount = amount });
        }

        public bool TrySpend(int amount, string label)
        {
            if (amount < 0 || MoneyEuros < amount)
                return false;
            MoneyEuros -= amount;
            _ledger.Add(new LedgerLine { Label = label, Amount = -amount });
            return true;
        }

        public void RestoreMoney(int value)
        {
            MoneyEuros = value < 0 ? 0 : value;
        }

        public void RestoreLedger(IList<LedgerLine> lines)
        {
            _ledger.Clear();
            if (lines == null || lines.Count == 0)
            {
                _ledger.Add(new LedgerLine { Label = "Balance", Amount = MoneyEuros });
                return;
            }
            for (int i = 0; i < lines.Count; i++)
                _ledger.Add(lines[i]);
        }
    }
}

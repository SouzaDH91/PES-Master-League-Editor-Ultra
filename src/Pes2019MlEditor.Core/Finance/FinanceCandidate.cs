namespace Pes2019MlEditor.Core.Finance;

public sealed class FinanceCandidate
{
    public int Offset { get; set; }
    public long FoundValue { get; set; }
    public int ProximityToSecondValue { get; set; } = -1;
    public long SecondValue { get; set; }
    public int SecondValueOffset { get; set; } = -1;

    public override string ToString()
    {
        if (ProximityToSecondValue >= 0)
        {
            return $"Offset 0x{Offset:X8} [Transf: {FoundValue:N0} € | Saldo/Teto: {SecondValue:N0} €]";
        }
        return $"Offset 0x{Offset:X8} [Transf: {FoundValue:N0} €]";
    }
}

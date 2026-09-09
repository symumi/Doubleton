namespace BalartroLike.Battle
{
    public sealed class BattleCommandResult
    {
        public bool Success { get; }
        public string Error { get; }
        public BattleCalculation Calculation { get; }

        private BattleCommandResult(bool success, string error, BattleCalculation calculation)
        {
            Success = success;
            Error = error;
            Calculation = calculation;
        }

        public static BattleCommandResult Ok(BattleCalculation calculation)
        {
            return new BattleCommandResult(true, string.Empty, calculation);
        }

        public static BattleCommandResult Fail(string error)
        {
            return new BattleCommandResult(false, error, null);
        }
    }
}
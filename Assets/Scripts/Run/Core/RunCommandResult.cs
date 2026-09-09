namespace BalartroLike.Run
{
    public sealed class RunCommandResult
    {
        public bool Success { get; }
        public string Error { get; }

        private RunCommandResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public static RunCommandResult Ok()
        {
            return new RunCommandResult(true, string.Empty);
        }

        public static RunCommandResult Fail(string error)
        {
            return new RunCommandResult(false, error);
        }
    }
}
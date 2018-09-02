namespace XamlCSS
{
    public class MatchResult
    {
        public static MatchResult Success = new MatchResult(true);
        public static MatchResult ItemFailed = new MatchResult(false, true, false, false);
        public static MatchResult DirectParentFailed = new MatchResult(false, false, true, false);
        public static MatchResult GeneralParentFailed = new MatchResult(false, false, false, true);

        private MatchResult(bool isSuccess, bool hasItemFailed, bool hasDirectParentFailed, bool hasGeneralParentFailed)
        {
            IsSuccess = isSuccess;
            HasItemFailed = hasItemFailed;
            HasDirectParentFailed = hasDirectParentFailed;
            HasGeneralParentFailed = hasGeneralParentFailed;
        }

        private MatchResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; private set; }
        public bool HasItemFailed { get; private set; }
        public bool HasDirectParentFailed { get; private set; }
        public bool HasGeneralParentFailed { get; private set; }
        public int Group { get; internal set; }
    }
}
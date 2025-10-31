namespace RosettaUI.Builder
{
    public enum InOrOut
    {
        In,
        Out
    }
    
    public static class InOrOutExtensions
    {
        public static InOrOut Opposite(this InOrOut inOrOut)
        {
            return inOrOut == InOrOut.In ? InOrOut.Out : InOrOut.In;
        }
    }
}
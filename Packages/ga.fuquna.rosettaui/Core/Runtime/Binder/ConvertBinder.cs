namespace RosettaUI
{
    public abstract class ConvertBinder<TParent, TValue> : ChildBinder<TParent, TValue>
    {
        public override bool IsConst => parentBinder.IsConst;
        public override bool IsReadOnly => parentBinder.IsReadOnly;
        

        protected ConvertBinder(IBinder<TParent> parentBinder) : base(parentBinder)
        {
        }
    }
}
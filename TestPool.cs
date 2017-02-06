namespace SimpleObjectPool
{
    class TestPool : ObjectPool<TestClass>
    {
        protected override TestClass Create()
        {
            return new TestClass();
        }

        public override bool Validate(TestClass o)
        {
            return true;
        }

        public override void Expire(TestClass o)
        {
           
        }
    }
}

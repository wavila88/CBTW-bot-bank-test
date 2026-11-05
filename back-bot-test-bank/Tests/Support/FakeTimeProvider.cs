namespace Tests.Support
{
    /// <summary>A clock the test moves by hand, so no test waits for real time.</summary>
    public sealed class FakeTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan time) => now += time;
    }
}

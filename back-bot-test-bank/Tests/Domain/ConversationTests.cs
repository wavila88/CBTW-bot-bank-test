using Domain.Models;

namespace Tests.Domain
{
    public class ConversationTests
    {
        private static readonly DateTimeOffset Start = new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);
        private static readonly TimeSpan Idle = TimeSpan.FromMinutes(10);

        [Fact]
        public void ShouldForget_IsFalseUpToTheIdleTimeAndTrueAfterItAndFalseWhenUnknown()
        {
            var conversation = new Conversation("c1", Start);

            Assert.False(Conversation.ShouldForget(conversation, Start + Idle, Idle));
            Assert.True(Conversation.ShouldForget(conversation, Start + Idle + TimeSpan.FromSeconds(1), Idle));
            Assert.False(Conversation.ShouldForget(null, Start + Idle + TimeSpan.FromSeconds(1), Idle));
        }
    }
}

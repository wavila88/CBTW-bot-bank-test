using Domain.Models;
using Infra.AI.SemanticKernel;

namespace Tests.Infra
{
    public class SemanticKernelConversationStoreTests
    {
        [Fact]
        public async Task GetTurnsAsync_ReturnsTheSavedTurnsInOrder()
        {
            var store = new SemanticKernelConversationStore();
            await store.AddTurnAsync("c1", new ChatTurn("q1", "a1"));
            await store.AddTurnAsync("c1", new ChatTurn("q2", "a2"));

            var turns = await store.GetTurnsAsync("c1");

            Assert.Equal(new[] { new ChatTurn("q1", "a1"), new ChatTurn("q2", "a2") }, turns);
        }

        [Fact]
        public async Task GetTurnsAsync_ReturnsNothingForAnUnknownConversation()
        {
            var turns = await new SemanticKernelConversationStore().GetTurnsAsync("unknown");

            Assert.Empty(turns);
        }

        [Fact]
        public async Task GetTurnsAsync_KeepsConversationsApartById()
        {
            var store = new SemanticKernelConversationStore();
            await store.AddTurnAsync("c1", new ChatTurn("q1", "a1"));
            await store.AddTurnAsync("c2", new ChatTurn("q2", "a2"));

            var turns = await store.GetTurnsAsync("c1");

            Assert.Equal(new[] { new ChatTurn("q1", "a1") }, turns);
        }
    }
}

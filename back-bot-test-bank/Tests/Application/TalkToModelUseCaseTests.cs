using Application.UseCases;
using Domain.Exceptions;
using Domain.Models;
using Domain.Ports;
using Moq;

namespace Tests.Application
{
    public class TalkToModelUseCaseTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExecuteAsync_RejectsAnEmptyMessageWithoutCallingTheModel(string message)
        {
            var model = new Mock<IChatModel>();

            var ex = await Assert.ThrowsAsync<DomainException>(() => new TalkToModelUseCase(model.Object).ExecuteAsync(message));

            Assert.Equal(ErrorCode.InvalidRequest, ex.Code);
            model.Verify(m => m.CompleteAsync(It.IsAny<IReadOnlyList<ChatMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_SendsTheTrimmedMessageAsTheOnlyUserMessageAndReturnsTheReply()
        {
            var model = new Mock<IChatModel>();
            IReadOnlyList<ChatMessage>? sent = null;
            model.Setup(m => m.CompleteAsync(It.IsAny<IReadOnlyList<ChatMessage>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyList<ChatMessage>, CancellationToken>((messages, _) => sent = messages)
                .ReturnsAsync("Hello!");

            var reply = await new TalkToModelUseCase(model.Object).ExecuteAsync("  Say hello  ");

            Assert.Equal("Hello!", reply);
            var message = Assert.Single(sent!);
            Assert.Equal(ChatRole.User, message.Role);
            Assert.Equal("Say hello", message.Content);
        }
    }
}

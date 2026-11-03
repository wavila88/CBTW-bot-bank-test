using Api.Controllers;
using Api.DTO;
using Application.DTO;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Api
{
    public class ChatControllerTests
    {
        private readonly Mock<IAskQuestionUseCase> askQuestion = new();

        private ChatController Controller() => new(askQuestion.Object, Mock.Of<ILogger<ChatController>>());

        [Fact]
        public async Task Post_SendsTheMessageAndTheConversationIdToTheUseCase()
        {
            askQuestion.Setup(a => a.ExecuteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AnswerDTO("ok", Array.Empty<SourceDTO>(), "c-1"));

            await Controller().Post(new ChatRequest("How do I block my card?", "c-1"), CancellationToken.None);

            askQuestion.Verify(a => a.ExecuteAsync("How do I block my card?", "c-1", It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Post_ReturnsTheAnswerTheSourcesInOrderAndTheConversationId()
        {
            askQuestion.Setup(a => a.ExecuteAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AnswerDTO(
                    "Open Card > Block. [KB-03]",
                    new[] { new SourceDTO("KB-03-q1", 0.52), new SourceDTO("KB-01-s2", 0.81) },
                    "c-1"));

            var result = await Controller().Post(new ChatRequest("hello", null), CancellationToken.None);

            var response = Assert.IsType<ChatResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
            Assert.Equal("Open Card > Block. [KB-03]", response.Answer);
            Assert.Equal(new[] { "KB-03-q1", "KB-01-s2" }, response.Sources.Select(s => s.Id));
            Assert.Equal(new[] { 0.52, 0.81 }, response.Sources.Select(s => s.Score));
            Assert.Equal("c-1", response.ConversationId);
        }
    }
}

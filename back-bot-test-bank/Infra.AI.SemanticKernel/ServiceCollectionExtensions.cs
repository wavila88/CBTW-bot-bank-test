using Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Infra.AI.SemanticKernel
{
    public static class ServiceCollectionExtensions
    {
        // Registers the chat model port, backed by OpenAI through Semantic Kernel. Fails at startup if a setting is missing.
        public static IServiceCollection AddSemanticKernelChatModel(this IServiceCollection services, OpenAIChatOptions options)
        {
            options.Validate();
            services.AddSingleton<IChatModel>(_ => new SemanticKernelChatModel(new OpenAIChatCompletionService(options.Model, options.ApiKey)));
            return services;
        }

        // Registers the in-memory conversation store. One instance for the whole app, so every request sees the same conversations.
        public static IServiceCollection AddSemanticKernelConversationStore(this IServiceCollection services)
        {
            services.AddSingleton<IConversationStore, SemanticKernelConversationStore>();
            return services;
        }
    }
}

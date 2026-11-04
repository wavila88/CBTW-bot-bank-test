import { createFeatureContext } from '../../../shared/context/createFeatureContext';
import { chatReducer, initialChatState, type ChatAction, type ChatState } from './chatReducer';

const chat = createFeatureContext<ChatState, ChatAction>('Chat', chatReducer, initialChatState);

export const ChatProvider = chat.Provider;
export const useChatState = chat.useFeature;

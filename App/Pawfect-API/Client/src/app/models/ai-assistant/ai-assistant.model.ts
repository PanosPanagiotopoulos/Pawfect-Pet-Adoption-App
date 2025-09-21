import { AiMessageRole } from "src/app/common/enum/ai-message-role.enum";

export interface CompletionsRequest {
    prompt: string;
    contextAnimalId?: string;
    conversationHistory?: AiMessage[];
}

export interface AiMessage {
    role: AiMessageRole;
    content: string;
}

export interface CompletionsResponse {
    response: string;
}

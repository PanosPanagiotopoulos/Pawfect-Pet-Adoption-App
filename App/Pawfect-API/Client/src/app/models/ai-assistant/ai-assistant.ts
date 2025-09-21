export interface CompletionsRequest {
    prompt: string;
    contextAnimalId?: string;
}

export interface CompletionsResponse {
    response: string;
}

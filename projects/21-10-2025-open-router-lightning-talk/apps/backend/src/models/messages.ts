export type Message = {
    id: number;
    content: string;
    created_at: string;
    sender: string;
    role: 'system' | 'user' | 'assistant'
}
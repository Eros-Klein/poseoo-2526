import type { OpenRouter } from "@openrouter/sdk";

export async function fallback(prompt: string, openRouter: OpenRouter) {
    const completion = await openRouter.chat.send({
        models: ['xiaomi/mimo-v2-flash', 'cognitivecomputations/dolphin-mistral-24b-venice-edition:free'],
        messages: [
            {
                role: 'user',
                content: prompt,
            },
        ],
    });

    return completion.choices[0]?.message?.content;
}
import { OpenRouter } from '@openrouter/sdk';

export async function variants(prompt: string, openRouter: OpenRouter) {
    const completion = await openRouter.chat.send({
        models: ['xiaomi/mimo-v2-flash:online', 'openai/gpt-4o-mini:nitro', 'openai/gpt-4o-mini:thinking', /*'openai/gpt-4o-mini:exacto'*/],
        messages: [
            {
                role: 'user',
                content: prompt,
            },
        ],
    });

    return completion.choices[0]?.message?.content;
}

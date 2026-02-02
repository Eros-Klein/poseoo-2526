import { OpenRouter } from '@openrouter/sdk';

export async function normal(prompt: string, openRouter: OpenRouter) {
    const completion = await openRouter.chat.send({
        model: 'xiaomi/mimo-v2-flash',
        messages: [
            {
                role: 'user',
                content: prompt,
            },
        ],
    });

    return completion.choices[0]?.message?.content;
}

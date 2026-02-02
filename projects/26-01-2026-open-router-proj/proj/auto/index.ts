import { OpenRouter } from '@openrouter/sdk';

export async function auto(prompt: string, openRouter: OpenRouter) {
    const completion = await openRouter.chat.send({
        model: 'openrouter/auto',
        messages: [
            {
                role: 'user',
                content: prompt,
            },
        ],
    });

    return completion.model + ': ' + completion.choices[0]?.message?.content;
}

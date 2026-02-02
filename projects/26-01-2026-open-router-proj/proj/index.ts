import { OpenRouter } from '@openrouter/sdk';
import dotenv from 'dotenv';
import readline from 'readline';
import { normal } from './normal/index.js';
import { fallback } from './fallback/index.js';
import { variants } from './variants/index.js';
import { auto } from './auto/index.js';

dotenv.config();

const openRouter = new OpenRouter({
    apiKey: process.env.OPENROUTER_API_KEY
});

const openRouterFree = new OpenRouter({
    apiKey: process.env.OPENROUTER_API_KEY_FREE
});

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
});

rl.write('\n\n\n\n\n\n\n\n\n----------------------\n');
rl.write('OpenRouter Demo: \n');
rl.write('----------------------\n');

const prompt: string = await new Promise((resolve) => {
    rl.question('Enter your prompt: ', (answer) => {
        resolve(answer);
    });
});

rl.write('----------------------\n');

const mode: string = await new Promise((resolve) => {
    rl.question('Enter the mode (1=normal, 2=fallback, 3=variants, 4=auto): ', (answer) => {
        rl.close();
        resolve(answer);
    });
});
        
if (mode === '1') {
    const result = await normal(prompt, openRouter);
    console.log(result);
}
else if (mode === '2') {
    const result = await fallback(prompt, openRouterFree);
    console.log(result);
}
else if (mode === '3') {
    const result = await variants(prompt, openRouter);
    console.log(result);
}
else if (mode === '4') {
    const result = await auto(prompt, openRouter);
    console.log(result);
}
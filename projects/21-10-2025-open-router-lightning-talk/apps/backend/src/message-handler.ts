import dotenv from 'dotenv';
import OpenAI from 'openai';
import fs from 'fs';
import { ChatCompletionMessageParam, ChatCompletionTool } from 'openai/resources/chat/completions';
import dbService from './db-service';
import { ToolCall } from './models/ai-comms';

dotenv.config();

const toolDefinitions : ChatCompletionTool[] = [
    {
        type: 'function',
        function: {
            name: 'list_home_directory',
            description: 'List the contents of the home of the logged in user directory'
        }
    },
    {
        type: 'function',
        function: {
            name: 'write_file',
            description: 'Write a file to the home directory, it is cruicial to pass both the filename and the content in the arguments',
            parameters: {
                type: 'object',
                properties: {
                    filename: { type: 'string', description: 'The name of the file to write' },
                    content: { type: 'string', description: 'The content of the file to write' }
                },
                required: ['filename', 'content']
            }
        }
    }
]

const openai = new OpenAI({
    baseURL: 'https://openrouter.ai/api/v1',
    apiKey: process.env.OPENROUTER_API_KEY
});

type ToolFunction = (args: any) => Promise<any> | any;

const toolFunctions: { [key: string]: ToolFunction } = {
    list_home_directory: async () => {
        return fs.readdirSync(process.env.HOME || '');
    },
    write_file: async (args: { filename: string, content: string }) => {
        fs.writeFileSync(args.filename, args.content);
        return `File ${args.filename} written successfully`;
    }
};

async function handleToolCall(toolCall: ToolCall) {
    const functionName = toolCall.function.name;
    const functionArgs = JSON.parse(toolCall.function.arguments);
    const fn = toolFunctions[functionName];
    if (fn) {
        return await fn(functionArgs);
    }
    return null;
}

async function handleMessage(messages: ChatCompletionMessageParam[]) {
    const response = await openai.chat.completions.create({
        model: process.env.OPENROUTER_MODEL || 'meta-llama/llama-4-maverick:free',
        messages: messages,
        tools: toolDefinitions
    });

    const message = response.choices[0]?.message;
    
    if (message?.tool_calls) {
        for (const toolCall of message.tool_calls) {
            if (toolCall.type === 'function') {
                const functionName = toolCall.function.name;
                const functionArgs = JSON.parse(toolCall.function.arguments);
                
                const fn = toolFunctions[functionName];
                if (fn) {
                    try {
                        const result = await fn(functionArgs);
                        console.log(`Tool call result for ${functionName}:`, result);
                        messages.push({ role: 'assistant', content: JSON.stringify(result) });
                        return await handleMessage(messages);
                    } catch (error) {
                        console.error(`Error executing tool ${functionName}:`, error);
                    }
                } else {
                    console.warn(`Unknown tool function: ${functionName}`);
                }
            }
        }
    } else {
        console.log('Response:', message);
        return message;
    }
}

async function* handleMessageStream(messages: ChatCompletionMessageParam[], toolCallCallback: (newMessage: string) => void) {
    const response = await openai.chat.completions.create({
        model: process.env.OPENROUTER_MODEL || 'meta-llama/llama-4-maverick:free',
        messages: messages,
        stream: true,
        tools: toolDefinitions
    });

    let message = "";
    let toolCalls: any[] = [];

    for await (const chunk of response) {
        if (chunk.choices[0].delta.content) {
            message += chunk.choices[0].delta.content;
            yield chunk.choices[0].delta.content;
        }

        if (chunk.choices[0].delta.tool_calls) {
            for (const toolCall of chunk.choices[0].delta.tool_calls) {
                if (toolCall.type === 'function') {
                    // Accumulate tool call data
                    const index = toolCall.index || 0;
                    if (!toolCalls[index]) {
                        toolCalls[index] = {
                            id: '',
                            type: 'function',
                            function: { name: '', arguments: '' }
                        };
                    }
                    
                    if (toolCall.id) toolCalls[index].id = toolCall.id;
                    if (toolCall.function?.name) toolCalls[index].function.name = toolCall.function.name;
                    if (toolCall.function?.arguments) toolCalls[index].function.arguments += toolCall.function.arguments;
                }
            }
        }
    }

    // Process completed tool calls
    for (const toolCall of toolCalls) {
        if (toolCall.function?.name && toolCall.function?.arguments) {
            console.log('Complete tool call:', toolCall);
            try {
                const result = await handleToolCall(toolCall as ToolCall);
                console.log(`Tool call result for ${toolCall.function.name}:`, result);

                dbService.addMessage("Tool call result (" + toolCall.function.name + "): " + JSON.stringify(result), '---Tool-Call---', 'assistant');
                toolCallCallback("Tool call result (" + toolCall.function.name + "): " + JSON.stringify(result));
            } catch (error) {
                console.error(`Error executing tool ${toolCall.function.name}:`, error);
                const errorMsg = `Error executing tool ${toolCall.function.name}: ${error}`;
                dbService.addMessage(errorMsg, '---Tool-Call---', 'assistant');
                toolCallCallback(errorMsg);
            }
        }
    }

    if (message !== "") {
        dbService.addMessage(message, process.env.OPENROUTER_MODEL || 'meta-llama/llama-4-maverick:free', 'assistant');
    }
}

export { handleMessageStream, handleMessage };
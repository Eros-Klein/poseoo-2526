import express, { Request, Response, text } from 'express';
import cors from 'cors';
import OpenAI from 'openai';
import dotenv from 'dotenv';
import { handleMessage, handleMessageStream } from './message-handler';
import { sseInterface } from './sse-interface';
import dbService from './db-service';
import { ChatCompletionMessageParam } from 'openai/resources/chat/completions';

const app = express();
const PORT = 3001;

dbService.init();

app.use(cors());
app.use(express.json());

app.get('/api/sse', (req: Request, res: Response) => {
  res.setHeader('Content-Type', 'text/event-stream');
  res.setHeader('Cache-Control', 'no-cache');
  res.setHeader('Connection', 'keep-alive');
  res.setHeader('Access-Control-Allow-Origin', '*');

  sseInterface.addClient(res);

  res.write(`data: ${JSON.stringify({ type: 'connected', message: 'Connected to chat server' })}\n\n`);

  req.on('close', () => {
    sseInterface.removeClient(res);
  });
});

app.post('/api/messages', async (req: Request, res: Response) => {
  const { text, sender } = req.body;
  try {
    dbService.addMessage(text, sender, 'user');

    return handleMessageStreamRecursive([...(await dbService.getMessages()), { role: 'user', content: text }]);
  } catch (error) {
    if (error instanceof OpenAI.APIError && Number(error.code) === 429) {
      sseInterface.streamBuilder(async (clients) => {
        clients.forEach((client) => {
          client.write(`data: ${JSON.stringify({ type: 'error', message: 'Rate limit exceeded, please try again later' })}\n\n`);
        });
      });
    }
  }
});

app.get('/api/health', (req: Request, res: Response) => {
  res.json({ status: 'ok', clients: sseInterface.getClients().size });
});

app.get('/api/messages', async (req: Request, res: Response) => {
  const messages = await dbService.getMessages();
  res.json({ success: true, messages: messages.reverse() });
});

app.delete('/api/messages', async (req: Request, res: Response) => {
  await dbService.clearMessages();
  res.json({ success: true });
});

app.get('/api/model', async (req: Request, res: Response) => {
  res.json({ success: true, model: process.env.OPENROUTER_MODEL || 'meta-llama/llama-4-maverick:free' });
});

app.listen(PORT, () => {
  console.log(`🚀 Backend server running on http://localhost:${PORT}`);
  console.log(`🤖 Model: ${process.env.OPENROUTER_MODEL || 'meta-llama/llama-4-maverick:free'}`);
  console.log(`📡 SSE endpoint: http://localhost:${PORT}/api/sse`);
  console.log(`💬 Messages endpoint: http://localhost:${PORT}/api/messages`);
});

async function handleMessageStreamRecursive(messages: ChatCompletionMessageParam[]) {
  const response = await handleMessageStream(messages, (newMessage: string) => {
    return handleMessageStreamRecursive(messages.concat([{ role: 'assistant', content: newMessage }]));
  });

  sseInterface.streamBuilder(async (clients) => {
    for await (const chunk of response) {
      clients.forEach((client) => {
        client.write(`data: ${JSON.stringify({ type: 'message_chunk', message: chunk })}\n\n`);
      });
    }
  });

  return response;
}
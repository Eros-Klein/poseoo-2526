import { Request, Response } from 'express';

class SSEInterface {
    private clients: Set<Response> = new Set();

    constructor(clients: Set<Response>) {
        this.clients = clients;
    }

    addClient(client: Response) {
        this.clients.add(client);
    }

    removeClient(client: Response) {
        this.clients.delete(client);
    }

    async streamBuilder(body: (clients: Set<Response>) => Promise<void>) {
        this.clients.forEach((client) => {
            client.write(`data: ${JSON.stringify({ type: 'message_start' })}\n\n`);
        });

        await body(this.clients);

        this.clients.forEach((client) => {
            client.write(`data: ${JSON.stringify({ type: 'message_end' })}\n\n`);
        });
    }

    getClients() {
        return this.clients;
    }
}

const sseInterface = new SSEInterface(new Set());

export { sseInterface };
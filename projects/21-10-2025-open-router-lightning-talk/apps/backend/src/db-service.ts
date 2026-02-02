import { Database } from "sqlite";
import sqlite3 from 'sqlite3';
import { Message } from "./models/messages";

class DbService {
    private db: Database;

    constructor() {
        this.db = new Database({
            filename: 'chat.db',
            driver: sqlite3.Database
        });

        this.db.open().catch(error => {
            console.error('Error opening database:', error);
        });
    }

    async init() {
        await this.db.exec('CREATE TABLE IF NOT EXISTS messages (id INTEGER PRIMARY KEY AUTOINCREMENT, content TEXT, created_at DATETIME, sender TEXT, role TEXT)');
        if ((await this.getMessages()).length === 0) {
            await this.addMessage('Do not include any text formatting other than \\n for new lines and - for bullet points', 'system', 'system');
        }
    }

    async addMessage(content: string, sender: string, role: 'system' | 'user' | 'assistant') {
        await this.db.run('INSERT INTO messages (content, sender, created_at, role) VALUES (?, ?, ?, ?)', [content, sender, new Date().toISOString(), role]);
    }

    async getMessages(): Promise<Message[]> {
        return await this.db.all('SELECT id, content, created_at, sender, role FROM messages ORDER BY created_at DESC') as Message[];
    }

    async clearMessages() {
        await this.db.run('DELETE FROM messages');
        await this.init();
    }
}

const dbService = new DbService();
export default dbService;
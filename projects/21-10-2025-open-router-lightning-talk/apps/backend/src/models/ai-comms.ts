export type ToolCall = {
    id: string;
    function: {
        name: string;
        arguments: string;
    }, 
    type: 'function';
}
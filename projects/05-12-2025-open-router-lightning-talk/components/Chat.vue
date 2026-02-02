<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'

// Username prompt and list of messages
const username = ref('')
const hasUsername = ref(false)
const input = ref('')
const messages = ref<string[]>([])
const ws = ref<WebSocket | null>(null)
const chatContainer = ref<HTMLElement | null>(null)

function connect() {
  if (!username.value) return
  ws.value = new WebSocket(`ws://localhost:3005/api/ws/${encodeURIComponent(username.value)}`)

  ws.value.onopen = () => {
    // Welcome message can be handled here if desired
  }

  ws.value.onmessage = (event) => {
    messages.value.push(event.data)
    // Scroll to bottom when new message arrives
    nextTick(() => {
      if (chatContainer.value) {
        chatContainer.value.scrollTop = chatContainer.value.scrollHeight
      }
    })
  }

  ws.value.onclose = () => {
    messages.value.push('[System] Disconnected.')
  }

  ws.value.onerror = (event) => {
    messages.value.push('[System] Connection error.')
  }
}

function sendMessage() {
  if (!input.value.trim() || !ws.value || ws.value.readyState !== WebSocket.OPEN)
    return
  ws.value.send(input.value)
  input.value = ''
}

function onEnterUser() {
  if (username.value.trim()) {
    hasUsername.value = true
    connect()
  }
}

function onInputKey(e: KeyboardEvent) {
  if (e.key === 'Enter') sendMessage()
}

onBeforeUnmount(() => {
  if (ws.value) ws.value.close()
})
</script>

<template>
  <div
    flex="~ col"
    items-center
    justify-center
    text="white"
    p="2"
  >
    <div
      w="340px"
      shadow="xl"
      rounded="xl"
      bg="white/10"
      border="~ 1px white/30"
      p="4"
      flex="~ col"
    >
      <h2 text="lg font-semibold" mb="3" text-center>
        🔮 AI Chat
      </h2>
      <template v-if="!hasUsername">
        <div flex="col" items-center gap="2">
          <label text="sm" mb="2">Enter your name:</label>
          <input
            v-model="username"
            @keyup.enter="onEnterUser"
            placeholder="Your name"
            p="2"
            rounded="full"
            text="center black sm"
            bg="white"
            border="~ main"
            outline="!none"
            w="70%"
            mb="2"
            autofocus
          />
          <button
            @click="onEnterUser"
            px="5"
            py="1"
            rounded="full"
            text="sm white"
            bg="blue-600 hover:blue-700"
            transition
          >
            Join
          </button>
        </div>
      </template>
      <template v-else>
        <div
          ref="chatContainer"
          h="180px"
          overflow-y-auto
          bg="white/12"
          p="2"
          rounded="md"
          border="~ 1px white/30"
          mb="3"
          flex="col gap-1"
          text="xs"
        >
          <template v-if="messages.length === 0">
            <div text="center gray-200 xs">No messages yet.</div>
          </template>
          <div v-for="(msg, i) in messages" :key="i" break-all>
            <span
              v-if="msg.includes(': ') && msg.startsWith('ai:')"
              text="blue-200 font-bold"
            >
              {{ msg.slice(0, msg.indexOf(':') + 1) }}
            </span>
            <span v-if="msg.includes(': ')" text="gray-100">
              {{ msg.slice(msg.indexOf(':') + 1).trim() }}
            </span>
            <template v-else>
              <span text="lime-200/80 font-bold">{{ msg }}</span>
            </template>
          </div>
        </div>
        <div flex="row gap-1" items-center>
          <input
            v-model="input"
            @keyup.enter="sendMessage"
            :disabled="!ws || ws.readyState!==1"
            placeholder="Message..."
            p="2"
            rounded="full"
            text="black xs"
            bg="white"
            border="~ main"
            outline="!none"
            flex="1"
            autocomplete="off"
          />
          <button
            @click="sendMessage"
            :disabled="!input.trim() || !ws || ws.readyState!==1"
            px="3"
            py="1"
            rounded="full"
            text="white xs"
            bg="cyan-600 hover:cyan-700"
            transition
          >
            Send
          </button>
        </div>
        <div text="xs center gray-400" mt="1" select-none>
          <b>{{ username }}</b>
        </div>
      </template>
    </div>
  </div>
</template>

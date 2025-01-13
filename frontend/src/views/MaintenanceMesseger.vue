<template>
    <div>
        <h5>WhatsApp Message <span class="text-success">(Maintenance Status)</span></h5>
        <hr>
        <p>Send Messege where status Maintenance is Outs.</p>
        <p>Status: {{ status }}</p>
        <button class="btn btn-sm btn-success me-2" @click="startSending" :disabled="isSending">Start Sending</button>
        <button class="btn btn-sm btn-success" @click="stopSending" :disabled="!isSending">Stop Sending</button>
    </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';

const status = ref('Ready');
const isSending = ref(false);
let sendInterval;

// Load the initial state from localStorage
onMounted(() => {
    const savedIsSending = localStorage.getItem('isSending');
    if (savedIsSending === 'true') {
        startSending();
    } else {
        status.value = 'Ready';
        isSending.value = false;
    }
});

// Watcher to persist the isSending state to localStorage
watch(isSending, (newValue) => {
    localStorage.setItem('isSending', newValue.toString());
    if (newValue) {
        status.value = 'Sending...';
    } else {
        status.value = 'Stopped';
    }
});

const startSending = async () => {
    if (!isSending.value) {
        isSending.value = true;
        sendInterval = setInterval(async () => {
            try {
                await sendMessage();
            } catch (error) {
                console.error(error);
            }
        }, 10 * 1000); // 10 minutes
    }
};

const stopSending = () => {
    if (isSending.value) {
        clearInterval(sendInterval);
        isSending.value = false;
    }
};

const sendMessage = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/send-whatsapp-messages`);
        console.log("Send Message is successful", response.data);
    } catch (error) {
        console.error("Failed to send messages.", error);
    }
};
</script>

<template>
    <div class="container mt-4">
        <h1>Update ZKTeco IP</h1>
        <hr />

        <!-- Alert for Response -->
        <div v-if="responseMessage" :class="`alert alert-${responseStatus}`" role="alert">
            {{ responseMessage }}
        </div>

        <!-- Form for Updating ZKTeco IP -->
        <div class="mb-3">
            <label for="newIp" class="form-label">New ZKTeco IP Address</label>
            <input type="text" id="newIp" class="form-control" v-model="newIp" placeholder="Enter new ZKTeco IP" />
        </div>

        <!-- Update IP Button -->
        <button class="btn btn-primary" @click="updateZktecoIp">
            Update IP
        </button>
    </div>
</template>

<script setup>
import { ref } from 'vue';
import Paho from 'paho-mqtt';
import axios from 'axios'; // Import axios for HTTP requests

// States
const newIp = ref('');  // Holds the new ZKTeco IP address
const responseMessage = ref('');
const responseStatus = ref('success'); // 'success' or 'danger'

// MQTT Configuration
const mqttBroker = '192.168.0.133'; // IP Broker
const mqttPort = 9000; // Port Broker
const mqttTopicCommand = 'access_control/command'; // Command topic
const mqttTopicResponse = 'access_control/response'; // Response topic

// Helper to Show Alerts
const showAlert = (message, status) => {
    responseMessage.value = message;
    responseStatus.value = status;
    setTimeout(() => {
        responseMessage.value = '';
    }, 3000);
};

// MQTT Connect Helper
const mqttConnect = () => {
    const client = new Paho.Client(mqttBroker, mqttPort, `webClient-${Date.now()}`);
    client.onMessageArrived = (message) => {
        const response = JSON.parse(message.payloadString);
        if (response.response === 'success' && response.command === 'update_zkteco_ip') {
            showAlert(`IP ZKTeco updated to: ${response.zkteco_ip}`, 'success');
        } else {
            showAlert('Failed to update IP', 'danger');
        }
    };

    client.connect({
        onSuccess: () => {
            client.subscribe(mqttTopicResponse); // Subscribe to response topic
        },
        onFailure: () => showAlert('Failed to connect to MQTT broker', 'danger'),
    });

    return client;
};

// Function to send new ZKTeco IP to MQTT
const updateZktecoIp = () => {
    if (!newIp.value) {
        showAlert('Please enter a new ZKTeco IP address.', 'danger');
        return;
    }

    const client = mqttConnect();

    const message = {
        command: 'update_zkteco_ip',
        zkteco_ip: newIp.value,
    };

    const mqttMessage = new Paho.Message(JSON.stringify(message));
    mqttMessage.destinationName = mqttTopicCommand; // Send command to ZKTeco update IP topic
    client.send(mqttMessage);

    showAlert('Request to update ZKTeco IP has been sent.', 'success');
};
</script>

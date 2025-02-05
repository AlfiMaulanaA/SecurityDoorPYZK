<template>
    <div class="container mt-4">
        <h1>Attendance Report</h1>
        <hr />

        <!-- Alert for Response -->
        <div v-if="responseMessage" :class="`alert alert-${responseStatus}`" role="alert">
            {{ responseMessage }}
        </div>
        <!-- Export Button -->
        <button v-if="attendanceRecords.length > 0" class="btn btn-success mb-3" @click="exportToExcel">
            Export to Excel
        </button>
        <!-- Attendance Table -->
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>#</th>
                    <th>UID</th>
                    <th>Username</th>
                    <th>Attendance Time</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="(record, i) in attendanceRecords" :key="record.id">
                    <td>{{ 1 + i }}</td>
                    <td>{{ record.uid }}</td>
                    <td>{{ record.username }}</td>
                    <td>{{ new Date(record.timestamp).toLocaleString() }}</td> <!-- Format timestamp -->
                </tr>
            </tbody>
            <tbody>
                <tr v-if="attendanceRecords.length === 0" class="text-center">
                    <td>Waiting for attendance data...</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import Paho from 'paho-mqtt';
import axios from 'axios'; // Import axios for HTTP requests
import * as XLSX from 'xlsx';

// States
const attendanceRecords = ref([]);
const responseMessage = ref('');
const responseStatus = ref('success'); // 'success' or 'danger'

// MQTT Configuration
const mqttBroker = '192.168.0.133'; // IP Broker
const mqttPort = 9000; // Port Broker
const mqttTopic = 'access_control/attendance_response';

// Helper to Show Alerts
const showAlert = (message, status) => {
    responseMessage.value = message;
    responseStatus.value = status;
    setTimeout(() => {
        responseMessage.value = '';
    }, 3000);
};

// MQTT Connect Helper
const mqttConnect = (onMessageCallback) => {
    const client = new Paho.Client(mqttBroker, mqttPort, `webClient-${Date.now()}`);
    client.onMessageArrived = (message) => {
        const response = JSON.parse(message.payloadString);
        onMessageCallback(response);
    };

    client.connect({
        onSuccess: () => {
            client.subscribe(mqttTopic); // Subscribe to attendance report topic
            console.log("Subscribed to topic:", mqttTopic);
        },
        onFailure: () => showAlert('Failed to connect to broker.', 'danger'),
    });
};

const exportToExcel = () => {
    if (attendanceRecords.value.length === 0) {
        showAlert('No attendance data to export.', 'danger');
        return;
    }

    const ws = XLSX.utils.json_to_sheet(attendanceRecords.value); // Convert JSON to worksheet
    const wb = XLSX.utils.book_new(); // Create a new workbook
    XLSX.utils.book_append_sheet(wb, ws, 'Attendance'); // Append the sheet to the workbook

    // Write the workbook to a file and trigger download
    XLSX.writeFile(wb, 'attendance_report.xlsx');
};

// Initialize MQTT connection
onMounted(() => {
    mqttConnect(processAttendanceData); // Only call mqttConnect once
    fetchAttendance();
});

// Process the incoming attendance data
const processAttendanceData = async (data) => {
    if (data && data.attendance) {
        const { uid, username, timestamp } = data.attendance;

        // Check if this attendance record already exists (based on UID and timestamp)
        const existingRecord = attendanceRecords.value.find(record => record.uid === uid && record.timestamp === timestamp);

        // If record doesn't exist, add the new attendance
        if (!existingRecord) {
            attendanceRecords.value.push({
                uid: uid,
                username: username || 'Unknown',
                timestamp: timestamp,
            });
            console.log('Attendance data:', { uid, username, timestamp });

            // Send the attendance data to the backend
            await saveAttendanceToBackend({ uid, username, timestamp });
        }
    } else {
        showAlert('Invalid attendance data received.', 'danger');
    }
};

// Save attendance data to the backend
const saveAttendanceToBackend = async (attendanceData) => {
    try {
        const response = await axios.post('http://localhost:5072/attendance', {
            uid: attendanceData.uid,
            username: attendanceData.username,
            timestamp: attendanceData.timestamp,
        });

        if (response.status === 200) {
            console.log('Attendance data saved successfully');
        } else {
            showAlert('Failed to save attendance data to backend.', 'danger');
        }
    } catch (error) {
        showAlert(`Error saving attendance data: ${error.message}`, 'danger');
    }
};

// Fetch attendance data from the backend
const fetchAttendance = async () => {
    try {
        const response = await axios.get('http://localhost:5072/attendance'); // Endpoint that fetches attendance data
        if (response.status === 200) {
            attendanceRecords.value = response.data; // Store received attendance data
        } else {
            showAlert('Failed to fetch attendance data.', 'danger');
        }
    } catch (error) {
        showAlert(`Error fetching attendance data: ${error.message}`, 'danger');
    }
};
</script>

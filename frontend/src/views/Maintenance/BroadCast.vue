<template>
    <div>
        <div class="content-style">
            <h5>BroadCast Message<span class="text-success"> (Broadcast WhatsApp)</span> <i
                    class="fab fa-whatsapp text-success"></i></h5>
            <p class="text-danger">The whatsApp API can work if Connected to Internet Network.*</p>

            <hr>
            <textarea class="form-control" v-model="message" placeholder="Enter your message here"></textarea>
            <button class="btn btn-success mt-2" @click="sendBroadcastMessage">Send Broadcast</button>
        </div>
        <div class="content-style-primary">
            <WhatsAppSenderMessage />
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, reactive } from "vue";
import axios from "axios";
import Swal from 'sweetalert2';

const myBreadcrumbs = [
    { text: "Dashboard", to: "/#/dashboard" },
    { text: "Notification Group Management" },
];

import WhatsAppSenderMessage from "./MaintenanceMesseger.vue"

const message = ref('');

const sendBroadcastMessage = async () => {
    try {
        const response = await axios.post(`http://localhost:5072/api/send-broadcast`, { message: message.value });
        console.log(response.data);
        Swal.fire({
            title: "Good job!",
            text: "Save successfully!",
            icon: "success",
            confirmButtonColor: '#3085d6',
        });
    } catch (error) {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Failed Save Data.",
        });
        console.error('Error creating company:', error);
    }
};

</script>

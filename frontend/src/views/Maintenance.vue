<template>
    <div>
        <div class="d-flex justify-content-between">
            <h5>Maintenance List</h5>
            <div class="export-buttons">
                <button class="btn btn-sm btn-success me-2" @click="exportToExcel">Export to Excel<i
                        class="far fa-file-excel text-light ms-2"></i></button>
            </div>

        </div>
        <hr>
        <div class="row">
            <div class="col-2">
                <select class="form-select form-select-sm" v-model="selectedStatus">
                    <option value="">All</option>
                    <option value="Outs">Outs</option>
                    <option value="Done">Done</option>
                </select>
            </div>
            <div class="col-4">
                <button class="btn btn-sm btn-primary" @click="fetchFilterMaintenances">Filter</button>
            </div>
        </div>

        <div class="table-responsive" id="tableToExport">
            <br />
            <!-- Maintenance Table -->
            <table class="table m-2 justify-content-between">
                <!-- Table headers -->
                <thead>
                    <tr>
                        <th scope="col">NO</th>
                        <th scope="col">Start Date</th>
                        <th scope="col">End Date</th>
                        <th scope="col">Description</th>
                        <th scope="col">Assign To</th>
                        <th scope="col">Device</th>
                        <th scope="col">Status</th>
                        <th scope="col">Notes</th>

                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(maintenance, index) in paginatedMaintenance" :key="maintenance.id">
                        <td>{{ index + 1 }}</td>
                        <td>{{ formatDateTime(maintenance.startDate) }}</td>
                        <td>{{ formatDateTime(maintenance.endDate) }}</td>
                        <td>{{ maintenance.description }}</td>
                        <td>{{ findUser(maintenance.userId) }}</td>
                        <td>{{ findDevice(maintenance.deviceId) }}</td>
                        <td :class="{
                            'text-danger': maintenance.status === 'Outs',
                            'text-success': maintenance.status === 'Done'
                        }">
                            {{ maintenance.status }}</td>
                        <td>{{ maintenance.notes }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
        <hr>
        <nav aria-label=" Maintenance Page navigation" class="d-flex">
            <ul class="pagination me-4">
                <li class="page-item" :class="{ disabled: currentPageMaintenance === 1 }">
                    <a class="page-link" href="#" @click.prevent="changeMaintenancePage(currentPageMaintenance - 1)">
                        &laquo;
                    </a>
                </li>
                <li class="page-item" v-for="n in totalMaintenancePages" :key="n"
                    :class="{ active: n === currentPageMaintenance }">
                    <a class="page-link" href="#" @click.prevent="changeMaintenancePage(n)">
                        {{ n }}
                    </a>
                </li>
                <li class="page-item" :class="{ disabled: currentPageMaintenance === totalMaintenancePages }">
                    <a class="page-link" href="#" @click.prevent="changeMaintenancePage(currentPageMaintenance + 1)">
                        &raquo;
                    </a>
                </li>
            </ul>
            <div class="pagination-text">
                Page {{ currentPageMaintenance }} of {{ itemsPerPageMaintenance }}
            </div>
        </nav>

    </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import axios from "axios";
import * as XLSX from 'xlsx';

const currentPageMaintenance = ref(1);
const itemsPerPageMaintenance = 25;

const paginatedMaintenance = computed(() => {
    const start = (currentPageMaintenance.value - 1) * itemsPerPageMaintenance;
    const end = start + itemsPerPageMaintenance;
    return maintenances.value.slice(start, end);
});

const totalMaintenancePages = computed(() => {
    return Math.ceil(maintenances.value.length / itemsPerPageMaintenance);
});

const computedIndexesMaintenance = computed(() => {
    return paginatedMaintenance.value.map((_, index) => (index + 1) + ((currentPageMaintenance.value - 1) * itemsPerPageMaintenance));
});

// Function to change maintenance page
const changeMaintenancePage = (page) => {
    currentPageMaintenance.value = page;
};


function formatDateTime(dateTimeString) {
    return dateTimeString.replace('T', ' ');
}

const selectedStatus = ref('');
const users = ref([]);
const maintenances = ref([]);

const findUser = (userId) => {
    const user = users.value.find(dt => dt.id === userId);
    return user ? user.username : 'Unknown';
};
const findDevice = (deviceId) => {
    const device = devices.value.find(dt => dt.id === deviceId);
    return device ? device.name : 'Unknown';
};

const fetchFilterMaintenances = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/maintenance?status=${selectedStatus.value}`);
        maintenances.value = response.data;
    } catch (error) {
        console.error(error);
    }
};

// Fetch Maintenances function
const fetchMaintenances = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/api/maintenance`);
        maintenances.value = response.data;
    } catch (error) {
        console.error("There was an error fetching the maintenances:", error);
    }
};


// Fetch Users
const fetchUsers = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/users`);
        users.value = response.data;
    } catch (error) {
        console.error("There was an error fetching the users:", error);
    }
};

// Method to export to Excel
const exportToExcel = () => {
    const ws = XLSX.utils.json_to_sheet(maintenances.value);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "MaintenanceReport");

    // Write the workbook file
    XLSX.writeFile(wb, "Maintenance Report.xlsx");
};

// Method for print functionality
const printTable = () => {
    const printWindow = window.open('', '_blank');
    const content = document.getElementById('maintenanceTable').outerHTML;
    printWindow.document.write('<html><head><title>Print</title>');
    printWindow.document.write('<link rel="stylesheet" href="path/to/bootstrap.css" type="text/css" />'); // Optional: Include Bootstrap or your styles
    printWindow.document.write('</head><body >');
    printWindow.document.write(content);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
};

const devices = ref([]); // Assuming you have a list of devices

const fetchDevices = async () => {
    try {
        // Replace with your actual API endpoint
        const response = await axios.get(`http://localhost:5072/api/devices`);
        devices.value = response.data;
    } catch (error) {
        console.error("Error fetching devices:", error);
        // Handle error appropriately
    }
};

onMounted(() => {
    fetchMaintenances();
    fetchUsers();
    fetchDevices();
});
</script>
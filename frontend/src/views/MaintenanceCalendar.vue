<template>
    <div>

        <div class="content-style">
            <div class="calendar">
                <div class="calendar-header d-flex justify-content-between">
                    <span class="text-primary fs-1" @click="prevMonth">
                        &laquo;
                    </span>
                    <span>{{ currentMonthName }} {{ currentYear }}</span>
                    <span class="text-primary fs-1" @click="nextMonth">
                        &raquo;
                    </span>

                </div>
                <div class="calendar-grid mt-4">
                    <div class="calendar-day-name" v-for="dayName in dayNames" :key="dayName">
                        {{ dayName }}
                    </div>
                    <div class="calendar-day" v-for="day in daysOfMonth" :key="day.date">
                        <span>{{ day.date }}</span>
                        <div v-for="maint in day.maintenance" :key="maint.Id" class="calendar-maintenance"
                            :class="{ 'maintenance-outs': maint.status === 'Outs', 'maintenance-done': maint.status === 'Done' }">
                            <span>
                                {{ maint.description }} - {{ findDevice(maint.deviceId) }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="content-style-primary">
            <div class="d-flex justify-content-between">
                <h5>Maintenance List</h5>
                <button class="btn btn-sm btn-success" data-bs-toggle="modal" data-bs-target="#addMaintenanceModal" v>
                    Add Maintenance
                </button>
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
                    <button class="btn btn-sm btn-primary" @click="fetchFiltermaintenance">Filter</button>
                </div>
            </div>

            <div class="table-responsive">
                <br />
                <!-- Maintenance Table -->
                <table class="table m-2 justify-content-between">
                    <!-- Table headers -->
                    <thead>
                        <tr>
                            <th scope="col">NO</th>
                            <th scope="col">Start Date</th>
                            <!-- <th scope="col">End Date</th> -->
                            <th scope="col">Assign To</th>
                            <th scope="col">Device Name</th>
                            <th scope="col">Description</th>
                            <th scope="col">Status</th>
                            <th scope="col">Notes</th>
                            <th scope="col">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(maintenance, index) in paginatedMaintenance" :key="maintenance.id">
                            <td>{{ computedIndexesMaintenance[index] }}</td>
                            <td>{{ formatDateTime(maintenance.startDate) }}</td>
                            <!-- <td>{{ formatDateTime(maintenance.endDate) }}</td> -->
                            <td>{{ findUser(maintenance.userId) }}</td>
                            <td>{{ findDevice(maintenance.deviceId) }}</td>
                            <td>{{ maintenance.description }}</td>
                            <td @click="updateStatus(maintenance)"
                                :class="maintenance.status == 'done' ? 'text-success' : 'text-danger'">
                                {{ maintenance.status }}
                            </td>
                            <td>{{ maintenance.notes }}</td>
                            <td>

                                <button class="btn btn-sm btn-success me-2" data-bs-toggle="modal"
                                    data-bs-target="#periodMaintenanceModal" @click="() => {
                                        selectedMaintenance = maintenance;
                                    }
                                        ">
                                    Copy
                                </button>
                                <button class="btn btn-sm btn-primary me-2" data-bs-toggle="modal"
                                    data-bs-target="#editMaintenanceModal" @click="() => {
                                        selectedMaintenance = maintenance;
                                    }
                                        ">Update
                                </button>

                                <button class="btn btn-sm btn-danger" @click="confirmDelete(maintenance.id)">
                                    Delete
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <hr>
            <nav aria-label="Maintenance Page navigation" class="d-flex">
                <ul class="pagination me-4">
                    <li class="page-item" :class="{ disabled: currentPageMaintenance === 1 }">
                        <a class="page-link" href="#"
                            @click.prevent="changeMaintenancePage(currentPageMaintenance - 1)">
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
                        <a class="page-link" href="#"
                            @click.prevent="changeMaintenancePage(currentPageMaintenance + 1)">
                            &raquo;
                        </a>
                    </li>
                </ul>
                <div class="pagination-text">
                    Page {{ currentPageMaintenance }} of {{ itemsPerPageMaintenance }}
                </div>
            </nav>
        </div>

        <!-- Add Maintenance Modal -->
        <div class="modal fade" id="addMaintenanceModal" tabindex="-1" aria-labelledby="addMaintenanceModalLabel"
            aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addMaintenanceModalLabel">
                            Add New Maintenance
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="addMaintenance">
                            <div class="row">

                                <div class="col-6 mb-3">

                                    <label for="user" class="form-label">Assign To </label>
                                    <select id="user" class="form-control mb-3" v-model="newMaintenance.userId">
                                        <option disabled value="">Select User</option>
                                        <option v-for="user in users" :key="user.id" :value="user.id">
                                            {{ user.username }} - {{ user.phone }}
                                        </option>
                                    </select>
                                </div>
                                <div class="col-6 mb-3">
                                    <label for="status" class="form-label">Device</label>
                                    <select id="status" class="form-control" v-model="newMaintenance.deviceId">
                                        <option disabled value="">Select Device</option>
                                        <option v-for="d in devices" :key="d.id" :value="d.id">
                                            {{ d.name }}
                                        </option>
                                    </select>
                                </div>
                            </div>
                            <div class="mb-3">
                                <label for="description" class="form-label">Description</label>
                                <textarea type="text" class="form-control" id="description"
                                    v-model="newMaintenance.description" placeholder="Description Task"
                                    required></textarea>
                            </div>

                            <div class="mb-3">
                                <label for="status" class="form-label">Status</label>
                                <select id="status" class="form-control" v-model="newMaintenance.status" required>
                                    <option disabled value="">Select Status</option>
                                    <option value="Outs">Outs</option>
                                    <option value="Done">Done</option>
                                </select>
                            </div>
                            <div class="row">

                                <div class="col-6 mb-3">
                                    <label for="startDate" class="form-label">Start Date</label>
                                    <input type="datetime-local" class="form-control" id="startDate"
                                        v-model="newMaintenance.startDate" required />
                                </div>
                                <div class="col-6 mb-3">
                                    <label for="endDate" class="form-label">End Date</label>
                                    <input type="datetime-local" class="form-control" id="endDate"
                                        v-model="newMaintenance.endDate" required />
                                </div>
                            </div>
                            <div class="mb-3">
                                <label for="notes" class="form-label">Notes</label>
                                <textarea class="form-control" id="notes" v-model="newMaintenance.notes"
                                    placeholder="Notes Completed Task"></textarea>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-danger" data-bs-dismiss="modal">
                                    Close
                                </button>
                                <button type="submit" class="btn btn-success">
                                    Add Maintenance
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>

        <!-- Edit Maintenance Modal -->
        <div class="modal fade" id="editMaintenanceModal" tabindex="-1" aria-labelledby="editMaintenanceModalLabel"
            aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="editMaintenanceModalLabel">
                            Edit Maintenance
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="updateMaintenance">
                            <div class="row">
                                <div class="col-6">
                                    <label for="user" class="form-label">Assign To </label>
                                    <select id="user" class="form-control mb-3" v-model="selectedMaintenance.userId">
                                        <option disabled value="">Select User</option>
                                        <option v-for="user in users" :key="user.id" :value="user.id">
                                            {{ user.username }}
                                        </option>
                                    </select>
                                </div>
                                <div class="col-6">
                                    <label for="status" class="form-label">Device</label>
                                    <select id="status" class="form-control" v-model="selectedMaintenance.deviceId"
                                        required>
                                        <option disabled value="">Select Device</option>
                                        <option v-for="d in devices" :key="d.id" :value="d.id">
                                            {{ d.name }}
                                        </option>
                                    </select>
                                </div>
                            </div>

                            <div class="mb-3">
                                <label for="editDescription" class="form-label">Description</label>
                                <textarea class="form-control" id="editDescription"
                                    v-model="selectedMaintenance.description" required></textarea>
                            </div>

                            <div class="mb-3">
                                <label for="status" class="form-label">Status</label>
                                <select id="status" class="form-control" v-model="selectedMaintenance.status" required>
                                    <option disabled value="">Select Status</option>
                                    <option value="Outs">Outs</option>
                                    <option value="Done">Done</option>
                                </select>
                            </div>
                            <!-- <div class="mb-3">
                <label for="editStartDate" class="form-label">Start Date</label>
                <input type="datetime-local" class="form-control" id="editStartDate"
                  v-model="selectedMaintenance.startDate" required />
              </div>
              <div class="mb-3">
                <label for="editEndDate" class="form-label">End Date</label>
                <input type="datetime-local" class="form-control" id="editEndDate" v-model="selectedMaintenance.endDate"
                  required />
              </div> -->
                            <div class="mb-3">
                                <label for="editNotes" class="form-label">Notes</label>
                                <textarea class="form-control" id="editNotes"
                                    v-model="selectedMaintenance.notes"></textarea>
                            </div>

                            <div class="modal-footer">
                                <button type="button" class="btn btn-danger" data-bs-dismiss="modal">
                                    Close
                                </button>
                                <button type="submit" class="btn btn-primary">
                                    Update Maintenance
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>


            <!-- Edit Maintenance Modal -->
            <div class="modal fade" id="periodMaintenanceModal" tabindex="-1"
                aria-labelledby="editMaintenanceModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="editMaintenanceModalLabel">
                                Add Period Maintenance
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <form>
                                <label for="user" class="form-label">Assign To </label>
                                <select id="user" class="form-control mb-3" v-model="selectedMaintenance.userId">
                                    <option disabled value="">Select User</option>
                                    <option v-for="user in users" :key="user.id" :value="user.id">
                                        {{ user.username }}
                                    </option>
                                </select>
                                <div class="mb-3">
                                    <label for="editDescription" class="form-label">Description</label>
                                    <input type="text" class="form-control" id="editDescription"
                                        v-model="selectedMaintenance.description" required disable />
                                </div>
                                <div class="mb-3">
                                    <label for="status" class="form-label">Device</label>
                                    <select id="status" class="form-control" v-model="selectedMaintenance.deviceId"
                                        required>
                                        <option disabled value="">Select Device</option>
                                        <option v-for="d in devices" :key="d.id" :value="d.id">
                                            {{ d.name }}
                                        </option>
                                    </select>
                                </div>

                                <div class="mb-3">
                                    <label for="duplicationPeriod" class="form-label">Duplication Period</label>
                                    <select id="duplicationPeriod" class="form-control" v-model="duplicationPeriod">
                                        <option value="Daily">Daily</option>
                                        <option value="Weekly">Weekly</option>
                                        <option value="Monthly">Monthly</option>
                                        <option value="Quarterly">Quarterly</option>
                                    </select>
                                </div>
                                <div class="mb-3">
                                    <label for="intervalStart" class="form-label">Interval Start Date</label>
                                    <input type="date" class="form-control" id="intervalStart" v-model="intervalStart"
                                        required />
                                </div>
                                <div class="mb-3">
                                    <label for="intervalEnd" class="form-label">Interval End Date</label>
                                    <input type="date" class="form-control" id="intervalEnd" v-model="intervalEnd"
                                        required />
                                </div>
                                <button class="btn btn-secondary" @click="duplicateMaintenance">Duplicate
                                    Maintenance</button>

                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import axios from "axios";
import Swal from 'sweetalert2';

const duplicationPeriod = ref('Daily');
const intervalStart = ref('');
const intervalEnd = ref('');

const duplicateMaintenance = async () => {
    try {
        const response = await axios.post(`http://localhost:5072/api/maintenance/duplicate`, {
            MaintenanceId: selectedMaintenance.value.id,
            PeriodType: duplicationPeriod.value,
            StartDate: intervalStart.value,
            EndDate: intervalEnd.value
        });
        if (response.status === 200) {
            alert('Maintenance duplicated successfully');
            fetchMaintenance();

        }
    } catch (error) {
        console.error(error);
        alert('An error occurred while trying to duplicate the maintenance record. Please try again.');
    }
};

function formatDateTime(dateTimeString) {
    return dateTimeString.replace('T', ' ');
}

const selectedStatus = ref('');

const fetchFiltermaintenance = async () => {
    try {
        const response = await axios.get(
            `http://localhost:5072/maintenance?status=${selectedStatus.value}`);
        maintenance.value = response.data;
    } catch (error) {
        console.error(error);
    }
};

const updateStatus = async (maintenance) => {
    try {
        const updatedMaintenance = { ...maintenance, status: 'Done' };
        await axios.put(
            `http://localhost:5072/api/maintenance/${maintenance.id}`,
            updatedMaintenance);
        maintenance.status = 'Done'; // Update status in the local state
    } catch (error) {
        console.error(error);
    }
};


const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const currentDate = ref(new Date());
const currentMonth = computed(() => currentDate.value.getMonth());
const currentYear = computed(() => currentDate.value.getFullYear());
const currentMonthName = computed(() => currentDate.value.toLocaleString('default', { month: 'long' }));

const daysOfMonth = computed(() => {
    const days = [];
    const firstDayOfMonth = new Date(currentYear.value, currentMonth.value, 1).getDay();
    const daysInMonth = new Date(currentYear.value, currentMonth.value + 1, 0).getDate();

    for (let i = 0; i < firstDayOfMonth; i++) {
        days.push({ date: '', maintenance: [] });
    }

    for (let i = 1; i <= daysInMonth; i++) {
        const formattedDate = `${currentYear.value}-${String(currentMonth.value + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
        days.push({
            date: i,
            maintenance: maintenance.value.filter(m => {
                const startDate = m.startDate.split('T')[0]; // Extract the date part
                return startDate === formattedDate;
            })
        });
    }

    return days;
});

const prevMonth = () => {
    currentDate.value = new Date(currentYear.value, currentMonth.value - 1, 1);
};

const nextMonth = () => {
    currentDate.value = new Date(currentYear.value, currentMonth.value + 1, 1);
};


const currentPageMaintenance = ref(1);
const itemsPerPageMaintenance = 25;

const paginatedMaintenance = computed(() => {
    const start = (currentPageMaintenance.value - 1) * itemsPerPageMaintenance;
    const end = start + itemsPerPageMaintenance;
    return maintenance.value.slice(start, end);
});

const totalMaintenancePages = computed(() => {
    return Math.ceil(maintenance.value.length / itemsPerPageMaintenance);
});

const computedIndexesMaintenance = computed(() => {
    return paginatedMaintenance.value.map((_, index) => (index + 1) + ((currentPageMaintenance.value - 1) * itemsPerPageMaintenance));
});

// Function to change maintenance page
const changeMaintenancePage = (page) => {
    currentPageMaintenance.value = page;
};

const findUser = (userId) => {
    const user = users.value.find(dt => dt.id === userId);
    return user ? user.username : 'N/A';
};

const findDevice = (deviceId) => {
    const device = devices.value.find(dt => dt.id === deviceId);
    return device ? device.name : 'Unknown';
};

const users = ref([]);
const maintenance = ref([]);
const selectedMaintenance = ref({
    description: "",
    status: "",
    startDate: "",
    endDate: "",
    notes: "",
    userId: null,
    deviceId: null,
});

const newMaintenance = ref({
    description: "",
    status: "",
    startDate: "",
    endDate: "",
    notes: "",
    userId: null,
    deviceId: null,
});

// Fetch maintenance function
const fetchMaintenance = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/api/maintenance`);
        maintenance.value = response.data;
    } catch (error) {
        console.error("There was an error fetching the maintenance:", error);
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

const addMaintenance = async () => {
    try {
        // Ensure that the status field is either "Outs" or "Done"
        if (newMaintenance.value.status !== "Outs" && newMaintenance.value.status !== "Done") {
            throw new Error("Invalid status value. Must be 'Outs' or 'Done'.");
        }

        const response = await axios.post(
            `http://localhost:5072/api/maintenance`,
            newMaintenance.value);
        // console.log("Maintenance added:", response.data);
        newMaintenance.value = { // Reset form
            description: "",
            status: "",
            startDate: "",
            endDate: "",
            notes: "",
            userId: null,
            rackId: null,
        };
        await fetchMaintenance(); // Refresh the list
        Swal.fire('Maintenance Added!', 'The maintenance record has been added.', 'success');
        await fetchMaintenance();
    } catch (error) {
        console.error("There was an error add the maintenance:", error);
        // Show an error message
        Swal.fire('Error!', 'There was an error add the maintenance.', 'error');
    }
};

const confirmDelete = async (maintenanceId) => {
    const result = await Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    });

    if (result.isConfirmed) {
        deleteMaintenance(maintenanceId);
    }
};

const deleteMaintenance = async (maintenanceId) => {
    try {
        await axios.delete(`http://localhost:5072/api/maintenance/${maintenanceId}`);
        maintenance.value = maintenance.value.filter((m) => m.id !== maintenanceId);

        // Show a success message
        Swal.fire('Deleted!', 'The maintenance record has been deleted.', 'success');
        await fetchMaintenance();
    } catch (error) {
        console.error("There was an error deleting the maintenance:", error);
        // Show an error message
        Swal.fire('Error!', 'There was an error deleting the maintenance.', 'error');
    }
};

const updateMaintenance = async () => {
    try {
        await axios.put(
            `http://localhost:5072/api/maintenance/${selectedMaintenance.value.id}`,
            selectedMaintenance.value);
        // console.log("Maintenance updated");
        Swal.fire('Updated!', 'The maintenance record has been updated.', 'success');
        await fetchMaintenance();
    } catch (error) {
        console.error("There was an error update the maintenance:", error);
        // Show an error message
        Swal.fire('Error!', 'There was an error updating the maintenance.', 'error');
    }
};

onMounted(() => {
    fetchMaintenance();
    fetchUsers();
    fetchDevices();
});
</script>

<style scoped>
.calendar-header {
    padding: 8px;
    font-size: 25px;
}

.calendar-grid {
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    /* Additional styles */
}

.calendar-day-name {
    font-weight: bold;
    text-align: center;
}

.calendar-day {
    border: 1px solid grey;
    border-left: 4px solid #5a7aa2;
    border-radius: 10px;
    padding: 5px 5px 30px 5px;
}

.calendar-maintenance {
    padding: 4px;
    border-radius: 6px;
    font-size: 12px;
    margin: 2px;
}

.maintenance-outs {
    background: rgb(255, 0, 0);
    font-weight: 800;
    color: white;
    /* Other styling as needed */
}

.maintenance-done {
    font-weight: 800;
    background: rgb(0, 255, 0);
    color: rgb(255, 255, 255);
    /* Other styling as needed */
}
</style>

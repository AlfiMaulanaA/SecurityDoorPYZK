<template>
    <div>
        <div class="content-style-primary">
            <div class="d-flex justify-content-between align-items-center"
                style="display: flex; justify-content: space-between">
                <div class="d-flex">
                    <h5>Device List</h5>
                    <span style="margin-left: 8px" class="me-3" :class="mqttConnected ? 'text-success' : 'text-danger'">
                        {{ mqttConnected ? "Connected" : "Disconnected" }}
                    </span>
                </div>
            </div>
        </div>
        <!-- Conditional rendering based on selected category -->
        <div>
            <div class="content-style-secondary">
                <div class="d-flex justify-content-between">
                    <div>
                        <span>
                            <button class="btn btn-sm btn-primary" @click="showCreateModal" data-bs-toggle="modal"
                                data-bs-target="#addTopicModal">
                                Add Devices
                            </button>
                        </span>
                    </div>
                </div>
                <hr />

                <input type="text" v-model="searchQuery" @input="filterTopics" class="form-control form-control-sm mb-2"
                    placeholder="Search Topics..." />

                <table class="table mt-3">
                    <thead>
                        <tr>
                            <th scope="col">#</th>
                            <th scope="col">Name</th>
                            <th scope="col">Topic Name</th>
                            <th scope="col">Payload</th>
                            <th scope="col" class="text-center">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(topic, index) in paginatedTopics" :key="topic.id">
                            <td>{{ (currentPage - 1) * pageSize + index + 1 }}</td>
                            <td>{{ topic.name }}</td>
                            <td>{{ topic.topicName }}</td>
                            <td>
                                <ul>
                                    <li v-for="(item, key) in topic.payload" :key="key">
                                        {{ key }}: {{ truncatePayload(item, 10) }}
                                    </li>
                                </ul>
                            </td>
                            <td class="text-end">
                                <i class="fas fa-edit text-primary me-2" data-bs-toggle="modal"
                                    data-bs-target="#editTopicModal" @click="showEditModal(topic)"></i>
                                <i class="fas fa-trash text-danger" @click="confirmDelete(topic.id)"></i>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <!-- Pagination -->
                <nav aria-label="Page navigation">
                    <ul class="pagination">
                        <li class="page-item" :class="{ disabled: currentPage === 1 }">
                            <a class="page-link" href="#" @click.prevent="previousPage" aria-label="Previous">
                                <span aria-hidden="true">&laquo; </span>
                            </a>
                        </li>
                        <li class="page-item" v-for="page in totalPages" :key="page"
                            :class="{ active: page === currentPage }">
                            <a class="page-link" href="#" @click.prevent="goToPage(page)">{{
                                page
                                }}</a>
                        </li>
                        <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                            <a class="page-link" href="#" @click.prevent="nextPage" aria-label="Next">
                                <span aria-hidden="true"> &raquo;</span>
                            </a>
                        </li>
                    </ul>
                </nav>

                <div v-if="loading" class="loading-overlay">
                    <div class="loading-card">
                        <div class="spinner-border" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <p>Loading data...</p>
                    </div>
                </div>

                <!-- Create Topic Modal -->
                <div class="modal fade" id="addTopicModal" tabindex="-1" aria-labelledby="addTopicModalLabel"
                    aria-hidden="true">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">Add Topic</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"
                                    aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <form @submit.prevent="createTopic">
                                    <div class="row">
                                        <div class="col mb-3">
                                            <label for="name">Name</label>
                                            <input type="text" class="form-control" id="name" v-model="newTopic.name"
                                                placeholder="Name" required />
                                        </div>
                                        <div class="col mb-3">
                                            <label for="topicName">Topic Name</label>
                                            <input type="text" class="form-control" id="topicName"
                                                placeholder="Topic Name" v-model="newTopic.topicName" />
                                        </div>
                                    </div>

                                    <button type="submit" class="btn btn-primary">Add</button>
                                </form>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Edit Topic Modal -->
                <div class="modal fade" id="editTopicModal" tabindex="-1" aria-labelledby="editTopicModalLabel"
                    aria-hidden="true">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">Edit Topic</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"
                                    aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <form @submit.prevent="updateTopic">
                                    <div class="row">
                                        <div class="col mb-3">
                                            <label for="editName">Name</label>
                                            <input type="text" class="form-control" id="editName" placeholder="Name"
                                                v-model="selectedTopic.name" required />
                                            <div class="col mb-3">
                                                <label for="editTopicName">Topic Name</label>
                                                <input type="text" class="form-control" id="editTopicName"
                                                    placeholder="Topic Name" v-model="selectedTopic.topicName" />
                                            </div>
                                        </div>

                                    </div>
                                    <button type="submit" class="btn btn-primary">Update</button>
                                </form>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import axios from "axios";
import Paho from "paho-mqtt";
import Swal from "sweetalert2";

const availablePartNumbers = [
    "RELAY",
    "RELAYMINI",
    "DRYCONTACT",
    "OPTOCOUPLER",
];

const topics = ref([]);
const newTopic = ref({
    name: "",
    topicName: "",
});
const selectedTopic = ref({});
const mqttClient = ref(null);
const loading = ref(false);
const searchQuery = ref("");
const currentPage = ref(1);
const pageSize = ref(5); // Change pageSize as needed
const mqttConnected = ref(false); // Track MQTT connection status

const totalPages = computed(() =>
    Math.ceil(filteredTopics.value.length / pageSize.value)
);

const paginatedTopics = computed(() => {
    const start = (currentPage.value - 1) * pageSize.value;
    const end = start + pageSize.value;
    return filteredTopics.value.slice(start, end);
});

const goToPage = (page) => {
    if (page >= 1 && page <= totalPages.value) {
        currentPage.value = page;
    }
};

const nextPage = () => {
    if (currentPage.value < totalPages.value) {
        currentPage.value++;
    }
};

const previousPage = () => {
    if (currentPage.value > 1) {
        currentPage.value--;
    }
};


const apiUrl = `http://localhost:5072/api/topics`;

// Function to parse payload into key-value pairsba
const parsePayload = (payload) => {
    const parsed = {};
    try {
        const data = JSON.parse(payload);
        if (typeof data === "object" && data !== null) {
            for (const key in data) {
                parsed[key] = data[key];
            }
        } else {
            parsed["data"] = data;
        }
    } catch (error) {
        parsed["data"] = payload;
    }
    return parsed;
};

// Function to truncate payload strings
const truncatePayload = (payload, maxLength) => {
    if (payload && payload.length > maxLength) {
        return payload.substring(0, maxLength) + "...";
    }
    return payload;
};

const fetchTopics = async () => {
    loading.value = true;
    try {
        const response = await axios.get(apiUrl, {
            headers: { Authorization: `Bearer ${localStorage.getItem("authToken")}` },
        });
        topics.value = response.data;
        subscribeToTopics();
    } catch (error) {
        console.error("Error fetching topics:", error);
    } finally {
        loading.value = false;
    }
};

const createTopic = async () => {
    loading.value = true;
    try {
        const payload = {
            ...newTopic.value,
            isModular: newTopic.value.isModular ? 1 : 0, // Ubah menjadi 0 atau 1
        };

        const response = await axios.post(apiUrl, payload, {
            headers: { Authorization: `Bearer ${localStorage.getItem("authToken")}` },
        });
        topics.value.push(response.data);
        resetForm();
        newTopic.value = {
            name: "",
            topicName: "",
        };
        subscribeToTopics();
        Swal.fire("Success", "Topic added successfully!", "success");
        fetchTopics();
    } catch (error) {
        Swal.fire("Error", "Failed to add topic.", "error");
        console.error("Error creating topic:", error);
    } finally {
        loading.value = false;
    }
};
const resetForm = () => {
    newTopic.value = {
        name: "",
        topicName: "",
    };
};
const showCreateModal = () => {
    newTopic.value = { name: "", topicName: "" };
};

const showEditModal = (topic) => {
    selectedTopic.value = {
        ...topic,
        isModular: topic.isModular === 1, // Ubah 1 menjadi true dan 0 menjadi false
    };
};

const updateTopic = async () => {
    loading.value = true;
    try {
        // Konversi isModular kembali ke 1 atau 0
        const payload = {
            ...selectedTopic.value
        };

        const response = await axios.put(
            `${apiUrl}/${selectedTopic.value.id}`,
            payload,
            {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("authToken")}`,
                },
            }
        );
        const index = topics.value.findIndex(
            (t) => t.id === selectedTopic.value.id
        );
        if (index !== -1) {
            topics.value[index] = response.data;
        }
        subscribeToTopics();
        Swal.fire("Success", "Topic updated successfully!", "success");
        fetchTopics();
    } catch (error) {
        Swal.fire("Error", "Failed to update topic.", "error");
        console.error("Error updating topic:", error);
    } finally {
        loading.value = false;
    }
};

// Watcher untuk mengatur partNumber dan deviceBus
watch(
    () => selectedTopic.value.isModular,
    (newValue) => {
        if (!newValue) {
            // Reset partNumber dan deviceBus jika isModular tidak dicentang
            selectedTopic.value.partNumber = ""; // atau null
            selectedTopic.value.deviceBus = null; // atau 0 jika Anda ingin mengatur ke 0
        }
    }
);
const confirmDelete = async (topicId) => {
    const result = await Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
        deleteTopic(topicId);
    }
};

const deleteTopic = async (topicId) => {
    loading.value = true;
    try {
        await axios.delete(`${apiUrl}/${topicId}`, {
            headers: { Authorization: `Bearer ${localStorage.getItem("authToken")}` },
        });
        topics.value = topics.value.filter((topic) => topic.id !== topicId);
        Swal.fire("Success", "The topic record has been deleted.", "success");
        fetchTopics();
    } catch (error) {
        Swal.fire("Error", "Failed to delete topic.", "error");
        console.error("Error deleting topic:", error);
    } finally {
        loading.value = false;
    }
};

// MQTT Functions
const subscribeToTopics = () => {
    topics.value.forEach((topic) => {
        if (topic.topicName) {
            mqttClient.value.subscribe(topic.topicName);
        }
    });
};

const onMessageArrived = (message) => {
    topics.value.forEach((topic) => {
        if (message.destinationName === topic.topicName) {
            topic.payload = parsePayload(message.payloadString);
        }
    });
};

const connectMqttClient = () => {
    mqttClient.value = new Paho.Client(
        '192.168.0.186',
        Number(9000),
        `client-${Math.random()}`
    );
    mqttClient.value.onMessageArrived = onMessageArrived;

    mqttClient.value.connect({
        onSuccess: () => {
            mqttConnected.value = true; // Update connection status
            subscribeToTopics();
            console.log("Connected to MQTT broker");
        },
        onFailure: (error) => {
            mqttConnected.value = false; // Update connection status
            console.error("Failed to connect to MQTT broker:", error);
        },
    });
};

// Search functionality
const filterTopics = () => {
    return topics.value.filter((topic) => {
        return (
            topic.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
            topic.topicName.toLowerCase().includes(searchQuery.value.toLowerCase())
        );
    });
};

const filteredTopics = computed(() => filterTopics());

onMounted(() => {
    fetchTopics();
    connectMqttClient();
});
</script>

<style scoped>
/* Add any necessary styles */
</style>

<template>
    <div class="container">
        <h5>User Access Control Management</h5>
        <hr />

        <!-- Button to Open Modal -->
        <div class="mb-3">
            <button class="btn btn-primary btn-sm" data-toggle="modal" data-target="#registerUserModal">
                Register User
            </button>
        </div>

        <!-- Alert for Response -->
        <div v-if="responseMessage" :class="`alert alert-${responseStatus}`" role="alert">
            {{ responseMessage }}
        </div>

        <!-- Modal for Register User -->
        <div class="modal fade" id="registerUserModal" tabindex="-1" role="dialog"
            aria-labelledby="registerUserModalLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="registerUserModalLabel">Register User</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="createUser">
                            <div class="form-group">
                                <label for="username">Username</label>
                                <input type="text" id="username" class="form-control" v-model="newUser.username"
                                    placeholder="Enter username" required />
                            </div>
                            <div class="form-group">
                                <label for="password">Password</label>
                                <input type="password" id="password" class="form-control" v-model="newUser.password"
                                    placeholder="Enter password" required />
                            </div>
                            <button type="submit" class="btn btn-success btn-sm">
                                Register
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal for Edit User -->
        <div class="modal fade" id="updateUserModal" tabindex="-1" role="dialog" aria-labelledby="updateUserModalLabel"
            aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="updateUserModalLabel">Update User</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="updateUser">
                            <div class="form-group">
                                <label for="updateUid">UID</label>
                                <input type="number" id="updateUid" class="form-control" v-model="userToUpdate.uid"
                                    disabled />
                            </div>
                            <div class="form-group">
                                <label for="updateUsername">Username</label>
                                <input type="text" id="updateUsername" class="form-control"
                                    v-model="userToUpdate.username" placeholder="Enter username" required />
                            </div>
                            <div class="form-group">
                                <label for="updatePassword">Password</label>
                                <input type="password" id="updatePassword" class="form-control"
                                    v-model="userToUpdate.password" placeholder="Enter password" required />
                            </div>
                            <button type="submit" class="btn btn-primary btn-sm">
                                Update
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </div>

        <!-- Table for User Management -->
        <table v-if="users.length > 0" class="table table-striped">
            <thead>
                <tr>
                    <th>UID</th>
                    <th>Username</th>
                    <th>Password</th>
                    <th>Privilege</th>
                    <th>User ID</th>
                    <th>Card</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="user in users" :key="user.uid">
                    <td>{{ user.uid }}</td>
                    <td>{{ user.username }}</td>
                    <td>{{ user.password }}</td>
                    <td>{{ user.privilege }}</td>
                    <td>{{ user.user_id }}</td>
                    <td>{{ user.card }}</td>
                    <td>
                        <button class="btn btn-sm btn-primary me-2"
                            @click="startFingerprintRegistration(user.uid)">Fingerprint
                        </button>
                        <button class="btn btn-sm btn-warning me-2" @click="setUserToUpdate(user)" data-toggle="modal"
                            data-target="#updateUserModal">
                            Update
                        </button>
                        <button class="btn btn-sm btn-danger" @click="deleteUser(user.uid)">
                            Delete
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>

        <!-- Modal for Fingerprint Registration -->
        <div class="modal fade" id="fingerprintModal" tabindex="-1" role="dialog"
            aria-labelledby="fingerprintModalLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="fingerprintModalLabel">Register Fingerprint</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <!-- Display Fingerprint Enrollment Status -->
                        <div v-if="fingerprintStatus" class="mt-3 alert alert-sm alert-info">
                            {{ fingerprintStatus }}
                        </div>

                        <!-- Show Registered Fingerprints as Badges -->
                        <div v-if="registeredFingerprints.length > 0" class="mb-3">
                            <h6>Registered Fingerprints:</h6>
                            <div class="d-flex flex-wrap">
                                <span v-for="index in registeredFingerprints" :key="index"
                                    class="badge badge-info me-2">
                                    {{ getFingerName(index) }}
                                </span>
                            </div>
                        </div>

                        <p>Select the finger index to register:</p>

                        <!-- List available finger indexes, showing registered ones -->
                        <select v-model.number="selectedFingerIndex" class="form-control">
                            <option v-for="index in Array.from({ length: 10 }, (_, i) => i)" :key="index" :value="index"
                                :disabled="registeredFingerprints.includes(index)">
                                {{ getFingerName(index) }}
                                <span v-if="registeredFingerprints.includes(index)"> (Registered)</span>
                            </option>
                        </select>

                        <button class="btn btn-primary btn-sm mt-3 me-2" @click="enrollFingerprint">Enrollment</button>
                        <!-- Button to Delete Fingerprint -->
                        <button class="btn btn-danger btn-sm mt-3" @click="deleteFingerprint">Delete Finger</button>

                    </div>
                </div>
            </div>
        </div>

    </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import axios from 'axios';
import Paho from 'paho-mqtt';

// States
const users = ref([]);
const responseMessage = ref('');
const responseStatus = ref('success'); // 'success' or 'danger'
const newUser = ref({ username: '', password: '', uid: null });
const userToUpdate = ref({ uid: null, username: '', password: '' });


const selectedFingerIndex = ref(0);
const currentUid = ref(null);
const fingerprintStatus = ref('');
const registeredFingerprints = ref([]);

// MQTT Configuration
const mqttBroker = '192.168.0.133'; // IP Broker
const mqttPort = 9000; // Port Broker
const commandTopic = 'access_control/command';
const responseTopic = 'access_control/response';
const backendUrl = 'http://localhost:5072'; // URL Backend Anda

// Helper to Show Alerts
const showAlert = (message, status) => {
    responseMessage.value = message;
    responseStatus.value = status;
    setTimeout(() => {
        responseMessage.value = '';
    }, 3000);
};

// MQTT Connect Helper
const mqttConnect = (payload, onMessageCallback) => {
    const client = new Paho.Client(mqttBroker, mqttPort, `webClient-${Date.now()}`);
    client.onMessageArrived = (message) => {
        const response = JSON.parse(message.payloadString);
        onMessageCallback(response);
        client.disconnect();
    };
    client.connect({
        onSuccess: () => {
            client.subscribe(responseTopic);
            const message = new Paho.Message(JSON.stringify(payload));
            message.destinationName = commandTopic;
            client.send(message);
        },
        onFailure: () => showAlert('Failed to connect to broker.', 'danger'),
    });
};

const getRegisteredFingerprints = async (uid) => {
    try {
        const response = await axios.get(`${backendUrl}/fingerprints/${uid}`);
        if (response.status === 200) {
            registeredFingerprints.value = response.data;
            console.log('Registered Fingerprints:', registeredFingerprints.value); // Debugging
        } else {
            showAlert('No fingerprints registered for this UID', 'warning');
        }
    } catch (error) {
        showAlert(`Error fetching registered fingerprints: ${error.message}`, 'danger');
    }
};

// Helper function to get the finger name
const getFingerName = (index) => {
    const fingerNames = [
        "L Little Finger", "L Ring Finger", "L Middle Finger",
        "L Index Finger", "L Thumb",
        "R Thumb", "R Index Finger",
        "R Middle Finger", "R Ring Finger", "R Little Finger"
    ];
    return fingerNames[index];
};

// Start Fingerprint Registration
const startFingerprintRegistration = (uid) => {
    currentUid.value = uid;
    fingerprintStatus.value = 'Ready To Enroll Finger Print';
    getRegisteredFingerprints(uid); // Fetch the registered fingerprints for this UID
    $('#fingerprintModal').modal('show'); // Open the modal to select finger index
};

// Enroll Fingerprint
const enrollFingerprint = () => {
    if (currentUid.value === null) {
        showAlert('UID is not selected.', 'danger');
        return;
    }

    // Update fingerprint status to indicate enrollment is in progress
    fingerprintStatus.value = 'Please Tap The Finger on the Green Box, Scanning fingerprint...';

    mqttConnect(
        {
            command: 'enroll_finger',
            uid: currentUid.value,
            finger_index: selectedFingerIndex.value,
            mode: 'register', // Mode for registering fingerprint
        },
        (response) => {
            if (response.response === 'success') {
                fingerprintStatus.value = `Fingerprint enrollment started for UID ${currentUid.value}`;
                // After enrollment, send the fingerprint index to backend to save it
                saveFingerprintIndexToDatabase(currentUid.value, selectedFingerIndex.value);
            } else {
                fingerprintStatus.value = response.error || 'Failed to start fingerprint enrollment.';
            }
        }
    );
};

// Function to send fingerprint index to the backend
const saveFingerprintIndexToDatabase = async (uid, fingerIndex) => {
    try {
        const response = await axios.post(`${backendUrl}/fingerprints`, {
            uid: uid,
            finger_index: fingerIndex
        });

        if (response.status === 200) {
            showAlert('Fingerprint index saved successfully!', 'success');
        } else {
            showAlert('Failed to save fingerprint index.', 'danger');
        }
    } catch (error) {
        showAlert(`Error saving fingerprint index: ${error.message}`, 'danger');
    }
};

// Delete Fingerprint
const deleteFingerprint = () => {
    if (currentUid.value === null) {
        showAlert('UID is not selected for deleting fingerprint.', 'danger');
        return;
    }

    // Kirim perintah untuk menghapus fingerprint ke ZKTeco melalui MQTT
    mqttConnect(
        {
            command: 'delete_finger',
            uid: currentUid.value,
            finger_index: selectedFingerIndex.value,
        },
        (response) => {
            if (response.response === 'success') {
                fingerprintStatus.value = `Fingerprint with index ${selectedFingerIndex.value} deleted successfully.`;
                // Hapus data fingerprint dari database setelah penghapusan berhasil di ZKTeco
                removeFingerprintFromDatabase(currentUid.value, selectedFingerIndex.value);
            } else {
                fingerprintStatus.value = response.error || 'Failed to delete fingerprint.';
            }
        }
    );
};

// Fungsi untuk menghapus fingerprint dari database
const removeFingerprintFromDatabase = async (uid, fingerIndex) => {
    try {
        const response = await axios.delete(`${backendUrl}/fingerprints/${uid}/${fingerIndex}`);

        if (response.status === 200) {
            showAlert('Fingerprint removed from database successfully!', 'success');
        } else {
            showAlert('Failed to remove fingerprint from database.', 'danger');
        }
    } catch (error) {
        showAlert(`Error removing fingerprint from database: ${error.message}`, 'danger');
    }
};

// Fetch All Users
const fetchUsers = () => {
    const client = new Paho.Client(mqttBroker, mqttPort, `webClient-${Date.now()}`);
    client.onMessageArrived = (message) => {
        const response = JSON.parse(message.payloadString);
        if (response.response === 'success' && response.command === 'get_all_users') {
            users.value = response.users;
            showAlert('Users retrieved successfully!', 'success');
        } else {
            showAlert(response.error || 'Failed to fetch users.', 'danger');
        }
        client.disconnect();
    };
    client.connect({
        onSuccess: () => {
            client.subscribe(responseTopic);
            const message = new Paho.Message(JSON.stringify({ command: 'get_all_users' }));
            message.destinationName = commandTopic;
            client.send(message);
        },
        onFailure: () => showAlert('Failed to connect to broker.', 'danger'),
    });
};

// Create User
const createUser = () => {
    mqttConnect({ command: 'add_user', ...newUser.value }, async (response) => {
        if (response.response === 'success') {
            showAlert('User created successfully!', 'success');

            // Kirim data ke backend termasuk UID yang diterima dari MQTT response
            try {
                const backendResponse = await axios.post(`${backendUrl}/users`, {
                    username: newUser.value.username,
                    password: newUser.value.password,
                    uid: response.uid // UID dari MQTT response
                });

                if (backendResponse.status === 201) {
                    showAlert('User saved to backend successfully!', 'success');
                } else {
                    showAlert('Failed to save user to backend.', 'danger');
                }
            } catch (error) {
                showAlert(`Error saving to backend: ${error.message}`, 'danger');
            }
        } else {
            showAlert(response.error || 'Failed to create user.', 'danger');
        }
    });
};

// Update User
const updateUser = () => {
    mqttConnect(
        {
            command: 'edit_user',
            uid: userToUpdate.value.uid,
            username: userToUpdate.value.username,
            password: userToUpdate.value.password,
        },
        async (response) => {
            if (response.response === 'success') {
                // Update data di backend
                try {
                    const backendResponse = await axios.put(`${backendUrl}/users/${userToUpdate.value.uid}`, {
                        username: userToUpdate.value.username,
                        password: userToUpdate.value.password,
                    });

                    if (backendResponse.status === 200) {
                        fetchUsers();
                        showAlert('User updated successfully!', 'success');
                    } else {
                        showAlert('Failed to update user in backend.', 'danger');
                    }
                } catch (error) {
                    showAlert(`Error updating in backend: ${error.message}`, 'danger');
                }
            } else {
                showAlert(response.error || 'Failed to update user.', 'danger');
            }
        }
    );
};

// Set User to Update (Fill Modal with Existing Data)
const setUserToUpdate = (user) => {
    userToUpdate.value = { ...user }; // Clone user data to avoid direct mutation
};

// Delete User
const deleteUser = (uid) => {
    mqttConnect({ command: 'delete_user', uid }, async (response) => {
        if (response.response === 'success') {
            fetchUsers();
            showAlert(`User with UID ${uid} deleted successfully!`, 'success');
        } else {
            showAlert(response.error || 'Failed to delete user.', 'danger');
        }
    });
};


onMounted(() => {
    fetchUsers();
});
</script>

<style>
.container {
    max-width: 800px;
    margin: 0 auto;
}
</style>

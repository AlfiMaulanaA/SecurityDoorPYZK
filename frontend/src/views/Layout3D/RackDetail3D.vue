<template>
    <div>
        <div class="d-flex mb-2">
            <h5>{{ rackNameRef }}</h5>
            <div class="ms-4 me-4">
                <span class="me-2 ms-4"><strong>Temperature:</strong> {{ latestTemp }} °C</span>
                <span><strong>Humidity:</strong> {{ latestHum }} %</span>
            </div>
            <div style="margin-left:550px;">
                <button type="button" class="btn btn-sm btn-primary ms-4" data-bs-toggle="modal"
                    data-bs-target="#exampleModal">
                    Device
                </button>
                <router-link to="/container" class="btn btn-sm btn-secondary ms-2 me-4">Back To Container</router-link>
            </div>
        </div>
        <!-- Detail Device -->
        <div v-if="selectedDeviceDetail" class="device-detail mt-4 p-3 border rounded bg-light">
            <h6><strong>Name:</strong> {{ selectedDeviceDetail.name }}</h6>
            <hr>
            <div><strong>Position :</strong> Unit {{ selectedDeviceDetail.position }}</div>
            <div><strong>Heigh Unit:</strong> {{ selectedDeviceDetail.totalU }} U</div>
            <div><strong>Customer :</strong>
                {{ selectedDeviceDetail.customer ? selectedDeviceDetail.customer : 'No Customer' }}</div>
            <div class="mt-2"><strong>Person Installed :</strong> {{ selectedDeviceDetail.person }}</div>
            <div><strong>Installed At :</strong> {{ new Date(selectedDeviceDetail.createdAt).toLocaleString() }}</div>
        </div>

        <!-- Modal -->
        <div class="modal fade" id="exampleModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="exampleModalLabel">Device on Rack</h5>
                        <button class="btn btn-sm btn-primary" data-bs-toggle="modal"
                            data-bs-target="#addDeviceModal">Add Device</button>
                    </div>
                    <div class="modal-body">
                        <div class="d-flex justify-content-between">
                            <h5>Total Heigh Unit <strong>42 U</strong></h5>
                            <div>
                                <div>Total U Used: {{ totalU }} U</div>
                                <div>Available: {{ 42 - totalU }} U</div>
                            </div>
                        </div>
                        Device Installed
                        <ul v-if="devices.length > 0">
                            <li v-for="device in devices" :key="device.id">
                                <div class="row mt-1">
                                    <div class="col"><strong>{{ device.name }}</strong></div>
                                    <div class="col">Position U: {{ device.position }}</div>
                                    <div class="col">High U: {{ device.totalU }}</div>
                                    <div class="col">
                                        <button class="btn btn-sm btn-warning ms-2" data-bs-toggle="modal"
                                            data-bs-target="#updateDeviceModal"
                                            @click="setUpdateDevice(device)">Update</button>
                                        <button class="btn btn-sm btn-danger ms-2"
                                            @click="deleteDevice(device.id)">Delete</button>
                                    </div>
                                </div>
                            </li>
                        </ul>
                        <p v-else class="text-danger text-center m-4">No Data Device Installed</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal untuk Add Device -->
        <div class="modal fade" id="addDeviceModal" tabindex="-1" aria-labelledby="addDeviceLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addDeviceLabel">Add New Device</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-6 mb-3">
                                <label for="deviceName" class="form-label">Device Name</label>
                                <input type="text" id="deviceName" v-model="newDevice.name" class="form-control"
                                    placeholder="Enter device name" />
                            </div>
                            <div class="col-6 mb-3">
                                <label for="pic" class="form-label">Person Installed</label>
                                <input type="text" id="pic" v-model="newDevice.person" class="form-control"
                                    placeholder="PIC" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-12">
                                <label for="customer" class="form-label">Customer</label>
                                <input type="text" id="customer" v-model="newDevice.customer" class="form-control"
                                    placeholder="Customer Name" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-6 mb-3">
                                <label for="devicePosition" class="form-label">Position U</label>
                                <input type="text" id="devicePosition" v-model="newDevice.position" class="form-control"
                                    placeholder="Enter Position U" />
                            </div>
                            <div class="col-6 mb-3">
                                <label for="deviceTotalU" class="form-label">Total U</label>
                                <input type="number" id="deviceTotalU" v-model="newDevice.totalU" class="form-control"
                                    placeholder="Enter Total U" />
                            </div>

                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <button type="button" class="btn btn-primary" @click="addDevice">Add Device</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal untuk Update Device -->
        <div class="modal fade" id="updateDeviceModal" tabindex="-1" aria-labelledby="updateDeviceLabel"
            aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="updateDeviceLabel">Update Device</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-6 mb-3">
                                <label for="updateDeviceName" class="form-label">Device Name</label>
                                <input type="text" id="updateDeviceName" v-model="selectedDevice.name"
                                    class="form-control" placeholder="Enter device name" />
                            </div>
                            <div class="col-6 mb-3">
                                <label for="pic" class="form-label">Person Installed</label>
                                <input type="text" id="pic" v-model="selectedDevice.person" class="form-control"
                                    placeholder="PIC" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-12">
                                <label for="customer" class="form-label">Customer</label>
                                <input type="text" id="customer" v-model="selectedDevice.customer" class="form-control"
                                    placeholder="Customer Name" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-6 mb-3">
                                <label for="updateDevicePosition" class="form-label">Position U</label>
                                <input type="text" id="updateDevicePosition" v-model="selectedDevice.position"
                                    class="form-control" placeholder="Enter Position U" />
                            </div>
                            <div class="col-6 mb-3">
                                <label for="updateDeviceTotalU" class="form-label">Total U</label>
                                <input type="number" id="updateDeviceTotalU" v-model="selectedDevice.totalU"
                                    class="form-control" placeholder="Enter Total U" />
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <button type="button" class="btn btn-primary" @click="updateDevice">Update Device</button>
                    </div>
                </div>
            </div>
        </div>


        <div ref="canvasRef"></div>

    </div>
</template>

<script setup>
import * as THREE from 'three';
import { onMounted, ref } from 'vue';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { FontLoader } from 'three/examples/jsm/loaders/FontLoader';
import { TextGeometry } from 'three/examples/jsm/geometries/TextGeometry';
import axios from 'axios'; // Install axios jika belum: npm install axios
import { useRoute } from 'vue-router';
import Paho from 'paho-mqtt';
const mqttClient = ref(null);
const latestTemp = ref(null);
const latestHum = ref(null);
const sensorTextMesh = ref(null);
const sensorText = ref('Temp: 0°C  Hum: 0%');

const connectMQTT = () => {
    mqttClient.value = new Paho.Client("192.168.0.186", Number(9000), `client-${Math.random()}`);

    mqttClient.value.onConnectionLost = (responseObject) => {
        console.error("MQTT Connection Lost:", responseObject.errorMessage);
    };

    mqttClient.value.onMessageArrived = (message) => {
        handleMQTTMessage(message);
    };

    mqttClient.value.connect({
        onSuccess: () => {
            console.log("Connected to MQTT broker");
            if (topic.value) {
                mqttClient.value.subscribe(topic.value);
            }
        },
        onFailure: (error) => {
            console.error("MQTT Connection Failed:", error.errorMessage);
        }
    });
};

const handleMQTTMessage = (message) => {
    try {
        const payload = JSON.parse(message.payloadString);
        if (payload.value) {
            const valueData = JSON.parse(payload.value);
            latestTemp.value = valueData.temp;
            latestHum.value = valueData.hum;
            updateTempUI(valueData.temp);
            updateHumUI(valueData.hum);
        }
    } catch (error) {
        console.error("Error parsing MQTT message:", error);
    }
};

const addSensorUI = (scene) => {
    const loader = new FontLoader();
    loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
        const tempGeometry = new TextGeometry(`0°C`, {
            font: font,
            size: 5,
            depth: 1,
        });
        const tempMaterial = new THREE.MeshLambertMaterial({ color: 0x999999 });
        const tempMesh = new THREE.Mesh(tempGeometry, tempMaterial);
        tempMesh.position.set(0, 100, 40);
        scene.add(tempMesh);

        const humGeometry = new TextGeometry(`0%`, {
            font: font,
            size: 5,
            depth: 1,
        });
        const humMaterial = new THREE.MeshLambertMaterial({ color: 0x999999 });
        const humMesh = new THREE.Mesh(humGeometry, humMaterial);
        humMesh.position.set(-10, 100, 40);
        scene.add(humMesh);

        sensorTextMesh.value = { temp: tempMesh, hum: humMesh };
    });
};

const updateTempUI = (temp) => {
    if (!sensorTextMesh.value || !sensorTextMesh.value.temp) return;
    const tempColor = temp > 40 ? 0xff0000 : temp > 30 ? 0xffff00 : 0x00ff00;

    const loader = new FontLoader();
    loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
        const tempGeometry = new TextGeometry(`${temp}°C`, {
            font: font,
            size: 5,
            depth: 1,
        });
        sensorTextMesh.value.temp.geometry.dispose();
        sensorTextMesh.value.temp.geometry = tempGeometry;
        sensorTextMesh.value.temp.material.color.set(tempColor);
        sensorTextMesh.value.temp.material.needsUpdate = true;
    });
};

const updateHumUI = (hum) => {
    if (!sensorTextMesh.value || !sensorTextMesh.value.hum) return;
    const humColor = hum > 60 ? 0xff0000 : hum > 40 ? 0xffff00 : 0x00ff00;

    const loader = new FontLoader();
    loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
        const humGeometry = new TextGeometry(`${hum}%`, {
            font: font,
            size: 5,
            depth: 1,
        });
        sensorTextMesh.value.hum.geometry.dispose();
        sensorTextMesh.value.hum.geometry = humGeometry;
        sensorTextMesh.value.hum.material.color.set(humColor);
        sensorTextMesh.value.hum.material.needsUpdate = true;
    });
};

const route = useRoute();
const rackId = route.params.rackId; // Ambil parameter rackId dari rute
const devices = ref([]); // Data perangkat di rack

const newDevice = ref({ name: '', position: '', totalU: 0, person: '', customer: '', containerRackId: rackId });
const selectedDevice = ref({ name: '', position: '', totalU: 0, person: '', customer: '', containerRackId: rackId });

const fetchContainerRackWithDevices = async (rackId) => {
    try {
        const response = await axios.get(`http://localhost:5072/containerRacks/${rackId}`);
        return response.data;
    } catch (error) {
        console.error("Error fetching container rack data:", error);
        return null;
    }
};

const fetchDevices = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/deviceRacks/byContainer/${rackId}`);
        devices.value = response.data;
        totalU.value = devices.value.reduce((sum, device) => sum + device.totalU, 0);
    } catch (error) {
        console.error("Error fetching devices:", error);
    }
};

const addDevice = async () => {
    if (totalU.value + newDevice.value.totalU > 42) {
        alert("Not enough space in the rack!");
        return;
    }
    try {
        const response = await axios.post("http://localhost:5072/deviceRacks", newDevice.value);
        devices.value.push(response.data);
        totalU.value += newDevice.value.totalU;
        newDevice.value = { name: '', position: '', totalU: 0, person: '', customer: '', containerRackId: rackId };
    } catch (error) {
        console.error("Error adding device:", error);
    }
};

const updateDevice = async () => {
    try {
        await axios.put(`http://localhost:5072/deviceRacks/${selectedDevice.value.id}`, selectedDevice.value);
        devices.value = devices.value.map(d => (d.id === selectedDevice.value.id ? selectedDevice.value : d));
    } catch (error) {
        console.error("Error updating device:", error);
    }
};

const deleteDevice = async (deviceId) => {
    try {
        await axios.delete(`http://localhost:5072/deviceRacks/${deviceId}`);
        devices.value = devices.value.filter(d => d.id !== deviceId);
    } catch (error) {
        console.error("Error deleting device:", error);
    }
};

const setUpdateDevice = (device) => {
    selectedDevice.value = { ...device };
};


const canvasRef = ref(null);
const rackNameRef = ref('');
const topic = ref('');
const heightPercentage = ref(0);
// Constants for dimensions
const rackWidth = 60; // 19 inches standard rack width (approx 60cm)
const rackHeight = 186; // 42U height (1U = 4.445 cm, 42U = ~186 cm)
const rackDepth = 100; // Standard depth of 1m
const shelfHeight = rackHeight / 42; // Each U height

// Initialize scene
const initializeScene = () => {
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0xeeeeee);
    return scene;
};

// Initialize camera
const initializeCamera = () => {
    const camera = new THREE.PerspectiveCamera(
        75,
        window.innerWidth / window.innerHeight,
        0.1,
        1000
    );
    camera.position.set(100, 50, 200);
    return camera;
};

// Initialize renderer
const initializeRenderer = (canvasRef) => {
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(window.innerWidth, window.innerHeight);
    canvasRef.value.appendChild(renderer.domElement);
    return renderer;
};

// Tambahkan lebih banyak pencahayaan
const addLights = (scene) => {
    // Tambahkan Ambient Light (Cahaya yang menyebar ke seluruh scene)
    const ambientLight = new THREE.AmbientLight(0xffffff, 2.5); // Lebih terang
    scene.add(ambientLight);

    // Tambahkan Directional Light (Cahaya datang dari satu arah)
    const directionalLight = new THREE.DirectionalLight(0xffffff, 2.5);
    directionalLight.position.set(50, 50, 50);
    scene.add(directionalLight);

    // Tambahkan Hemispheric Light (Cahaya dari atas ke bawah, lebih natural)
    const hemisphereLight = new THREE.HemisphereLight(0xffffff, 0x404040, 2); // Cahaya atas lebih terang, bawah lebih gelap
    scene.add(hemisphereLight);

    // Tambahkan SpotLight (Cahaya terfokus ke satu titik)
    const spotLight = new THREE.SpotLight(0xffffff, 3);
    spotLight.position.set(100, 150, 100);
    spotLight.angle = Math.PI / 6; // Lebar sinar
    spotLight.penumbra = 0.3; // Gradasi antara terang dan gelap
    spotLight.castShadow = true;
    scene.add(spotLight);
};


// Rack structure
const createRack = (scene) => {
    const rackGeometry = new THREE.BoxGeometry(rackWidth, rackHeight, rackDepth);
    const rackMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
    const rackMesh = new THREE.Mesh(rackGeometry, rackMaterial);
    scene.add(rackMesh);
    return rackMesh;
};

// Rack shelves
const createShelves = (scene) => {
    const shelfMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
    for (let i = 0; i < 42; i++) {
        const shelfGeometry = new THREE.BoxGeometry(rackWidth, 0.5, rackDepth);
        const shelfMesh = new THREE.Mesh(shelfGeometry, shelfMaterial);
        shelfMesh.position.set(0, -rackHeight / 2 + shelfHeight * (i + 1), 0);
        scene.add(shelfMesh);
    }
};

// Add U numbering
const addUNumbers = (scene) => {
    const loader = new FontLoader();
    loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
        for (let i = 0; i < 42; i++) {
            const textGeometry = new TextGeometry((i + 1).toString(), { // Reverse order to start from 1 at the bottom
                font: font,
                size: 2,
                height: 0.5,
            });
            const textMaterial = new THREE.MeshLambertMaterial({ color: 0x787878 });
            const textMesh = new THREE.Mesh(textGeometry, textMaterial);
            textMesh.position.set(-rackWidth / 2 - 5, -rackHeight / 2 + shelfHeight * i + shelfHeight / 2 - 2, rackDepth / 2 + 1);
            scene.add(textMesh);
        }
    });
};

const addRackNameToScene = (scene, rackName) => {
    if (!rackName || rackName.trim() === '') {
        console.warn("Rack name is empty, skipping 3D text creation.");
        return;
    }

    const loader = new FontLoader();
    loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
        const textGeometry = new TextGeometry(rackName, {
            font: font,
            size: 7,
            depth: 2,
        });

        const textMaterial = new THREE.MeshLambertMaterial({ color: 0xffffff });
        const textMesh = new THREE.Mesh(textGeometry, textMaterial);

        textMesh.position.set(-12, 108, 40);
        textMesh.castShadow = true; // Tambahkan efek bayangan
        textMesh.receiveShadow = true;

        scene.add(textMesh);
    });
};

const botomCoverRack = (scene) => {
    const objectHeight = 10;
    const objectGeometry = new THREE.BoxGeometry(62, objectHeight, 102);
    const objectMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
    const objectMesh = new THREE.Mesh(objectGeometry, objectMaterial);
    objectMesh.position.set(0, -rackHeight / 2 - 10 + objectHeight / 2, 0); // Place at the bottom-most U
    scene.add(objectMesh);
    return objectMesh;
};

const topCoverRack = (scene) => {
    const objectHeight = 5;
    const objectGeometry = new THREE.BoxGeometry(61, objectHeight, 101);
    const objectMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
    const objectMesh = new THREE.Mesh(objectGeometry, objectMaterial);
    objectMesh.position.set(0, rackHeight / 2 + objectHeight / 2, 0); // Place at the bottom-most U
    scene.add(objectMesh);
    return objectMesh;
};

// Render loop
const renderLoop = (renderer, scene, camera, controls) => {
    const animate = () => {
        requestAnimationFrame(animate);
        controls.update(); // Required if damping is enabled
        renderer.render(scene, camera);
    };
    animate();
};

const addDeviceNames = (scene, devices) => {
    const loader = new FontLoader();

    loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
        devices.forEach((device) => {
            const positionY = -rackHeight / 2 + shelfHeight * (parseInt(device.position) + device.totalU / 2 - 1);

            const textGeometry = new TextGeometry(device.name, {
                font: font,
                size: 3, // Ukuran teks
                depth: 0.5, // Ketebalan teks (pengganti height)
            });

            const textMaterial = new THREE.MeshLambertMaterial({ color: 0xffffff });
            const textMesh = new THREE.Mesh(textGeometry, textMaterial);

            textMesh.position.set(-rackWidth / 2 + 2, positionY - 1.5, rackDepth / 2 + 1.25); // Posisi di samping rak
            scene.add(textMesh);
        });
    });
};

const totalU = ref(0);

// Fungsi untuk menghitung total U dari semua perangkat
const calculateTotalU = (deviceList) => {
    return deviceList.reduce((sum, device) => sum + device.totalU, 0);
};

import gsap from "gsap";

const addArrowIndicators = (scene) => {
    const arrowLength = 15; // Panjang batang panah
    const arrowColor = 0x006df2; // Warna biru
    const arrowDirection = new THREE.Vector3(0, 0, -1); // Arah panah ke depan (sumbu Z-)
    arrowDirection.normalize(); // Normalisasi vektor

    const moveDistance = 160; // Jarak animasi maju
    const duration = 1.2; // Kecepatan animasi lebih smooth
    const resetDuration = 0.2; // Waktu reset ke posisi awal lebih cepat
    const delayBetweenArrows = 0.2; // Delay antar panah

    // Posisi awal panah dalam pola zigzag
    const arrowPositions = [
        new THREE.Vector3(-rackWidth / 2 + 10, 0, 80),  // Panah kiri bawah
        new THREE.Vector3(rackWidth / 2 - 10, 10, 80),  // Panah kanan atas
        new THREE.Vector3(-rackWidth / 2 + 10, 20, 80), // Panah kiri atas
        new THREE.Vector3(rackWidth / 2 - 10, 30, 80),  // Panah kanan atas lebih tinggi
    ];

    const shaftThickness = 4; // Ukuran batang panah
    const headSize = 6; // Ukuran kepala panah

    // Simpan panah untuk animasi
    const arrows = [];

    arrowPositions.forEach(position => {
        // **1. Buat panah dengan `ArrowHelper`**
        const arrowHelper = new THREE.ArrowHelper(arrowDirection, position, arrowLength, arrowColor, headSize, shaftThickness);
        scene.add(arrowHelper);
        arrows.push(arrowHelper);
    });

    // **Fungsi untuk Animasi Smooth**
    const animateArrow = (arrow, index) => {
        const timeline = gsap.timeline({ repeat: -1, delay: index * delayBetweenArrows });

        timeline.to(arrow.position, {
            z: arrow.position.z - moveDistance, // Maju sejauh 160 unit
            duration: duration,
            ease: "power2.inOut"
        })
            .set(arrow.position, { z: arrowPositions[index].z }) // Reset posisi ke awal
            .to(arrow, {
                duration: resetDuration // Transisi kembali lebih cepat
            });
    };

    // Mulai animasi untuk setiap panah
    arrows.forEach((arrow, index) => animateArrow(arrow, index));
};
const rendererRef = ref(null); // Simpan renderer global
const selectedDeviceId = ref(null);
const selectedDeviceDetail = ref(null);
const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();

const fetchDeviceDetail = async (deviceId) => {
    try {
        const response = await axios.get(`http://localhost:5072/deviceRacks/${deviceId}`);
        selectedDeviceDetail.value = response.data;
    } catch (error) {
        console.error("Error fetching device detail:", error);
    }
};


const onMouseClick = (event, camera, scene) => {
    if (!rendererRef.value) return;

    const rect = rendererRef.value.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(scene.children, true);

    if (intersects.length > 0) {
        const clickedObject = intersects[0].object;

        if (clickedObject.userData.deviceId) {
            selectedDeviceId.value = clickedObject.userData.deviceId;
            console.log("Device id clicked:", selectedDeviceId.value);

            // Ambil detail perangkat
            fetchDeviceDetail(selectedDeviceId.value);
        }
    }
};

// **Tambahkan Devices dari API ke Scene**
const addDevicesFromAPI = (scene, deviceRacks) => {
    const objectMaterial = new THREE.MeshLambertMaterial({ color: 0x7a7679 });
    const frameMaterial = new THREE.LineBasicMaterial({ color: 0xd4d4d4 });

    deviceRacks.forEach((device) => {
        const objectGeometry = new THREE.BoxGeometry(rackWidth - 1, shelfHeight * device.totalU, rackDepth + 1);
        const deviceMesh = new THREE.Mesh(objectGeometry, objectMaterial);
        deviceMesh.position.set(
            0,
            -rackHeight / 2 + shelfHeight * (parseInt(device.position) + device.totalU / 2 - 1),
            1
        );
        scene.add(deviceMesh);

        const edges = new THREE.EdgesGeometry(objectGeometry);
        const frame = new THREE.LineSegments(edges, frameMaterial);
        frame.position.copy(deviceMesh.position);
        scene.add(frame);

        // **Tambahkan Data ID ke userData untuk Identifikasi**
        deviceMesh.userData.deviceId = device.id;
    });
};

// **Modifikasi Render3D untuk Menangkap Klik**
const Render3D = async () => {
    const scene = initializeScene();
    const camera = initializeCamera();
    const renderer = initializeRenderer(canvasRef);
    rendererRef.value = renderer; // Simpan renderer ke dalam ref global

    addLights(scene);
    createRack(scene);
    botomCoverRack(scene);
    topCoverRack(scene);
    createShelves(scene);
    addUNumbers(scene);
    addSensorUI(scene);
    addArrowIndicators(scene);

    // Ambil data perangkat dari API
    const containerRackData = await fetchContainerRackWithDevices(rackId);
    if (containerRackData) {
        rackNameRef.value = containerRackData.rackName;
        topic.value = containerRackData.topic;
        heightPercentage.value = containerRackData.heightPercentage;

        devices.value = containerRackData.deviceRacks || [];
        totalU.value = calculateTotalU(devices.value);

        addRackNameToScene(scene, rackNameRef.value);
        addDeviceNames(scene, containerRackData.deviceRacks);
        addDevicesFromAPI(scene, containerRackData.deviceRacks);
    }

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;

    renderLoop(renderer, scene, camera, controls);

    // Tangani perubahan ukuran layar
    window.addEventListener("resize", () => {
        camera.aspect = window.innerWidth / window.innerHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(window.innerWidth, window.innerHeight);
    });

    // **Tambahkan Event Listener Klik**
    window.addEventListener("mousedown", (event) => onMouseClick(event, camera, scene));
};

// Hook onMounted
onMounted(async () => {
    fetchDevices();
    Render3D();
    connectMQTT();
});

</script>

<style scoped>
.device-detail {
    position: absolute;
    width: 300px;
    margin: 100px 100px 100px 30px;
}
</style>

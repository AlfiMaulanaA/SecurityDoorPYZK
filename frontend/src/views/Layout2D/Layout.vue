<template>
    <div>
        <div class="d-flex justify-content-between">
            <h5>Shapes for Layout Group</h5>
            Select Shape for Update
            <div class="col-6">
                <select v-model="selectedShapeId" id="shapeSelect" class="form-select form-select-sm">
                    <option value="Select Shape" disabled>Select Shape</option>
                    <option v-for="shape in shapes" :key="shape.id" :value="shape.id">
                        {{ shape.name ? shape.name : 'No Named' }}
                    </option>
                </select>
            </div>
            <router-link :to="`/layoutgroups/${layoutGroupId}/shapes`"
                class="btn btn-sm btn-primary me-1">Shapes</router-link>
            <button @click="goBack" class="btn btn-sm btn-secondary">Back To List</button>
        </div>
        <hr>

        <!-- Buttons to Move Shape -->
        <div v-if="selectedShapeId" class="mb-2">
            <!-- Buttons to Move Shape -->
            <div class=" mb-3">
                <div class="joystick">
                    <!-- Row 1: Rotate Left -->
                    <div>
                        <button class="btn btn-sm btn-primary me-2" @click="rotateShapeAntiClockWise"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-rotate-left"></i> Rotate
                        </button>

                        <button class="btn btn-sm btn-warning me-2" @click="deselectShape">
                            Cancel
                        </button>

                        <button class="btn btn-sm btn-primary" @click="rotateShapeClockWise"
                            :disabled="!selectedShapeId">
                            Rotate <i class="fa-solid fa-rotate-right"></i>
                        </button>
                    </div>

                    <!-- Row 2: Move Up -->
                    <div>
                        <button @click="moveShape('up')" class="btn btn-sm btn-secondary btn-lg"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-up"></i>
                        </button>
                    </div>

                    <!-- Row 3: Move Left, Edit, Move Right -->
                    <div class="d-flex justify-content-center">
                        <button @click="moveShape('left')" class="btn btn-sm btn-secondary btn-lg me-1"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-left"></i>
                        </button>
                        <button class="btn btn-sm btn-success" @click="showModal = true" :disabled="!selectedShapeId">
                            <i class="fa-solid fa-pen-to-square"></i>
                        </button>
                        <button @click="moveShape('right')" class="btn btn-sm btn-secondary btn-lg ms-1"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-right"></i>
                        </button>
                    </div>

                    <!-- Row 4: Move Down -->
                    <div>
                        <button @click="moveShape('down')" class="btn btn-sm btn-secondary btn-lg"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-down"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Edit Shape Modal -->
        <div v-if="showModal" class="modal fade show" tabindex="-1" style="display: block;"
            aria-labelledby="editShapeModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="editShapeModalLabel">Edit Shape</h5>
                        <button type="button" class="btn-close" @click="showModal = false" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="updateShape">
                            <div class="row">
                                <!-- Shape Name -->
                                <div class="col-md-3 mb-2">
                                    <label for="shapeName" class="form-label">Name</label>
                                    <input type="text" id="shapeName" v-model="selectedShape.name"
                                        class="form-control form-control-sm" />
                                </div>

                                <!-- Shape Type -->
                                <div class="col-md-3 mb-2">
                                    <label for="shapeType" class="form-label">Shape Type</label>
                                    <input type="text" id="shapeType" v-model="selectedShape.shapeType"
                                        class="form-control form-control-sm" readonly />
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label for="output" class="form-label">Output Name</label>
                                    <input type="text" id="output" v-model="selectedShape.outputName"
                                        class="form-control form-control-sm" />
                                </div>
                                <div class="col-md-3 mb-2">
                                    <label for="shapeRotation" class="form-label">Rotation</label>
                                    <input type="number" id="shapeRotation" v-model="selectedShape.rotation"
                                        class="form-control form-control-sm" />
                                </div>

                            </div>

                            <div class="row">
                                <!-- Fill Color Checkbox -->
                                <div class="col-md-5 mb-2 ms-4">
                                    <input type="checkbox" id="fillColorCheckbox" v-model="selectedShape.fillColor"
                                        class="form-check-input" />
                                    <label for="fillColorCheckbox" class="form-label">Enable Fill Color</label>
                                </div>

                                <!-- Fill Color -->
                                <div class="col-md-3 mb-2" :class="{ 'd-none': !selectedShape.fillColor }">
                                    <label for="shapeColor" class="form-label">Fill Color</label>
                                    <input type="color" id="shapeColor" v-model="selectedShape.color"
                                        class="form-control form-control-sm" />
                                </div>
                            </div>

                            <div class="row">
                                <!-- Border Checkbox -->
                                <div class="col-md-5 mb-2 ms-4">
                                    <input type="checkbox" id="borderCheckbox" v-model="selectedShape.border"
                                        class="form-check-input" />
                                    <label for="borderCheckbox" class="form-label">Enable Border</label>
                                </div>
                                <!-- Border Color -->
                                <div class="col-md-3 mb-2" :class="{ 'd-none': !selectedShape.border }">
                                    <label for="borderColor" class="form-label">Border Color</label>
                                    <input type="color" id="borderColor" v-model="selectedShape.borderColor"
                                        class="form-control form-control-sm" />
                                </div>

                                <!-- Border Width -->
                                <div class="col-md-3 mb-2" :class="{ 'd-none': !selectedShape.border }">
                                    <label for="borderWidth" class="form-label">Border Width</label>
                                    <input type="number" id="borderWidth" v-model="selectedShape.borderWidth"
                                        class="form-control form-control-sm" />
                                </div>
                            </div>

                            <div class="row">
                                <!-- Width -->
                                <div class="col-md-3 mb-2">
                                    <label for="shapeWidth" class="form-label">Width</label>
                                    <input type="number" id="shapeWidth" v-model="selectedShape.width"
                                        class="form-control form-control-sm" />
                                </div>
                                <!-- Height -->
                                <div class="col-md-3 mb-2">
                                    <label for="shapeHeight" class="form-label">Height</label>
                                    <input type="number" id="shapeHeight" v-model="selectedShape.height"
                                        class="form-control form-control-sm" />
                                </div>
                                <!-- X Position -->
                                <div class="col-md-3 mb-2">
                                    <label for="shapeX" class="form-label">X Position</label>
                                    <input type="number" id="shapeX" v-model="selectedShape.x"
                                        class="form-control form-control-sm" />
                                </div>
                                <!-- Y Position -->
                                <div class="col-md-3 mb-2">
                                    <label for="shapeY" class="form-label">Y Position</label>
                                    <input type="number" id="shapeY" v-model="selectedShape.y"
                                        class="form-control form-control-sm" />
                                </div>
                            </div>

                            <div class="text-end mt-3">
                                <button type="submit" class="btn btn-primary me-2">Save Changes</button>
                                <button type="button" class="btn btn-secondary"
                                    @click="showModal = false">Cancel</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>

        <!-- 
        <div class="d-flex">
            <div class="joystick-container me-4">
                <div class="joystick">
                    <div>
                        <button class="btn btn-sm btn-primary me-2" @click="rotateShapeAntiClockWise"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-rotate-left"></i> Rotate
                        </button>
                        <button class="btn btn-sm btn-warning me-2" @click="deselectShape">
                            Cancel
                        </button>
                        <button class="btn btn-sm btn-primary" @click="rotateShapeClockWise"
                            :disabled="!selectedShapeId">
                            Rotate <i class="fa-solid fa-rotate-right"></i>
                        </button>
                    </div>

                    <div>
                        <button @click="moveShape('up')" class="btn btn-sm btn-secondary btn-lg"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-up"></i>
                        </button>
                    </div>

                    <div class="d-flex justify-content-center">
                        <button @click="moveShape('left')" class="btn btn-sm btn-secondary btn-lg me-1"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-left"></i>
                        </button>
                        <button class="btn btn-sm btn-success" :disabled="!selectedShapeId">
                            <i class="fa-solid fa-pen-to-square"></i>
                        </button>
                        <button @click="moveShape('right')" class="btn btn-sm btn-secondary btn-lg ms-1"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-right"></i>
                        </button>
                    </div>

                    <div>
                        <button @click="moveShape('down')" class="btn btn-sm btn-secondary btn-lg"
                            :disabled="!selectedShapeId">
                            <i class="fa-solid fa-arrow-down"></i>
                        </button>
                    </div>
                </div>
            </div>

            <div class="edit-shape-form">
                <h5>Edit Shape</h5>
                <form @submit.prevent="updateShape">
                    <div class="row">
                        <div class="col-md-3 mb-2">
                            <label for="shapeName" class="form-label">Name</label>
                            <input type="text" id="shapeName" v-model="selectedShape.name"
                                class="form-control form-control-sm" />
                        </div>

                        <div class="col-md-3 mb-2">
                            <label for="shapeType" class="form-label">Shape Type</label>
                            <input type="text" id="shapeType" v-model="selectedShape.shapeType"
                                class="form-control form-control-sm" readonly />
                        </div>
                        <div class="col-md-3 mb-2  d-flex justify-content-center align-items-center text-center">
                            <div>
                                <input type="checkbox" id="fillColorCheckbox" v-model="selectedShape.fillColor"
                                    class="form-check-input" />
                                <label for="fillColorCheckbox" class="form-label">Enable Fill Color</label>
                            </div>
                        </div>

                        <div class="col-md-3 mb-2 d-flex justify-content-center align-items-center text-center">
                            <div>
                                <input type="checkbox" id="borderCheckbox" v-model="selectedShape.border"
                                    class="form-check-input mb-1" />
                                <label for="borderCheckbox" class="form-label">Enable Border</label>
                            </div>
                        </div>

                    </div>
                    <div class="row">
                        <div class="col-md-3 mb-2">
                            <label for="shapeRotation" class="form-label">Rotation</label>
                            <input type="number" id="shapeRotation" v-model="selectedShape.rotation"
                                class="form-control form-control-sm" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <label for="output" class="form-label">Output Name</label>
                            <input type="text" id="output" v-model="selectedShape.outputName"
                                class="form-control form-control-sm" />
                        </div>


                        <div class="col-md-2 mb-2" :class="{ 'd-none': !selectedShape.fillColor }">
                            <label for="shapeColor" class="form-label">Fill Color</label>
                            <input type="color" id="shapeColor" v-model="selectedShape.color"
                                class="form-control form-control-sm" />
                        </div>
                        <div class="col-md-2 mb-2" :class="{ 'd-none': !selectedShape.border }">
                            <label for="borderColor" class="form-label">Border Color</label>
                            <input type="color" id="borderColor" v-model="selectedShape.borderColor"
                                class="form-control form-control-sm" />
                        </div>
                        <div class="col-md-2 mb-2" :class="{ 'd-none': !selectedShape.border }">
                            <label for="borderWidth" class="form-label">Border Width</label>
                            <input type="number" id="borderWidth" v-model="selectedShape.borderWidth"
                                class="form-control form-control-sm" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-3 mb-2">
                            <label for="shapeWidth" class="form-label">Width</label>
                            <input type="number" id="shapeWidth" v-model="selectedShape.width"
                                class="form-control form-control-sm" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <label for="shapeHeight" class="form-label">Height</label>
                            <input type="number" id="shapeHeight" v-model="selectedShape.height"
                                class="form-control form-control-sm" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <label for="shapeX" class="form-label">X Position</label>
                            <input type="number" id="shapeX" v-model="selectedShape.x"
                                class="form-control form-control-sm" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <label for="shapeY" class="form-label">Y Position</label>
                            <input type="number" id="shapeY" v-model="selectedShape.y"
                                class="form-control form-control-sm" />
                        </div>
                    </div>

                    <div class="text-end mt-3">
                        <button type="submit" class="btn btn-sm btn-primary me-2">Save Changes</button>
                        <button type="button" class="btn btn-sm btn-secondary" @click="deselectShape">Cancel</button>
                    </div>
                </form>
            </div>
        </div> -->

        <div class="mt-2">
            <canvas id="shapesCanvas" width="1200" height="1000" class="border" @mousedown="startDrag"
                @mousemove="dragShape" @mouseup="endDrag"></canvas>
        </div>
    </div>
</template>
<script setup>
import { ref, onMounted, watch, computed } from "vue";
import axios from "axios";
import { useRoute, useRouter } from "vue-router";
import Paho from 'paho-mqtt';

const deselectShape = () => {
    selectedShapeId.value = null;
    selectedShape.value = null; // Pastikan shape yang dipilih juga dihapus
};


const selectedShapeId = ref(null);
const selectedShape = ref(null);
const showModal = ref(false);

let dragging = ref(false);
let currentShape = ref(null);
let offsetX = ref(0);
let offsetY = ref(0);


const topics = ref([]);
const mqttConnected = ref(false); // MQTT connection status
const mqttClient = ref(null);

const fetchTopics = async () => {
    try {
        const response = await axios.get('http://localhost:5072/api/topics');
        topics.value = response.data.map((topic) => ({
            ...topic,
            payload: {},
        }));
    } catch (error) {
        console.error("Error fetching topics:", error);
    }
};


const onMessageArrived = (message) => {
    let messageProcessed = false; // Flag to track if the message was processed

    topics.value.forEach((topic) => {
        if (message.destinationName === topic.topicName) {

            try {
                const parsedPayload = JSON.parse(message.payloadString);

                if (parsedPayload.value) {
                    topic.payload = JSON.parse(parsedPayload.value);
                    messageProcessed = true;
                } else {
                    console.warn("Payload does not contain 'value':", parsedPayload);
                }
            } catch (error) {
                console.error("Failed to parse message payloadString:", error);
            }
        }
    });

    if (messageProcessed) {
        updateDataShapes(); // Update shapes data after receiving MQTT data
        drawShapes(); // Redraw the canvas to reflect the new data
    } else {
        console.warn("No matching topic found for message:", message.destinationName);
    }
};

const updateDataShapes = () => {
    shapes.value = shapes.value.map((shape) => {
        const topic = topics.value.find((t) => t.id === shape.topicId);
        const value = topic ? topic.payload[shape.key] : undefined;

        return { ...shape, value: value !== undefined ? value : " " };
    });
};


const connectMqttClient = () => {
    mqttClient.value = new Paho.Client(
        '192.168.0.186', // MQTT Broker IP
        9000,            // MQTT Broker Port
        `client-${Math.random()}`
    );
    mqttClient.value.onMessageArrived = onMessageArrived;

    mqttClient.value.connect({
        onSuccess: () => {
            mqttConnected.value = true;
            subscribeToTopics();
        },
        onFailure: (error) => {
            mqttConnected.value = false;
        },
    });
};

const subscribeToTopics = () => {
    topics.value.forEach((topic) => {
        if (topic.topicName) {
            mqttClient.value.subscribe(topic.topicName);
        }
    });
};

const parseDynamicValues = (payload) => {
    try {
        const parsed = JSON.parse(payload);
        return parsed;
    } catch (error) {
        return {};
    }
};

// Function to draw all shapes, including line
const drawShapes = () => {
    const canvas = document.getElementById('shapesCanvas');
    const ctx = canvas.getContext('2d');

    // Clear the canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Draw all shapes
    shapes.value.forEach(shape => {
        ctx.fillStyle = shape.fillColor ? shape.color : 'transparent';
        ctx.strokeStyle = shape.border ? shape.borderColor : 'transparent';
        ctx.lineWidth = shape.borderWidth;

        ctx.save(); // Save the context before rotating

        // Apply rotation
        ctx.translate(shape.x + shape.width / 2, shape.y + shape.height / 2); // Move the canvas origin to the center of the shape
        ctx.rotate((shape.rotation * Math.PI) / 180); // Apply the rotation in radians
        ctx.translate(-(shape.x + shape.width / 2), -(shape.y + shape.height / 2)); // Reset the origin back

        if (shape.shapeType === 'rec') {
            ctx.fillRect(shape.x, shape.y, shape.width, shape.height);
            ctx.strokeRect(shape.x, shape.y, shape.width, shape.height);
        } else if (shape.shapeType === 'circle') {
            ctx.beginPath();
            ctx.arc(shape.x, shape.y, shape.width / 2, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'line') {
            // For line shape, use x, y, width, and height for positioning
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y); // Start point
            ctx.lineTo(shape.x + shape.width, shape.y + shape.height); // End point
            ctx.stroke();
        } else if (shape.shapeType === 'elbow') {
            // For L-shape (elbow) use two lines to form a corner shape
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y); // Start point (vertical part)
            ctx.lineTo(shape.x, shape.y + shape.height); // Vertical line
            ctx.moveTo(shape.x, shape.y + shape.height);
            ctx.lineTo(shape.x + shape.width, shape.y + shape.height); // Horizontal line
            ctx.stroke();
        } else if (shape.shapeType === 'parallelogram') {
            // Jajar Genjang
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y);
            ctx.lineTo(shape.x + shape.width, shape.y);
            ctx.lineTo(shape.x + shape.width - shape.height * 0.5, shape.y + shape.height);
            ctx.lineTo(shape.x - shape.height * 0.5, shape.y + shape.height);
            ctx.closePath();
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'trapezoid') {
            // Trapesium
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y);
            ctx.lineTo(shape.x + shape.width, shape.y);
            ctx.lineTo(shape.x + shape.width - shape.height * 0.5, shape.y + shape.height);
            ctx.lineTo(shape.x + shape.height * 0.5, shape.y + shape.height);
            ctx.closePath();
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'oval') {
            // Oval (ellipse)
            ctx.beginPath();
            ctx.ellipse(shape.x, shape.y, shape.width / 2, shape.height / 2, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'rhombus') {
            // Belah Ketupat (Rhombus)
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y + shape.height / 2);
            ctx.lineTo(shape.x + shape.width / 2, shape.y);
            ctx.lineTo(shape.x + shape.width, shape.y + shape.height / 2);
            ctx.lineTo(shape.x + shape.width / 2, shape.y + shape.height);
            ctx.closePath();
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'half-circle') {
            // Setengah lingkaran (Half Circle)
            ctx.beginPath();
            ctx.arc(shape.x, shape.y, shape.width / 2, 0, Math.PI); // 180 degrees (half circle)
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'triangle') {
            // Segitiga (Triangle)
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y); // First point
            ctx.lineTo(shape.x + shape.width, shape.y); // Second point
            ctx.lineTo(shape.x + shape.width / 2, shape.y - shape.height); // Third point
            ctx.closePath();
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'quarter-circle') {
            // 1/4 Lingkaran (Quarter Circle)
            ctx.beginPath();
            ctx.arc(shape.x, shape.y, shape.width / 2, 0, Math.PI / 2); // 90 degrees (quarter circle)
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'arrow') {
            // Draw Arrow (One Direction)
            const arrowHeadSize = 10; // Size of arrowhead
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y); // Starting point
            ctx.lineTo(shape.x + shape.width, shape.y + shape.height); // Ending point
            ctx.stroke();

            // Drawing the arrowhead
            const angle = Math.atan2(shape.height, shape.width);
            ctx.beginPath();
            ctx.moveTo(shape.x + shape.width, shape.y + shape.height);
            ctx.lineTo(shape.x + shape.width - arrowHeadSize * Math.cos(angle - Math.PI / 6), shape.y + shape.height - arrowHeadSize * Math.sin(angle - Math.PI / 6));
            ctx.lineTo(shape.x + shape.width - arrowHeadSize * Math.cos(angle + Math.PI / 6), shape.y + shape.height - arrowHeadSize * Math.sin(angle + Math.PI / 6));
            ctx.closePath();
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'double-arrow') {
            // Draw Double Arrow (Two Directions)
            const arrowHeadSize = 10; // Size of arrowhead
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y); // Starting point
            ctx.lineTo(shape.x + shape.width, shape.y + shape.height); // Ending point
            ctx.stroke();

            // Draw arrowhead at the start
            const angleStart = Math.atan2(shape.height, shape.width);
            ctx.beginPath();
            ctx.moveTo(shape.x, shape.y);
            ctx.lineTo(shape.x + arrowHeadSize * Math.cos(angleStart - Math.PI / 6), shape.y + arrowHeadSize * Math.sin(angleStart - Math.PI / 6));
            ctx.lineTo(shape.x + arrowHeadSize * Math.cos(angleStart + Math.PI / 6), shape.y + arrowHeadSize * Math.sin(angleStart + Math.PI / 6));
            ctx.closePath();
            ctx.fill();
            ctx.stroke();

            // Draw arrowhead at the end
            const angleEnd = Math.atan2(shape.height, shape.width);
            ctx.beginPath();
            ctx.moveTo(shape.x + shape.width, shape.y + shape.height);
            ctx.lineTo(shape.x + shape.width - arrowHeadSize * Math.cos(angleEnd - Math.PI / 6), shape.y + shape.height - arrowHeadSize * Math.sin(angleEnd - Math.PI / 6));
            ctx.lineTo(shape.x + shape.width - arrowHeadSize * Math.cos(angleEnd + Math.PI / 6), shape.y + shape.height - arrowHeadSize * Math.sin(angleEnd + Math.PI / 6));
            ctx.closePath();
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'pill') {
            // Drawing pill shape (ellipse with larger width than height)
            ctx.beginPath();
            ctx.ellipse(shape.x + shape.width / 2, shape.y + shape.height / 2, shape.width / 2, shape.height / 2, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
        } else if (shape.shapeType === 'arc') {
            // Drawing an arc (part of a circle)
            ctx.beginPath();
            ctx.arc(shape.x + shape.width / 2, shape.y + shape.height / 2, shape.width / 2, 0, Math.PI);  // 180 degrees arc
            ctx.lineWidth = shape.borderWidth;  // Optional: Adjust the line width
            ctx.stroke();
        } else if (shape.shapeType === 'half-arc') {
            // Drawing a half arc (half of a circle)
            ctx.beginPath();
            ctx.arc(shape.x + shape.width / 2, shape.y + shape.height / 2, shape.width / 2, 0, Math.PI / 2); // 90 degrees arc
            ctx.lineWidth = 5;  // Optional: Adjust the line width
            ctx.stroke();
        }

        const outputName = shape.outputName ?? shape.key ?? " ";
        const value = shape.value ?? " ";
        const text = `${outputName} ${value}`;

        // Add text in the center of the shape
        ctx.font = '16px Arial';
        ctx.fillStyle = 'black';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(shape.name, shape.x + shape.width / 2, shape.y + shape.height / 2 - 20);

        ctx.fillText(text, shape.x + shape.width / 2, shape.y + shape.height / 2);
        ctx.restore(); // Restore the context after drawing the shape
    });
};


watch(selectedShapeId, (newId) => {
    selectedShape.value = shapes.value.find(shape => shape.id === newId);
});

const updateShape = async () => {
    if (!selectedShape.value) return;

    try {
        await axios.put(`http://localhost:5072/shapes/${selectedShape.value.id}`, {
            name: selectedShape.value.name,
            color: selectedShape.value.color,
            borderWidth: selectedShape.value.borderWidth,
            borderColor: selectedShape.value.borderColor,
            rotation: selectedShape.value.rotation,
            outputName: selectedShape.value.outputName,
            width: selectedShape.value.width,
            height: selectedShape.value.height,
            x: selectedShape.value.x,
            y: selectedShape.value.y,
            selectedKey: selectedShape.value.selectedKey,
            layoutGroupId: layoutGroupId,
        });
        drawShapes(); // Re-render the canvas
        fetchShapes();
    } catch (error) {
        console.error('Error updating shape:', error);
    }
};


// Move shape based on direction
const moveShape = async (direction) => {
    const selectedShape = shapes.value.find(shape => shape.id === selectedShapeId.value);

    if (!selectedShape) return;

    // Modify position based on direction
    if (direction === 'left') {
        selectedShape.x -= 10;
    } else if (direction === 'right') {
        selectedShape.x += 10;
    } else if (direction === 'up') {
        selectedShape.y -= 10;
    } else if (direction === 'down') {
        selectedShape.y += 10;
    }

    // Update the canvas
    drawShapes();

    // Save the new position to the backend
    await saveShapePosition(selectedShape);
};

// Rotate shape by 15 degrees
const rotateShapeClockWise = async () => {
    const selectedShape = shapes.value.find(shape => shape.id === selectedShapeId.value);
    if (!selectedShape) return;

    selectedShape.rotation += 15; // Increment the rotation by 15 degrees
    if (selectedShape.rotation >= 360) selectedShape.rotation = 0; // Reset rotation to 0 if it reaches 360

    // Update the canvas
    drawShapes();

    // Save the new rotation to the backend
    await saveShapePosition(selectedShape);
};

// Rotate shape by 15 degrees
const rotateShapeAntiClockWise = async () => {
    const selectedShape = shapes.value.find(shape => shape.id === selectedShapeId.value);
    if (!selectedShape) return;

    selectedShape.rotation -= 15; // Increment the rotation by 15 degrees
    if (selectedShape.rotation >= 360) selectedShape.rotation = 0; // Reset rotation to 0 if it reaches 360

    // Update the canvas
    drawShapes();

    // Save the new rotation to the backend
    await saveShapePosition(selectedShape);
};

const saveShapePosition = async (shape) => {
    // if (shape.shapeType === 'line') {
    //     shape.width = shape.width ?? 100;  
    //     shape.height = shape.height ?? 0;  
    // } else if (shape.shapeType === 'elbow') {
    //     shape.width = shape.width ?? 100;
    //     shape.height = shape.height ?? 100;
    // }

    try {
        await axios.put(`http://localhost:5072/shapes/${shape.id}`, {
            x: shape.x,
            y: shape.y,
            rotation: shape.rotation,
            outputName: shape.outputName,
            width: shape.width,  // Ensure width and height are sent for line
            height: shape.height,
            borderWidth: shape.borderWidth,
            borderColor: shape.borderColor,
            border: shape.border
        });
    } catch (error) {
        console.error('Error saving shape position:', error);
    }
};

// Start dragging shape
const startDrag = (e) => {
    const canvas = document.getElementById('shapesCanvas');
    const mouseX = e.offsetX;
    const mouseY = e.offsetY;

    // Set cursor to "grab"
    canvas.style.cursor = 'grab';

    // Check if mouse is inside any shape (considering rotation)
    shapes.value.forEach(shape => {
        // Get the center of the shape
        const shapeCenterX = shape.x + shape.width / 2;
        const shapeCenterY = shape.y + shape.height / 2;

        // Calculate the rotated coordinates of the mouse relative to the shape's center
        const angle = (shape.rotation * Math.PI) / 180;
        const cosAngle = Math.cos(angle);
        const sinAngle = Math.sin(angle);

        // Translate mouse coordinates to the shape's local coordinate system (centered)
        const localX = cosAngle * (mouseX - shapeCenterX) + sinAngle * (mouseY - shapeCenterY);
        const localY = -sinAngle * (mouseX - shapeCenterX) + cosAngle * (mouseY - shapeCenterY);

        // Check if the mouse is inside the shape (in its local coordinates)
        if (Math.abs(localX) <= shape.width / 2 && Math.abs(localY) <= shape.height / 2) {
            currentShape.value = shape;
            dragging.value = true;
            offsetX.value = mouseX - shape.x;
            offsetY.value = mouseY - shape.y;
        }
    });
};


// Drag the shape
const dragShape = (e) => {
    const canvas = document.getElementById('shapesCanvas');

    if (dragging.value && currentShape.value) {
        const mouseX = e.offsetX;
        const mouseY = e.offsetY;

        // Update the position of the shape
        currentShape.value.x = mouseX - offsetX.value;
        currentShape.value.y = mouseY - offsetY.value;

        drawShapes();

        // Change the cursor to "move" while dragging
        canvas.style.cursor = 'move';
    }
};

// End dragging and save the position
const endDrag = async () => {
    const canvas = document.getElementById('shapesCanvas');

    if (currentShape.value) {
        // Update position on the server
        await saveShapePosition(currentShape.value);
    }

    // Reset the cursor style to default
    canvas.style.cursor = 'default';

    dragging.value = false;
    currentShape.value = null;
};


const route = useRoute();
const router = useRouter();
const layoutGroupId = route.params.id;
const shapes = ref([]);

const fetchShapes = async () => {
    try {
        const response = await axios.get(`http://localhost:5072/layoutgroups/${layoutGroupId}/shapes`);
        shapes.value = response.data.map((shape) => ({
            ...shape,
            dynamicValue: parseDynamicValues(shape.value),
        }));

        console.log("Shapes fetched:", shapes.value);
        drawShapes();
    } catch (error) {
        console.error("Error fetching shapes:", error);
    }
};

onMounted(() => {
    fetchShapes();
    fetchTopics();
    connectMqttClient();
});

const goBack = () => {
    router.push("/layout-groups");
};

</script>
<style scoped>
.joystick {
    display: grid;
    grid-template-rows: auto auto auto auto auto;
    grid-gap: 10px;
    justify-items: center;
}
</style>>
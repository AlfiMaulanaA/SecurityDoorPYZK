<template>
    <div class="content-style mt-2">
        <div class="d-flex justify-content-between">
            <h5 class="mb-2">Layout 2D Dashboard</h5>
            <router-link to="/layout-groups" class="btn">Change Dashboard <i
                    class="fa-solid fa-table-columns"></i></router-link>
        </div>
        <hr>

        <!-- Canvas for Shapes -->
        <div class="dashboard-container mt-2">
            <canvas id="shapesCanvas" width="1000" height="1000" class="border" @mousedown="startDrag"
                @mousemove="dragShape" @mouseup="endDrag"
                :style="{ backgroundImage: `url(${backgroundImage})` }"></canvas>
        </div>

    </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import Paho from 'paho-mqtt';

const selectedShapeId = ref(null);
const selectedShape = ref(null);

let dragging = ref(false);
let currentShape = ref(null);
let offsetX = ref(0);
let offsetY = ref(0);


const topics = ref([]);
const shapes = ref([]); // Renamed savedItems to shapes
const mqttConnected = ref(false); // MQTT connection status
const mqttClient = ref(null);

const apiUrl = `http://localhost:5072/api/topics`;
const shapesUrl = `http://localhost:5072/dashboard/shapes`;

const fetchTopics = async () => {
    try {
        const response = await axios.get(apiUrl);
        topics.value = response.data.map((topic) => ({
            ...topic,
            payload: {},
        }));
    } catch (error) {
        console.error("Error fetching topics:", error);
    }
};

const backgroundImage = ref(""); // URL untuk background image
const loading = ref(true);

const fetchDashboardData = async () => {
    try {
        // Fetch shapes dari endpoint
        const shapesResponse = await axios.get("http://localhost:5072/dashboard/shapes");
        shapes.value = shapesResponse.data;

        // Fetch layout group dengan isUse == true
        const layoutGroupResponse = await axios.get("http://localhost:5072/api/layoutgroups");
        const activeGroup = layoutGroupResponse.data.find((group) => group.isUse);
        console.log("Active Group Data:", activeGroup);

        if (activeGroup && activeGroup.imageData) {
            backgroundImage.value = `data:image/jpeg;base64,${activeGroup.imageData}`;
            console.log('Data Image is available and rendered');
        } else {
            console.warn("No background image data available.");
        }

    } catch (error) {
        console.error("Error fetching dashboard data:", error);
        shapes.value = [];
        backgroundImage.value = "";
    } finally {
        loading.value = false;
    }
};

// const fetchShapes = async () => {
//     try {
//         const response = await axios.get(shapesUrl);
//         shapes.value = response.data.map((shape) => ({
//             ...shape,
//             dynamicValue: parseDynamicValues(shape.value),
//         }));
//         drawShapes();
//     } catch (error) {
//         console.error("Error fetching shapes:", error);
//     }
// };

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


const saveShapePosition = async (shape) => {
    // Ensure line shape has width and height
    if (shape.shapeType === 'line') {
        shape.width = shape.width ?? 100;  // Set default width for line if undefined
        shape.height = shape.height ?? 0;  // Set default height for line to 0
    } else if (shape.shapeType === 'elbow') {
        // Set default width and height for elbow if undefined
        shape.width = shape.width ?? 100;
        shape.height = shape.height ?? 100;
    }

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

onMounted(() => {
    connectMqttClient();
    // fetchShapes();
    fetchDashboardData();
    fetchTopics();
});
</script>

<style scoped>
.container {
    margin: 0 auto;
    text-align: center;
}

/* .dashboard-container {
    position: relative;
    width: 100%;
    height: 1000px;
    background-position: center;
    background-repeat: no-repeat;
    overflow: hidden;
} */

#shapesCanvas {
    position: relative;
    width: 100%;
    height: 1000px;
    /* Atur tinggi sesuai kebutuhan */
    background-size: auto;
    background-position: center;
    background-repeat: no-repeat;
}

canvas {
    width: 100%;
    border: 1px solid #262020;
    cursor: grab;
}

canvas:active {
    cursor: grabbing;
}

.btn-group {
    margin: 10px;
}

i {
    margin-right: 5px;
}

form-label {
    font-weight: bold;
}
</style>

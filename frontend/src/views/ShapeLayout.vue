<template>
    <div class="content-style mt-2">
        <div class="d-flex justify-content-between">
            <h5 class="mb-2">Canvas Shapes</h5>
            <div class="text-center ">
                <button @click="loadShapes" class="btn btn-sm btn-primary btn-lg">Load Shapes</button>
            </div>
        </div>
        <hr>
        <!-- Select Shape Dropdown -->
        <div class="mb-2">
            <label for="shapeSelect" class="form-label">Select Shape to Move</label>
            <select v-model="selectedShapeId" id="shapeSelect" class="form-select form-select-lg">
                <option v-for="shape in shapes" :key="shape.id" :value="shape.id">
                    {{ shape.name }} (ID: {{ shape.id }})
                </option>
            </select>
        </div>

        <!-- Buttons to Move Shape -->
        <div v-if="selectedShapeId" class="text-center mb-2">
            <div class="btn-group" role="group" aria-label="Move shape">
                <button @click="moveShape('left')" class="btn btn-sm btn-secondary btn-lg">
                    Move Left
                </button>
                <button @click="moveShape('right')" class="btn btn-sm btn-secondary btn-lg">
                    Move Right
                </button>
                <button @click="moveShape('up')" class="btn btn-sm btn-secondary btn-lg">
                    Move Up
                </button>
                <button @click="moveShape('down')" class="btn btn-sm btn-secondary btn-lg">
                    Move Down
                </button>
                <button class="btn btn-sm btn-primary" @click="rotateShape">Rotate</button>
            </div>
        </div>

        <!-- Canvas for Shapes -->
        <div class="mt-52">
            <canvas id="shapesCanvas" width="1000" height="1000" class="border" @mousedown="startDrag"
                @mousemove="dragShape" @mouseup="endDrag"></canvas>
        </div>

    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const shapes = ref([]);
const selectedShapeId = ref(null);

let dragging = ref(false);
let currentShape = ref(null);
let offsetX = ref(0);
let offsetY = ref(0);

// Fungsi untuk memuat bentuk
const loadShapes = async () => {
    try {
        const response = await axios.get('http://localhost:5072/shapes');
        shapes.value = response.data;
        drawShapes();
    } catch (error) {
        console.error('Error loading shapes:', error);
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
        }

        // Add text in the center of the shape
        ctx.font = '16px Arial';
        ctx.fillStyle = '#FFFFFF';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(shape.name, shape.x + shape.width / 2, shape.y + shape.height / 2);

        ctx.restore(); // Restore the context after drawing the shape
    });
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
const rotateShape = async () => {
    const selectedShape = shapes.value.find(shape => shape.id === selectedShapeId.value);
    if (!selectedShape) return;

    selectedShape.rotation += 15; // Increment the rotation by 15 degrees
    if (selectedShape.rotation >= 360) selectedShape.rotation = 0; // Reset rotation to 0 if it reaches 360

    // Update the canvas
    drawShapes();

    // Save the new rotation to the backend
    await saveShapePosition(selectedShape);
};

const saveShapePosition = async (shape) => {
    // Ensure line shape has width and height
    if (shape.shapeType === 'line') {
        shape.width = shape.width ?? 100;  // Set default width for line if undefined
        shape.height = shape.height ?? 0;  // Set default height for line to 0
    }

    try {
        await axios.put(`http://localhost:5072/shapes/${shape.id}`, {
            x: shape.x,
            y: shape.y,
            rotation: shape.rotation,
            width: shape.width,  // Ensure width and height are sent for line
            height: shape.height,
            borderWidth: shape.borderWidth,
            borderColor: shape.borderColor,
            border: shape.border
        });
        console.log(`Shape position and rotation saved:`, shape);
    } catch (error) {
        console.error('Error saving shape position:', error);
    }
};


// Start dragging shape
const startDrag = (e) => {
    const canvas = document.getElementById('shapesCanvas');
    const mouseX = e.offsetX;
    const mouseY = e.offsetY;

    // Check if mouse is inside any shape
    shapes.value.forEach(shape => {
        if (
            mouseX > shape.x && mouseX < shape.x + shape.width &&
            mouseY > shape.y && mouseY < shape.y + shape.height
        ) {
            currentShape.value = shape;
            dragging.value = true;
            offsetX.value = mouseX - shape.x;
            offsetY.value = mouseY - shape.y;
        }
    });
};

// Drag the shape
const dragShape = (e) => {
    if (dragging.value && currentShape.value) {
        const mouseX = e.offsetX;
        const mouseY = e.offsetY;

        // Update the position of the shape
        currentShape.value.x = mouseX - offsetX.value;
        currentShape.value.y = mouseY - offsetY.value;

        drawShapes();
    }
};

// End dragging and save the position
const endDrag = async () => {
    if (currentShape.value) {
        // Update position on the server
        await saveShapePosition(currentShape.value);
    }
    dragging.value = false;
    currentShape.value = null;
};

onMounted(() => {
    loadShapes();
});
</script>

<style scoped>
.container {
    max-width: 90%;
    margin: 0 auto;
    text-align: center;
}

canvas {
    border: 1px solid #ccc;
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

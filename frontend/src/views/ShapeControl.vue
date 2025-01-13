<template>
    <div class="mt-2">
        <div class="d-flex justify-content-between">
            <h5 class="mb-2">Manage Shapes</h5>
            <button @click="openModal()" class="btn btn-sm btn-primary mb-2">Add Shape</button>
        </div>
        <hr>
        <!-- Shapes Table -->
        <table class="table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>Name</th>
                    <th>Shape Type</th>
                    <th>Color</th>
                    <th>Rotation</th>
                    <th>Size (w, h)</th>
                    <th>Position (x, y)</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="(shape, i) in shapes" :key="shape.id">
                    <td>{{ 1 + i }}</td>
                    <td>{{ shape.name }}</td>
                    <td>{{ shape.shapeType }}</td>
                    <td>{{ shape.color }}</td>
                    <td>{{ shape.rotation }}</td>
                    <td>{{ shape.width }}, {{ shape.height }}</td>
                    <td>{{ shape.x }}, {{ shape.y }}</td>
                    <td>
                        <button @click="openModal(shape)" class="btn btn-warning btn-sm me-2">Edit</button>
                        <button @click="deleteShape(shape.id)" class="btn btn-danger btn-sm">Delete</button>
                    </td>
                </tr>
            </tbody>
        </table>
        <!-- Modal for Creating/Editing Shape -->
        <div class="modal fade" id="shapeModal" tabindex="-1" aria-labelledby="shapeModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="shapeModalLabel">{{ isEditMode ? 'Edit' : 'Add' }} Shape</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="saveShape">
                            <div class="row">
                                <div class="col-7 mb-3">
                                    <label for="name" class="form-label">Shape Name</label>
                                    <input type="text" id="name" v-model="shapeForm.name" class="form-control"
                                        required />
                                </div>
                                <div class="col-5 mb-3">
                                    <label for="shapeType" class="form-label">Shape Type</label>
                                    <select id="shapeType" v-model="shapeForm.shapeType" class="form-select" required>
                                        <option value="rec">Rectangle</option>
                                        <option value="circle">Circle</option>
                                        <option value="line">Line</option>
                                    </select>
                                </div>
                            </div>

                            <!-- Fill Color and Color only for non-line shapes -->
                            <div v-if="shapeForm.shapeType !== 'line'" class="row">
                                <div class="col-6 mb-3">
                                    <div class="form-check">
                                        <input type="checkbox" id="fillColor" v-model="shapeForm.fillColor"
                                            class="form-check-input" />
                                        <label for="fillColor" class="form-check-label">Fill Color</label>
                                    </div>
                                </div>
                                <div class="col-6 mb-3">
                                    <label for="color" class="form-label">Color</label>
                                    <input type="color" id="color" v-model="shapeForm.color" class="form-control" />
                                </div>
                            </div>

                            <!-- Border Settings (applies to all shapes) -->
                            <div class="row">
                                <div class="col-6 mb-3">
                                    <div class="form-check">
                                        <input type="checkbox" id="border" v-model="shapeForm.border"
                                            class="form-check-input" />
                                        <label for="border" class="form-check-label">Show Border</label>
                                    </div>
                                </div>
                                <div class="col-6 mb-3">
                                    <label for="borderColor" class="form-label">Border Color</label>
                                    <input type="color" id="borderColor" v-model="shapeForm.borderColor"
                                        class="form-control" />
                                </div>

                            </div>
                            <!-- Rotation -->
                            <div class="row">
                                <div class="col-6 mb-3">
                                    <label for="borderWidth" class="form-label">Border Width</label>
                                    <input type="number" id="borderWidth" v-model="shapeForm.borderWidth"
                                        class="form-control" min="1" />
                                </div>
                                <div class="col-6 mb-3">
                                    <label for="rotation" class="form-label">Rotation</label>
                                    <input type="number" id="rotation" v-model="shapeForm.rotation" class="form-control"
                                        min="0" max="360" />
                                </div>
                            </div>

                            <!-- Width and Height (height is hidden for line shapes) -->
                            <div class="row">
                                <div :class="['mb-3', shapeForm.shapeType === 'line' ? 'col-12' : 'col-6']">
                                    <label for="width" class="form-label">Width</label>
                                    <input type="number" id="width" v-model="shapeForm.width" class="form-control"
                                        required />
                                </div>
                                <div v-if="shapeForm.shapeType !== 'line'" class="col-6 mb-3">
                                    <label for="height" class="form-label">Height</label>
                                    <input type="number" id="height" v-model="shapeForm.height" class="form-control"
                                        required />
                                </div>
                            </div>

                            <!-- Position X and Y -->
                            <div class="row">
                                <div class="col-6 mb-3">
                                    <label for="x" class="form-label">Position X</label>
                                    <input type="number" id="x" v-model="shapeForm.x" class="form-control" required />
                                </div>
                                <div class="col-6 mb-3">
                                    <label for="y" class="form-label">Position Y</label>
                                    <input type="number" id="y" v-model="shapeForm.y" class="form-control" required />
                                </div>
                            </div>



                            <button type="submit" class="btn btn-primary">{{ isEditMode ? 'Update' : 'Add' }}
                                Shape</button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const shapes = ref([]);
const shapeForm = ref({
    name: '',
    shapeType: 'rec',
    x: 0,
    y: 0,
    color: '#000000',
    fillColor: true,
    border: true,
    borderColor: '#000000',
    borderWidth: 1,
    width: 0,
    height: 0,
    rotation: 0,
});
const isEditMode = ref(false);
const selectedShape = ref(null);

// Function to load all shapes
const loadShapes = async () => {
    try {
        const response = await axios.get('http://localhost:5072/shapes');
        shapes.value = response.data;
    } catch (error) {
        console.error('Error loading shapes:', error);
    }
};

// Function to open modal for adding or editing shape
const openModal = (shape = null) => {
    if (shape) {
        // Edit existing shape
        selectedShape.value = shape;
        shapeForm.value = { ...shape };
        isEditMode.value = true;
    } else {
        // Add new shape
        isEditMode.value = false;
        shapeForm.value = {
            name: '',
            shapeType: 'rec',
            x: 0,
            y: 0,
            color: '#000000',
            fillColor: true,
            border: true,
            borderColor: '#000000',
            borderWidth: 1,
            width: 0,
            height: 0,
            rotation: 0,
        };
    }
    const modal = new bootstrap.Modal(document.getElementById('shapeModal'));
    modal.show();
};

const saveShape = async () => {
    // Log to check form data
    console.log('Shape Data Before Save:', shapeForm.value);

    // Ensure non-nullable fields are always set
    const shapeData = {
        name: shapeForm.value.name ?? 'Unnamed Shape',  // Default name if undefined
        shapeType: shapeForm.value.shapeType ?? 'rec',  // Default to 'rec' if undefined
        fillColor: shapeForm.value.fillColor ?? true,  // Default to true if undefined
        color: shapeForm.value.color ?? '#000000',  // Default to black if undefined
        border: shapeForm.value.border ?? false,  // Default to false if undefined
        borderColor: shapeForm.value.borderColor ?? '#000000',  // Default border color if undefined
        borderWidth: shapeForm.value.borderWidth ?? 1,  // Default border width if undefined
        width: shapeForm.value.width ?? 0,  // Default width if undefined
        height: shapeForm.value.height ?? 0,  // Default height if undefined
        x: shapeForm.value.x ?? 0,  // Default position X if undefined
        y: shapeForm.value.y ?? 0,  // Default position Y if undefined
        rotation: shapeForm.value.rotation ?? 0,  // Default rotation if undefined
    };

    console.log('Shape Data to be sent:', shapeData);

    try {
        if (isEditMode.value) {
            // Update shape
            await axios.put(`http://localhost:5072/shapes/${selectedShape.value.id}`, shapeData);
        } else {
            // Create new shape
            await axios.post('http://localhost:5072/shapes', shapeData);
        }
        loadShapes(); // Reload shapes
        const modal = new bootstrap.Modal(document.getElementById('shapeModal'));
        modal.hide(); // Hide modal after save
    } catch (error) {
        console.error('Error saving shape:', error);
    }
};


// Function to delete shape
const deleteShape = async (id) => {
    try {
        await axios.delete(`http://localhost:5072/shapes/${id}`);
        loadShapes(); // Reload shapes
    } catch (error) {
        console.error('Error deleting shape:', error);
    }
};

// Load shapes on page mount
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

.form-label {
    font-weight: bold;
}
</style>

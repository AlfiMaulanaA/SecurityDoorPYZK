<template>
    <div>
        <!-- Content -->
        <div class="content-style-primary">
            <div class="d-flex justify-content-between align-items-center">
                <h5>Data Shape Item</h5>
                <span>
                    <span class="me-3" :class="mqttConnected ? 'text-success' : 'text-danger'">
                        {{ mqttConnected ? "Connected" : "Disconnected" }}
                    </span>
                    <router-link to="/shape" class="btn btn-sm btn-secondary me-2">Go To Canvas</router-link>
                    <button class="btn btn-sm btn-primary" data-bs-toggle="modal" data-bs-target="#addKeyModal">
                        Add Item
                    </button>
                </span>
            </div>
            <hr />
            <div class="d-flex justify-content-between align-items-center mb-3">
                <input type="text" v-model="searchQuery" @input="searchItems" class="form-control form-control-sm w-25"
                    placeholder="Search..." />
            </div>

            <div class="mb-3">
                <h5>Select Shape Template</h5>
                <div class="d-flex flex-wrap gap-2">
                    <button v-for="template in templateShape" :key="template.id" class="btn btn-sm btn-outline-primary"
                        @click="addShapeFromTemplate(template)">
                        {{ template.name }} ({{ template.shapeType }})
                    </button>
                </div>
            </div>

            <div v-if="paginatedItems.length">
                <table class="table">
                    <thead>
                        <tr>
                            <th>#</th>
                            <th>Name</th>
                            <th>Topic Name</th>
                            <th>Key</th>
                            <th>Value</th>
                            <th>Shape Type</th>
                            <th>Fill Color</th>
                            <th>Color</th>
                            <th>Border</th>
                            <th>B. Color</th>
                            <th>B. Width</th>
                            <th>W, H</th>
                            <th>X, Y</th>
                            <th class="text-end">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(item, index) in paginatedItems" :key="index">
                            <td>{{ index + 1 + (currentPage - 1) * itemsPerPage }}</td>
                            <td>{{ item.name }}</td>
                            <td>{{ item.topicName }}</td>
                            <td class="text-secondary" style="font-style: italic">
                                `{{ item.key }}`
                            </td>
                            <td>
                                {{ item.value !== undefined ? item.value : "None" }}
                            </td>
                            <td>{{ item.shapeType }}</td>
                            <td>{{ item.fillColor ? 'Yes' : 'No' }}</td>
                            <td>{{ item.color }}</td>
                            <td>{{ item.border ? 'Yes' : 'No' }}</td>
                            <td>{{ item.borderColor }}</td>
                            <td>{{ item.borderWidth }}</td>
                            <td>{{ item.width }}, {{ item.height }}</td>
                            <td>{{ item.x }}, {{ item.y }}</td>

                            <td class="text-end">
                                <i class="fas fa-edit text-primary me-2" @click="showEditModal(item)"></i>
                                <i class="fas fa-trash text-danger" @click="confirmDelete(item.id)"></i>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div v-else>
                <p>No item selected.</p>
            </div>

            <!-- Pagination -->
            <nav aria-label="Page navigation">
                <ul class="pagination">
                    <li class="page-item" :class="{ disabled: currentPage === 1 }">
                        <a class="page-link" href="#" @click.prevent="prevPage" aria-label="Previous">
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
        </div>

        <!-- Add Key Modal -->
        <div class="modal fade" id="addKeyModal" tabindex="-1" aria-labelledby="addKeyModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addKeyModalLabel">Add Key</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="form-group col-6">
                                <label for="topicSelect">Select Device</label>
                                <select v-model="newItem.topicId" class="form-control" @change="onTopicChange">
                                    <option value="" disabled>Select a topic</option>
                                    <option v-for="topic in topics" :key="topic.id" :value="topic.id">
                                        {{ topic.name }}
                                    </option>
                                </select>
                            </div>
                            <div class="form-group col-6" v-if="selectedTopic">
                                <label for="keySelect">Select Data</label>
                                <select v-model="newItem.key" class="form-control" @change="updateSelectedValue">
                                    <option value="" disabled>Select a key</option>
                                    <option v-for="(value, key) in flattenedPayload" :key="key" :value="key">
                                        {{ key }}
                                    </option>
                                </select>
                            </div>
                        </div>


                        <!-- Additional Fields -->
                        <div class="row">
                            <div class="col-6">
                                <label for="">Output Name</label>
                                <input type="text" v-model="newItem.OutputName" class="form-control">
                            </div>
                            <div class=" col-6">
                                <label for="shapeType">Shape Type</label>
                                <select v-model="newItem.shapeType" class="form-control" required>
                                    <option value="" disabled>Select shape type</option>
                                    <option value="line">Line</option>
                                    <option value="elbow">Elbow</option>
                                    <option value="arrow">Arrow</option>
                                    <option value="double-arrow">Double Arrow</option>
                                    <option value="rec">Rectangle</option>
                                    <option value="circle">Circle</option>
                                    <option value="half-circle">Half Circle</option>
                                    <option value="quarter-circle">Quarter Circle</option>
                                    <option value="oval">Oval (Ellipse)</option>
                                    <option value="pill">Pill (Ellipse)</option>
                                    <option value="arc">Arc</option>
                                    <option value="half-arc">Half Arc</option>
                                    <option value="triangle">Triangle</option>
                                    <option value="parallelogram">Parallelogram (Jajar Genjang)</option>
                                    <option value="trapezoid">Trapezoid (Trapesium)</option>
                                    <option value="rhombus">Rhombus (Belah Ketupat)</option>
                                </select>
                            </div>
                        </div>

                        <div class="d-flex m-4">
                            <div class="col-6 me-3">
                                <input type="checkbox" v-model="newItem.fillColor" class="form-check-input" />
                                <label for="fillColor">Fill Color</label>
                            </div>

                            <div class="col-6 me-3">
                                <input type="checkbox" v-model="newItem.border" class="form-check-input" />
                                <label for="fillColor">Fill Border</label>
                            </div>
                        </div>

                        <!-- Conditional rendering for Fill Color and Border -->
                        <div v-if="newItem.fillColor" class="row">
                            <div class="form-group col-6">
                                <label for="color">Fill Color</label>
                                <input type="color" v-model="newItem.color" class="form-control" />
                            </div>
                        </div>

                        <div v-if="newItem.border" class="row">
                            <div class="form-group col-6">
                                <label for="borderColor">Border Color</label>
                                <input type="color" v-model="newItem.borderColor" class="form-control" />
                            </div>
                            <div class="form-group col-6">
                                <label for="borderWidth">Border Width</label>
                                <input type="number" v-model="newItem.borderWidth" class="form-control" />
                            </div>
                        </div>

                        <!-- Shape dimensions and position -->
                        <div class="row">
                            <div class="form-group col-6">
                                <label for="width">Width</label>
                                <input type="number" v-model="newItem.width" class="form-control" />
                            </div>
                            <div class="form-group col-6">
                                <label for="height">Height</label>
                                <input type="number" v-model="newItem.height" class="form-control" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="form-group col-6">
                                <label for="x">X Position</label>
                                <input type="number" v-model="newItem.x" class="form-control" />
                            </div>
                            <div class="form-group col-6">
                                <label for="y">Y Position</label>
                                <input type="number" v-model="newItem.y" class="form-control" />
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            Close
                        </button>
                        <button type="button" class="btn btn-primary" @click="saveNewItem" data-bs-dismiss="modal">
                            Save Selected Item
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Edit Key Modal -->
        <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="editModalLabel">Edit Key</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="form-group col-6">
                                <label for="editTopicSelect">Select Device</label>
                                <select v-model="selectedItem.topicId" class="form-control" @change="onEditTopicChange">
                                    <option v-for="topic in topics" :key="topic.id" :value="topic.id">
                                        {{ topic.name }}
                                    </option>
                                </select>
                            </div>
                            <div class="form-group col-6" v-if="selectedTopic">
                                <label for="keySelect">Select Data</label>
                                <select v-model="selectedItem.key" class="form-control"
                                    @change="updateEditSelectedValue">
                                    <option v-for="(value, key) in flattenedPayload" :key="key" :value="key">
                                        {{ key }}
                                    </option>
                                </select>
                            </div>
                        </div>

                        <!-- Additional Fields -->
                        <div class="row">
                            <div class="col-6">
                                <label for="shapeType">Shape Type</label>
                                <select v-model="selectedItem.shapeType" class="form-control" required>
                                    <option value="" disabled>Select shape type</option>
                                    <option value="line">Line</option>
                                    <option value="elbow">Elbow</option>
                                    <option value="arrow">Arrow</option>
                                    <option value="double-arrow">Double Arrow</option>
                                    <option value="rec">Rectangle</option>
                                    <option value="circle">Circle</option>
                                    <option value="half-circle">Half Circle</option>
                                    <option value="quarter-circle">Quarter Circle</option>
                                    <option value="oval">Oval (Ellipse)</option>
                                    <option value="pill">Pill (Ellipse)</option>
                                    <option value="arc">Arc</option>
                                    <option value="half-arc">Half Arc</option>
                                    <option value="triangle">Triangle</option>
                                    <option value="parallelogram">Parallelogram (Jajar Genjang)</option>
                                    <option value="trapezoid">Trapezoid (Trapesium)</option>
                                    <option value="rhombus">Rhombus (Belah Ketupat)</option>
                                </select>
                            </div>
                            <div class="col-2 me-3">
                                <input type="checkbox" v-model="selectedItem.fillColor" class="form-check-input" />
                                <label for="fillColor">Fill Color</label>
                            </div>

                            <div class="col-2 me-3">
                                <input type="checkbox" v-model="selectedItem.border" class="form-check-input" />
                                <label for="fillColor">Fill Border</label>
                            </div>
                        </div>

                        <!-- Conditional rendering for Fill Color and Border -->
                        <div v-if="selectedItem.fillColor" class="row">
                            <div class="form-group col-6">
                                <label for="color">Fill Color</label>
                                <input type="color" v-model="selectedItem.color" class="form-control" />
                            </div>
                        </div>

                        <div v-if="selectedItem.border" class="row">
                            <div class="form-group col-6">
                                <label for="borderColor">Border Color</label>
                                <input type="color" v-model="selectedItem.borderColor" class="form-control" />
                            </div>
                            <div class="form-group col-6">
                                <label for="borderWidth">Border Width</label>
                                <input type="number" v-model="selectedItem.borderWidth" class="form-control" />
                            </div>
                        </div>

                        <!-- Shape dimensions and position -->
                        <div class="row">
                            <div class="form-group col-6">
                                <label for="width">Width</label>
                                <input type="number" v-model="selectedItem.width" class="form-control" />
                            </div>
                            <div class="form-group col-6">
                                <label for="height">Height</label>
                                <input type="number" v-model="selectedItem.height" class="form-control" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="form-group col-6">
                                <label for="x">X Position</label>
                                <input type="number" v-model="selectedItem.x" class="form-control" />
                            </div>
                            <div class="form-group col-6">
                                <label for="y">Y Position</label>
                                <input type="number" v-model="selectedItem.y" class="form-control" />
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            Close
                        </button>
                        <button type="button" class="btn btn-primary" @click="updateSelectedItem"
                            data-bs-dismiss="modal">
                            Update Selected Item
                        </button>
                    </div>
                </div>
            </div>
        </div>

    </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from "vue";
import axios from "axios";
import Paho from "paho-mqtt";
import Swal from "sweetalert2";

const templateShape = ref([
    {
        id: 1,
        name: "Line",
        shapeType: "line",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 1,
        width: 100,
        height: 0,
        x: 50,
        y: 50,
    },
    {
        id: 2,
        name: "Elbow",
        shapeType: "elbow",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "#FF0000", // Red border
        borderWidth: 2,
        width: 100,
        height: 50,
        x: 100,
        y: 100,
    },
    {
        id: 3,
        name: "Arrow",
        shapeType: "arrow",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "#0000FF", // Blue border
        borderWidth: 2,
        width: 120,
        height: 50,
        x: 150,
        y: 150,
    },
    {
        id: 4,
        name: "Double Arrow",
        shapeType: "double-arrow",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "#00FF00", // Green border
        borderWidth: 2,
        width: 150,
        height: 50,
        x: 200,
        y: 200,
    },
    {
        id: 5,
        name: "Rectangle",
        shapeType: "rec",
        fillColor: true,
        color: "#FFA500", // Orange fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 100,
        height: 50,
        x: 250,
        y: 250,
    },
    {
        id: 6,
        name: "Circle",
        shapeType: "circle",
        fillColor: true,
        color: "#4682B4", // SteelBlue color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 3,
        width: 60,
        height: 60,
        x: 300,
        y: 300,
    },
    {
        id: 7,
        name: "Half Circle",
        shapeType: "half-circle",
        fillColor: true,
        color: "#DC143C", // Crimson fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 80,
        height: 40,
        x: 350,
        y: 350,
    },
    {
        id: 8,
        name: "Quarter Circle",
        shapeType: "quarter-circle",
        fillColor: true,
        color: "#FFD700", // Gold fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 40,
        height: 40,
        x: 400,
        y: 400,
    },
    {
        id: 9,
        name: "Oval",
        shapeType: "oval",
        fillColor: true,
        color: "#A52A2A", // Brown fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 150,
        height: 80,
        x: 450,
        y: 450,
    },
    {
        id: 10,
        name: "Pill",
        shapeType: "pill",
        fillColor: true,
        color: "#40E0D0", // Turquoise fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 150,
        height: 60,
        x: 500,
        y: 500,
    },
    {
        id: 11,
        name: "Arc",
        shapeType: "arc",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "#FF6347", // Tomato border
        borderWidth: 2,
        width: 100,
        height: 50,
        x: 550,
        y: 550,
    },
    {
        id: 12,
        name: "Half Arc",
        shapeType: "half-arc",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "#8A2BE2", // BlueViolet border
        borderWidth: 2,
        width: 80,
        height: 40,
        x: 600,
        y: 600,
    },
    {
        id: 13,
        name: "Triangle",
        shapeType: "triangle",
        fillColor: true,
        color: "#FF4500", // OrangeRed fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 100,
        height: 100,
        x: 650,
        y: 650,
    },
    {
        id: 14,
        name: "Parallelogram",
        shapeType: "parallelogram",
        fillColor: true,
        color: "#FF69B4", // HotPink fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 120,
        height: 60,
        x: 700,
        y: 700,
    },
    {
        id: 15,
        name: "Trapezoid",
        shapeType: "trapezoid",
        fillColor: true,
        color: "#4169E1", // RoyalBlue fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 2,
        width: 130,
        height: 80,
        x: 750,
        y: 750,
    },
    {
        id: 16,
        name: "Rhombus",
        shapeType: "rhombus",
        fillColor: true,
        color: "#DC143C", // Crimson fill color
        border: true,
        borderColor: "#000000", // Black border
        borderWidth: 3,
        width: 100,
        height: 120,
        x: 800,
        y: 800,
    },
]);



const addShapeFromTemplate = async (template) => {
    const result = await Swal.fire({
        title: `Add ${template.name}?`,
        text: "Do you want to save this shape to the database?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, add it!",
    });

    if (result.isConfirmed) {
        const newShape = { ...template };
        delete newShape.id;
        newShape.name;

        try {
            await axios.post(selectedItemUrl, newShape);

            Swal.fire("Success", `${template.name} has been added!`, "success");
            await fetchShapes();
        } catch (error) {
            console.error("Error adding shape:", error);
            Swal.fire("Error", `Failed to add ${template.name}.`, "error");
        }
    }
};


const topics = ref([]);
const selectedTopic = ref(null);
const mqttConnected = ref(false); // MQTT connection status

const newItem = ref({
    topicId: null,
    key: null,
    name: "",
    topicName: "",
    OutputName: "",
    shapeType: "",
    fillColor: false,
    color: "",
    border: true,
    borderColor: "",
    borderWidth: null,
    width: null,
    rotation: null,
    height: null,
    x: null,
    y: null,
});

const selectedItem = ref({
    topicId: null,
    key: null,
    name: "",
    topicName: "",
    OutputName: "",
    shapeType: "",
    fillColor: false,
    color: "",
    border: true,
    borderColor: "",
    borderWidth: null,
    width: null,
    rotation: null,
    height: null,
    x: null,
    y: null,
});

const savedItems = ref([]);
const mqttClient = ref(null);

const apiUrl = `http://localhost:5072/api/topics`;
const selectedItemUrl = `http://localhost:5072/shapes`;


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

const saveNewItem = async () => {
    try {
        const valueAsString =
            newItem.value.value !== undefined ? String(newItem.value.value) : ""; // Ensure this line is present

        const response = await axios.post(
            selectedItemUrl,
            {
                topicId: newItem.value.topicId,
                name: newItem.value.name,
                topicName: newItem.value.topicName,
                OutputName: newItem.value.OutputName,
                key: newItem.value.key,
                value: valueAsString,
                shapeType: newItem.value.shapeType,
                fillColor: newItem.value.fillColor,
                color: newItem.value.color,
                border: newItem.value.border,
                borderColor: newItem.value.borderColor,
                borderWidth: newItem.value.borderWidth,
                width: newItem.value.width,
                rotation: newItem.value.rotation,
                height: newItem.value.height,
                x: newItem.value.x,
                y: newItem.value.y

            }
        );

        // Handle success (e.g., show a success message or refresh data)
        Swal.fire("Success", "New item saved successfully!", "success");

        await fetchShapes();
        resetNewItem(); // Reset the newItem to clear the form
    } catch (error) {
        console.error("Error saving new item:", error);
        Swal.fire("Error", error.message, "error");
    }
};

const fetchShapes = async () => {
    try {
        const response = await axios.get(selectedItemUrl);
        savedItems.value = response.data.map((item) => ({
            ...item,
            dynamicValue: parseDynamicValues(item.value),
        }));
    } catch (error) {
        console.error("Error fetching saved items:", error);
    }
};

const updateSelectedItem = async () => {
    try {
        // Convert the value to string, handling boolean values correctly
        const valueAsString =
            selectedItem.value.value !== undefined
                ? String(selectedItem.value.value)
                : ""; // Ensure this line is present

        // Prepare the payload for the PUT request
        const payload = {
            id: selectedItem.value.id,
            topicId: selectedItem.value.topicId,
            topicName: selectedItem.value.topicName,
            OutputName: selectedItem.value.OutputName,
            key: selectedItem.value.key,
            value: valueAsString,
            shapeType: selectedItem.value.shapeType,
            fillColor: selectedItem.value.fillColor,
            color: selectedItem.value.color,
            border: selectedItem.value.border,
            borderColor: selectedItem.value.borderColor,
            borderWidth: selectedItem.value.borderWidth,
            width: selectedItem.value.width,
            rotation: selectedItem.value.rotation,
            height: selectedItem.value.height,
            x: selectedItem.value.x,
            y: selectedItem.value.y
        };

        // Make the PUT request to update the selected item
        await axios.put(`${selectedItemUrl}/${selectedItem.value.id}`, payload);

        // Refresh saved items after successful update
        await fetchShapes();

        // Notify success
        Swal.fire({
            title: "Good job!",
            text: "Selected item successfully updated!",
            icon: "success",
            confirmButtonColor: "#3085d6",
        });
    } catch (error) {
        console.error("Error updating selected item:", error);
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Failed to update selected item.",
        });
    }
};

const confirmDelete = async (id) => {
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
        deleteSelectedItem(id);
    }
};

const deleteSelectedItem = async (id) => {
    try {
        await axios.delete(`${selectedItemUrl}/${id}`);
        savedItems.value = savedItems.value.filter((item) => item.id !== id);
        Swal.fire("Deleted!", "The selected item has been deleted.", "success");
        fetchShapes();
    } catch (error) {
        console.error("Error deleting selected item:", error);
    }
};

const resetNewItem = () => {
    newItem.value = {

        topicId: null,
        key: null,
        name: "",
        topicName: "",
        OutputName: "",
        shapeType: "",
        fillColor: false,
        color: "",
        border: true,
        borderColor: "",
        borderWidth: null,
        width: null,
        rotation: null,
        height: null,
        x: null,
        y: null,
    };
};

const onMessageArrived = (message) => {

    let messageProcessed = false; // Flag to track if the message was processed

    topics.value.forEach((topic) => {
        if (message.destinationName === topic.topicName) {

            try {
                const parsedPayload = JSON.parse(message.payloadString);

                if (parsedPayload.value) {
                    // Ambil hanya 'value' dari payload
                    topic.payload = JSON.parse(parsedPayload.value);

                    messageProcessed = true; // Mark as processed
                } else {
                    console.warn("Payload does not contain 'value':", parsedPayload);
                }
            } catch (error) {
                console.error("Failed to parse message payloadString:", error);
            }
        }
    });

    // Perbarui nilai terkait topik yang dipilih
    if (selectedTopic.value) {
        updateSelectedValue();
        fetchShapes();
    }
};

const onTopicChange = async () => {
    const topic = topics.value.find((t) => t.id === newItem.value.topicId);
    if (topic) {
        selectedTopic.value = topic;
        newItem.value.topicName = topic.name;
        newItem.value.key = Object.keys(topic.payload || {})[0] || null; // Pilih key pertama dari payload
        updateSelectedValue();
    } else {
        // Reset nilai jika topik tidak ditemukan
        newItem.value.key = null;
        newItem.value.topicName = "";
    }
};

// Untuk flatten payload (key-value dari payload)
const flattenedPayload = computed(() => {
    return selectedTopic.value?.payload || {};
});

const onEditTopicChange = async () => {
    const topic = topics.value.find((t) => t.id === selectedItem.value.topicId);
    if (topic) {
        selectedTopic.value = topic;
        selectedItem.value.key = Object.keys(topic.payload)[0] || null;
        updateEditSelectedValue();
    } else {
        selectedItem.value.key = null;
        selectedItem.value.value = "";
    }
};

const updateSelectedValue = () => {
    if (newItem.value.key) {
        newItem.value.value = selectedTopic.value.payload[newItem.value.key];
    }
};

const updateEditSelectedValue = () => {
    if (selectedItem.value.key) {
        selectedItem.value.value =
            selectedTopic.value.payload[selectedItem.value.key];
    }
};

const showEditModal = (item) => {
    selectedItem.value = { ...item };
    onEditTopicChange();
    const modal = new bootstrap.Modal(document.getElementById("editModal"));
    modal.show();
};

const subscribeToTopics = () => {
    topics.value.forEach((topic) => {
        if (topic.topicName) {
            mqttClient.value.subscribe(topic.topicName);
        }
    });
};

const getComputedValue = (value, factor) => {
    if (typeof value === "boolean") {
        return value ? "True" : "False";
    }

    const val = parseFloat(value);
    const fac = parseFloat(factor);

    if (!isNaN(val) && !isNaN(fac)) {
        return (val * fac).toFixed(2);
    }

    return "";
};

const updateSavedItems = () => {

    savedItems.value = savedItems.value.map((item) => {
        const topic = topics.value.find((t) => t.id === item.topicId);
        const value = topic ? topic.payload[item.key] : undefined;

        console.log("Updated item:", {
            ...item,
            value: (value !== undefined ? value : "").toString(),
        });

        return { ...item, value: (value !== undefined ? value : "").toString() };
    });

};

const connectMqttClient = () => {
    mqttClient.value = new Paho.Client(
        '192.168.0.146',
        Number(9000),
        `client-${Math.random()}`
    );
    mqttClient.value.onMessageArrived = onMessageArrived;

    mqttClient.value.connect({
        onSuccess: () => {
            mqttConnected.value = true; // Set connection status to true
            subscribeToTopics();
        },
        onFailure: (error) => {
            mqttConnected.value = false; // Set connection status to false
            console.error("Failed to connect to MQTT broker:", error);
        },
    });
};

watch([newItem.value, selectedItem.value], () => {
    updateSelectedValue();
    updateEditSelectedValue();
});

const flattenObject = (obj, parent = "", res = {}) => {
    for (let key in obj) {
        let propName = parent ? `${parent}.${key}` : key;
        if (typeof obj[key] == "object" && !Array.isArray(obj[key])) {
            flattenObject(obj[key], propName, res);
        } else {
            res[propName] = obj[key];
        }
    }
    return res;
};

const currentPage = ref(1);
const itemsPerPage = ref(10);
const totalPages = computed(() => {
    return Math.ceil(filteredItems.value.length / itemsPerPage.value);
});

const paginatedItems = computed(() => {
    const start = (currentPage.value - 1) * itemsPerPage.value;
    const end = start + itemsPerPage.value;
    return filteredItems.value.slice(start, end);
});

const nextPage = () => {
    if (currentPage.value < totalPages.value) {
        currentPage.value++;
    }
};

const prevPage = () => {
    if (currentPage.value > 1) {
        currentPage.value--;
    }
};

const searchQuery = ref("");
const filteredItems = computed(() => {
    if (!searchQuery.value) {
        return savedItems.value;
    }
    return savedItems.value.filter(
        (item) =>
            item.topicName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
            item.customName.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
            item.key.toLowerCase().includes(searchQuery.value.toLowerCase())
    );
});

const searchItems = () => {
    currentPage.value = 1;
};



const parseDynamicValues = (payload) => {
    try {
        const parsed = JSON.parse(payload);
        return parsed;
    } catch (error) {
        return {};
    }
};
import { useRoute } from "vue-router";

const route = useRoute();
const selectedDevice = ref(null);
onMounted(async () => {
    selectedDevice.value = route.query.selectedDevice;

    // Jika ada `selectedDevice`, buka modal edit
    if (selectedDevice.value) {
        const itemToEdit = savedItems.value.find(
            (item) => item.id === parseInt(selectedDevice.value)
        );

        if (itemToEdit) {
            showEditModal(itemToEdit);
        } else {
            console.warn(
                "No matching item found for selectedDevice:",
                selectedDevice.value
            );
        }
    }
    await fetchTopics();
    await fetchShapes();
    connectMqttClient();
});
</script>

<style scoped>
.loading-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(128, 128, 128, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
}

.loading-card {
    background: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    text-align: center;
}

.spinner-border {
    margin-bottom: 10px;
}
</style>

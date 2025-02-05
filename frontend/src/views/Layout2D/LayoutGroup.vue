<template>
    <div>
        <div class="d-flex justify-content-between">
            <h5>Layout Groups</h5>
            <button @click="showCreateModal" class="btn btn-sm btn-primary mb-3">Add Layout Group</button>
        </div>
        <table class="table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>Name</th>
                    <th class="text-center">Image</th>
                    <th class="text-center">Is Active</th>
                    <th class="text-end">View</th>
                    <th class="text-end">Actions</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="group in layoutGroups" :key="group.id">
                    <td>{{ group.id }}</td>
                    <td>
                        <router-link to="/dashboard" class="btn">{{ group.name }}</router-link>
                    </td>
                    <td class="text-center">
                        <img v-if="group.imageUrl" :src="group.imageUrl" alt="Group Image" class="img-thumbnail"
                            style="width: 80px; height: 80px; object-fit: cover; cursor: pointer;"
                            @click="previewImage(group.imageUrl)" />
                        <button v-else class="btn btn-sm" @click="showImageUploadModal(group)">
                            Upload <i class="fa-regular fa-images"></i>
                        </button>
                    </td>

                    <td class="text-center">
                        <input type="checkbox" :checked="group.isUse" @change="toggleIsUse(group)"
                            title="Toggle Active Status" />
                    </td>
                    <td class="text-end">
                        <router-link :to="`/layout-groups/${group.id}`" class="btn btn-sm btn-success me-2">Layout
                        </router-link>
                        <router-link :to="`/layoutgroups/${group.id}/shapes`" class="btn btn-sm btn-primary me-2">Shapes
                        </router-link>
                    </td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-warning me-2" @click="showEditModal(group)">Edit</button>
                        <button class="btn btn-sm btn-danger" @click="confirmDelete(group.id)">Delete</button>
                    </td>
                </tr>
            </tbody>
        </table>

        <!-- Modal for Add/Edit Layout Group -->
        <div v-if="showModal" class="modal fade show" tabindex="-1" style="display: block;">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">{{ isEditing ? 'Edit Layout Group' : 'Add Layout Group' }}</h5>
                        <button type="button" class="btn-close" @click="closeModal"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="isEditing ? updateLayoutGroup() : createLayoutGroup()">
                            <div class="mb-3">
                                <label for="name" class="form-label">Name</label>
                                <input type="text" id="name" v-model="form.name" class="form-control" required />
                            </div>
                            <div class="text-end">
                                <button type="submit" class="btn btn-sm btn-primary me-2">
                                    {{ isEditing ? 'Update' : 'Add' }}
                                </button>
                                <button type="button" class="btn btn-sm btn-secondary"
                                    @click="closeModal">Cancel</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal for Upload Image -->
        <div v-if="showImageModal" class="modal fade show" tabindex="-1" style="display: block;">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Upload Image</h5>
                        <button type="button" class="btn-close" @click="closeImageModal"></button>
                    </div>
                    <div class="modal-body">
                        <form @submit.prevent="uploadImage()">
                            <div class="mb-3">
                                <label for="image" class="form-label">Select Image</label>
                                <input type="file" id="image" class="form-control" @change="onImageChange"
                                    accept="image/png, image/jpeg" required />
                            </div>
                            <div class="text-end">
                                <button type="submit" class="btn btn-sm btn-primary me-2">Upload</button>
                                <button type="button" class="btn btn-sm btn-secondary"
                                    @click="closeImageModal">Cancel</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal for Preview Image -->
        <div v-if="showPreviewModal" class="modal fade show" tabindex="-1" style="display: block;">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Preview Image</h5>
                        <button type="button" class="btn-close" @click="closePreviewModal"></button>
                    </div>
                    <div class="modal-body text-center">
                        <img :src="previewImageUrl" alt="Preview Image" class="img-fluid" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>


<script setup>
import { ref, onMounted } from 'vue';
import Swal from 'sweetalert2';
import axios from 'axios';
import { useRouter } from 'vue-router';

const router = useRouter();
const layoutGroups = ref([]);
const showModal = ref(false);
const showImageModal = ref(false);
const selectedGroup = ref(null);
const isEditing = ref(false);
const imageFile = ref(null);
const form = ref({
    id: null,
    name: '',
});

const showPreviewModal = ref(false); // State untuk modal preview
const previewImageUrl = ref(''); // URL gambar yang akan di-preview

// Fungsi untuk menampilkan modal preview
const previewImage = (url) => {
    previewImageUrl.value = url;
    showPreviewModal.value = true;
};

// Fungsi untuk menutup modal preview
const closePreviewModal = () => {
    previewImageUrl.value = '';
    showPreviewModal.value = false;
};


const fetchLayoutGroups = async () => {
    try {
        const response = await axios.get('http://localhost:5072/api/layoutgroups');
        layoutGroups.value = response.data.map((group) => ({
            ...group,
            imageUrl: group.imageData ? `data:image/jpeg;base64,${group.imageData}` : null,
        }));
    } catch (error) {
        console.error('Error fetching layout groups:', error);
    }
};

const showImageUploadModal = (group) => {
    selectedGroup.value = group;
    showImageModal.value = true;
};

const closeImageModal = () => {
    selectedGroup.value = null;
    imageFile.value = null;
    showImageModal.value = false;
};

const onImageChange = (event) => {
    imageFile.value = event.target.files[0];
};

const uploadImage = async () => {
    if (!imageFile.value || !selectedGroup.value) return;

    const formData = new FormData();
    formData.append('file', imageFile.value);

    try {
        await axios.post(`http://localhost:5072/api/layoutgroups/${selectedGroup.value.id}/upload-image`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });

        Swal.fire('Success', 'Image uploaded successfully!', 'success');
        closeImageModal();
        fetchLayoutGroups();
    } catch (error) {
        console.error('Error uploading image:', error);
        Swal.fire('Error', 'Failed to upload image!', 'error');
    }
};


const showCreateModal = () => {
    isEditing.value = false;
    form.value = { id: null, name: '' };
    showModal.value = true;
};

const showEditModal = (group) => {
    isEditing.value = true;
    form.value = { id: group.id, name: group.name };
    showModal.value = true;
};

const closeModal = () => {
    showModal.value = false;
    form.value = { id: null, name: '' };
};

const toggleIsUse = async (group) => {
    if (group.isUse) {
        // If unchecked, allow turning it off directly
        await updateIsUse(group.id, false);
        group.isUse = false;
    } else {
        // Ensure only one is true
        const activeGroup = layoutGroups.value.find((g) => g.isUse);
        if (activeGroup) {
            // Set the previous active group to false
            await updateIsUse(activeGroup.id, false);
            activeGroup.isUse = false;
        }

        // Activate the new group
        await updateIsUse(group.id, true);
        group.isUse = true;
    }
};

const updateIsUse = async (id, isUse) => {
    try {
        await axios.put(`http://localhost:5072/api/layoutgroups/isuse/${id}`, { isUse });
        Swal.fire({
            title: 'Success',
            text: `Layout group ${isUse ? 'activated' : 'deactivated'} successfully!`,
            icon: 'success',
            confirmButtonText: 'Go to Dashboard',
        }).then(() => {
            // Redirect to /dashboard
            router.push('/dashboard');
        });
    } catch (error) {
        console.error('Error updating isUse:', error);
        Swal.fire('Error', 'Failed to update layout group status!', 'error');
    }
};


const createLayoutGroup = async () => {
    try {
        const response = await axios.post('http://localhost:5072/api/layoutgroups', {
            name: form.value.name,
        });
        layoutGroups.value.push(response.data);

        Swal.fire('Success', 'Layout group added successfully!', 'success');
        closeModal();
    } catch (error) {
        console.error('Error adding layout group:', error);
        Swal.fire('Error', 'Failed to add layout group!', 'error');
    }
};

const updateLayoutGroup = async () => {
    try {
        await axios.put(`http://localhost:5072/api/layoutgroups/${form.value.id}`, {
            name: form.value.name,
        });

        const index = layoutGroups.value.findIndex((group) => group.id === form.value.id);
        if (index !== -1) {
            layoutGroups.value[index].name = form.value.name;
        }

        Swal.fire('Success', 'Layout group updated successfully!', 'success');
        closeModal();
    } catch (error) {
        console.error('Error updating layout group:', error);
        Swal.fire('Error', 'Failed to update layout group!', 'error');
    }
};

const confirmDelete = async (id) => {
    const result = await Swal.fire({
        title: 'Are you sure?',
        text: 'This action cannot be undone!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
    });

    if (result.isConfirmed) {
        deleteLayoutGroup(id);
    }
};

const deleteLayoutGroup = async (id) => {
    try {
        await axios.delete(`http://localhost:5072/api/layoutgroups/${id}`);
        layoutGroups.value = layoutGroups.value.filter((group) => group.id !== id);

        Swal.fire('Deleted!', 'Layout group has been deleted.', 'success');
    } catch (error) {
        console.error('Error deleting layout group:', error);
        Swal.fire('Error', 'Failed to delete layout group!', 'error');
    }
};

onMounted(fetchLayoutGroups);
</script>

<style scoped>
.modal {
    background-color: rgba(0, 0, 0, 0.5);
}
</style>

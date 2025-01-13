<template>
  <div class="d-flex">
    <!-- Sidebar -->
    <Sidebar />
    <!-- Content -->
    <div class="content flex-grow-1 p-3">
      <router-view />
    </div>
  </div>
</template>

<script setup>
import Sidebar from './components/Sidebar.vue';
</script>

<style>
.content {
  background-color: #f8f9fa;
  min-height: 100vh;
}
</style>

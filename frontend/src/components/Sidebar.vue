<template>
  <div class="bg-dark text-white vh-100 p-3" style="width: 250px">
    <h4 class="mb-4">Access Control</h4>
    <div class="fw-bold">
      Menu
    </div>
    <ul class="nav flex-column">
      <li class="nav-item">
        <router-link to="/" class="nav-link text-white">User Management</router-link>
      </li>
      <li class="nav-item">
        <router-link to="/report" class="nav-link text-white">Report Attendance</router-link>
      </li>
      <li class="nav-item">
        <router-link to="/devices" class="nav-link text-white">IP Device</router-link>
      </li>
      <li class="nav-item">
        <router-link to="/shape" class="nav-link text-white">Shape Layout</router-link>
      </li>
      <li class="nav-item">
        <router-link to="/maintenance" class="nav-link text-white">Maintenance</router-link>
      </li>
      <li class="nav-item">
        <router-link to="/maintenance-call" class="nav-link text-white">Maintenance Calendar</router-link>
      </li>
      <li class="nav-item">
        <router-link to="/broadcast" class="nav-link text-white">BroadCast</router-link>
      </li>
    </ul>
  </div>
</template>

<script setup>
</script>

<style scoped>
.nav-link {
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
}

.nav-link:hover {
  background-color: #495057;
}
</style>

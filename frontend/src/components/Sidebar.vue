<template>
  <div class="sidebar">
    <h4 class="mb-4 text-center">Access Control</h4>
    <div class="fw-bold px-3">Menu</div>
    <ul class="nav flex-column">
      <li class="nav-item" v-for="item in menuItems" :key="item.name">
        <router-link :to="item.path" class="nav-link">
          <i :class="item.icon"></i>
          <span>{{ item.name }}</span>
        </router-link>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref } from "vue";

// Daftar menu dengan ikon dari Font Awesome
const menuItems = ref([
  { name: "Dashboard", path: "/dashboard", icon: "fas fa-tachometer-alt" },
  { name: "Container", path: "/container", icon: "fas fa-box" },
  { name: "User Management", path: "/", icon: "fas fa-users" },
  { name: "Report Attendance", path: "/report", icon: "fas fa-clipboard-list" },
  { name: "IP Device", path: "/devices", icon: "fas fa-network-wired" },
  { name: "Shape Layout", path: "/shape", icon: "fas fa-th-large" },
  { name: "Layout Group", path: "/layout-groups", icon: "fas fa-object-group" },
  { name: "Topic Device", path: "/topic", icon: "fas fa-comments" },
  { name: "Maintenance", path: "/maintenance", icon: "fas fa-tools" },
  { name: "Maintenance Calendar", path: "/maintenance-call", icon: "fas fa-calendar-alt" },
  { name: "BroadCast", path: "/broadcast", icon: "fas fa-bullhorn" },
]);
</script>

<style scoped>
/* Sidebar Container */
.sidebar {
  width: 250px;
  height: 100vh;
  background-color: #212529;
  color: white;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

/* List Menu */
.nav-link {
  display: flex;
  align-items: center;
  padding: 10px 15px;
  color: white;
  font-size: 1rem;
  font-weight: 500;
  border-radius: 8px;
  transition: background-color 0.3s, transform 0.2s;
}

.nav-link i {
  font-size: 1.2rem;
  margin-right: 12px;
  transition: transform 0.2s;
}

/* Hover Effect */
.nav-link:hover {
  background-color: #495057;
  transform: translateX(5px);
}

/* Aktif Link */
.router-link-active {
  background-color: #8d8e8f;
  color: white !important;
  font-weight: bold;
}

/* Header */
h4 {
  font-size: 1.4rem;
  font-weight: bold;
  border-bottom: 2px solid #495057;
  padding-bottom: 10px;
}
</style>

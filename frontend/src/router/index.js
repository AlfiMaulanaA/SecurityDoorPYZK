import { createRouter, createWebHistory } from "vue-router";
import Report from "../views/Report.vue";
import Device from "../views/DevicesIP.vue";
import ShapeControl from "../views/ShapeControl.vue";
import Shape from "../views/ShapeLayout.vue";
import Maintenance from "../views/Maintenance.vue";
import MaintenanceCall from "../views/MaintenanceCalendar.vue";
import Broadcast from "../views/BroadCast.vue";
import User from "../views/Users.vue";

const routes = [
  { path: "/report", component: Report },
  { path: "/devices", component: Device },
  { path: "/shape-control", component: ShapeControl },
  { path: "/shape", component: Shape },
  { path: "/maintenance", component: Maintenance },
  { path: "/maintenance-call", component: MaintenanceCall },
  { path: "/broadcast", component: Broadcast },
  { path: "/", component: User },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;

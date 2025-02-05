import { createRouter, createWebHistory } from "vue-router";
import Report from "../views/AccessControl/Report.vue";
import Device from "../views/AccessControl/DevicesIP.vue";
import ShapeControl from "../views/Layout2D/GeneralShapeControl.vue";
import Shape from "../views/Layout2D/GeneralShape.vue";
import LayoutGroups from "../views/Layout2D/LayoutGroup.vue";
import LayoutByGroup from "../views/Layout2D/Layout.vue";
import ShapesByLayoutGroup from "../views/Layout2D/ShapeLayout.vue";
import Maintenance from "../views/Maintenance/Maintenance.vue";
import MaintenanceCall from "../views/Maintenance/MaintenanceCalendar.vue";
import Broadcast from "../views/Maintenance/BroadCast.vue";
import User from "../views/AccessControl/Users.vue";
import Topic from "../views/Layout2D/Topic.vue";
import Dashboard from "../views/Dashboard.vue";
import Test from "../views/test.vue";
import Container from "../views/Layout3D/Container3D.vue";
import Rack from "../views/Layout3D/RackDetail3D.vue";

const routes = [
  { path: "/dashboard", component: Dashboard },
  { path: "/container", component: Container },
  { path: "/rack/:rackId", name: "rack", component: Rack, props: true },
  { path: "/report", component: Report },
  { path: "/devices", component: Device },
  { path: "/shape-control", component: ShapeControl },
  { path: "/shape", component: Shape },
  {
    path: "/layout-groups",
    component: LayoutGroups, // Daftar layout group
  },
  {
    path: "/layout-groups/:id",
    component: LayoutByGroup, // Shapes dalam layout group tertentu
    props: true,
  },
  {
    path: "/layoutgroups/:id/shapes",
    name: "ShapesByLayoutGroup",
    component: ShapesByLayoutGroup,
    props: true,
  },
  { path: "/maintenance", component: Maintenance },
  { path: "/maintenance-call", component: MaintenanceCall },
  { path: "/broadcast", component: Broadcast },
  { path: "/topic", component: Topic },
  { path: "/", component: User },
  { path: "/test", component: Test },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;

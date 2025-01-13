import { createApp } from "vue";
import App from "./App.vue";
import router from "./router/index.js";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js";
import "font-awesome/css/font-awesome.min.css";

const app = createApp(App);

// Gunakan router
app.use(router);

app.mount("#app");

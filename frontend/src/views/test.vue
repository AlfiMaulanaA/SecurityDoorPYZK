<script setup>
import * as THREE from 'three';
import { onMounted, ref } from 'vue';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { FontLoader } from 'three/examples/jsm/loaders/FontLoader';
import { TextGeometry } from 'three/examples/jsm/geometries/TextGeometry';
import axios from 'axios'; // Install axios jika belum: npm install axios
import { useRoute } from 'vue-router';

const route = useRoute();
const rackId = route.params.rackId; // Ambil parameter rackId dari rute
const devices = ref([]); // Data perangkat di rack

const fetchContainerRackWithDevices = async (rackId) => {
  try {
    const response = await axios.get(`http://localhost:5072/containerRacks/${rackId}`);
    return response.data;
  } catch (error) {
    console.error("Error fetching container rack data:", error);
    return null;
  }
};

const canvasRef = ref(null);
const rackNameRef = ref('');

// Constants for dimensions
const rackWidth = 60; // 19 inches standard rack width (approx 60cm)
const rackHeight = 186; // 42U height (1U = 4.445 cm, 42U = ~186 cm)
const rackDepth = 100; // Standard depth of 1m
const shelfHeight = rackHeight / 42; // Each U height

// Initialize scene
const initializeScene = () => {
  const scene = new THREE.Scene();
  scene.background = new THREE.Color(0xeeeeee);
  return scene;
};

// Initialize camera
const initializeCamera = () => {
  const camera = new THREE.PerspectiveCamera(
    75,
    window.innerWidth / window.innerHeight,
    0.1,
    1000
  );
  camera.position.set(100, 50, 200);
  return camera;
};

// Initialize renderer
const initializeRenderer = (canvasRef) => {
  const renderer = new THREE.WebGLRenderer({ antialias: true });
  renderer.setSize(window.innerWidth, window.innerHeight);
  canvasRef.value.appendChild(renderer.domElement);
  return renderer;
};

// Add lights
const addLights = (scene) => {
  const light = new THREE.PointLight(0xffffff, 2); // Brighter light
  light.position.set(50, 50, 50);
  scene.add(light);

  const ambientLight = new THREE.AmbientLight(0xffffff, 1.5); // Stronger ambient light
  scene.add(ambientLight);
};

// Rack structure
const createRack = (scene) => {
  const rackGeometry = new THREE.BoxGeometry(rackWidth, rackHeight, rackDepth);
  const rackMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
  const rackMesh = new THREE.Mesh(rackGeometry, rackMaterial);
  scene.add(rackMesh);
  return rackMesh;
};

// Rack shelves
const createShelves = (scene) => {
  const shelfMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
  for (let i = 0; i < 42; i++) {
    const shelfGeometry = new THREE.BoxGeometry(rackWidth, 0.5, rackDepth);
    const shelfMesh = new THREE.Mesh(shelfGeometry, shelfMaterial);
    shelfMesh.position.set(0, -rackHeight / 2 + shelfHeight * (i + 1), 0);
    scene.add(shelfMesh);
  }
};

// Add U numbering
const addUNumbers = (scene) => {
  const loader = new FontLoader();
  loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
    for (let i = 0; i < 42; i++) {
      const textGeometry = new TextGeometry((i + 1).toString(), { // Reverse order to start from 1 at the bottom
        font: font,
        size: 2,
        height: 0.5,
      });
      const textMaterial = new THREE.MeshLambertMaterial({ color: 0xffffff });
      const textMesh = new THREE.Mesh(textGeometry, textMaterial);
      textMesh.position.set(-rackWidth / 2 - 5, -rackHeight / 2 + shelfHeight * i + shelfHeight / 2 - 1, rackDepth / 2 + 1);
      scene.add(textMesh);
    }
  });
};

const addRackNameToScene = (scene, rackName) => {
  const loader = new FontLoader();
  loader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', (font) => {
    const textGeometry = new TextGeometry(rackName, {
      font: font,
      size: 6,
      height: 1,
    });
    const textMaterial = new THREE.MeshLambertMaterial({ color: 0xffffff });
    const textMesh = new THREE.Mesh(textGeometry, textMaterial);
    textMesh.position.set(-rackWidth / 2, rackHeight / 2 + 5, 50); // Above the rack
    scene.add(textMesh);
  });
};

const addDevicesFromAPI = (scene, deviceRacks) => {
  const objectMaterial = new THREE.MeshLambertMaterial({ color: 0x7a7679 }); // Green for devices
  const frameMaterial = new THREE.LineBasicMaterial({ color: 0x000000 }); // Black for frame

  deviceRacks.forEach((device) => {
    // Main device
    const objectGeometry = new THREE.BoxGeometry(rackWidth - 1, shelfHeight * device.totalU, rackDepth + 1); // Adjusted size
    const deviceMesh = new THREE.Mesh(objectGeometry, objectMaterial);
    deviceMesh.position.set(
      0,
      -rackHeight / 2 + shelfHeight * (parseInt(device.position) + device.totalU / 2 - 1),
      0
    );
    scene.add(deviceMesh);

    // Add frame around the device
    const edges = new THREE.EdgesGeometry(objectGeometry);
    const frame = new THREE.LineSegments(edges, frameMaterial);
    frame.position.copy(deviceMesh.position);
    scene.add(frame);
  });
};


const botomCoverRack = (scene) => {
  const objectHeight = 10;
  const objectGeometry = new THREE.BoxGeometry(62, objectHeight, 102);
  const objectMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
  const objectMesh = new THREE.Mesh(objectGeometry, objectMaterial);
  objectMesh.position.set(0, -rackHeight / 2 - 10 + objectHeight / 2, 0); // Place at the bottom-most U
  scene.add(objectMesh);
  return objectMesh;
};

const topCoverRack = (scene) => {
  const objectHeight = 5;
  const objectGeometry = new THREE.BoxGeometry(61, objectHeight, 101);
  const objectMaterial = new THREE.MeshLambertMaterial({ color: 0x000000 });
  const objectMesh = new THREE.Mesh(objectGeometry, objectMaterial);
  objectMesh.position.set(0, rackHeight / 2 + objectHeight / 2, 0); // Place at the bottom-most U
  scene.add(objectMesh);
  return objectMesh;
};

// Render loop
const renderLoop = (renderer, scene, camera, controls) => {
  const animate = () => {
    requestAnimationFrame(animate);
    controls.update(); // Required if damping is enabled
    renderer.render(scene, camera);
  };
  animate();
};

onMounted(async () => {
  const route = useRoute();
  const rackId = route.params.rackId; // Ambil rackId dari URL

  const scene = initializeScene();
  const camera = initializeCamera();
  const renderer = initializeRenderer(canvasRef);

  addLights(scene);
  createRack(scene);
  botomCoverRack(scene);
  topCoverRack(scene);
  createShelves(scene);
  addUNumbers(scene);

  // Fetch container rack data
  const containerRackData = await fetchContainerRackWithDevices(rackId);

  if (containerRackData) {
    rackNameRef.value = containerRackData.rackName; // Set rack name untuk UI
    addRackNameToScene(scene, containerRackData.rackName); // Tambahkan nama rack ke scene

    if (containerRackData.deviceRacks) {
      addDevicesFromAPI(scene, containerRackData.deviceRacks);
    }
  }

  const controls = new OrbitControls(camera, renderer.domElement);
  controls.enableDamping = true; // Optional smoothing
  controls.dampingFactor = 0.05;

  renderLoop(renderer, scene, camera, controls);

  // Handle window resize
  window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
  });
});

</script>

<template>
  <div>
    <h5>Detail Rack: {{ rackNameRef }}</h5>
    <div ref="canvasRef" style="width: 100%; height: 100vh;"></div>
  </div>
</template>
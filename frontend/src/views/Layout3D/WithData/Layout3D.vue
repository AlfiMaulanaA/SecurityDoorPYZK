<template>
  <div>
    <h1>3D Design</h1>
    <canvas ref="canvasRef"></canvas>
  </div>
</template>

<script setup>
import * as THREE from 'three';
import axios from 'axios';
import { onMounted, ref } from 'vue';

const canvasRef = ref(null); // Reference ke canvas
const config = ref(null); // Data konfigurasi dari API

// Ambil konfigurasi dari API
const fetchConfig = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/config');
    config.value = response.data;
  } catch (error) {
    console.error('Error fetching config:', error);
  }
};

// Fungsi untuk membuat renderer
const createRenderer = (canvas) => {
  const renderer = new THREE.WebGLRenderer({ canvas });
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setClearColor(config.value.renderer.backgroundColor); // Set background
  return renderer;
};

// Fungsi untuk membuat kamera
const createCamera = () => {
  const { fov, near, far, positionX, positionY, positionZ } = config.value.camera;
  const camera = new THREE.PerspectiveCamera(fov, window.innerWidth / window.innerHeight, near, far);
  camera.position.set(positionX, positionY, positionZ);
  return camera;
};

// Fungsi untuk membuat scene
const createScene = () => {
  const scene = new THREE.Scene();
  const { color, intensity, positionX, positionY, positionZ } = config.value.light;
  const light = new THREE.PointLight(color, intensity);
  light.position.set(positionX, positionY, positionZ);
  scene.add(light);
  return scene;
};

// Fungsi untuk membuat objek
const createObject = (objectConfig) => {
  let geometry;
  const { type, color, opacity, positionX, positionY, positionZ, width, height, depth, radius } = objectConfig;

  if (type === 'box') {
    geometry = new THREE.BoxGeometry(width, height, depth);
  } else if (type === 'sphere') {
    geometry = new THREE.SphereGeometry(radius, 32, 32);
  }

  const material = new THREE.MeshStandardMaterial({
    color,
    transparent: true,
    opacity,
  });

  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(positionX, positionY, positionZ);
  return mesh;
};

// Render objek
const renderObject = async () => {
  await fetchConfig();

  if (!config.value) return;

  const renderer = createRenderer(canvasRef.value);
  const camera = createCamera();
  const scene = createScene();

  config.value.objects.forEach((objectConfig) => {
    const object = createObject(objectConfig);
    if (object) scene.add(object);
  });

  renderer.render(scene, camera);
};

onMounted(() => {
  renderObject();
});
</script>

<style>
canvas {
  display: block;
  width: 100%;
  height: 100vh;
  background-color: white;
}
</style>

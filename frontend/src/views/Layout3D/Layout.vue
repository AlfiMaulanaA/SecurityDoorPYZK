<template>
  <div>
    <h1>3D Design</h1>
    <canvas ref="canvasRef"></canvas>
  </div>
</template>

<script setup>
import * as THREE from 'three';
import { onMounted, ref } from 'vue';

// Konfigurasi global dalam satu file
const config = {
  // Pengaturan kamera
  camera: {
    fov: 75, // Field of view
    near: 0.1, // Jarak terdekat
    far: 1000, // Jarak terjauh
    position: { x: 2, y:5, z: 6 }, // Posisi kamera
  },
  // Pengaturan pencahayaan
  light: {
    color: 0xffffff, // Warna cahaya
    intensity: 1, // Intensitas cahaya
    position: { x: 5, y: 5, z: 5 }, // Posisi cahaya
  },
  // Pengaturan renderer
  renderer: {
    backgroundColor: 0xffffff, // Warna latar belakang
  },
  // Konfigurasi objek (bentuk persegi panjang)
  objects: [
    {
      id: 'box1',
      width: 2,
      height: 3,
      depth: 1,
      color: 0x0077ff, // Biru
      opacity: 0.5,
      position: { x: -3, y: 3, z: 0 }, // Geser ke kiri
    },
    {
      id: 'box2',
      width: 3,
      height: 2,
      depth: 2,
      color: 0xff0000, // Merah
      opacity: 0.7,
      position: { x: 0, y: 3, z: 0 }, // Tengah
    },
    {
      id: 'box3',
      width: 1,
      height: 4,
      depth: 1,
      color: 0x00ff00, // Hijau
      opacity: 0.6,
      position: { x: 3, y: 3, z: 0 }, // Geser ke kanan
    },
  ],
};

const canvasRef = ref(null); // Reference ke canvas

// Fungsi untuk membuat renderer
const createRenderer = (canvas) => {
  const renderer = new THREE.WebGLRenderer({ canvas });
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setClearColor(config.renderer.backgroundColor); // Set background
  return renderer;
};

// Fungsi untuk membuat kamera
const createCamera = () => {
  const { fov, near, far, position } = config.camera;
  const camera = new THREE.PerspectiveCamera(fov, window.innerWidth / window.innerHeight, near, far);
  camera.position.set(position.x, position.y, position.z);
  return camera;
};

// Fungsi untuk membuat scene
const createScene = () => {
  const scene = new THREE.Scene();

  // Tambahkan pencahayaan
  const { color, intensity, position } = config.light;
  const light = new THREE.PointLight(color, intensity);
  light.position.set(position.x, position.y, position.z);
  scene.add(light);

  return scene;
};

// Fungsi untuk membuat objek persegi panjang
const createBox = ({ width, height, depth, color, opacity, position }) => {
  const boxGeometry = new THREE.BoxGeometry(width, height, depth);
  const boxMaterial = new THREE.MeshStandardMaterial({
    color,
    transparent: true,
    opacity,
  });
  const box = new THREE.Mesh(boxGeometry, boxMaterial);
  box.position.set(position.x, position.y, position.z);
  return box;
};

// Fungsi utama untuk merender
const renderObject = async () => {
  const renderer = createRenderer(canvasRef.value);
  const camera = createCamera();
  const scene = createScene();

  // Tambahkan semua objek ke scene
  config.objects.forEach((objectConfig) => {
    const box = createBox(objectConfig);
    scene.add(box);
  });

  // Render
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

<template>
  <div>
    <h5>3D Container Design</h5>
    <div>
      <button class="btn btn-sm btn-secondary me-1" @click="setView('isometric')">Isometric</button>
      <button class="btn btn-sm btn-secondary me-1" @click="setView('front')">Front View</button>
      <button class="btn btn-sm btn-secondary me-1" @click="setView('back')">Back View</button>
      <button class="btn btn-sm btn-secondary me-1" @click="setView('top')">Top View</button>
      <button class="btn btn-sm btn-secondary me-1" @click="setView('side')">Side View</button>
      <button class="btn btn-sm btn-primary me-1" @click="toggleLeftCover">Left Cover</button>
      <button class="btn btn-sm btn-danger me-1" @click="removeAllPanels">Remove All Panels</button>
      <button class="btn btn-sm btn-success me-1" @click="restoreAllPanels">Restore All Panels</button>
    </div>
    <canvas ref="canvasRef"></canvas>
  </div>
</template>

<script setup>
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { onMounted, ref } from 'vue';
import { Text } from 'troika-three-text';
import { useRouter } from 'vue-router';
import axios from 'axios';
import Paho from 'paho-mqtt';

// Dimensi Kontainer dan Rack Server
const containerDimensions = {
  length: 1219.2, // Panjang dalam cm
  width: 243.8,   // Lebar dalam cm
  height: 259.1,  // Tinggi dalam cm
};

const rackDimensions = {
  height: 186.69, // Tinggi rack 42U dalam cm
  width: 100,      // Lebar rack dalam cm
  depth: 60,     // Kedalaman rack dalam cm
};

const client = new Paho.Client("192.168.0.186", Number(9000), `client-${Math.random()}`);
const mqttConnectionStatus = ref('Disconnected');
const subscribedTopics = ref([]); // Menyimpan daftar topik yang disubscribe

// Handle connection lost
client.onConnectionLost = responseObject => {
  mqttConnectionStatus.value = 'Disconnected';
};

const latestTopicData = ref({});

client.onMessageArrived = message => {
  try {
    const payload = JSON.parse(message.payloadString);
    const valueData = JSON.parse(payload.value);

    latestTopicData.value[message.destinationName] = {
      temp: valueData.temp,
      hum: valueData.hum,
    };

    // Perbarui label pada rack yang relevan
    rackMeshes.value.forEach(rack => {
      if (rack.data.topic === message.destinationName) {
        const labelGroup = rack.label; // Ambil group label

        const tempLabel = labelGroup.children[1]; // Label suhu
        const humLabel = labelGroup.children[2]; // Label kelembaban

        const tempColor = valueData.temp > 40 ? 0xff0000 : valueData.temp > 30 ? 0xffff00 : 0x00ff00;
        const humColor = valueData.hum > 60 ? 0xff0000 : valueData.hum > 40 ? 0xffff00 : 0x00ff00;

        tempLabel.text = `${valueData.temp}°C`;
        tempLabel.color = tempColor;
        tempLabel.sync();

        humLabel.text = `${valueData.hum}%`;
        humLabel.color = humColor;
        humLabel.sync();
      }
    });
  } catch (error) {
    console.error('Failed to parse message:', error);
  }
};


// Connect to MQTT Broker
const connectClient = () => {
  client.connect({
    onSuccess: () => {
      console.log("Connected to MQTT broker");
      mqttConnectionStatus.value = 'Connected';

      // Subscribe to topics after connection
      subscribedTopics.value.forEach(topic => {
        client.subscribe(topic);
      });
    },
    onFailure: (error) => {
      console.error("Connection failed:", error.errorMessage);
      mqttConnectionStatus.value = 'Failed to Connect';
    }
  });
};

// Reference ke canvas
const canvasRef = ref(null);
let camera, controls, renderer, scene;
let containerMesh;
let initialMaterials = [];

const rackMeshes = ref([]);

// Fetch data rack dan topik dari database
const fetchRackData = async () => {
  try {
    const response = await axios.get('http://localhost:5072/containerracks');
    const data = response.data;

    // Tambahkan semua topik ke daftar topik yang disubscribe
    subscribedTopics.value = data.map(rack => rack.topic);

    return data;
  } catch (error) {
    console.error('Failed to fetch rack data:', error);
    return [];
  }
};

const createRackLabel = (rackData, positionX, positionY, positionZ) => {
  const group = new THREE.Group(); // Group untuk menggabungkan nama rack dan data

  // Label untuk nama rack
  const rackNameLabel = new Text();
  rackNameLabel.text = rackData.rackName;
  rackNameLabel.fontSize = 0.15;
  rackNameLabel.color = 0xffffff;
  rackNameLabel.position.set(0, -0.05, 0); // Offset sedikit ke atas
  rackNameLabel.anchorX = 'center';
  rackNameLabel.anchorY = 'middle';
  // rackNameLabel.material.depthTest = false;
  // rackNameLabel.material.depthWrite = false;
  rackNameLabel.sync();
  group.add(rackNameLabel);

  // Label untuk suhu
  const tempLabel = new Text();
  tempLabel.text = 'No data';
  tempLabel.fontSize = 0.12;
  tempLabel.color = 0x808080; // Warna default abu-abu
  tempLabel.position.set(0, -0.3, 0); // Offset sedikit ke bawah
  tempLabel.anchorX = 'center';
  tempLabel.anchorY = 'middle';
  tempLabel.sync();
  group.add(tempLabel);

  // Label untuk kelembaban
  const humLabel = new Text();
  humLabel.text = '';
  humLabel.fontSize = 0.12;
  humLabel.color = 0x808080; // Warna default abu-abu
  humLabel.position.set(0, -0.5, 0); // Offset lebih ke bawah dari suhu
  humLabel.anchorX = 'center';
  humLabel.anchorY = 'middle';
  humLabel.sync();
  group.add(humLabel);

  // Posisikan group ke lokasi rack
  group.position.set(positionX, positionY + 0.75, positionZ + 1.35);
  group.rotation.y = 0;

  return group;
};


// Fungsi untuk membuat kontainer
const createContainer = () => {
  const geometry = new THREE.BoxGeometry(
    containerDimensions.length / 100,
    containerDimensions.height / 100,
    containerDimensions.width / 100
  );

  const materials = [
    new THREE.MeshStandardMaterial({ color: 0xe3e3e3, side: THREE.DoubleSide }), // Depan
    new THREE.MeshStandardMaterial({ color: 0xe3e3e3, side: THREE.DoubleSide }), // Belakang
    new THREE.MeshStandardMaterial({ color: 0xffffff, side: THREE.DoubleSide }), // Atas
    new THREE.MeshStandardMaterial({ color: 0xffffff, side: THREE.DoubleSide }), // Bawah
    null, // Kiri (terbuka)
    new THREE.MeshStandardMaterial({ color: 0xe3e3e3, side: THREE.DoubleSide }), // Kanan
  ];

  initialMaterials = materials.map((material) => material ? material.clone() : null); // Simpan salinan material awal

  containerMesh = new THREE.Mesh(geometry, materials);
  return containerMesh;
};

let leftCoverVisible = true;
let containerCovers = []; // Array untuk menyimpan referensi objek cover

const toggleLeftCover = () => {
  if (leftCoverVisible) {
    // Hapus bagian cover dari scene
    containerCovers.forEach((cover) => {
      scene.remove(cover);
    });
  } else {
    // Tambahkan kembali bagian cover ke scene
    containerCovers.forEach((cover) => {
      scene.add(cover);
    });
  }

  leftCoverVisible = !leftCoverVisible; // Toggle status
};


const removeAllPanels = () => {
  if (!containerMesh) return;

  // Iterasi melalui semua material pada kontainer
  for (let i = 0; i < containerMesh.material.length; i++) {
    if (i !== 3) { // Indeks 3 adalah bagian bawah (mengacu pada definisi material dalam `createContainer`)
      containerMesh.material[i] = null; // Hapus material pada bagian selain bawah
    }
  }

  // Hapus cover samping jika ada
  if (containerCovers.length > 0) {
    containerCovers.forEach((cover) => {
      scene.remove(cover);
    });
  }

  leftCoverVisible = false; // Pastikan status cover kiri diperbarui
};


// Fungsi untuk memulihkan semua panel kontainer
const restoreAllPanels = () => {
  if (!containerMesh || !initialMaterials.length) return;

  for (let i = 0; i < containerMesh.material.length; i++) {
    containerMesh.material[i] = initialMaterials[i] ? initialMaterials[i].clone() : null;
  }
};

// Fungsi untuk membuat sekat dalam kontainer
const createPartition = (positionX) => {
  const geometry = new THREE.PlaneGeometry(
    containerDimensions.width / 100,
    containerDimensions.height / 100
  );

  const material = new THREE.MeshStandardMaterial({
    color: 0xdcdcdc,
    side: THREE.DoubleSide,
  });

  const partition = new THREE.Mesh(geometry, material);
  partition.rotation.y = Math.PI / 2; // Rotasi menghadap sumbu X
  partition.position.set(positionX / 100, containerDimensions.height / 200, 0);

  return partition;
};

// Fungsi untuk membuat rack server
const createRec = (positionX, positionY, positionZ, color, customDimensions = null, opacity = 1) => {
  const dimensions = customDimensions || rackDimensions;

  const geometry = new THREE.BoxGeometry(
    dimensions.width / 100,
    dimensions.height / 100,
    dimensions.depth / 100
  );

  const material = new THREE.MeshStandardMaterial({
    color,
    transparent: opacity < 1, // Aktifkan transparansi hanya jika opacity < 1
    opacity: opacity,         // Atur opacity
  });
  const rack = new THREE.Mesh(geometry, material);
  rack.position.set(positionX, positionY, positionZ);
  rack.rotation.y = Math.PI / 2; // Rotasi menghadap sumbu Y
  return rack;
};

// Fungsi untuk membuat tabung
const createCylinder = (positionX, positionY, positionZ, color, dimensions = { radius: 10, height: 50 }) => {
  const geometry = new THREE.CylinderGeometry(
    dimensions.radius / 100,
    dimensions.radius / 100,
    dimensions.height / 100,
    32
  );

  const material = new THREE.MeshStandardMaterial({ color });
  const cylinder = new THREE.Mesh(geometry, material);
  cylinder.position.set(positionX, positionY, positionZ);

  return cylinder;
};

const createCylinderWithFillet = (positionX, positionY, positionZ, color, dimensions = { radius: 20, height: 150 }) => {
  const group = new THREE.Group();

  // Tabung utama (bagian tengah)
  const cylinderGeometry = new THREE.CylinderGeometry(
    dimensions.radius / 100,   // Radius atas dan bawah sama
    dimensions.radius / 100,   // Radius atas dan bawah sama
    (dimensions.height / 2) / 100, // Tinggi tabung (setengah total tinggi)
    32                        // Jumlah segmen
  );

  const material = new THREE.MeshStandardMaterial({ color });

  const cylinder = new THREE.Mesh(cylinderGeometry, material);
  cylinder.position.y = (dimensions.height / 4) / 100; // Letakkan tabung di tengah grup
  group.add(cylinder);

  // Fillet atas (bagian bundar)
  const topFilletGeometry = new THREE.SphereGeometry(
    dimensions.radius / 100, // Radius fillet sama dengan radius tabung
    32,                      // Segmen horizontal
    16,                      // Segmen vertikal
    0,                       // Sudut horizontal awal
    Math.PI * 2,             // Sudut horizontal akhir (lingkar penuh)
    0,                       // Sudut vertikal awal
    Math.PI / 2              // Sudut vertikal akhir (setengah bola)
  );

  const topFillet = new THREE.Mesh(topFilletGeometry, material);
  topFillet.position.y = (dimensions.height / 2) / 100; // Letakkan di atas tabung
  group.add(topFillet);

  // Posisi seluruh grup
  group.position.set(positionX, positionY, positionZ);

  return group;
};

// Fungsi untuk membuat tabung horizontal
const createHorizontalCylinder = (positionX, positionY, positionZ, color, dimensions) => {
  const cylinder = createCylinder(positionX, positionY, positionZ, color, dimensions);
  cylinder.rotation.z = Math.PI / 2; // Rotasi horizontal
  return cylinder;
};
// Fungsi untuk membuat tray berlubang
const createOpenTrayWithHoles = (positionX, positionY, positionZ, color, dimensions = { width: 30, height: 10, depth: 700 }, holeRadius = 2, holeSpacing = 5) => {
  const group = new THREE.Group();

  // Dimensi
  const width = dimensions.width / 100;  // Konversi cm ke meter
  const height = dimensions.height / 100;
  const depth = dimensions.depth / 100;

  const material = new THREE.MeshStandardMaterial({ color, side: THREE.DoubleSide });

  // Bagian bawah dengan lubang
  const shape = new THREE.Shape();
  shape.moveTo(-width / 2, -depth / 2);
  shape.lineTo(-width / 2, depth / 2);
  shape.lineTo(width / 2, depth / 2);
  shape.lineTo(width / 2, -depth / 2);
  shape.lineTo(-width / 2, -depth / 2);

  // Tambahkan lubang pada tray
  const holes = [];
  for (let x = -width / 2 + holeSpacing; x < width / 2; x += holeSpacing) {
    for (let y = -depth / 2 + holeSpacing; y < depth / 2; y += holeSpacing) {
      const hole = new THREE.Path();
      hole.absarc(x, y, holeRadius / 100, 0, Math.PI * 2, false); // Lubang bulat
      holes.push(hole);
    }
  }
  shape.holes = holes;

  // Ekstrusi untuk membuat tray berlubang
  const extrudeSettings = { depth: 0.01, bevelEnabled: false };
  const bottomGeometry = new THREE.ExtrudeGeometry(shape, extrudeSettings);
  const bottom = new THREE.Mesh(bottomGeometry, material);
  bottom.rotation.x = -Math.PI / 2; // Rotasi agar menghadap ke atas
  bottom.position.y = -height / 2; // Posisi bagian bawah
  group.add(bottom);

  // Bagian belakang
  const backGeometry = new THREE.PlaneGeometry(width, height);
  const back = new THREE.Mesh(backGeometry, material);
  back.rotation.y = Math.PI; // Menghadap ke arah depan
  back.position.z = -depth / 2; // Posisi bagian belakang
  group.add(back);

  // Bagian depan
  const frontGeometry = new THREE.PlaneGeometry(width, height);
  const front = new THREE.Mesh(frontGeometry, material);
  front.position.z = depth / 2; // Posisi bagian depan
  group.add(front);

  // Posisi seluruh grup
  group.position.set(positionX, positionY, positionZ);

  return group;
};

// Fungsi untuk membuat objek di dalam rack dengan tinggi disesuaikan persentase
const createObjectInsideRack = (rackData, positionX, positionY, positionZ, heightPercentage) => {
  // Menghitung tinggi objek berdasarkan persentase dari tinggi rack 
  const objectHeight = (rackDimensions.height * heightPercentage) / 100; // Tinggi objek disesuaikan

  // Dimensi objek sama dengan rack, kecuali untuk tinggi yang sudah disesuaikan
  const geometry = new THREE.BoxGeometry(0.55,    // Lebar objek sama dengan rack (dalam meter)
    objectHeight / 100,            // Tinggi objek disesuaikan dengan persentase (dalam meter)
    (rackDimensions.depth / 100) + 0.02     // Kedalaman objek sama dengan rack (dalam meter)
  );

  const material = new THREE.MeshStandardMaterial({ color: 0x00ff00 }); // Warna objek hijau (sesuaikan sesuai kebutuhan)

  // Membuat objek 3D di dalam rack
  const objectMesh = new THREE.Mesh(geometry, material);
  objectMesh.position.set(positionX, positionY + (objectHeight / 200) - 1, 0.2); // Sesuaikan posisi objek dalam rack

  return objectMesh;
};

// Menambahkan objek ke dalam rack
const addObjectToRack = (rackData, positionX, positionY, positionZ) => {
  const heightPercentage = rackData.heightPercentage || 0; // Mengambil persentase tinggi dari data rack (default 50%)
  const objectInsideRack = createObjectInsideRack(rackData, positionX, positionY, positionZ, heightPercentage);
  scene.add(objectInsideRack); // Tambahkan objek ke dalam scene
  return objectInsideRack;
};


const renderScene = async () => {

  const rackData = await fetchRackData();

  const canvas = canvasRef.value;

  renderer = new THREE.WebGLRenderer({ canvas });
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setClearColor(0xffffff);

  scene = new THREE.Scene();

  camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
  camera.position.set(-5, 3, 6);

  controls = new OrbitControls(camera, renderer.domElement);
  controls.enableDamping = true;
  controls.dampingFactor = 0.1;

  // Pencahayaan
  const ambientLight = new THREE.AmbientLight(0xffffff, 1);
  const directionalLight = new THREE.DirectionalLight(0xffffff, 2);
  directionalLight.position.set(0, 5, 5);
  directionalLight.castShadow = true;

  scene.add(ambientLight, directionalLight);

  // Kontainer dan Partisi
  const container = createContainer();
  container.position.y = containerDimensions.height / 200;
  scene.add(container);

  const partitionFront = createPartition(-450);
  const partitionFront1 = createPartition(-609.6);
  const partitionBack = createPartition(450);
  const partitionBack1 = createPartition(609.6);
  scene.add(partitionFront, partitionBack, partitionBack1, partitionFront1);

  // Rack Server dan Panel
  const framePanelFSS = createRec(-5.5, 1.75, -1, 0x000000, { width: 21, height: 25, depth: 50 });
  const panelFSS = createRec(-5.5, 1.5, -1, 0xff0000, { width: 20, height: 80, depth: 60 });
  const framePanelPower = createRec(5.65, 1.01, 0.6, 0x000000, { width: 102, height: 190, depth: 70 });
  const panelPower = createRec(5.65, 1.01, 0.6, 0xc3c4c5, { width: 100, height: 200, depth: 80 });
  const accessControl = createRec(-4.55, 1.2, -0.3, 0x0d542e, { width: 30, height: 20, depth: 8 });
  const openTray = createOpenTrayWithHoles(0, 2.4, 0, 0x0077ff, { width: 1100, height: 10, depth: 30 }, 1.5, 5);
  const partitionDoorCover1 = createRec(-3.8, 1, -0.7, 0x000000, { width: 100, height: 189, depth: 8 });
  const partitionDoorCover2 = createRec(3.8, 1, -0.7, 0x000000, { width: 100, height: 189, depth: 8 });
  const Polycarbonat1 = createRec(-3.8, 2.25, -0.7, 0xFFFFFF, { width: 100, height: 60, depth: 8 }, 0.5);
  const Polycarbonat2 = createRec(3.8, 2.25, -0.7, 0xFFFFFF, { width: 100, height: 60, depth: 8 }, 0.5);
  const Polycarbonat3 = createRec(0, 2.25, -0.2, 0xFFFFFF, { width: 8, height: 60, depth: 770 }, 0.5);
  const securityDoor = createRec(-4.5, 1, 0.5, 0xFFFFFF, { width: 100, height: 200, depth: 8 });
  const frontDoor = createRec(-6.1, 1, 0.5, 0xFFFFFF, { width: 100, height: 200, depth: 8 });
  const backDoor = createRec(6.1, 1, -0.5, 0xFFFFFF, { width: 100, height: 200, depth: 8 });
  const hornAlarm = createRec(-4.5, 2.25, 0.75, 0xFF0000, { width: 40, height: 15, depth: 10 }, 0.5);
  const buzzerAlarm = createRec(-4.5, 2.25, 0.25, 0xFF0000, { width: 15, height: 15, depth: 10 }, 0.5);
  const baseFloor = createRec(0, -0.165, 0, 0x000000, { width: 243.8, height: 30, depth: 1219.2 });

  scene.add(baseFloor, framePanelFSS, panelFSS, buzzerAlarm, hornAlarm, panelPower, framePanelPower, accessControl, openTray, partitionDoorCover1, partitionDoorCover2, Polycarbonat1, Polycarbonat2, Polycarbonat3, securityDoor, frontDoor, backDoor);

  const containerCoverRight = createRec(4.35, 1.295, 1.225, 0xffffff, { width: 1, height: 259.1, depth: 350 });
  const containerCoverLeft = createRec(-4.35, 1.295, 1.225, 0xffffff, { width: 1, height: 259.1, depth: 350 });
  const containerCoverBot = createRec(0, 0.15, 1.225, 0xffffff, { width: 1, height: 30, depth: 520 });
  const containerCoverMid = createRec(0, 1.3, 1.225, 0xffffff, { width: 1, height: 198, depth: 519 }, 0.1);
  const containerCoverTop = createRec(0, 2.45, 1.225, 0xffffff, { width: 1, height: 30, depth: 520 });

  // Tambahkan cover ke scene
  containerCovers = [containerCoverRight, containerCoverLeft, containerCoverBot, containerCoverMid, containerCoverTop];
  containerCovers.forEach((cover) => scene.add(cover));

  // Tabung
  const cylinderWithFillet = createCylinderWithFillet(-4.75, 0.01, -0.75, 0xff0000, { radius: 20, height: 250 });
  const smallCylinder = createCylinder(-4.75, 1.5, -0.75, 0xff0000, { radius: 3, height: 150 });
  const nozzle1 = createCylinder(-2, 2.1, -0.75, 0xff0000, { radius: 3, height: 30 });
  const nozzle2 = createCylinder(2, 2.1, -0.75, 0xff0000, { radius: 3, height: 30 });
  const horizontalCylinder = createHorizontalCylinder(-0.75, 2.25, -0.75, 0xff0000, { radius: 3, height: 800 });
  const smoke1 = createCylinder(-2.5, 2.555, 0.5, 0xFFFFFF, { radius: 8, height: 7 });
  const smoke2 = createCylinder(-1, 2.555, 0.5, 0xFFFFFF, { radius: 8, height: 7 });
  const smoke3 = createCylinder(0.5, 2.555, 0.5, 0xFFFFFF, { radius: 8, height: 7 });
  const smoke4 = createCylinder(2, 2.555, 0.5, 0xFFFFFF, { radius: 8, height: 7 });
  const sensorSmoke1 = createCylinder(-2.5, 2.55, 0.5, 0x000000, { radius: 3, height: 7 });
  const sensorSmoke2 = createCylinder(-1, 2.55, 0.5, 0x000000, { radius: 3, height: 7 });
  const sensorSmoke3 = createCylinder(0.5, 2.55, 0.5, 0x000000, { radius: 3, height: 7 });
  const sensorSmoke4 = createCylinder(2, 2.55, 0.5, 0x000000, { radius: 3, height: 7 });

  scene.add(cylinderWithFillet, smallCylinder, nozzle1, nozzle2, horizontalCylinder, smoke1, smoke2, smoke3, smoke4, sensorSmoke1, sensorSmoke2, sensorSmoke3, sensorSmoke4);

  rackData.forEach((rack, index) => {
    const positionX = -3.5 + index * 0.7; // Posisi X rack
    const positionY = rackDimensions.height / 200; // Posisi Y sesuai dengan tinggi rack
    const positionZ = -rackDimensions.depth / 200 - 0.5; // Sedikit di depan rack

    const rackMesh = createRec(positionX, positionY, 0, 0x000000); // Warna hitam untuk rack
    const label = createRackLabel(rack, positionX, positionY, positionZ); // Buat label untuk rack

    scene.add(rackMesh); // Tambahkan rack ke scene
    scene.add(label); // Tambahkan label ke scene

    // Tambahkan objek ke dalam rack
    const heightPercentage = 50; // Misalnya objek di dalam rack memiliki tinggi 50% dari tinggi rack
    addObjectToRack(rack, positionX, positionY, positionZ, heightPercentage); // Tambahkan objek ke rack

    // Simpan referensi rack dan objek
    rackMeshes.value.push({ mesh: rackMesh, data: rack, label });
  });


  const animate = () => {
    requestAnimationFrame(animate);

    if (controls) {
      controls.update();
    }

    renderer.render(scene, camera);
  };

  animate();
};

// Fungsi untuk animasi perpindahan kamera
const animateCamera = (targetPosition) => {
  const duration = 1000; // Durasi animasi dalam milidetik
  const startPosition = camera.position.clone(); // Posisi awal kamera
  const startTime = performance.now();

  const animate = () => {
    const elapsedTime = performance.now() - startTime; // Waktu yang telah berlalu
    const t = Math.min(elapsedTime / duration, 1); // Normalisasi waktu (0 hingga 1)

    // Interpolasi posisi kamera
    camera.position.lerpVectors(startPosition, targetPosition, t);
    camera.lookAt(0, 0, 0); // Kamera selalu mengarah ke pusat scene

    if (controls) {
      controls.update();
    }

    renderer.render(scene, camera);

    if (t < 1) {
      requestAnimationFrame(animate); // Lanjutkan animasi
    }
  };

  animate();
};

// Fungsi untuk mengatur tampilan kamera
const setView = (view) => {
  if (!camera || !renderer || !controls) {
    console.warn('Camera, renderer, or controls is not initialized yet.');
    return;
  }

  let targetPosition;
  switch (view) {
    case 'isometric':
      targetPosition = new THREE.Vector3(-5, 4, 7.5); // Isometric View
      break;
    case 'front':
      targetPosition = new THREE.Vector3(0, 2, 7.5); // Front View
      break;
    case 'top':
      targetPosition = new THREE.Vector3(0, 10, 0); // Top View
      break;
    case 'back':
      targetPosition = new THREE.Vector3(0, 2, -7.5); // Back View
      break;
    case 'side':
      targetPosition = new THREE.Vector3(10, 0, 0); // Side View
      break;
    default:
      console.warn('Unknown view:', view);
      return;
  }

  // Animasi kamera ke posisi target
  animateCamera(targetPosition);
};


const router = useRouter();

const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();
const selectedRack = ref(null);

const onCanvasClick = (event) => {
  const rect = canvasRef.value.getBoundingClientRect();
  mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
  mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

  raycaster.setFromCamera(mouse, camera);
  const intersects = raycaster.intersectObjects(rackMeshes.value.map(r => r.mesh));

  if (intersects.length > 0) {
    const clickedRack = intersects[0].object;
    const rackInfo = rackMeshes.value.find(r => r.mesh === clickedRack)?.data;

    if (rackInfo) {
      router.push({ name: 'rack', params: { rackId: rackInfo.id } }); // Navigasi ke halaman rack dengan ID
    }
  }
};

onMounted(() => {
  renderScene();
  connectClient();
  canvasRef.value.addEventListener('click', onCanvasClick);
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

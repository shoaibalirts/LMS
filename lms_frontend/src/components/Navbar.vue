<template>
  <nav>
    <ul class="nav-left">
      <li class="brand">
        <router-link to="/">LMS Portal</router-link>
      </li>
      <template v-if="loggedIn && isTeacher">
        <li><router-link to="/teacher-dashboard">Dashboard</router-link></li>
        <li><router-link to="/create-task">Opret Opgave</router-link></li>
        <li><router-link to="/create-taskset">Opret Opgavesæt</router-link></li>  
        <li><router-link to="/assigned-submissions">Tildel & Bedøm</router-link></li>
        <li><router-link to="/create-studyclass">Opret klasse</router-link></li>
        <li><router-link to="/register-student">Opret elev(er)</router-link></li>  
      </template>
    </ul>

    <ul class="nav-right">
      <template v-if="loggedIn">
        <li><a href="#" @click.prevent="logout">Log ud</a></li>
      </template>
      <template v-else>
        <li><router-link to="/login">Log ind</router-link></li>
        <li><router-link to="/register">Opret konto</router-link></li>
      </template>
    </ul>
  </nav>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { clearAuthSession, getAuthSession } from '../Services/api';

const router = useRouter();
const route = useRoute();
const loggedIn = ref(false);
const isTeacher = ref(false);

function checkLoginStatus() {
  const auth = getAuthSession();
  loggedIn.value = !!(auth?.token || auth?.Token);
  isTeacher.value = (auth?.role || auth?.Role || '').toLowerCase() === 'teacher';
}

onMounted(() => {
  checkLoginStatus();
});

// Re-check login status on route change
watch(() => route.path, () => {
  checkLoginStatus();
});

function logout() {
  clearAuthSession();
  loggedIn.value = false;
  isTeacher.value = false;
  router.replace('/');
}
</script>

<style scoped>
nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #0f172a;
  padding: 0.75rem 1.5rem;
  position: sticky;
  top: 0;
  z-index: 1000;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

nav ul {
  list-style: none;
  display: flex;
  align-items: center;
  margin: 0;
  padding: 0;
  gap: 0.5rem;
}

nav li {
  margin: 0;
}

.brand a {
  font-size: 1.1rem;
  font-weight: 700;
  color: #22d3ee !important;
  letter-spacing: -0.02em;
}

nav a {
  color: #94a3b8;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.95rem;
  padding: 0.5rem 0.85rem;
  border-radius: 8px;
  transition: color 0.15s ease, background 0.15s ease;
}

nav a:hover {
  color: #f8fafc;
  background: rgba(255, 255, 255, 0.05);
}

nav a.router-link-active:not(.brand a) {
  color: #22d3ee;
  background: rgba(34, 211, 238, 0.1);
}

.nav-right {
  gap: 0.35rem;
}
</style>

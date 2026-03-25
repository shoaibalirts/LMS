<template>
  <nav>
    <ul class="nav-left">
      <li class="brand">
        <router-link to="/">LMS Portal</router-link>
      </li>
      <template v-if="loggedIn">
        <li><router-link to="/teacher-dashboard">Dashboard</router-link></li>
      </template>
    </ul>

    <ul class="nav-right">
      <template v-if="loggedIn">
        <li><a href="#" @click.prevent="logout">Logout</a></li>
      </template>
      <template v-else>
        <li><router-link to="/login">Login</router-link></li>
        <li><router-link to="/register">Register</router-link></li>
      </template>
    </ul>
  </nav>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';

const router = useRouter();
const route = useRoute();
const loggedIn = ref(false);

function checkLoginStatus() {
  loggedIn.value = !!localStorage.getItem('user');
}

onMounted(() => {
  checkLoginStatus();
});

// Re-check login status on route change
watch(() => route.path, () => {
  checkLoginStatus();
});

function logout() {
  localStorage.removeItem('user');
  loggedIn.value = false;
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

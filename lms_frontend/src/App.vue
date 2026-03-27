<script setup>
import Navbar from './components/Navbar.vue';
</script>

<template>
  <Navbar />
  <router-view />
</template>

<style>
/* Global styles */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html, body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  height: 100%;
  overflow: hidden;
  background: #0f172a;
}

#app {
  height: 100vh;
  overflow: hidden;
}
</style>

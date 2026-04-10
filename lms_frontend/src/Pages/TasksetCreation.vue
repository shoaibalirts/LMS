<template>
  <div class="page-shell">
    <div class="container">
      <div class="header">
        <div>
          <h1>Dine opgavesæt</h1>
          <p class="subtitle">Vaelg opgavesættene du vil bruge.</p>
        </div>
        <button class="refresh-btn" @click="loadAssignments" :disabled="loading">
          {{ loading ? 'Indlaeser...' : 'Opdater liste' }}
        </button>
      </div>

      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

      <div v-if="loading" class="state">Henter opgavesæt...</div>

      <div v-else-if="assignments.length === 0" class="state">
        Du har ingen oprettede opgavesæt
      </div>

      <div v-else class="grid">
        <article v-for="assignment in assignments" :key="assignment.id" class="card">
          <div class="card-top">
            <h2>{{ assignment.subject }}</h2>
            <span class="points">{{ assignment.points }} point</span>
          </div>

          <p class="meta">{{ assignment.classLevel }} - {{ assignment.type }}</p>

          <div class="links">
            <a v-if="assignment.pictureUrl" :href="assignment.pictureUrl" target="_blank" rel="noreferrer">
              Billede
            </a>
            <a v-if="assignment.videoUrl" :href="assignment.videoUrl" target="_blank" rel="noreferrer">
              Video
            </a>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { GetTeacherAssignmentSets } from '../Services/api';

const assignments = ref([]);
const loading = ref(false);
const errorMessage = ref('');

async function loadAssignments() {
  loading.value = true;
  errorMessage.value = '';

  try {
    const data = await GetTeacherAssignmentSets();
    assignments.value = Array.isArray(data) ? data : [];
  } catch (error) {
    errorMessage.value = error?.message || 'Kunne ikke hente opgaver.';
    assignments.value = [];
  } finally {
    loading.value = false;
  }
}

onMounted(loadAssignments);
</script>

<style scoped>
.page-shell {
  min-height: calc(100vh - 52px);
  background: #f8fafc;
  padding: 2rem;
}

.container {
  max-width: 1100px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.badge {
  display: inline-block;
  padding: 0.3rem 0.7rem;
  border-radius: 999px;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: rgba(20, 184, 166, 0.12);
  color: #0f766e;
  margin-bottom: 0.6rem;
}

h1 {
  margin: 0;
  font-size: 2rem;
  color: #0f172a;
}

.subtitle {
  margin: 0.35rem 0 0;
  color: #475569;
}

.refresh-btn {
  border: none;
  background: #0f172a;
  color: #fff;
  padding: 0.7rem 1rem;
  border-radius: 10px;
  cursor: pointer;
}

.refresh-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
  padding: 0.8rem 1rem;
  border-radius: 10px;
}

.state {
  background: #fff;
  border: 1px dashed #cbd5e1;
  padding: 1rem;
  border-radius: 12px;
  color: #475569;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 1rem;
}

.card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 1rem;
}

.card-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
}

h2 {
  margin: 0;
  font-size: 1.05rem;
  color: #1e293b;
}

.points {
  font-size: 0.8rem;
  color: #0f766e;
  background: rgba(20, 184, 166, 0.12);
  border-radius: 999px;
  padding: 0.2rem 0.55rem;
  white-space: nowrap;
}

.meta {
  margin: 0.55rem 0;
  color: #64748b;
}

.links {
  display: flex;
  gap: 0.75rem;
}

.links a {
  color: #0f766e;
  text-decoration: none;
  font-weight: 600;
}

@media (max-width: 640px) {
  .page-shell {
    padding: 1rem;
  }

  .header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>

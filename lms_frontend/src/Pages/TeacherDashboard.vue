<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <div>
        <p class="badge">Lærerportal</p>
        <h1>Velkommen til dit dashboard</h1>
        <p class="subtitle">Administrer dine kurser, elever og undervisningsindhold</p>
      </div>
    </div>

    <p v-if="errorMessage" class="status error">{{ errorMessage }}</p>

    <div class="dashboard-content">
      <div class="content-card">
        <h2>Dine klasser ({{ studyClasses.length }})</h2>
        <p v-if="loading" class="muted">Henter klasser...</p>
        <p v-else-if="studyClasses.length === 0" class="muted">Du har ingen klasser endnu.</p>
        <ul v-else class="overview-list">
          <li v-for="studyClass in studyClasses" :key="studyClass.id">
            <router-link :to="`/studyclass/${studyClass.id}`" class="item-link">
              {{ studyClass.name }}
            </router-link>
            <span class="item-meta">{{ studyClass.students?.length || 0 }} elever</span>
          </li>
        </ul>
      </div>

      <div class="content-card">
        <h2>Dine elever ({{ students.length }})</h2>
        <p v-if="loading" class="muted">Henter elever...</p>
        <p v-else-if="students.length === 0" class="muted">Du har ingen elever endnu.</p>
        <ul v-else class="overview-list">
          <li v-for="student in students" :key="student.id">
            <span>{{ student.firstName }} {{ student.lastName }}</span>
            <span class="item-meta">{{ student.email }}</span>
          </li>
        </ul>
      </div>

      <div class="content-card full-width">
        <h2>Hurtige handlinger</h2>
        <div class="quick-actions">
          <router-link to="/create-studyclass" class="action-btn">
            <span>🏫</span> Opret klasse
          </router-link>
          <router-link to="/register-student" class="action-btn">
            <span>👨‍🎓</span> Opret elev
          </router-link>
          <router-link to="/create-task" class="action-btn">
            <span>📝</span> Opret opgave
          </router-link>
          <router-link to="/create-taskset" class="action-btn">
            <span>📚</span> Opret opgavesæt
          </router-link>
          <router-link to="/assigned-submissions" class="action-btn">
            <span>✅</span> Tildel og bedøm afleveringer
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { GetTeacherStudents, GetTeacherStudyClasses } from '../Services/api';

const loading = ref(false);
const errorMessage = ref('');
const studyClasses = ref([]);
const students = ref([]);

async function loadOverview() {
  loading.value = true;
  errorMessage.value = '';

  try {
    const [classData, studentData] = await Promise.all([
      GetTeacherStudyClasses(),
      GetTeacherStudents()
    ]);

    studyClasses.value = Array.isArray(classData) ? classData : [];
    students.value = Array.isArray(studentData) ? studentData : [];
  } catch (error) {
    errorMessage.value = error?.message || 'Kunne ikke hente dashboard data.';
    studyClasses.value = [];
    students.value = [];
  } finally {
    loading.value = false;
  }
}

onMounted(async () => {
  await loadOverview();
});
</script>

<style scoped>
.dashboard {
  height: calc(100vh - 52px);
  background: #f8fafc;
  padding: 2rem;
  overflow: hidden;
}

.dashboard-header {
  max-width: 1200px;
  margin: 0 auto 2rem;
  padding: 2rem;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  border-radius: 18px;
  color: #f8fafc;
}

.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  background: rgba(34, 211, 238, 0.15);
  border: 1px solid rgba(34, 211, 238, 0.25);
  border-radius: 999px;
  font-size: 0.85rem;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: #22d3ee;
  margin-bottom: 0.5rem;
}

.dashboard-header h1 {
  font-size: clamp(1.8rem, 3vw, 2.4rem);
  margin: 0.5rem 0;
}

.subtitle {
  color: #94a3b8;
  font-size: 1rem;
  margin: 0;
}

.dashboard-content {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.full-width {
  grid-column: 1 / -1;
}

.content-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06);
  border: 1px solid rgba(15, 23, 42, 0.06);
}

.content-card h2 {
  margin: 0 0 1.2rem;
  font-size: 1.2rem;
  color: #0f172a;
}

.muted {
  color: #94a3b8 !important;
  font-size: 0.9rem;
}

.status {
  max-width: 1200px;
  margin: 0 auto 1rem;
  border-radius: 8px;
  padding: 0.65rem;
}

.status.error {
  background: #fee8e8;
  color: #7f1d1d;
}

.overview-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.55rem;
}

.overview-list li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.6rem;
  background: #f8fafc;
  border: 1px solid rgba(15, 23, 42, 0.08);
  border-radius: 10px;
  padding: 0.7rem 0.8rem;
}

.item-link {
  color: #0f172a;
  text-decoration: none;
  font-weight: 600;
}

.item-link:hover {
  color: #0f766e;
}

.item-meta {
  color: #64748b;
  font-size: 0.88rem;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  background: #f8fafc;
  border: 1px solid rgba(15, 23, 42, 0.08);
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 500;
  color: #0f172a;
  cursor: pointer;
  transition: background 0.15s ease, border-color 0.15s ease;
  text-decoration: none;
}

.action-btn:hover {
  background: #f1f5f9;
  border-color: rgba(34, 211, 238, 0.3);
}

.action-btn span {
  font-size: 1.1rem;
}

@media (max-width: 768px) {
  .dashboard {
    padding: 1rem;
  }

  .dashboard-content {
    grid-template-columns: 1fr;
  }

  .dashboard-header {
    padding: 1.5rem;
  }
}
</style>

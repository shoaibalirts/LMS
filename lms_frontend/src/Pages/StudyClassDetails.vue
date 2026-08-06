<template>
  <div class="page-shell">
    <div class="container">
      <div class="header">
        <div>
          <h1>{{ studyClass?.name || 'Klasse' }}</h1>
          <p class="subtitle">Administrer elever for denne klasse.</p>
        </div>
      </div>

      <p v-if="errorMessage" class="status error">{{ errorMessage }}</p>
      <p v-if="successMessage" class="status ok">{{ successMessage }}</p>

      <section class="card">
        <h2>Tilføj elever til denne klasse</h2>

        <div v-if="loading" class="helper">Henter data...</div>

        <div v-else-if="availableStudents.length === 0" class="helper">
          Alle dine elever er allerede i denne klasse, eller du har ingen elever endnu.
        </div>

        <div v-else class="assign-panel">
          <ul>
            <li v-for="student in availableStudents" :key="student.id" class="student-row">
              <label class="student-checkbox">
                <input type="checkbox" :value="String(student.id)" v-model="selectedStudentIds" />
                <span>{{ student.firstName }} {{ student.lastName }} · {{ student.email }}</span>
              </label>
            </li>
          </ul>

          <button @click="addSelectedStudents" :disabled="selectedStudentIds.length === 0 || loadingAction">
            {{ loadingAction ? 'Tilføjer...' : 'Tilføj valgte elever' }}
          </button>
        </div>
      </section>

      <section class="card">
        <h2>Elever i denne klasse ({{ assignedStudents.length }})</h2>
        <p v-if="assignedStudents.length === 0" class="helper">Ingen elever i denne klasse endnu.</p>
        <ul v-else>
          <li v-for="student in assignedStudents" :key="student.id">
            {{ student.firstName }} {{ student.lastName }} · {{ student.email }}
          </li>
        </ul>
      </section>
    </div>
  </div>
</template>

<script setup>
// The page of a specific class, showing assigned students and allowing adding more students to the class
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { AddStudentsToStudyClass, GetStudyClassById, GetTeacherStudents } from '../Services/api';

const route = useRoute();
const loading = ref(false);
const loadingAction = ref(false);
const studyClass = ref(null);
const teacherStudents = ref([]);
const selectedStudentIds = ref([]);
const errorMessage = ref('');
const successMessage = ref('');

const assignedStudents = computed(() => studyClass.value?.students || []);

const assignedIdSet = computed(
  () => new Set(assignedStudents.value.map((student) => student.id))
);

const availableStudents = computed(() =>
  teacherStudents.value.filter((student) => !assignedIdSet.value.has(student.id))
);

async function loadData() {
  loading.value = true;
  errorMessage.value = '';

  try {
    const classId = Number(route.params.id);
    const [classData, students] = await Promise.all([
      GetStudyClassById(classId),
      GetTeacherStudents()
    ]);

    studyClass.value = classData || null;
    teacherStudents.value = Array.isArray(students) ? students : [];

    const allowedIds = new Set(availableStudents.value.map((student) => String(student.id)));
    selectedStudentIds.value = selectedStudentIds.value.filter((id) => allowedIds.has(String(id)));
  } catch (error) {
    errorMessage.value = error?.message || 'Kunne ikke hente klasse data.';
    studyClass.value = null;
    teacherStudents.value = [];
    selectedStudentIds.value = [];
  } finally {
    loading.value = false;
  }
}

async function addSelectedStudents() {
  errorMessage.value = '';
  successMessage.value = '';

  if (selectedStudentIds.value.length === 0) {
    errorMessage.value = 'Vælg mindst en elev først.';
    return;
  }

  loadingAction.value = true;
  try {
    const classId = Number(route.params.id);
    const ids = selectedStudentIds.value.map((id) => Number(id));

    await AddStudentsToStudyClass(classId, ids);
    selectedStudentIds.value = [];
    successMessage.value = 'Elever blev tilføjet til klassen.';
    await loadData();
  } catch (error) {
    errorMessage.value = error?.message || 'Kunne ikke tilføje elever til klassen.';
  } finally {
    loadingAction.value = false;
  }
}

onMounted(async () => {
  await loadData();
});
</script>

<style scoped>
.page-shell {
  min-height: calc(100vh - 52px);
  background: #f8fafc;
  padding: 2rem;
}

.container {
  max-width: 950px;
  margin: 0 auto;
  display: grid;
  gap: 1rem;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
}

h1 {
  margin: 0;
  color: #0f172a;
}

.subtitle {
  margin: 0.3rem 0 0;
  color: #64748b;
}

.card {
  background: #fff;
  border: 1px solid #d8e5dc;
  border-radius: 14px;
  box-shadow: 0 8px 20px rgba(15, 23, 42, 0.08);
  padding: 1rem;
}

h2 {
  margin: 0 0 0.6rem;
}

.helper {
  color: #5e6c77;
}

.assign-panel {
  display: grid;
  gap: 0.7rem;
}

ul {
  margin: 0;
  padding-left: 1rem;
}

.student-row {
  margin-bottom: 0.4rem;
}

.student-checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.student-checkbox input[type='checkbox'] {
  width: 16px;
  height: 16px;
}

button {
  border: none;
  background: #1f8a70;
  color: #fff;
  font-weight: 600;
  cursor: pointer;
  border-radius: 10px;
  padding: 0.65rem 0.9rem;
  width: fit-content;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.status {
  border-radius: 8px;
  padding: 0.65rem;
}

.status.ok {
  background: #e7f8ef;
  color: #14532d;
}

.status.error {
  background: #fee8e8;
  color: #7f1d1d;
}

@media (max-width: 800px) {
  .page-shell {
    padding: 1rem;
  }

  .header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>

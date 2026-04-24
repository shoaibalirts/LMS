<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <div>
        <p class="badge">Elevportal</p>
        <h1>Velkommen til dit dashboard</h1>
        <p class="subtitle">Hold styr på opgaver, opgavesæt og din læring</p>
      </div>
    </div>

    <p v-if="errorMessage" class="status error">{{ errorMessage }}</p>

    <div class="dashboard-content">
      <div class="content-card">
        <h2>Din profil</h2>
        <div class="profile-grid">
          <p><strong>E-mail:</strong> {{ studentEmail || 'Ikke tilgængelig' }}</p>
        </div>
      </div>

      <div class="content-card">
        <h2>Dine opgaver</h2>
        <p v-if="loadingAssigned" class="muted">Henter tildelte opgaver...</p>
        <p v-else-if="assignedSets.length === 0" class="muted">Du har ingen tildelte opgavesæt endnu.</p>

        <div v-else class="sets-list">
          <article v-for="set in assignedSets" :key="set.id" class="set-card">
            <div class="set-head">
              <p><strong>Tildelt:</strong> {{ formatDate(set.dateOfAssigned || set.DateOfAssigned) }}</p>
              <p><strong>Deadline:</strong> {{ formatDate(set.deadline || set.Deadline) }}</p>
            </div>

            <ul class="overview-list">
              <li
                v-for="assignment in set.assignedAssignments || set.AssignedAssignments || []"
                :key="assignment.id"
              >
                <div>
                  <p>
                    <strong>{{ assignment.assignmentSubject || assignment.AssignmentSubject }}</strong>
                    · {{ assignment.assignmentType || assignment.AssignmentType }}
                    · Niveau {{ assignment.classLevel || assignment.ClassLevel }}
                    · {{ assignment.assignmentPoints || assignment.AssignmentPoints }} point
                  </p>
                  <p class="item-meta" v-if="assignment.feedback || assignment.Feedback">
                    Feedback: {{ assignment.feedback || assignment.Feedback }}
                  </p>
                  <p class="item-meta" v-if="assignment.studentResultFileName || assignment.StudentResultFileName">
                    Uploadet: {{ assignment.studentResultFileName || assignment.StudentResultFileName }}
                  </p>
                </div>

                <div class="upload-box">
                  <input
                    type="file"
                    accept="application/pdf,.pdf"
                    @change="onFileSelected(assignment.id, $event)"
                    :disabled="isPastDeadline(set.deadline || set.Deadline) || uploadingAssignmentId === assignment.id"
                  />
                  <button
                    @click="submitAssignment(assignment.id)"
                    :disabled="!selectedFiles[assignment.id] || isPastDeadline(set.deadline || set.Deadline) || uploadingAssignmentId === assignment.id"
                  >
                    {{ uploadingAssignmentId === assignment.id ? 'Uploader...' : 'Upload PDF' }}
                  </button>
                  <p v-if="isPastDeadline(set.deadline || set.Deadline)" class="item-meta deadline-warning">
                    Deadline er overskredet
                  </p>
                </div>
              </li>
            </ul>
          </article>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { GetStudentAssignedAssignmentSets, UploadAssignedAssignmentResult, getAuthSession } from '../Services/api';

const errorMessage = ref('');
const loadingAssigned = ref(false);
const uploadingAssignmentId = ref(null);
const assignedSets = ref([]);
const selectedFiles = ref({});
const auth = getAuthSession();

if (!(auth?.token || auth?.Token)) {
  errorMessage.value = 'Mangler login-session. Log venligst ind igen.';
}

const studentEmail = computed(() => auth?.email || auth?.Email || '');

function formatDate(value) {
  if (!value) return '-';
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return value;
  }
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) return value;
  return parsed.toLocaleDateString();
}

function isPastDeadline(deadline) {
  if (!deadline) return false;
  if (/^\d{4}-\d{2}-\d{2}$/.test(deadline)) {
    const [year, month, day] = deadline.split('-').map(Number);
    const endOfDay = new Date(year, month - 1, day, 23, 59, 59, 999);
    return Date.now() > endOfDay.getTime();
  }

  const parsed = new Date(deadline);
  if (Number.isNaN(parsed.getTime())) return false;
  parsed.setHours(23, 59, 59, 999);
  return Date.now() > parsed.getTime();
}

function onFileSelected(assignedAssignmentId, event) {
  const file = event?.target?.files?.[0] || null;
  selectedFiles.value = {
    ...selectedFiles.value,
    [assignedAssignmentId]: file
  };
}

async function loadAssignedSets() {
  loadingAssigned.value = true;
  try {
    const sets = await GetStudentAssignedAssignmentSets();
    assignedSets.value = Array.isArray(sets) ? sets : [];
  } catch (error) {
    errorMessage.value = error?.message || 'Kunne ikke hente tildelte opgaver.';
    assignedSets.value = [];
  } finally {
    loadingAssigned.value = false;
  }
}

async function submitAssignment(assignedAssignmentId) {
  const file = selectedFiles.value[assignedAssignmentId];
  if (!file) {
    errorMessage.value = 'Vælg en PDF-fil først.';
    return;
  }

  if (!file.name.toLowerCase().endsWith('.pdf')) {
    errorMessage.value = 'Kun PDF-filer er tilladt.';
    return;
  }

  uploadingAssignmentId.value = assignedAssignmentId;
  errorMessage.value = '';

  try {
    await UploadAssignedAssignmentResult(assignedAssignmentId, file);
    selectedFiles.value = {
      ...selectedFiles.value,
      [assignedAssignmentId]: null
    };
    await loadAssignedSets();
  } catch (error) {
    errorMessage.value = error?.message || 'Upload fejlede.';
  } finally {
    uploadingAssignmentId.value = null;
  }
}

onMounted(loadAssignedSets);
</script>

<style scoped>
.dashboard {
  min-height: calc(100vh - 52px);
  background: #f8fafc;
  padding: 2rem;
  overflow-y: auto;
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
  text-transform: uppercase;
  font-size: 0.85rem;
  letter-spacing: 0.05em;
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
  grid-template-columns: 1fr;
  gap: 1.5rem;
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

.profile-grid {
  display: grid;
  gap: 0.5rem;
}

.profile-grid p {
  margin: 0;
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

.sets-list {
  display: grid;
  gap: 1rem;
}

.set-card {
  border: 1px solid rgba(15, 23, 42, 0.1);
  border-radius: 12px;
  padding: 0.8rem;
  background: #ffffff;
}

.set-head {
  display: flex;
  justify-content: space-between;
  gap: 0.8rem;
  margin-bottom: 0.7rem;
  color: #0f172a;
}

.overview-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.55rem;
}

.overview-list li {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: start;
  gap: 0.6rem;
  background: #f8fafc;
  border: 1px solid rgba(15, 23, 42, 0.08);
  border-radius: 10px;
  padding: 0.7rem 0.8rem;
}

.item-meta {
  color: #64748b;
  font-size: 0.88rem;
}

.upload-box {
  display: grid;
  gap: 0.4rem;
}

.upload-box input,
.upload-box button {
  font-size: 0.85rem;
}

.upload-box button {
  border: none;
  border-radius: 8px;
  padding: 0.45rem 0.65rem;
  background: #0f766e;
  color: #f8fafc;
  cursor: pointer;
}

.upload-box button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.deadline-warning {
  color: #b91c1c;
}

@media (max-width: 768px) {
  .dashboard {
    padding: 1rem;
  }

  .dashboard-header {
    padding: 1.5rem;
  }

  .overview-list li {
    grid-template-columns: 1fr;
  }

  .set-head {
    flex-direction: column;
  }
}
</style>
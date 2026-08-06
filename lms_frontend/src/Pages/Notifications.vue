<template>
  <div class="notifications-page">
    <div class="page-head">
      <div class="head-row">
        <div>
          <h1>Notifications</h1>
          <p class="subtitle">Her kan du se beskeder om tildelinger, afleveringer og feedback.</p>
        </div>

        <button class="ghost" :disabled="loading" @click="loadNotifications">
          {{ loading ? 'Opdaterer...' : 'Opdater' }}
        </button>
      </div>
    </div>

    <p v-if="status" :class="['status', statusType]">{{ status }}</p>

    <section class="panel">
      <h2>Dine notifikationer</h2>

      <p v-if="loading" class="muted">Henter notifikationer...</p>
      <p v-else-if="notifications.length === 0" class="muted">Ingen notifikationer endnu.</p>

      <ul v-else class="notification-list">
        <li v-for="n in notifications" :key="n.id || n.Id" class="notification-item">
          <div class="item-main">
            <p class="message">{{ n.message || n.Message }}</p>
            <p class="meta">
              <span>{{ formatRelative(n.createdAtUtc || n.CreatedAtUtc) }}</span>
              <span class="dot">·</span>
              <span>{{ formatDate(n.createdAtUtc || n.CreatedAtUtc) }}</span>
              <template v-if="n.assignedAssignmentSetId || n.AssignedAssignmentSetId">
                <span class="dot">·</span>
                <span>Opgavesæt #{{ n.assignedAssignmentSetId || n.AssignedAssignmentSetId }}</span>
              </template>
              <template v-if="n.assignedAssignmentId || n.AssignedAssignmentId">
                <span class="dot">·</span>
                <span>Opgave #{{ n.assignedAssignmentId || n.AssignedAssignmentId }}</span>
              </template>
            </p>
          </div>
        </li>
      </ul>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { GetMyNotifications } from '../Services/api';

const loading = ref(false);
const status = ref('');
const statusType = ref('ok');
const notifications = ref([]);

function formatDate(value) {
  if (!value) return '-';
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) return String(value);
  return parsed.toLocaleString();
}

async function loadNotifications() {
  loading.value = true;
  status.value = '';

  try {
    const data = await GetMyNotifications();
    notifications.value = Array.isArray(data) ? data : [];
  } catch (error) {
    notifications.value = [];
    status.value = error?.message || 'Kunne ikke hente notifikationer.';
    statusType.value = 'error';
  } finally {
    loading.value = false;
  }
}

function formatRelative(value) {
  if (!value) return '';
  const created = new Date(value);
  if (Number.isNaN(created.getTime())) return '';
  const diffMs = Date.now() - created.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return 'Lige nu';
  if (diffMin < 60) return `${diffMin} min siden`;
  const diffHours = Math.floor(diffMin / 60);
  if (diffHours < 24) return `${diffHours} t siden`;
  const diffDays = Math.floor(diffHours / 24);
  return `${diffDays} dage siden`;
}

onMounted(loadNotifications);
</script>

<style scoped>
.notifications-page {
  min-height: calc(100vh - 52px);
  overflow-y: auto;
  background: linear-gradient(170deg, #f2fbf5 0%, #eef5ff 50%, #fff8ed 100%);
  padding: 1.2rem;
}

.page-head {
  max-width: 1200px;
  margin: 0 auto 1rem;
}

.head-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.subtitle {
  color: #475569;
}

.panel {
  max-width: 1200px;
  margin: 0 auto 1rem;
  border-radius: 14px;
  border: 1px solid #dce7e1;
  background: #fff;
  box-shadow: 0 8px 18px rgba(15, 23, 42, 0.07);
  padding: 1rem;
}

.ghost {
  border: 1px solid #cbd5e1;
  background: #f1f5f9;
  color: #1e293b;
  border-radius: 10px;
  padding: 0.6rem 0.9rem;
  cursor: pointer;
}

.ghost:disabled {
  opacity: 0.6;
  cursor: default;
}

.notification-list {
  list-style: none;
  padding: 0;
  margin: 0.75rem 0 0;
  display: grid;
  gap: 0.6rem;
}

.notification-item {
  border: 1px solid #dbe5e0;
  border-radius: 12px;
  padding: 0.8rem;
  background: #fff;
}

.item-main {
  display: grid;
  gap: 0.25rem;
}

.message {
  margin: 0 0 0.35rem;
  color: #0f172a;
}

.meta {
  margin: 0;
  color: #475569;
  font-size: 0.9rem;
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}

.dot {
  opacity: 0.7;
}

.status {
  max-width: 1200px;
  margin: 0 auto 1rem;
  border-radius: 8px;
  padding: 0.65rem;
  border: 1px solid #dbe5e0;
  background: #fff;
}

.status.ok {
  color: #0f172a;
}

.status.error {
  color: #0f172a;
}

.muted {
  color: #475569;
}
</style>

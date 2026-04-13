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
        <p class="muted">Dine opgaver vises her, når de bliver tildelt af din lærer. (ikke implenenteret)</p>
      </div>
        
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import { getAuthSession } from '../Services/api';

const errorMessage = ref('');
const auth = getAuthSession();

if (!(auth?.token || auth?.Token)) {
  errorMessage.value = 'Mangler login-session. Log venligst ind igen.';
}

const studentEmail = computed(() => auth?.email || auth?.Email || '');
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

.item-meta {
  color: #64748b;
  font-size: 0.88rem;
}

@media (max-width: 768px) {
  .dashboard {
    padding: 1rem;
    height: auto;
    min-height: calc(100vh - 52px);
  }

  .dashboard-content {
    grid-template-columns: 1fr;
  }

  .dashboard-header {
    padding: 1.5rem;
  }
}
</style>
<template>
  <div class="page-shell">
    <div class="container">
      <div class="header">
        <h1>Opret klasse</h1>
        <p class="subtitle">Opret en ny klasse.</p>
      </div>

      <form class="card" @submit.prevent="handleCreate" novalidate>
        <label class="field">
          <span>Klassenavn</span>
          <input
            v-model="className"
            type="text"
            maxlength="100"
            placeholder="Fx Matematik A - Hold 1"
            :class="{ invalid: errorMessage }"
          />
        </label>

        <div class="actions">
          <button type="submit" :disabled="loading">
            {{ loading ? 'Opretter...' : 'Opret klasse' }}
          </button>
        </div>

        <p v-if="errorMessage" class="status error">{{ errorMessage }}</p>
        <p v-if="successMessage" class="status ok">{{ successMessage }}</p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { CreateStudyClass } from '../Services/api';

const className = ref('');
const loading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

async function handleCreate() {
  errorMessage.value = '';
  successMessage.value = '';

  const trimmedName = className.value.trim();
  if (!trimmedName) {
    errorMessage.value = 'Klassenavn er påkrævet.';
    return;
  }

  loading.value = true;
  try {
    const created = await CreateStudyClass(trimmedName);
    className.value = '';
    successMessage.value = `Klassen "${created?.name || trimmedName}" blev oprettet.`;
  } catch (error) {
    errorMessage.value = error?.message || 'Kunne ikke oprette klassen.';
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.page-shell {
  min-height: calc(100vh - 52px);
  background: #f8fafc;
  padding: 2rem;
}

.container {
  max-width: 900px;
  margin: 0 auto;
  display: grid;
  gap: 1rem;
}

.header h1 {
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

.field {
  display: grid;
  gap: 0.4rem;
}

input {
  border-radius: 10px;
  border: 1px solid #c7d8ce;
  padding: 0.65rem 0.75rem;
}

input.invalid {
  border-color: #b91c1c;
}

.actions {
  margin-top: 0.8rem;
}

button {
  border: none;
  background: #1f8a70;
  color: #fff;
  font-weight: 600;
  cursor: pointer;
  border-radius: 10px;
  padding: 0.65rem 0.9rem;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.status {
  margin-top: 0.75rem;
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
}
</style>

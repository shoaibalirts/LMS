<template>
  <div class="page-shell">
    <section class="login-layout">
      <div class="welcome">
        <p class="badge">LMS Portal</p>
        <h1>Velkommen tilbage</h1>
        <p class="lede">
          Log ind som elev for at se dine opgaver, følge dit arbejde og holde styr på dine klasser.
        </p>
        <ul>
          <li>Sikker login for elever</li>
          <li>Få adgang til dit elev-dashboard</li>
          <li>Se opgavesæt og kommende aktiviteter</li>
        </ul>
      </div>

      <form class="card" @submit.prevent="handleLogin" novalidate>
        <div class="header">
          <div class="circle"></div>
          <div>
            <p class="eyebrow">Elev login</p>
            <h2>Få adgang til din konto</h2>
          </div>
        </div>

        <label class="field">
          <span>E-mail</span>
          <input
            v-model="email"
            type="email"
            placeholder="dig@eksempel.dk"
            :class="{ invalid: errors.email }"
            autocomplete="username"
            required
          />
          <small v-if="errors.email">{{ errors.email }}</small>
        </label>

        <label class="field">
          <span>Adgangskode</span>
          <input
            v-model="password"
            type="password"
            placeholder="••••••••"
            :class="{ invalid: errors.password }"
            autocomplete="current-password"
            required
          />
          <small v-if="errors.password">{{ errors.password }}</small>
        </label>

        <div class="actions">
          <router-link to="/" class="back-link">Tilbage til forsiden</router-link>
          <button type="submit" :disabled="loading">
            {{ loading ? 'Logger ind...' : 'Log ind' }}
          </button>
        </div>

        <p v-if="status" :class="statusClass">
          {{ statusMessage }}
        </p>
      </form>
    </section>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { LoginStudent, setAuthSession } from '../Services/api';
import { useRouter } from 'vue-router';

const email = ref('');
const password = ref('');
const router = useRouter();
const errors = ref({});
const status = ref('');
const loading = ref(false);

const statusMessage = computed(() => {
  if (status.value === 'success') {
    return 'Signed in successfully! Redirecting...';
  }
  if (status.value === 'invalid') {
    return 'Invalid email or password. Please try again.';
  }
  if (status.value === 'error') {
    return 'An error occurred. Please try again.';
  }
  return '';
});

const statusClass = computed(() => {
  if (status.value === 'success') return 'status success';
  return 'status error';
});

const validate = () => {
  const nextErrors = {};
  if (!email.value) {
    nextErrors.email = 'Email is required';
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
    nextErrors.email = 'Enter a valid email address';
  }

  if (!password.value) {
    nextErrors.password = 'Password is required';
  } else if (password.value.length < 6) {
    nextErrors.password = 'Minimum 6 characters';
  }

  errors.value = nextErrors;
  return Object.keys(nextErrors).length === 0;
};

async function handleLogin() {
  status.value = '';

  if (!validate()) return;

  loading.value = true;

  try {
    const data = await LoginStudent(email.value, password.value);

    if (!data?.token && !data?.Token) {
      status.value = 'invalid';
      return;
    }

    setAuthSession({ ...data, role: 'Student' });
    status.value = 'success';

    setTimeout(() => {
      router.push('/student-dashboard');
    }, 500);
  } catch (error) {
    console.error('Student login failed:', error);
    status.value = error.message?.toLowerCase().includes('invalid') || error.message?.toLowerCase().includes('unauthorized')
      ? 'invalid'
      : 'error';
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.page-shell {
  width: 100%;
  height: calc(100vh - 52px);
  padding: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: radial-gradient(circle at 20% 20%, rgba(58, 123, 213, 0.15), transparent 35%),
    radial-gradient(circle at 80% 0%, rgba(255, 166, 43, 0.18), transparent 30%),
    linear-gradient(135deg, #0f172a 0%, #0b1324 45%, #0f172a 100%);
}

.login-layout {
  width: min(1200px, 100%);
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 2.5rem;
  align-items: center;
}

.welcome {
  color: #e2e8f0;
  padding: 1rem;
}

.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 999px;
  font-size: 0.85rem;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.welcome h1 {
  font-size: clamp(2rem, 3vw, 2.6rem);
  margin: 0.8rem 0;
  color: #f8fafc;
}

.lede {
  color: #cbd5e1;
  font-size: 1.05rem;
  max-width: 46ch;
  line-height: 1.7;
}

.welcome ul {
  margin-top: 1.25rem;
  list-style: none;
  padding: 0;
  color: #cbd5e1;
}

.welcome li {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.4rem 0;
}

.welcome li::before {
  content: '•';
  color: #22d3ee;
  font-weight: bold;
}

.card {
  background: #0b1222;
  color: #e2e8f0;
  padding: 2rem;
  border-radius: 18px;
  box-shadow: 0 18px 45px rgba(0, 0, 0, 0.45), 0 0 0 1px rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.header {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 1rem;
  align-items: center;
  margin-bottom: 1.5rem;
}

.circle {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: radial-gradient(circle at 30% 30%, #22d3ee, #0ea5e9);
  box-shadow: 0 10px 25px rgba(34, 211, 238, 0.3);
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-size: 0.8rem;
  color: #94a3b8;
  margin: 0;
}

h2 {
  margin: 0.2rem 0;
  font-size: 1.6rem;
  color: #f8fafc;
}

.field {
  display: grid;
  gap: 0.35rem;
  margin-bottom: 1rem;
}

.field span {
  font-size: 0.9rem;
  color: #cbd5e1;
}

input {
  width: 100%;
  border-radius: 10px;
  border: 1px solid rgba(148, 163, 184, 0.35);
  background: rgba(15, 23, 42, 0.45);
  color: #f8fafc;
  padding: 0.75rem 0.85rem;
  outline: none;
}

input:focus {
  border-color: #22d3ee;
  box-shadow: 0 0 0 3px rgba(34, 211, 238, 0.2);
}

input.invalid {
  border-color: #f87171;
}

small {
  color: #fca5a5;
}

.actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  margin-top: 0.5rem;
}

.back-link {
  color: #94a3b8;
  text-decoration: none;
  font-size: 0.9rem;
}

.back-link:hover {
  color: #22d3ee;
}

button {
  border: none;
  border-radius: 10px;
  padding: 0.7rem 1.2rem;
  font-weight: 700;
  color: #0f172a;
  background: linear-gradient(135deg, #22d3ee, #67e8f9);
  cursor: pointer;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.status {
  border-radius: 10px;
  padding: 0.75rem 0.85rem;
  margin-top: 1rem;
  font-size: 0.92rem;
}

.status.success {
  background: rgba(22, 163, 74, 0.2);
  color: #86efac;
  border: 1px solid rgba(34, 197, 94, 0.35);
}

.status.error {
  background: rgba(220, 38, 38, 0.2);
  color: #fecaca;
  border: 1px solid rgba(248, 113, 113, 0.35);
}

@media (max-width: 960px) {
  .page-shell {
    height: auto;
    min-height: calc(100vh - 52px);
    padding: 1.2rem;
    overflow: auto;
  }

  .login-layout {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .card {
    padding: 1.4rem;
  }
}
</style>
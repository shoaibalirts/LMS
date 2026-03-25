<template>
  <div class="page-shell">
    <section class="register-layout">
      <div class="welcome">
        <p class="badge">LMS Portal</p>
        <h1>Create your account</h1>
        <p class="lede">
          Join our learning management system and start managing your courses, students, and educational content.
        </p>
        <ul>
          <li>Create and organize courses</li>
          <li>Track student progress</li>
          <li>Manage assignments and grades</li>
        </ul>
      </div>

      <form class="card" @submit.prevent="handleRegister" novalidate>
        <div class="header">
          <div class="circle"></div>
          <div>
            <p class="eyebrow">Teacher Registration</p>
            <h2>Get started</h2>
          </div>
        </div>

        <div class="name-fields">
          <label class="field">
            <span>First Name</span>
            <input
              v-model="firstname"
              type="text"
              placeholder="John"
              :class="{ invalid: errors.firstname }"
              autocomplete="given-name"
              required
            />
            <small v-if="errors.firstname">{{ errors.firstname }}</small>
          </label>

          <label class="field">
            <span>Last Name</span>
            <input
              v-model="lastname"
              type="text"
              placeholder="Doe"
              :class="{ invalid: errors.lastname }"
              autocomplete="family-name"
              required
            />
            <small v-if="errors.lastname">{{ errors.lastname }}</small>
          </label>
        </div>

        <label class="field">
          <span>Email</span>
          <input
            v-model="email"
            type="email"
            placeholder="you@example.com"
            :class="{ invalid: errors.email }"
            autocomplete="email"
            required
          />
          <small v-if="errors.email">{{ errors.email }}</small>
        </label>

        <label class="field">
          <span>Password</span>
          <input
            v-model="password"
            type="password"
            placeholder="••••••••"
            :class="{ invalid: errors.password }"
            autocomplete="new-password"
            required
          />
          <small v-if="errors.password">{{ errors.password }}</small>
        </label>

        <div class="actions">
          <router-link to="/login" class="back-link">Already have an account?</router-link>
          <button type="submit" :disabled="loading">
            {{ loading ? 'Creating account...' : 'Create account' }}
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
import { RegisterTeacher, LoginTeacher } from '../Services/api';
import { useRouter } from 'vue-router';

const firstname = ref('');
const lastname = ref('');
const email = ref('');
const password = ref('');
const router = useRouter();
const errors = ref({});
const status = ref('');
const loading = ref(false);

const statusMessage = computed(() => {
  if (status.value === 'success') {
    return 'Account created successfully! Redirecting...';
  }
  if (status.value === 'error') {
    return 'Registration failed. Please try again.';
  }
  return '';
});

const statusClass = computed(() => {
  if (status.value === 'success') return 'status success';
  return 'status error';
});

const validate = () => {
  const nextErrors = {};

  if (!firstname.value.trim()) {
    nextErrors.firstname = 'First name is required';
  }

  if (!lastname.value.trim()) {
    nextErrors.lastname = 'Last name is required';
  }

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

async function handleRegister() {
  status.value = '';

  if (!validate()) return;

  loading.value = true;

  try {
    const data = await RegisterTeacher(firstname.value, lastname.value, email.value, password.value);
    console.log('Registration successful:', data);

    const loginData = await LoginTeacher(email.value, password.value);
    console.log('Login successful:', loginData);

    status.value = 'success';

    localStorage.setItem('user', JSON.stringify({
      email: email.value,
      role: 'Teacher'
    }));

    setTimeout(() => {
      router.push('/teacher-dashboard');
    }, 500);
  } catch (error) {
    console.error('Registration/Login failed:', error);
    status.value = 'error';
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

.register-layout {
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
  background: radial-gradient(circle at 30% 30%, #4ade80, #22c55e);
  box-shadow: 0 10px 25px rgba(74, 222, 128, 0.3);
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

.name-fields {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.field {
  display: grid;
  gap: 0.45rem;
  margin-bottom: 1rem;
  font-size: 0.95rem;
}

.field span {
  color: #cbd5e1;
}

input[type='text'],
input[type='email'],
input[type='password'] {
  background: #0f172a;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 12px;
  padding: 0.85rem 1rem;
  color: #e2e8f0;
  font-size: 1rem;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

input:focus {
  outline: none;
  border-color: #4ade80;
  box-shadow: 0 0 0 3px rgba(74, 222, 128, 0.2);
}

.invalid {
  border-color: #fb7185 !important;
  box-shadow: 0 0 0 3px rgba(251, 113, 133, 0.2);
}

small {
  color: #fb7185;
  font-size: 0.85rem;
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
  font-size: 0.95rem;
  transition: color 0.2s ease;
}

.back-link:hover {
  color: #4ade80;
}

button {
  background: linear-gradient(135deg, #4ade80, #22c55e);
  color: #0b1222;
  border: none;
  border-radius: 12px;
  padding: 0.85rem 1.4rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 12px 30px rgba(34, 197, 94, 0.35);
  transition: transform 0.15s ease, box-shadow 0.15s ease, filter 0.15s ease;
}

button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 16px 36px rgba(34, 197, 94, 0.4);
  filter: brightness(1.05);
}

button:active:not(:disabled) {
  transform: translateY(0);
}

button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.status {
  margin-top: 1rem;
  padding: 0.9rem 1rem;
  border-radius: 12px;
  font-weight: 600;
}

.success {
  background: rgba(34, 197, 94, 0.12);
  color: #4ade80;
  border: 1px solid rgba(74, 222, 128, 0.35);
}

.error {
  background: rgba(251, 113, 133, 0.12);
  color: #fca5a5;
  border: 1px solid rgba(251, 113, 133, 0.35);
}

@media (max-width: 960px) {
  .register-layout {
    grid-template-columns: 1fr;
  }

  .welcome {
    text-align: center;
  }

  .welcome ul {
    display: flex;
    flex-direction: column;
    align-items: center;
  }
}

@media (max-width: 560px) {
  .card {
    padding: 1.5rem;
  }

  .name-fields {
    grid-template-columns: 1fr;
  }

  .actions {
    flex-direction: column;
    align-items: stretch;
  }

  .back-link {
    text-align: center;
    order: 2;
    margin-top: 0.5rem;
  }

  button {
    width: 100%;
    text-align: center;
  }
}
</style>

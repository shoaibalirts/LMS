<script setup>
import { computed, ref } from 'vue'

const form = ref({
  email: '',
  password: '',
  remember: false
})

const errors = ref({})
const status = ref('')
const loggedIn = ref(false)

const hardcodedUser = {
  email: 'student@example.com',
  password: 'Password123!'
}

const statusMessage = computed(() => {
  if (status.value === 'success') {
    return 'Signed in successfully. You can now continue to the dashboard.'
  }
  if (status.value === 'invalid') {
    return 'Invalid email or password. Please try again.'
  }
  return ''
})

const statusTone = computed(() =>
  status.value === 'success' ? 'status success' : 'status error'
)

const validate = () => {
  const nextErrors = {}
  if (!form.value.email) {
    nextErrors.email = 'Email is required'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email)) {
    nextErrors.email = 'Enter a valid email address'
  }

  if (!form.value.password) {
    nextErrors.password = 'Password is required'
  } else if (form.value.password.length < 8) {
    nextErrors.password = 'Minimum 8 characters'
  }

  errors.value = nextErrors
  return Object.keys(nextErrors).length === 0
}

const signIn = (event) => {
  event.preventDefault()
  status.value = ''

  if (!validate()) return

  const isValidUser =
    form.value.email === hardcodedUser.email &&
    form.value.password === hardcodedUser.password

  status.value = isValidUser ? 'success' : 'invalid'
  loggedIn.value = isValidUser
}
</script>

<template>
  <div :class="['page-shell', loggedIn ? 'page-shell--home' : 'page-shell--login']">
    <section v-if="!loggedIn" class="login-layout">
      <div class="welcome">
        <p class="badge">LMS Portal</p>
        <h1>Welcome back</h1>
        <p class="lede">
          Sign in to access your learning resources, track progress, and stay on top of upcoming courses.
        </p>
        <ul>
          <li>Secure sign-in ready to wire to the C# backend.</li>
          <li>Clear error feedback and validation.</li>
          <li>Responsive layout for mobile and desktop.</li>
        </ul>
      </div>

      <form class="card" @submit="signIn" novalidate>
        <div class="header">
          <div class="circle"></div>
          <div>
            <p class="eyebrow">Log in</p>
            <h2>Access your account</h2>
            <p class="hint">Use <strong>{{ hardcodedUser.email }}</strong> and <strong>{{ hardcodedUser.password }}</strong></p>
          </div>
        </div>

        <label class="field">
          <span>Email</span>
          <input
            v-model="form.email"
            type="email"
            placeholder="you@example.com"
            :class="{ invalid: errors.email }"
            autocomplete="username"
            required
          />
          <small v-if="errors.email">{{ errors.email }}</small>
        </label>

        <label class="field">
          <span>Password</span>
          <input
            v-model="form.password"
            type="password"
            placeholder="••••••••"
            :class="{ invalid: errors.password }"
            autocomplete="current-password"
            required
          />
          <small v-if="errors.password">{{ errors.password }}</small>
        </label>

        <div class="actions">
          <label class="remember">
            <input v-model="form.remember" type="checkbox" />
            <span>Keep me signed in</span>
          </label>
          <button type="submit">Sign in</button>
        </div>

        <p v-if="status" :class="statusTone">
          {{ statusMessage }}
        </p>
      </form>
    </section>

    <section v-else class="home">
      <div class="home-card">
        <header class="home-header">
          <div>
            <p class="badge badge-light">LMS Portal</p>
            <h1>Welcome, Student</h1>
            <p class="lede home-lede">
              You’re signed in. This is a blank dashboard shell—connect it to your C# backend to load courses, progress, and announcements.
            </p>
          </div>
          <div class="home-actions">
            <button class="primary">Go to Courses</button>
            <button class="ghost" @click="loggedIn = false">Sign out</button>
          </div>
        </header>

        <div class="home-blank">
          <div>
            <p class="muted">Your dashboard is ready for content.</p>
            <p class="muted">Add tiles for courses, assignments, and announcements when connected to the backend.</p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.page-shell {
  width: 100%;
  min-height: 100vh;
  padding: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.3s ease;
}

.page-shell--login {
  background: radial-gradient(circle at 20% 20%, rgba(58, 123, 213, 0.15), transparent 35%),
    radial-gradient(circle at 80% 0%, rgba(255, 166, 43, 0.18), transparent 30%),
    linear-gradient(135deg, #0f172a 0%, #0b1324 45%, #0f172a 100%);
}

.page-shell--home {
  background: #f8fafc;
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
}

h2 {
  margin: 0.2rem 0;
  font-size: 1.6rem;
  color: #f8fafc;
}

.hint {
  color: #cbd5e1;
  font-size: 0.95rem;
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
  border-color: #22d3ee;
  box-shadow: 0 0 0 3px rgba(34, 211, 238, 0.2);
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

.remember {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: #cbd5e1;
  font-size: 0.95rem;
}

button {
  background: linear-gradient(135deg, #22d3ee, #6366f1);
  color: #0b1222;
  border: none;
  border-radius: 12px;
  padding: 0.85rem 1.4rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 12px 30px rgba(99, 102, 241, 0.35);
  transition: transform 0.15s ease, box-shadow 0.15s ease, filter 0.15s ease;
}

button:hover {
  transform: translateY(-1px);
  box-shadow: 0 16px 36px rgba(99, 102, 241, 0.4);
  filter: brightness(1.02);
}

button:active {
  transform: translateY(0);
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

.home {
  width: 100%;
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 1rem;
}

.home-card {
  width: min(720px, 95%);
  background: #ffffff;
  border-radius: 20px;
  padding: 2.5rem;
  color: #0f172a;
  box-shadow: 0 25px 60px rgba(15, 23, 42, 0.12);
  border: 1px solid rgba(15, 23, 42, 0.06);
  text-align: left;
}

.home-header {
  display: flex;
  gap: 1.5rem;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
}

.home-card h1 {
  margin: 0.6rem 0 0.8rem;
  font-size: clamp(2rem, 3vw, 2.6rem);
  color: #0f172a;
}

.home-card .lede {
  color: #475569;
  line-height: 1.8;
}

.home-actions {
  margin-top: 1.8rem;
  display: flex;
  gap: 0.8rem;
  flex-wrap: wrap;
  align-items: center;
}

.home-actions .primary {
  background: linear-gradient(135deg, #22d3ee, #6366f1);
  color: #0b1222;
  padding: 0.95rem 1.6rem;
  border-radius: 12px;
  border: none;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 12px 30px rgba(99, 102, 241, 0.35);
}

.home-actions .ghost {
  background: transparent;
  color: #0f172a;
  border: 1px solid rgba(15, 23, 42, 0.12);
  padding: 0.95rem 1.4rem;
  border-radius: 12px;
  cursor: pointer;
  transition: border-color 0.15s ease, color 0.15s ease;
}

.home-actions .ghost:hover {
  border-color: rgba(34, 211, 238, 0.5);
  color: #0ea5e9;
}

.home-grid {
  margin-top: 1.8rem;
  display: grid;
  gap: 1rem;
  grid-template-columns: 1fr;
}

.home-blank {
  background: #f8fafc;
  border: 1px dashed rgba(15, 23, 42, 0.18);
  border-radius: 14px;
  padding: 1.4rem 1.2rem;
  color: #475569;
}

.muted {
  margin: 0.2rem 0;
  color: #64748b;
}

.badge-light {
  background: #e0f2fe;
  color: #0369a1;
  border-color: #bae6fd;
}

.home-lede {
  max-width: 62ch;
}

@media (max-width: 960px) {
  .login-layout {
    grid-template-columns: 1fr;
  }

  .welcome {
    text-align: center;
  }

  .welcome ul {
    justify-items: center;
  }
}

@media (max-width: 560px) {
  .card {
    padding: 1.5rem;
  }

  .actions {
    flex-direction: column;
    align-items: flex-start;
  }

  button {
    width: 100%;
    text-align: center;
  }
}
</style>

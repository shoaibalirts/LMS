<template>
  <div class="student-register-page">
    <div class="header">
      <h1>Opret elever</h1>
      <p class="subtitle">Opret elever manuelt eller upload CSV i bulk.</p>
    </div>

    <section class="grid">
      <div class="card">
        <h2>Manuel registration</h2>

        <label>
          <span>Fornavn</span>
          <input v-model="manual.firstName" type="text" />
        </label>

        <label>
          <span>Efternavn</span>
          <input v-model="manual.lastName" type="text" />
        </label>

        <label>
          <span>Email</span>
          <input v-model="manual.email" type="email" />
        </label>

        <label>
          <span>Kodeord</span>
          <input v-model="manual.password" type="password" />
        </label>

        <button @click="createManual">Opret elev</button>
      </div>

      <div class="card">
        <h2>CSV upload</h2>
        <p class="helper">Format: Fornavn,Efternavn,Email,Kodeord</p>
        <input type="file" accept=".csv" @change="onCsvSelected" />
        <button @click="createFromCsv" :disabled="!csvFile">Upload CSV</button>

        <p v-if="csvResult" class="csv-result">{{ csvResult }}</p>
        <ul v-if="csvErrors.length" class="csv-errors">
          <li v-for="item in csvErrors" :key="item">{{ item }}</li>
        </ul>
      </div>
    </section>

    <section class="card roster">
      <h2>Oprettede elever ({{ students.length }})</h2>
      <div v-if="students.length === 0" class="helper">Ingen elever er oprettet endnu.</div>
      <ul v-else>
        <li v-for="student in students" :key="student.id" class="student-row">
          <span>{{ student.firstName }} {{ student.lastName }} · {{ student.email }}</span>
        </li>
      </ul>
    </section>

    <p v-if="status" :class="['status', statusType]">{{ status }}</p>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue';
import { RegisterStudent } from '../Services/api';

const manual = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: ''
});

const csvFile = ref(null);
const csvResult = ref('');
const csvErrors = ref([]);
const students = ref([]); // shows newly created students, gone if refresh
const status = ref('');
const statusType = ref('ok');

async function createManual() {
  status.value = '';

  if (!manual.firstName || !manual.lastName || !manual.email || !manual.password) {
    status.value = 'All fields are required.';
    statusType.value = 'error';
    return;
  }

  try {
    const created = await RegisterStudent(
      manual.firstName,
      manual.lastName,
      manual.email,
      manual.password
    );

    students.value.unshift(created);

    manual.firstName = '';
    manual.lastName = '';
    manual.email = '';
    manual.password = '';

    status.value = 'Student created successfully.';
    statusType.value = 'ok';
  } catch (error) {
    status.value = 'Could not create student. Email may already exist.';
    statusType.value = 'error';
  }
}
// Not tested AI implementation 
function onCsvSelected(event) {
  const file = event.target.files?.[0];
  csvFile.value = file || null;
}
// Not tested AI implementation 
async function createFromCsv() {
  status.value = '';
  csvResult.value = '';
  csvErrors.value = [];

  if (!csvFile.value) {
    status.value = 'Select a CSV file first.';
    statusType.value = 'error';
    return;
  }

  const csvText = await csvFile.value.text();
  const rows = csvText
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);

  const created = [];
  let skipped = 0;

  for (let index = 0; index < rows.length; index += 1) {
    const row = rows[index];
    const rowNumber = index + 1;
    const parsed = parseCsvRow(row);
    if (isHeaderRow(parsed)) {
      continue;
    }

    const [firstName, lastName, email, password] = parsed.map((x) => x?.trim());

    if (!firstName || !lastName || !email || !password) {
      skipped += 1;
      csvErrors.value.push(`Row ${rowNumber}: Missing one or more required fields.`);
      continue;
    }

    try {
      const student = await RegisterStudent(firstName, lastName, email, password);
      created.push(student);
    } catch (error) {
      const message = (error?.message || '').toLowerCase();
      const statusCode = error?.status;

      if (statusCode === 401 || message.includes('unauthorized')) {
        status.value = 'Your session has expired. Please log in again.';
        statusType.value = 'error';
        return;
      }

      if (message.includes('failed to fetch') || message.includes('networkerror')) {
        status.value = 'Could not reach the API server. Start the backend and try again.';
        statusType.value = 'error';
        return;
      }

      skipped += 1;
      if (statusCode === 409 || message.includes('already exists')) {
        csvErrors.value.push(`Row ${rowNumber}: Email already exists.`);
      } else if (message.includes('password must be at least 6')) {
        csvErrors.value.push(`Row ${rowNumber}: Password must be at least 6 characters.`);
      } else if (message.includes('invalid email')) {
        csvErrors.value.push(`Row ${rowNumber}: Invalid email format.`);
      } else {
        csvErrors.value.push(`Row ${rowNumber}: Could not create student (${error?.message || 'unknown error'}).`);
      }
    }
  }

  if (created.length > 0) {
    students.value = [...created, ...students.value];
  }

  csvResult.value = `${created.length} created, ${skipped} skipped.`;
  status.value = 'CSV import completed.';
  statusType.value = 'ok';
}

function parseCsvRow(row) {
  const normalized = row.replace(/^\uFEFF/, '');
  const commaCount = (normalized.match(/,/g) || []).length;
  const semicolonCount = (normalized.match(/;/g) || []).length;
  const delimiter = semicolonCount > commaCount ? ';' : ',';

  const values = [];
  let current = '';
  let inQuotes = false;

  for (let i = 0; i < normalized.length; i += 1) {
    const ch = normalized[i];

    if (ch === '"') {
      if (inQuotes && normalized[i + 1] === '"') {
        current += '"';
        i += 1;
      } else {
        inQuotes = !inQuotes;
      }
      continue;
    }

    if (ch === delimiter && !inQuotes) {
      values.push(current);
      current = '';
      continue;
    }

    current += ch;
  }

  values.push(current);
  return values;
}

function isHeaderRow(values) {
  if (!Array.isArray(values) || values.length < 4) {
    return false;
  }

  const normalized = values.slice(0, 4).map((v) => String(v || '').trim().toLowerCase());
  return (normalized[0] === 'fornavn' || normalized[0] === 'firstname')
    && (normalized[1] === 'efternavn' || normalized[1] === 'lastname')
    && normalized[2] === 'email'
    && (normalized[3] === 'kodeord' || normalized[3] === 'password');
}
</script>

<style scoped>
.student-register-page {
  min-height: calc(100vh - 52px);
  overflow-y: auto;
  padding: 1.25rem;
  background: linear-gradient(155deg, #f4f8fc 0%, #ecf8ef 55%, #fffaef 100%);
}

.header {
  max-width: 1050px;
  margin: 0 auto 1rem;
}

.subtitle,
.helper {
  color: #5e6c77;
}

.grid {
  max-width: 1050px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.card {
  background: #fff;
  border: 1px solid #d8e5dc;
  border-radius: 14px;
  box-shadow: 0 8px 20px rgba(15, 23, 42, 0.08);
  padding: 1rem;
  display: grid;
  gap: 0.7rem;
}

label {
  display: grid;
  gap: 0.35rem;
}

input,
button {
  border-radius: 10px;
  border: 1px solid #c7d8ce;
  padding: 0.65rem 0.75rem;
}

button {
  border: none;
  background: #1f8a70;
  color: #fff;
  font-weight: 600;
  cursor: pointer;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.roster {
  max-width: 1050px;
  margin: 1rem auto 0;
}

.roster ul {
  margin: 0;
  padding-left: 1rem;
}

.student-row {
  margin-bottom: 0.4rem;
}

.status {
  max-width: 1050px;
  margin: 1rem auto 0;
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

.csv-result {
  color: #14532d;
  font-weight: 600;
}

.csv-errors {
  margin: 0;
  padding-left: 1rem;
  color: #7f1d1d;
  font-size: 0.9rem;
}

@media (max-width: 900px) {
  .grid {
    grid-template-columns: 1fr;
  }
}
</style>
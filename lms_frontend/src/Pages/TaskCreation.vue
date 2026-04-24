<template>
  <div class="page-shell">
    <div class="task-form-container">
      <div class="form-header">
        <h1>Opret ny opgave</h1>
        <p class="subtitle">
          Udfyld detaljerne nedenfor for at oprette en ny opgave til dine elever
        </p>
      </div>

      <form @submit.prevent="handleSubmit" novalidate>
        <div class="form-sections">
          <!-- Section 1 -->
          <div class="form-section">
            <h2>Opgave indstillinger</h2>

            <div class="field-grid">
              <label class="field">
                <span>Emne F.eks. "Linæer Algebra"</span>
                <input
                  v-model="form.subject"
                  type="text"
                  placeholder="Vælg emne"
                  :class="{ invalid: errors.subject }"
                />
                <small v-if="errors.subject">{{ errors.subject }}</small>
              </label>

              <label class="field">
                <span>Niveau</span>
                <select v-model="form.niveau" :class="{ invalid: errors.niveau }">
                  <option value="">Vælg niveau...</option>
                  <option value="A">A-niveau</option>
                  <option value="B">B-niveau</option>
                  <option value="C">C-niveau</option>
                </select>
                <small v-if="errors.niveau">{{ errors.niveau }}</small>
              </label>

              <label class="field">
                <span>Delprøve</span>
                <select v-model="form.delprove" :class="{ invalid: errors.delprove }">
                  <option value="">Vælg delprøve...</option>
                  <option value="Delprøve 1">Delprøve 1</option>
                  <option value="Delprøve 2">Delprøve 2</option>
                  <option value="Mundtlig">Mundtlig</option>
                </select>
                <small v-if="errors.delprove">{{ errors.delprove }}</small>
              </label>

              <label class="field">
                <span>Point (1-10)</span>
                <input
                  v-model.number="form.point"
                  type="number"
                  min="1"
                  max="10"
                  placeholder="Enter points"
                  :class="{ invalid: errors.point }"
                />
                <small v-if="errors.point">{{ errors.point }}</small>
              </label>
            </div>
          </div>

          <!-- Section 2 -->
          <div class="form-section">
            <h2>Opgave indhold</h2>

            <label class="field">
              <span>Opgavebillede <span class="optional">(jpg, png, gif, webp — maks 10 MB)</span></span>
              <div
                class="file-drop"
                :class="{ invalid: errors.pictureFile, 'has-file': form.pictureFile }"
                @dragover.prevent
                @drop.prevent="onFileDrop"
              >
                <input
                  type="file"
                  accept=".jpg,.jpeg,.png,.gif,.webp"
                  class="file-input"
                  @change="onFileChange"
                />
                <div v-if="imagePreview" class="preview-wrapper">
                  <img :src="imagePreview" class="image-preview" alt="Preview" />
                  <button type="button" class="remove-file" @click.prevent="removeFile">✕</button>
                </div>
                <div v-else class="drop-hint">
                  <span>Træk et billede hertil eller klik for at vælge</span>
                </div>
              </div>
              <small v-if="errors.pictureFile">{{ errors.pictureFile }}</small>
            </label>

            <label class="field">
              <span>Video løsning <span class="optional">(valgfri)</span></span>
              <input
                v-model="form.relatedVideo"
                type="url"
                placeholder="https://example.com/video"
                :class="{ invalid: errors.relatedVideo }"
              />
              <small v-if="errors.relatedVideo">{{ errors.relatedVideo }}</small>
            </label>

            <label class="field">
              <span>Resultat <span class="optional">(valgfri)</span></span>
              <textarea
                v-model="form.result"
                placeholder="Beskriv det forventede resultat..."
                rows="4"
                :class="{ invalid: errors.result }"
              />
              <small v-if="errors.result">{{ errors.result }}</small>
            </label>
          </div>
        </div>

        <div class="form-actions">
          <router-link to="/teacher-dashboard" class="back-link">Tilbage</router-link>
          <button type="submit" :disabled="loading">
            <span v-if="loading" class="spinner"></span>
            {{ loading ? 'Opretter...' : 'Opret Opgave' }}
          </button>
        </div>
      </form>

      <div v-if="toast.show" :class="['toast', toast.type]">
        {{ toast.message }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import { CreateAssignment } from '../Services/api';

const form = reactive({
  subject: '',
  niveau: '',
  delprove: '',
  point: null,
  pictureFile: null,
  relatedVideo: '',
  result: ''
});

const imagePreview = ref(null);
const errors = ref({});
const loading = ref(false);

const toast = reactive({
  show: false,
  message: '',
  type: 'success'
});

function showToast(message, type = 'success') {
  toast.show = true;
  toast.message = message;
  toast.type = type;
  setTimeout(() => (toast.show = false), 3000);
}

function onFileChange(event) {
  const file = event.target.files[0];
  setFile(file);
}

function onFileDrop(event) {
  const file = event.dataTransfer.files[0];
  setFile(file);
}

function setFile(file) {
  if (!file) return;
  form.pictureFile = file;
  imagePreview.value = URL.createObjectURL(file);
}

function removeFile() {
  form.pictureFile = null;
  imagePreview.value = null;
}

function validate() {
  const nextErrors = {};

  if (!form.subject) nextErrors.subject = 'Please select a subject';
  if (!form.niveau) nextErrors.niveau = 'Please select a niveau';
  if (!form.delprove) nextErrors.delprove = 'Please select a delprøve';

  if (form.point === null || form.point === '') {
    nextErrors.point = 'Please enter points';
  } else if (form.point < 0 || form.point > 10) {
    nextErrors.point = 'Points must be between 0 and 10';
  }

  if (!form.pictureFile) nextErrors.pictureFile = 'An image is required';

  if (form.relatedVideo && !isValidUrl(form.relatedVideo)) {
    nextErrors.relatedVideo = 'Please enter a valid URL';
  }

  errors.value = nextErrors;
  return Object.keys(nextErrors).length === 0;
}

function isValidUrl(string) {
  try {
    new URL(string);
    return true;
  } catch {
    return false;
  }
}

function resetForm() {
  form.subject = '';
  form.niveau = '';
  form.delprove = '';
  form.point = null;
  form.pictureFile = null;
  form.relatedVideo = '';
  form.result = '';
  imagePreview.value = null;
  errors.value = {};
}

async function handleSubmit() {
  if (!validate()) return;

  loading.value = true;

  try {
    await CreateAssignment({
      Points: Number(form.point),
      Type: form.delprove,
      ClassLevel: form.niveau,
      Subject: form.subject,
      PictureFile: form.pictureFile,
      VideoUrl: form.relatedVideo || null,
      Result: form.result || null
    });

    showToast('Opgave oprettet!', 'success');
    resetForm();
  } catch (error) {
    console.error('Failed to create task:', error);
    showToast('Kunne ikke oprette opgave.', 'error');
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
  align-items: flex-start;
  justify-content: center;
  overflow-y: auto;
  background: #f8fafc;
}

.task-form-container {
  width: 100%;
  max-width: 900px;
  position: relative;
}

.form-header {
  text-align: center;
  margin-bottom: 2rem;
}

.form-header h1 {
  font-size: 2rem;
  color: #0f172a;
  margin: 0.5rem 0;
}

.subtitle {
  color: #64748b;
  font-size: 1rem;
  margin: 0;
}

.form-sections {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.form-section {
  background: #ffffff;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06);
  border: 1px solid rgba(15, 23, 42, 0.06);
}

.form-section h2 {
  font-size: 1.1rem;
  color: #0f172a;
  margin: 0 0 1.25rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #e2e8f0;
}

.field-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  position: relative;
}

.field span {
  font-size: 0.9rem;
  font-weight: 500;
  color: #334155;
}

.optional {
  color: #94a3b8;
  font-weight: 400;
  font-size: 0.85rem;
}

textarea {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 0.75rem 1rem;
  font-size: 0.95rem;
  color: #0f172a;
  resize: vertical;
  font-family: inherit;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

textarea:focus {
  outline: none;
  border-color: #6366f1;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

.file-drop {
  position: relative;
  border: 2px dashed #e2e8f0;
  border-radius: 10px;
  background: #f8fafc;
  min-height: 130px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: border-color 0.2s ease;
}

.file-drop:hover {
  border-color: #6366f1;
}

.file-drop.invalid {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.15);
}

.file-drop.has-file {
  border-style: solid;
  border-color: #6366f1;
}

.file-input {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
  width: 100%;
  height: 100%;
}

.drop-hint {
  color: #94a3b8;
  font-size: 0.9rem;
  text-align: center;
  padding: 1rem;
  pointer-events: none;
}

.preview-wrapper {
  position: relative;
  width: 100%;
  display: flex;
  justify-content: center;
}

.image-preview {
  max-height: 140px;
  max-width: 100%;
  border-radius: 8px;
  object-fit: contain;
}

.remove-file {
  position: absolute;
  top: 6px;
  right: 6px;
  background: rgba(0,0,0,0.55);
  color: #fff;
  border: none;
  border-radius: 50%;
  width: 22px;
  height: 22px;
  font-size: 0.7rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1;
}

select,
input[type="number"],
input[type="url"],
input[type="text"] {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 0.75rem 1rem;
  font-size: 0.95rem;
  color: #0f172a;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
select {
  cursor: pointer;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%2364748b' d='M6 8L1 3h10z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 1rem center;
  padding-right: 2.5rem;
}

select:focus,
input:focus,
input:focus {
  outline: none;
  border-color: #6366f1;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

.invalid {
  border-color: #ef4444 !important;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.15);
}

small {
  color: #ef4444;
  font-size: 0.8rem;
}

.form-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #ffffff;
  border-radius: 16px;
  padding: 1.25rem 1.5rem;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.06);
  border: 1px solid rgba(15, 23, 42, 0.06);
}

.back-link {
  color: #64748b;
  text-decoration: none;
  font-size: 0.95rem;
  font-weight: 500;
  transition: color 0.2s ease;
}

.back-link:hover {
  color: #0f172a;
}

button[type="submit"] {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  color: #ffffff;
  border: none;
  border-radius: 10px;
  padding: 0.85rem 1.75rem;
  font-weight: 600;
  font-size: 0.95rem;
  cursor: pointer;
  box-shadow: 0 8px 20px rgba(99, 102, 241, 0.3);
  transition: transform 0.15s ease, box-shadow 0.15s ease;
}

button[type="submit"]:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 12px 25px rgba(99, 102, 241, 0.35);
}

button[type="submit"]:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.toast {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  padding: 1rem 1.5rem;
  border-radius: 12px;
  font-weight: 500;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
  animation: slideIn 0.3s ease;
  z-index: 1000;
}

.toast.success {
  background: #10b981;
  color: #ffffff;
}

.toast.error {
  background: #ef4444;
  color: #ffffff;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

@media (max-width: 768px) {
  .page-shell {
    padding: 1rem;
  }

  .form-sections {
    grid-template-columns: 1fr;
  }

  .field-grid {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
    gap: 1rem;
  }

  .back-link {
    order: 2;
  }

  button[type="submit"] {
    width: 100%;
    justify-content: center;
  }
}
</style>

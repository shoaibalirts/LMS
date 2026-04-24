<template>
	<div class="assigned-page">
		<div class="page-head">
			<h1>Tildel Opgavesaet Og Bedomning</h1>
			<p class="subtitle">Vaelg elev, opgaver og deadline. Se aflevering og giv feedback.</p>
		</div>

		<p v-if="status" :class="['status', statusType]">{{ status }}</p>

		<section class="panel">
			<h2>Ny tildeling</h2>
			<div class="assign-grid">
				<label>
					<span>Elev</span>
					<select v-model.number="createForm.studentId">
						<option :value="0">Vaelg elev</option>
						<option v-for="student in students" :key="student.id" :value="student.id">
							{{ student.firstName }} {{ student.lastName }} ({{ student.email }})
						</option>
					</select>
				</label>

				<label>
					<span>Dato for tildeling</span>
					<input v-model="createForm.dateOfAssigned" type="date" />
				</label>

				<label>
					<span>Deadline</span>
					<input v-model="createForm.deadline" type="date" />
				</label>
			</div>

			<div class="task-list">
				<label v-for="assignment in assignments" :key="assignment.id" class="task-item">
					<input v-model="createForm.assignmentIds" :value="assignment.id" type="checkbox" />
					<span>
						#{{ assignment.id }} {{ assignment.subject }} · {{ assignment.type }} · Niveau {{ assignment.classLevel }}
					</span>
				</label>
			</div>

			<div class="actions">
				<button @click="createAssignedSet" :disabled="creating">
					{{ creating ? 'Gemmer...' : 'Tildel opgaver' }}
				</button>
			</div>
		</section>

		<section class="panel">
			<h2>Tildelte opgaver og afleveringer</h2>
			<p v-if="loading" class="muted">Henter data...</p>
			<p v-else-if="assignedSets.length === 0" class="muted">Ingen tildelinger fundet.</p>

			<div v-else class="set-list">
				<article v-for="set in assignedSets" :key="set.id" class="set-card">
					<header class="set-header">
						<p>
							<strong>Elev:</strong>
							{{ studentName(set.studentId || set.StudentId) }}
						</p>
						<p><strong>Tildelt:</strong> {{ set.dateOfAssigned || set.DateOfAssigned }}</p>
						<p><strong>Deadline:</strong> {{ set.deadline || set.Deadline }}</p>
					</header>

					<ul class="submission-list">
						<li v-for="assignment in set.assignedAssignments || set.AssignedAssignments || []" :key="assignment.id">
							<div class="row-top">
								<p>
									<strong>{{ assignment.assignmentSubject || assignment.AssignmentSubject }}</strong>
									· {{ assignment.assignmentType || assignment.AssignmentType }}
									· {{ assignment.assignmentPoints || assignment.AssignmentPoints }} point
								</p>
								<p class="meta" v-if="assignment.studentResultFileName || assignment.StudentResultFileName">
									Afleveret: {{ assignment.studentResultFileName || assignment.StudentResultFileName }}
								</p>
								<p class="meta" v-else>Ikke afleveret endnu</p>
							</div>

							<div class="row-actions">
								<button
									@click="downloadResult(assignment.id)"
									:disabled="!(assignment.studentResultFileName || assignment.StudentResultFileName)"
									class="ghost"
								>
									Download PDF
								</button>

								<input
									v-model="feedbackDrafts[assignment.id]"
									type="text"
									maxlength="2000"
									placeholder="Skriv feedback til eleven"
								/>

								<button @click="saveFeedback(assignment.id)" :disabled="savingFeedbackId === assignment.id">
									{{ savingFeedbackId === assignment.id ? 'Gemmer...' : 'Gem feedback' }}
								</button>
							</div>
						</li>
					</ul>
				</article>
			</div>
		</section>
	</div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import {
	CreateAssignedAssignmentSet,
	DownloadAssignedAssignmentResult,
	GetTeacherAssignedAssignmentSets,
	GetTeacherAssignments,
	GetTeacherStudents,
	UpdateAssignedAssignmentFeedback
} from '../Services/api';

const loading = ref(false);
const creating = ref(false);
const savingFeedbackId = ref(null);
const status = ref('');
const statusType = ref('ok');

const students = ref([]);
const assignments = ref([]);
const assignedSets = ref([]);
const feedbackDrafts = ref({});

const createForm = reactive({
	studentId: 0,
	dateOfAssigned: new Date().toISOString().split('T')[0],
	deadline: new Date().toISOString().split('T')[0],
	assignmentIds: []
});

function studentName(studentId) {
	const student = students.value.find((s) => s.id === studentId);
	if (!student) return `#${studentId}`;
	return `${student.firstName} ${student.lastName}`;
}

function normalizeFeedbackDrafts() {
	const draftMap = {};
	for (const set of assignedSets.value) {
		for (const assignment of set.assignedAssignments || set.AssignedAssignments || []) {
			draftMap[assignment.id] = assignment.feedback || assignment.Feedback || '';
		}
	}
	feedbackDrafts.value = draftMap;
}

async function loadPage() {
	loading.value = true;
	status.value = '';

	try {
		const [studentData, assignmentData, assignedData] = await Promise.all([
			GetTeacherStudents(),
			GetTeacherAssignments(),
			GetTeacherAssignedAssignmentSets()
		]);

		students.value = Array.isArray(studentData) ? studentData : [];
		assignments.value = Array.isArray(assignmentData) ? assignmentData : [];
		assignedSets.value = Array.isArray(assignedData) ? assignedData : [];
		normalizeFeedbackDrafts();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke hente data.';
		statusType.value = 'error';
	} finally {
		loading.value = false;
	}
}

async function createAssignedSet() {
	status.value = '';

	if (!createForm.studentId) {
		status.value = 'Vaelg en elev.';
		statusType.value = 'error';
		return;
	}

	if (!createForm.dateOfAssigned || !createForm.deadline) {
		status.value = 'Dato og deadline er paakraevet.';
		statusType.value = 'error';
		return;
	}

	if (createForm.assignmentIds.length === 0) {
		status.value = 'Vaelg mindst en opgave.';
		statusType.value = 'error';
		return;
	}

	creating.value = true;

	try {
		await CreateAssignedAssignmentSet({
			studentId: createForm.studentId,
			dateOfAssigned: createForm.dateOfAssigned,
			deadline: createForm.deadline,
			assignmentIds: createForm.assignmentIds
		});

		createForm.assignmentIds = [];
		status.value = 'Tildeling oprettet.';
		statusType.value = 'ok';
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke oprette tildeling.';
		statusType.value = 'error';
	} finally {
		creating.value = false;
	}
}

async function saveFeedback(assignedAssignmentId) {
	savingFeedbackId.value = assignedAssignmentId;
	status.value = '';

	try {
		await UpdateAssignedAssignmentFeedback(assignedAssignmentId, feedbackDrafts.value[assignedAssignmentId] || null);
		status.value = 'Feedback gemt.';
		statusType.value = 'ok';
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke gemme feedback.';
		statusType.value = 'error';
	} finally {
		savingFeedbackId.value = null;
	}
}

async function downloadResult(assignedAssignmentId) {
	status.value = '';

	try {
		const { blob, fileName } = await DownloadAssignedAssignmentResult(assignedAssignmentId);
		const url = URL.createObjectURL(blob);
		const link = document.createElement('a');
		link.href = url;
		link.download = fileName;
		document.body.appendChild(link);
		link.click();
		link.remove();
		URL.revokeObjectURL(url);
	} catch (error) {
		status.value = error?.message || 'Kunne ikke hente PDF.';
		statusType.value = 'error';
	}
}

onMounted(loadPage);
</script>

<style scoped>
.assigned-page {
	min-height: calc(100vh - 52px);
	overflow-y: auto;
	background: linear-gradient(170deg, #f2fbf5 0%, #eef5ff 50%, #fff8ed 100%);
	padding: 1.2rem;
}

.page-head {
	max-width: 1200px;
	margin: 0 auto 1rem;
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

.assign-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
	gap: 0.7rem;
}

label {
	display: grid;
	gap: 0.3rem;
}

input,
select,
button {
	border-radius: 10px;
	border: 1px solid #c7d8ce;
	padding: 0.6rem 0.7rem;
}

button {
	border: none;
	background: #0f766e;
	color: #fff;
	cursor: pointer;
}

button.ghost {
	background: #f1f5f9;
	color: #1e293b;
	border: 1px solid #cbd5e1;
}

button:disabled {
	opacity: 0.6;
	cursor: not-allowed;
}

.actions {
	margin-top: 0.75rem;
}

.task-list {
	margin-top: 0.75rem;
	max-height: 220px;
	overflow-y: auto;
	display: grid;
	gap: 0.4rem;
}

.task-item {
	display: flex;
	align-items: center;
	gap: 0.5rem;
}

.set-list {
	display: grid;
	gap: 0.8rem;
}

.set-card {
	border: 1px solid #dbe5e0;
	border-radius: 12px;
	padding: 0.8rem;
}

.set-header {
	display: flex;
	flex-wrap: wrap;
	gap: 1rem;
	margin-bottom: 0.6rem;
}

.submission-list {
	list-style: none;
	margin: 0;
	padding: 0;
	display: grid;
	gap: 0.5rem;
}

.submission-list li {
	border: 1px solid #e2e8f0;
	border-radius: 10px;
	padding: 0.7rem;
	background: #f8fafc;
}

.row-top {
	margin-bottom: 0.5rem;
}

.meta {
	color: #475569;
	font-size: 0.9rem;
}

.row-actions {
	display: grid;
	grid-template-columns: auto 1fr auto;
	gap: 0.5rem;
}

.status {
	max-width: 1200px;
	margin: 0 auto 1rem;
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

.muted {
	color: #64748b;
}

@media (max-width: 900px) {
	.row-actions {
		grid-template-columns: 1fr;
	}
}
</style>

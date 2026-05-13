<template>
	<div class="assigned-page">
		<div class="page-head">
			<h1>Tildel Opgavesaet Og Bedomning</h1>
			<p class="subtitle">Vaelg klasse, opgavesaet og deadline. Se aflevering og giv feedback.</p>
		</div>

		<p v-if="status" :class="['status', statusType]">{{ status }}</p>

		<section class="panel">
			<h2>Ny tildeling</h2>
			<div class="assign-grid">
				<label>
					<span>Klasse</span>
					<select v-model.number="createForm.studyClassId">
						<option :value="0">Vaelg klasse</option>
						<option v-for="sc in studyClasses" :key="sc.id || sc.Id" :value="sc.id || sc.Id">
							{{ sc.name || sc.Name }} ({{ (sc.students || sc.Students || []).length }} elever)
						</option>
					</select>
				</label>

				<label>
					<span>Opgavesaet</span>
					<select v-model.number="createForm.assignmentSetId">
						<option :value="0">Vaelg opgavesaet</option>
						<option v-for="set in assignmentSets" :key="set.id || set.Id" :value="set.id || set.Id">
							{{ set.name || set.Name }}
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

			<div class="task-list" v-if="selectedAssignments.length">
				<div class="muted" style="margin-bottom: 0.4rem;">Opgaver i opgavesaettet:</div>
				<div v-for="assignment in selectedAssignments" :key="assignment.id || assignment.Id" class="task-item">
					<span>
						#{{ assignment.id || assignment.Id }} {{ assignment.subject || assignment.Subject }} ·
						{{ assignment.type || assignment.Type }} · Niveau {{ assignment.classLevel || assignment.ClassLevel }}
					</span>
				</div>
			</div>

			<div class="actions">
				<button @click="createAssignedSet" :disabled="creating">
					{{ creating ? 'Gemmer...' : 'Tildel opgavesaet' }}
				</button>
			</div>
		</section>

		<section class="panel">
			<h2>Slet opgaver og opgavesaet</h2>
			<p class="muted">Slet skjuler opgaver/opgavesaet for fremtidig brug. Eksisterende tildelinger bevares.</p>

			<div class="manage-grid">
				<div>
					<h3>Opgavesaet</h3>
					<p v-if="assignmentSets.length === 0" class="muted">Ingen opgavesaet.</p>
					<ul v-else class="manage-list">
						<li v-for="set in assignmentSets" :key="set.id || set.Id" class="manage-row">
							<span>{{ set.name || set.Name }}</span>
							<button class="ghost" @click="deleteAssignmentSet(set.id || set.Id)">Slet</button>
						</li>
					</ul>
				</div>

				<div>
					<h3>Opgaver</h3>
					<p v-if="assignments.length === 0" class="muted">Ingen opgaver.</p>
					<ul v-else class="manage-list">
						<li v-for="assignment in assignments" :key="assignment.id || assignment.Id" class="manage-row">
							<span>
								#{{ assignment.id || assignment.Id }}
								{{ assignment.subject || assignment.Subject }}
							</span>
							<button class="ghost" @click="deleteAssignment(assignment.id || assignment.Id)">Slet</button>
						</li>
					</ul>
				</div>
			</div>
		</section>

		<section class="panel">
			<h2>Tildelte opgaver og afleveringer</h2>
			<p v-if="loading" class="muted">Henter data...</p>
			<p v-else-if="assignedSets.length === 0" class="muted">Ingen tildelinger fundet.</p>

			<div v-else class="set-list">
				<article v-for="set in assignedSets" :key="set.id || set.Id" class="set-card">
					<header class="set-header">
						<p>
							<strong>Elev:</strong>
							{{ studentName(set.studentId || set.StudentId) }}
						</p>
						<p v-if="set.assignmentSetName || set.AssignmentSetName"><strong>Opgavesaet:</strong> {{ set.assignmentSetName || set.AssignmentSetName }}</p>
						<p><strong>Tildelt:</strong> {{ set.dateOfAssigned || set.DateOfAssigned }}</p>
						<p><strong>Deadline:</strong> {{ set.deadline || set.Deadline }}</p>
						<button class="ghost" style="margin-left:auto" @click="downloadTaskset(set.id || set.Id)">
							Download DOCX
						</button>
						<button
							class="danger"
							:disabled="deletingAllForStudentId === (set.studentId || set.StudentId)"
							@click="deleteAllForStudent(set.studentId || set.StudentId)"
						>
							{{ deletingAllForStudentId === (set.studentId || set.StudentId) ? 'Sletter alt...' : 'Slet alle tildelinger for elev' }}
						</button>
						<button class="danger" :disabled="revokingSetId === (set.id || set.Id)" @click="revokeAssignedSet(set.id || set.Id)">
							{{ revokingSetId === (set.id || set.Id) ? 'Sletter...' : 'Slet alle tildelte opgaver' }}
						</button>
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

								<button
									class="danger"
									:disabled="deletingAssignedId === assignment.id"
									@click="deleteAssignedAssignment(assignment.id)"
								>
									{{ deletingAssignedId === assignment.id ? 'Sletter...' : 'Slet tildelt opgave' }}
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
import { computed, onMounted, reactive, ref } from 'vue';
import {
	CreateAssignedAssignmentSetForClass,
	DeleteAssignment,
	DeleteAssignmentSet,
	DeleteAssignedAssignment,
	DownloadAssignedAssignmentSetTaskDocument,
	DownloadAssignedAssignmentResult,
	GetTeacherAssignedAssignmentSets,
	GetTeacherAssignments,
	GetTeacherAssignmentSets,
	GetTeacherStudyClasses,
	RevokeAllAssignedAssignmentSetsForStudent,
	RevokeAssignedAssignmentSet,
	GetTeacherStudents,
	UpdateAssignedAssignmentFeedback
} from '../Services/api';

const loading = ref(false);
const creating = ref(false);
const savingFeedbackId = ref(null);
const revokingSetId = ref(null);
const deletingAssignedId = ref(null);
const deletingAllForStudentId = ref(null);
const status = ref('');
const statusType = ref('ok');

const students = ref([]);
const studyClasses = ref([]);
const assignmentSets = ref([]);
const assignments = ref([]);
const assignedSets = ref([]);
const feedbackDrafts = ref({});

const createForm = reactive({
	studyClassId: 0,
	assignmentSetId: 0,
	dateOfAssigned: new Date().toISOString().split('T')[0],
	deadline: new Date().toISOString().split('T')[0],
});

const selectedAssignments = computed(() => {
	const selectedId = createForm.assignmentSetId;
	if (!selectedId) return [];
	const set = assignmentSets.value.find((x) => (x.id ?? x.Id) === selectedId);
	return (set?.assignments ?? set?.Assignments ?? []) || [];
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
		const [studentData, classData, assignmentSetData, assignmentData, assignedData] = await Promise.allSettled([
			GetTeacherStudents(),
			GetTeacherStudyClasses(),
			GetTeacherAssignmentSets(),
			GetTeacherAssignments(),
			GetTeacherAssignedAssignmentSets()
		]);

		students.value =
			studentData.status === 'fulfilled' && Array.isArray(studentData.value)
				? studentData.value
				: [];

		studyClasses.value =
			classData.status === 'fulfilled' && Array.isArray(classData.value)
				? classData.value
				: [];

		assignmentSets.value =
			assignmentSetData.status === 'fulfilled' && Array.isArray(assignmentSetData.value)
				? assignmentSetData.value
				: [];

		assignments.value =
			assignmentData.status === 'fulfilled' && Array.isArray(assignmentData.value)
				? assignmentData.value
				: [];

		assignedSets.value =
			assignedData.status === 'fulfilled' && Array.isArray(assignedData.value)
				? assignedData.value
				: [];

		if (classData.status === 'rejected') {
			status.value = classData.reason?.message || 'Kunne ikke hente klasser.';
			statusType.value = 'error';
		}

		if (assignmentSetData.status === 'rejected') {
			status.value = assignmentSetData.reason?.message || 'Kunne ikke hente opgavesaet.';
			statusType.value = 'error';
		}

		if (assignmentData.status === 'rejected') {
			status.value = assignmentData.reason?.message || 'Kunne ikke hente opgaver.';
			statusType.value = 'error';
		}

		if (assignedData.status === 'rejected') {
			status.value = assignedData.reason?.message || 'Kunne ikke hente eksisterende tildelinger. Du kan stadig oprette nye.';
			statusType.value = 'error';
		}

		normalizeFeedbackDrafts();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke hente data.';
		statusType.value = 'error';
	} finally {
		loading.value = false;
	}
}

async function deleteAssignment(assignmentId) {
	status.value = '';
	try {
		await DeleteAssignment(assignmentId);
		status.value = 'Opgave slettet.';
		statusType.value = 'ok';
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke slette opgave.';
		statusType.value = 'error';
	}
}

async function deleteAssignmentSet(assignmentSetId) {
	status.value = '';
	try {
		await DeleteAssignmentSet(assignmentSetId);
		status.value = 'Opgavesaet slettet.';
		statusType.value = 'ok';
		if (createForm.assignmentSetId === assignmentSetId) {
			createForm.assignmentSetId = 0;
		}
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke slette opgavesaet.';
		statusType.value = 'error';
	}
}

async function createAssignedSet() {
	status.value = '';

	if (!createForm.studyClassId) {
		status.value = 'Vaelg en klasse.';
		statusType.value = 'error';
		return;
	}

	if (!createForm.assignmentSetId) {
		status.value = 'Vaelg et opgavesaet.';
		statusType.value = 'error';
		return;
	}

	if (!createForm.dateOfAssigned || !createForm.deadline) {
		status.value = 'Dato og deadline er paakraevet.';
		statusType.value = 'error';
		return;
	}

	creating.value = true;

	try {
		await CreateAssignedAssignmentSetForClass({
			studyClassId: createForm.studyClassId,
			assignmentSetId: createForm.assignmentSetId,
			dateOfAssigned: createForm.dateOfAssigned,
			deadline: createForm.deadline
		});

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

async function revokeAssignedSet(assignedAssignmentSetId) {
	status.value = '';
	revokingSetId.value = assignedAssignmentSetId;
	try {
		await RevokeAssignedAssignmentSet(assignedAssignmentSetId);
		status.value = 'Alle tildelte opgaver fjernet.';
		statusType.value = 'ok';
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke fjerne alle tildelte opgaver.';
		statusType.value = 'error';
	} finally {
		revokingSetId.value = null;
	}
}

async function deleteAssignedAssignment(assignedAssignmentId) {
	status.value = '';
	deletingAssignedId.value = assignedAssignmentId;
	try {
		await DeleteAssignedAssignment(assignedAssignmentId);
		status.value = 'Tildelt opgave slettet.';
		statusType.value = 'ok';
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke slette tildelt opgave.';
		statusType.value = 'error';
	} finally {
		deletingAssignedId.value = null;
	}
}

async function deleteAllForStudent(studentId) {
	status.value = '';
	deletingAllForStudentId.value = studentId;
	try {
		const result = await RevokeAllAssignedAssignmentSetsForStudent(studentId);
		const count = result?.revokedCount;
		status.value = typeof count === 'number'
			? `Alle tildelinger fjernet (${count}).`
			: 'Alle tildelinger fjernet.';
		statusType.value = 'ok';
		await loadPage();
	} catch (error) {
		status.value = error?.message || 'Kunne ikke fjerne alle tildelinger for elev.';
		statusType.value = 'error';
	} finally {
		deletingAllForStudentId.value = null;
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

async function downloadTaskset(assignedAssignmentSetId) {
	status.value = '';
	try {
		const { blob, fileName } = await DownloadAssignedAssignmentSetTaskDocument(assignedAssignmentSetId);
		const url = URL.createObjectURL(blob);
		const link = document.createElement('a');
		link.href = url;
		link.download = fileName;
		document.body.appendChild(link);
		link.click();
		link.remove();
		URL.revokeObjectURL(url);
	} catch (error) {
		status.value = error?.message || 'Kunne ikke hente DOCX.';
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

.danger {
	background: #f1f5f9;
	color: #1e293b;
	border: 1px solid #cbd5e1;
	padding: 0.5rem 0.8rem;
	border-radius: 10px;
	cursor: pointer;
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

.manage-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
	gap: 1rem;
	margin-top: 0.75rem;
}

.manage-list {
	list-style: none;
	margin: 0.5rem 0 0;
	padding: 0;
	display: grid;
	gap: 0.5rem;
}

.manage-row {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 0.75rem;
	border: 1px solid var(--color-border);
	border-radius: 10px;
	padding: 0.6rem;
	background: var(--color-background-soft);
}

.manage-row span {
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
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

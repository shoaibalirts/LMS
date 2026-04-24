import { createRouter, createWebHistory } from 'vue-router';

import Login from '../Pages/Login.vue';
import Register from '../Pages/Register.vue';
import RegisterStudent from '../Pages/RegisterStudent.vue';
import StudyClassCreation from '../Pages/StudyClassCreation.vue';
import StudyClassDetails from '../Pages/StudyClassDetails.vue';
import TeacherDashboard from '../Pages/TeacherDashboard.vue';
import FrontPage from '../Pages/FrontPage.vue';
import TaskCreation from '../Pages/TaskCreation.vue';
import TasksetCreation from '../Pages/TasksetCreation.vue';
import AssignedSubmissions from '../Pages/AssignedSubmissions.vue';
import { getAuthSession } from '../Services/api';
import StudentLogin from '../Pages/StudentLogin.vue';
import StudentDashbboard from '../Pages/StudentDashbboard.vue';

const routes = [
  { path: '/', name: 'FrontPage', component: FrontPage, meta: { guestOnly: true } },
  { path: '/login', name: 'Login', component: Login, meta: { guestOnly: true } },
  { path: '/student-login', name: 'StudentLogin', component: StudentLogin, meta: { guestOnly: true } },
  { path: '/student-dashboard', name: 'StudentDashboard', component: StudentDashbboard, meta: { requiresAuth: true, requiresStudent: true } },
  { path: '/register', name: 'Register', component: Register, meta: { guestOnly: true } },
  { path: '/teacher-dashboard', name: 'TeacherDashboard', component: TeacherDashboard, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/create-studyclass', name: 'StudyClassCreation', component: StudyClassCreation, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/studyclass/:id', name: 'StudyClassDetails', component: StudyClassDetails, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/register-student', name: 'RegisterStudent', component: RegisterStudent, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/create-task', name: 'TaskCreation', component: TaskCreation, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/create-taskset', name: 'TasksetCreation', component: TasksetCreation, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/assigned-submissions', name: 'AssignedSubmissions', component: AssignedSubmissions, meta: { requiresAuth: true, requiresTeacher: true } }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach((to) => {
  const auth = getAuthSession();
  const loggedIn = !!(auth?.token || auth?.Token);
  const role = (auth?.role || auth?.Role || '').toLowerCase();

  // Guest-only pages
  if (to.meta.guestOnly && loggedIn) {
    if (role === 'teacher') return '/teacher-dashboard';
    if (role === 'student') return '/student-dashboard';
    return '/teacher-dashboard';
  }

  // Require login
  if (to.meta.requiresAuth && !loggedIn) {
    return '/login';
  }

  // Require teacher role
  if (to.meta.requiresTeacher && role !== 'teacher') {
    return '/';
  }

  // Require student role
  if (to.meta.requiresStudent && role !== 'student') {
    return '/';
  }

  return true;
});


export default router;
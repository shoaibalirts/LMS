import { createRouter, createWebHistory } from 'vue-router';

import Login from '../Pages/Login.vue';
import Register from '../Pages/Register.vue';
import TeacherDashboard from '../Pages/TeacherDashboard.vue';
import FrontPage from '../Pages/FrontPage.vue';
import TaskCreation from '../Pages/TaskCreation.vue';
const routes = [
  { path: '/', name: 'FrontPage', component: FrontPage, meta: { guestOnly: true } },
  { path: '/login', name: 'Login', component: Login, meta: { guestOnly: true } },
  { path: '/register', name: 'Register', component: Register, meta: { guestOnly: true } },
  { path: '/teacher-dashboard', name: 'TeacherDashboard', component: TeacherDashboard, meta: { requiresAuth: true, requiresTeacher: true } },
  { path: '/create-task', name: 'TaskCreation', component: TaskCreation, meta: { requiresAuth: true, requiresTeacher: true } }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach((to, from, next) => {
  const user = JSON.parse(localStorage.getItem('user'));
  const loggedIn = !!user;

  // Guest-only pages
  if (to.meta.guestOnly && loggedIn) {
    // Redirect teachers to dashboard - in future redirect students to /student-dashboard
    if (user?.role?.toLowerCase() === 'teacher') {
      return next('/teacher-dashboard');
    }
    // Redirect other logged-in users to a default page
    return next('/'); 
  }

  // Require login
  if (to.meta.requiresAuth && !loggedIn) {
    return next('/login');
  }

  // Require teacher role
  if (to.meta.requiresTeacher && user?.role?.toLowerCase() !== 'teacher') {
    return next('/');
  }

  next();
});


export default router;
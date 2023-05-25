import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';

// Import your views here
import Home from '@/views/HomeView.vue';
import User from '@/views/UserView.vue';
import { StorageService } from './services/storage.service';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: Home,
  },
  {
    path: '/user',
    name: 'User',
    component: User,
    meta: { requiresAuth: true },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Define navigation guard to check if user is authenticated
router.beforeEach((to, from, next) => {
  const isAuthenticated = new StorageService().getAuthenticatedUserId();

  if (to.matched.some((record) => record.meta.requiresAuth) && !isAuthenticated) {
    next({ name: 'Home' });
  } else {
    next();
  }
});

export default router;

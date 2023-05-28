<template>
  <div class="user">
    <h2>Hello {{ user?.email }}</h2>
    <UserCredentials :user="user" />
    <hr />
    <button @click="signOut">Sign out</button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ApiService } from '@/services/api.service';
import { StorageService } from '@/services/storage.service';
import { useRouter } from 'vue-router';
import UserCredentials from '@/components/UserCredentials.vue';

const router = useRouter();

const storegeService = new StorageService();
const apiService = new ApiService();

const fetchUser = async () => {
  var userId = storegeService.getAuthenticatedUserId();
  return await apiService.getUser(userId!);
};

const signOut = () => {
  storegeService.removeAuthenticatedUserId();
  router.push({ name: 'Home' });
};

const user = ref();
fetchUser().then((data) => {
  user.value = data;
});
</script>

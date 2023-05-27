<template>
  <form @submit.prevent="createUser">
    <input type="text" v-model="email" placeholder="email" autocomplete="username webauthn" />
    <button type="submit">Enter</button>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ApiService } from '@/services/api.service';
import { useRouter } from 'vue-router';
import { StorageService } from '@/services/storage.service';
const router = useRouter();

const email = ref('');

const createUser = async () => {
  const user = await new ApiService().createUser(email.value);
  new StorageService().setAuthenticatedUserId(user.id);
  // Redirect to user view

  router.push({ name: 'User' });
};
</script>

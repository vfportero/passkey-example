<template>
  <div class="user">
    <h2>Hello {{ user?.email }}</h2>
    <button v-if="browserHasPasskeyFeature" @click="createNewPassKey">Create a new Passkey</button>
    <div v-else>This browser has no compatibililty with Passkey</div>
    <hr />
    <button @click="signOut">Sign out</button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ApiService } from '@/services/api.service';
import { PasskeyService } from '@/services/passkey.service';
import { StorageService } from '@/services/storage.service';
import { useRouter } from 'vue-router';

const router = useRouter();

const storegeService = new StorageService();
const apiService = new ApiService();
const passKeyService = new PasskeyService();

const fetchUser = async () => {
  var userId = storegeService.getAuthenticatedUserId();
  return await apiService.getUser(userId!);
};

const getBrowserHasPasskeyFeature = async () => {
  return await passKeyService.browserHasPasskeyFeature();
};

const createNewPassKey = async () => {
  return await passKeyService.createPasskey(user.value?.email);
};

const signOut = () => {
  storegeService.removeAuthenticatedUserId();
  router.push({ name: 'Home' });
};

const user = ref();
fetchUser().then((data) => {
  user.value = data;
});

const browserHasPasskeyFeature = ref();
getBrowserHasPasskeyFeature().then((data) => {
  browserHasPasskeyFeature.value = data;
});
</script>

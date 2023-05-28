<template>
  <template v-if="!userCredentials || !userCredentials.length">
    <button v-if="browserHasPasskeyFeature" @click="createNewPassKey">Create a new Passkey</button>
    <div v-else>This browser has no compatibililty with Passkey</div>
  </template>
  <template v-else>
    <div class="credential-card" v-for="credential in userCredentials" :key="credential.id">
      <div>Id: {{ credential.id }}</div>
      <div>PublicKey: {{ credential.publicKey }}</div>
    </div>
  </template>
</template>

<script setup lang="ts">
import { ref, defineProps } from 'vue';
import { ApiService } from '@/services/api.service';
import { PasskeyService } from '@/services/passkey.service';

const apiService = new ApiService();
const passKeyService = new PasskeyService();

const props = defineProps({
  user: Object,
});

const getBrowserHasPasskeyFeature = async () => {
  return await passKeyService.browserHasPasskeyFeature();
};

const createNewPassKey = async () => {
  return await passKeyService.createPasskey(props.user?.email);
};

const browserHasPasskeyFeature = ref();
getBrowserHasPasskeyFeature().then((data) => {
  browserHasPasskeyFeature.value = data;
});

const fetchUserCredentials = async () => {
  return await apiService.getUserCredentials(props.user?.id);
};

const userCredentials = ref();
fetchUserCredentials().then((data) => {
  userCredentials.value = data;
});
</script>

<style scoped>
.credential-card {
  border: 1px solid black;
  padding: 10px;
  margin: 10px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.2);
}
</style>

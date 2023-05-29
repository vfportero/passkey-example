<template>
  <form @submit.prevent="createUser">
    <input type="text" v-model="email" placeholder="email" autocomplete="username webauthn" />
    <div class="error" v-if="userError">{{ userError }}</div>
    <button type="submit">Create user</button>
  </form>
  <div v-if="browserHasWebAuthnSupport">
    <div class="login-with-passkey">— or —</div>
    <button @click="login">Login with Passkey</button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ApiService } from '@/services/api.service';
import { PasskeyService } from '@/services/passkey.service';
import { useRouter } from 'vue-router';
import { StorageService } from '@/services/storage.service';
const router = useRouter();

const email = ref('');
const userError = ref('');
const passKeyService = new PasskeyService();
const storageService = new StorageService();

const createUser = async () => {
  userError.value = '';
  const userExists = (await new ApiService().queryUser(email.value))?.length > 0;

  if (!userExists) {
    const user = await new ApiService().createUser(email.value);
    storageService.setAuthenticatedUserId(user.id);
    // Redirect to user view

    router.push({ name: 'User' });
  } else {
    userError.value = 'User already exists';
  }
};

const login = async () => {
  userError.value = '';

  const validateResponse = await passKeyService.validatePasskey(email.value);
  if (validateResponse) {
    storageService.setAuthenticatedUserId(validateResponse.userId);
    router.push({ name: 'User' });
  } else {
    userError.value = 'Passkey validation error';
  }
};

const getBrowserHasWebAuthnSupport = async () => {
  return await passKeyService.browserHasWebAuthnSupport();
};
const browserHasWebAuthnSupport = ref();
getBrowserHasWebAuthnSupport().then((data) => {
  browserHasWebAuthnSupport.value = data;
});
</script>

<style scoped>
.error {
  color: red;
}
.login-with-passkey {
  margin: 10px 0;
  text-align: center;
  font-weight: bold;
}
</style>

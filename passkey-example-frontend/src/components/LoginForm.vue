<template>
  <form @submit.prevent="createUser">
    <input type="text" v-model="email" placeholder="email" autocomplete="username webauthn" />
    <div class="error" v-if="userExists">User already exists</div>
    <button type="submit">Create user</button>
  </form>
  <div v-if="browserHasWebAuthnSupport">
    <div class="login-with-passkey">— or —</div>
    <button>Login with Passkey</button>
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
const userExists = ref(false);
const passKeyService = new PasskeyService();

const createUser = async () => {
  userExists.value = (await new ApiService().queryUser(email.value))?.length > 0;

  if (userExists.value) {
    const user = await new ApiService().createUser(email.value);
    new StorageService().setAuthenticatedUserId(user.id);
    // Redirect to user view

    router.push({ name: 'User' });
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

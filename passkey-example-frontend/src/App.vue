<template>
  <header>
    <h1>Passkey Example</h1>
  </header>
  <router-view />
  <footer class="fixed-bottom">
    <p>Made with ❤️ for @vfportero</p>
  </footer>
</template>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  position: relative;
  min-height: calc(100vh - 30px);
}

nav {
  padding: 30px;
}

nav a {
  font-weight: bold;
  color: #2c3e50;
}

nav a.router-link-exact-active {
  color: #42b983;
}

footer {
  position: absolute;
  bottom: 0;
  width: 100%;
  padding: 30px;
  background-color: #42b983;
  color: #fff;
}

form {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

input {
  padding: 10px;
  border-radius: 5px;
  border: 1px solid #ccc;
  width: calc(300px - 22px);
}

button {
  padding: 10px;
  border-radius: 5px;
  background-color: #42b983;
  border: none;
  width: 300px;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;
  color: #fff;
  text-transform: uppercase;
  font-weight: bold;
}

button:hover {
  background-color: #c0e4d4;
}

button:active {
  background-color: #c0e4d4;
}

button:focus {
  outline: none;
}
</style>

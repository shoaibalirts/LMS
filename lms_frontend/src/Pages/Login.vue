<template>
    <h1>Login</h1>


    <div>
        <input v-model="email" placeholder="Email" />
        <input v-model="password" placeholder="Password" type="password" />
        <button @click="handleLogin">Login</button>
    </div>
</template>

<script setup>
import { ref } from 'vue';
import { LoginTeacher } from '../Services/api';
import { useRouter } from 'vue-router';

const email = ref('');
const password = ref('');
const router = useRouter();

async function handleLogin() {
    try {
        const data = await LoginTeacher(email.value, password.value);

        console.log('Login successful:', data);

        if (data != true) {
            alert('Invalid credentials');
            return;
        }

        localStorage.setItem('user', JSON.stringify({
            email: data.email,
            role: 'Teacher'
        }));

        router.push('/teacher-dashboard');
    } catch (error) {
        console.error('Login failed:', error);
    }
}
</script>
    
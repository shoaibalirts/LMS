<template>
    <h1>Register</h1>


    <div>
        <input v-model="firstname" placeholder="First Name" />
        <input v-model="lastname" placeholder="Last Name" />
        <input v-model="email" placeholder="Email" />
        <input v-model="password" placeholder="Password" type="password" />
       <button @click="handleRegister">Register</button>
    </div>
</template>

<script setup>
import { ref } from 'vue';
import { RegisterTeacher, LoginTeacher } from '../Services/api';
import { useRouter } from 'vue-router';

const firstname = ref('');
const lastname = ref('');
const email = ref('');
const password = ref('');
const router = useRouter();


async function handleRegister() { 
    try {
    
        const data = await RegisterTeacher(firstname.value, lastname.value, email.value, password.value);
        console.log('Registration successful:', data);


        const loginData = await LoginTeacher(email.value, password.value);
        console.log('Login successful:', loginData);

        localStorage.setItem('user', JSON.stringify({
            email: loginData.email,
            role: 'Teacher'
        }));

        router.push('/teacher-dashboard');

    } catch (error) {
        console.error('Registration/Login failed:', error);
    }
}
    

</script>
<style>

</style>

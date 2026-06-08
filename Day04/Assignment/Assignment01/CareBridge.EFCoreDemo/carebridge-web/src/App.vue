<script setup>
import { ref, onMounted } from 'vue'
import { watch } from 'vue'
const patients = ref([])

const city = ref("") 
const isActive = ref(false)
const fullName = ref("")

const fetchPatients = async () => {
  const response = await fetch(
    `https://localhost:7252/api/patients/search?city=${city.value}&
    activeOnly=${isActive.value}&fullName=${fullName.value}`
  )
  patients.value = await response.json()
}

onMounted(fetchPatients)

watch([city, isActive, fullName], fetchPatients)
</script>

<template>

  <h1>CareBridge Patients</h1>
 <label>
    Search by City:
    <input v-model="city" placeholder="Enter city name" />
  </label>

   <label>
    Search by Name:
    <input v-model="fullName" placeholder="Enter patient name" />
  </label>


  <label>
    Active Only:
    <input type="checkbox" v-model="isActive" />
  </label>
  <p>Showing  {{ patients?.length }} Patients</p>

  <table border="1">

    <tr>
      <th>Patient Id</th>
      <th>Full Name</th>
      <th>City</th>
      <th>IsActive</th>
    </tr>

    <!-- Loop through all patients -->

    <tr
      v-for="p in patients"
      :key="p.patientId">

      <td>{{ p.patientId }}</td>
      <td>{{ p.fullName }}</td>
      <td>{{ p.city }}</td>
      <td>{{ p.isActive }}</td>

    </tr>

  </table>

</template>

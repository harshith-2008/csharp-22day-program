<script setup>
import { ref, onMounted } from 'vue'
import { watch } from 'vue'
const department = ref([])

const fetchAnalytics = async () => {
  const response = await fetch(
    `https://localhost:7252/api/department-analytics`
  )
  department.value = await response.json()
}

onMounted(fetchAnalytics)

</script>

<template>

  <h1>Department Analytics</h1>


  <table border="1">
    <tr>
      <th>Department Name</th>
      <th>Inpatient</th>
      <th>Outpatient</th>
      <th>ED</th>
      <th>Total</th>
    </tr>

  <tr
  v-for="(d, index) in department"
  :key="d.departmentName"
  :class="{ highlight: index === 0 }"
>
  <td>{{ d.departmentName }}</td>
  <td>{{ d.inpatient }}</td>
  <td>{{ d.outpatient }}</td>
  <td>{{ d.ed }}</td>
  <td>{{ d.total }}</td>
</tr>
  </table>
</template>

<style>
.highlight {
  background-color: #b22222; 
  color: #fff;             
}
</style>

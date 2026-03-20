<template>
    <div class="chart-container">
        <div class="chart-header">
            <h2>Statistical Process Control Chart</h2>
            <div class="controls">
                <select v-model="selectedProductType" @change="filterData" class="product-select">
                    <option value="">All Products</option>
                    <option v-for="product in productTypes" :key="product" :value="product">
                        {{ product }}
                    </option>
                </select>
                <button @click="refreshData" class="refresh-btn">Refresh</button>
            </div>
        </div>

        <div v-if="loading" class="loading">Loading chart data...</div>

        <div v-else-if="error" class="error">
            Error loading data: {{ error }}
        </div>

        <div v-else class="charts-grid">
            <!-- Main Chart -->
            <div class="chart-wrapper">
                <canvas ref="mainChart"></canvas>
            </div>

            <!-- Statistics Table -->
            <div class="stats-table">
                <h3>Process Statistics</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Product Type</th>
                            <th>USL</th>
                            <th>LSL</th>
                            <th>Average</th>
                            <th>Std Dev</th>
                            <th>CPU</th>
                            <th>CPL</th>
                            <th>CPK</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in filteredData" :key="item.productSize"
                            :class="{ 'warning': item.cpk < 1.33, 'critical': item.cpk < 1.0 }">
                            <td>{{ item.productSize }}</td>
                            <td>{{ item.usl.toFixed(3) }}</td>
                            <td>{{ item.lsl.toFixed(3) }}</td>
                            <td>{{ item.average.toFixed(3) }}</td>
                            <td>{{ item.standardDeviation.toFixed(4) }}</td>
                            <td>{{ item.cpu.toFixed(2) }}</td>
                            <td>{{ item.cpl.toFixed(2) }}</td>
                            <td>{{ item.cpk.toFixed(2) }}</td>
                            <td>
                                <span :class="getStatusClass(item.cpk)">
                                    {{ getStatusText(item.cpk) }}
                                </span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- CPK Chart -->
            <div class="chart-wrapper">
                <canvas ref="cpkChart"></canvas>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue'
import Chart from 'chart.js/auto'

interface ChartDataItem {
    productSize: string  // เปลี่ยนจาก productType เป็น productSize ให้ตรงกับ API
    usl: number
    lsl: number
    average: number
    standardDeviation: number
    cpu: number
    cpl: number
    cpk: number
}


const chartData = ref<ChartDataItem[]>([])
const loading = ref<boolean>(true)
const error = ref<string | null>(null)
const selectedProductType = ref<string>('')
const mainChart = ref<HTMLCanvasElement | null>(null)
const cpkChart = ref<HTMLCanvasElement | null>(null)

let mainChartInstance: Chart | null = null
let cpkChartInstance: Chart | null = null

const productTypes = computed(() => {
    if (!Array.isArray(chartData.value)) return []
    return [...new Set(chartData.value.map(item => item.productSize))] // เปลี่ยนจาก productType
})

const filteredData = computed(() => {
    if (!Array.isArray(chartData.value)) return []

    if (!selectedProductType.value) {
        return chartData.value
    }
    return chartData.value.filter(item => item.productSize === selectedProductType.value) // เปลี่ยนจาก productType
})


const fetchData = async () => {
    try {
        loading.value = true
        error.value = null

        // ใช้ $fetch แทน useFetch สำหรับการเรียก API ที่ตรงไปตรงมา
        const response = await $fetch<ChartDataItem[]>('/api/SI25024/chart-data')

        if (response && Array.isArray(response)) {
            // แปลงข้อมูลจาก API response ให้ตรงกับ interface
            chartData.value = response.map(item => ({
                productSize: item.productSize, // API ส่งมาเป็น ProductSize (uppercase P)
                usl: item.usl,
                lsl: item.lsl,
                average: item.average,
                standardDeviation: item.standardDeviation,
                cpu: item.cpu,
                cpl: item.cpl,
                cpk: item.cpk
            }))
        } else {
            chartData.value = []
            console.warn('API returned non-array data:', response)
        }

        await nextTick()
        createCharts()
    } catch (err: any) {
        error.value = err?.data?.message || err?.message || 'Unknown error occurred'
        chartData.value = [] // Reset to empty array on error
        console.error('Error fetching chart data:', err)
    } finally {
        loading.value = false
    }
}
const createMainChart = () => {
  if (!mainChart.value) return

  const ctx = mainChart.value.getContext('2d')
  if (!ctx) return

  if (mainChartInstance) {
    mainChartInstance.destroy()
  }

  const data = filteredData.value

  if (!data || data.length === 0) {
    return
  }

  mainChartInstance = new Chart(ctx, {
    type: 'line',
    data: {
      labels: data.map(item => item.productSize), // เปลี่ยนจาก productType
      datasets: [
        {
          label: 'Average',
          data: data.map(item => item.average),
          borderColor: 'rgb(75, 192, 192)',
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          tension: 0.1
        },
        {
          label: 'USL',
          data: data.map(item => item.usl),
          borderColor: 'rgb(255, 99, 132)',
          backgroundColor: 'rgba(255, 99, 132, 0.2)',
          borderDash: [5, 5],
          tension: 0.1
        },
        {
          label: 'LSL',
          data: data.map(item => item.lsl),
          borderColor: 'rgb(255, 159, 64)',
          backgroundColor: 'rgba(255, 159, 64, 0.2)',
          borderDash: [5, 5],
          tension: 0.1
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        title: {
          display: true,
          text: 'Process Control Chart'
        },
        legend: {
          display: true,
          position: 'top'
        }
      },
      scales: {
        y: {
          beginAtZero: false,
          title: {
            display: true,
            text: 'Measurement Value'
          }
        },
        x: {
          title: {
            display: true,
            text: 'Product Type'
          }
        }
      }
    }
  })
}

const createCpkChart = () => {
  if (!cpkChart.value) return

  const ctx = cpkChart.value.getContext('2d')
  if (!ctx) return

  if (cpkChartInstance) {
    cpkChartInstance.destroy()
  }

  const data = filteredData.value

  if (!data || data.length === 0) {
    return
  }

  cpkChartInstance = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: data.map(item => item.productSize), // เปลี่ยนจาก productType
      datasets: [
        {
          label: 'CPK',
          data: data.map(item => item.cpk),
          backgroundColor: data.map(item => {
            if (item.cpk >= 1.67) return 'rgba(34, 197, 94, 0.8)' // Green - Excellent
            if (item.cpk >= 1.33) return 'rgba(59, 130, 246, 0.8)' // Blue - Good
            if (item.cpk >= 1.0) return 'rgba(245, 158, 11, 0.8)'  // Yellow - Acceptable
            return 'rgba(239, 68, 68, 0.8)' // Red - Poor
          }),
          borderColor: data.map(item => {
            if (item.cpk >= 1.67) return 'rgba(34, 197, 94, 1)'
            if (item.cpk >= 1.33) return 'rgba(59, 130, 246, 1)'
            if (item.cpk >= 1.0) return 'rgba(245, 158, 11, 1)'
            return 'rgba(239, 68, 68, 1)'
          }),
          borderWidth: 1
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        title: {
          display: true,
          text: 'Process Capability Index (CPK)'
        },
        legend: {
          display: false
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: 'CPK Value'
          },
          ticks: {
            callback: function(value) {
              return value.toFixed(2)
            }
          }
        },
        x: {
          title: {
            display: true,
            text: 'Product Type'
          }
        }
      }
    }
  })
}


const createCharts = () => {
    createMainChart()
    createCpkChart()
}

const filterData = () => {
    createCharts()
}

const refreshData = () => {
    fetchData()
}

const getStatusClass = (cpk: number) => {
    if (cpk >= 1.67) return 'status-excellent'
    if (cpk >= 1.33) return 'status-good'
    if (cpk >= 1.0) return 'status-acceptable'
    return 'status-poor'
}

const getStatusText = (cpk: number) => {
    if (cpk >= 1.67) return 'Excellent'
    if (cpk >= 1.33) return 'Good'
    if (cpk >= 1.0) return 'Acceptable'
    return 'Poor'
}

onMounted(() => {
    fetchData()
})
</script>

<style scoped>
.chart-container {
  padding: var(--space-6);
  max-width: 1400px;
  margin: 0 auto;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-6);
}
.chart-header h2 { color: var(--color-text); font-size: var(--text-xl); font-weight: 600; }

.controls { display: flex; gap: var(--space-4); align-items: center; }

.product-select {
  padding: var(--space-2) var(--space-4);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: var(--text-sm);
}

.refresh-btn {
  padding: var(--space-2) var(--space-4);
  background: var(--color-primary);
  color: #fff;
  border: none;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--text-sm);
  transition: background var(--transition-fast);
}
.refresh-btn:hover { background: var(--color-primary-dark); }

.loading { text-align: center; padding: var(--space-6); font-size: var(--text-base); color: var(--color-text-muted); }
.error { text-align: center; padding: var(--space-6); color: var(--color-error); background: var(--color-error-bg); border-radius: var(--radius-md); }

.charts-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-6); margin-bottom: var(--space-6); }

.chart-wrapper {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  border: 1px solid var(--color-border);
  padding: var(--space-4);
  height: 400px;
}

.stats-table {
  grid-column: 1 / -1;
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-card);
  border: 1px solid var(--color-border);
  padding: var(--space-4);
  overflow-x: auto;
}
.stats-table h3 { margin-bottom: var(--space-4); color: var(--color-text); font-size: var(--text-lg); font-weight: 600; }

table { width: 100%; border-collapse: collapse; font-size: var(--text-sm); }
th, td { text-align: left; padding: var(--space-3); border-bottom: 1px solid var(--color-border-light); }
th { background: var(--color-table-header); font-weight: 600; color: var(--color-text); }
tr:hover { background: var(--color-surface-2); }
tr.warning  { background: var(--color-warning-bg); }
tr.critical { background: var(--color-error-bg); }

.status-excellent { color: var(--color-success); font-weight: 600; }
.status-good      { color: var(--color-primary);  font-weight: 600; }
.status-acceptable{ color: var(--color-warning);  font-weight: 600; }
.status-poor      { color: var(--color-error);    font-weight: 600; }

@media (max-width: 768px) {
  .chart-container { padding: var(--space-4); }
  .charts-grid { grid-template-columns: 1fr; }
  .chart-header { flex-direction: column; gap: var(--space-4); align-items: stretch; }
  .controls { justify-content: center; }
}
</style>

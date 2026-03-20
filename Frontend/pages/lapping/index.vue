<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'

// SEO และ Meta
useHead({
  title: 'Material Withdrawal - LP Control System',
  meta: [
    { name: 'description', content: 'Material withdrawal system for LP manufacturing' },
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ]
})

// Type definitions - ปรับให้ตรงกับ Backend Model
interface MateralInventory {
  Id: string
  MatName: string
  MatQuantity: number | null
  matUnit?: string
  matTypeId?: string
  case?: string
  expDate: string | null  // DateOnly จะกลายเป็น string ใน JSON
  insertDate: string | null  // DateOnly จะกลายเป็น string ใน JSON
  empId?: string
  shift?: string
  product?: string
  supplier?: string
  lotNumber?: string
  Location?: string
}

interface WithdrawalRequest {
  shift: string
  product: string
  MatName: string
  machine: string
  quantity: number
  cause: string
  employee: string
  materialId: string
}

interface Withdrawal {
  id: number
  timestamp: Date
  MatName: string
  quantity: number
  machine: string
  cause: string
  employee: string
  shift: string
  product: string
}

// Reactive state
const formData = ref<WithdrawalRequest>({
  shift: 'B',
  product: 'Mobile 0.1',
  MatName: 'FO1500',
  machine: 'LP603',
  quantity: 0.5,
  cause: 'Add In Loop',
  employee: '',
  materialId: ''
})

const materials = ref<MateralInventory[]>([])
const selectedMaterial = ref<MateralInventory | null>(null)
const loadingStock = ref<boolean>(false)
const loadingMaterials = ref<boolean>(false)
const isSubmitting = ref<boolean>(false)
const validationErrors = ref<string[]>([])
const successMessage = ref<string>('')
const recentWithdrawals = ref<Withdrawal[]>([])

// Dynamic options from API
const products = ref<any[]>([])
const shifts = ref<any[]>([])
const suppliers = ref<any[]>([])
const Locations = ref<any[]>([])
const employees = ref<any[]>([])

// API Configuration from runtime config
const { public: { apiBase } } = useRuntimeConfig()

// Computed properties
const availableStock = computed((): number => {
  return selectedMaterial.value?.MatQuantity || 0
})

// Static constants (fallback if API fails)
const DEFAULT_MACHINES = ['LP603', 'LP604', 'LP605']
const DEFAULT_CAUSES = [
  'Add In Loop',
  'Gravity Low',
  'Cor New Plate',
  'Cor Plate FN Over',
  'Cor Plate Scratch',
  'Cor Start up',
  'Grinding Carrier',
  'Slurry Abnormal'
]

// Methods
const loadMasterData = async (): Promise<void> => {
  try {
    // Load products
    const { data: productsData, error: productsError } = await useFetch<any[]>(`${apiBase}/api/SI25006/products`, {
      server: false,
      default: () => []
    })

    if (!productsError.value && productsData.value) {
      products.value = productsData.value
    }

    // Load shifts
    const { data: shiftsData, error: shiftsError } = await useFetch<any[]>(`${apiBase}/api/SI25006/shifts`, {
      server: false,
      default: () => []
    })

    if (!shiftsError.value && shiftsData.value) {
      shifts.value = shiftsData.value
    }

    // Load suppliers
    const { data: suppliersData, error: suppliersError } = await useFetch<any[]>(`${apiBase}/api/SI25006/suppliers`, {
      server: false,
      default: () => []
    })

    if (!suppliersError.value && suppliersData.value) {
      suppliers.value = suppliersData.value
    }

    // Load Locations
    const { data: LocationsData, error: LocationsError } = await useFetch<any[]>(`${apiBase}/api/SI25006/Locations`, {
      server: false,
      default: () => []
    })

    if (!LocationsError.value && LocationsData.value) {
      Locations.value = LocationsData.value
    }

    // Load employees
    const { data: employeesData, error: employeesError } = await useFetch<any[]>(`${apiBase}/api/SI25006/employees`, {
      server: false,
      default: () => []
    })

    if (!employeesError.value && employeesData.value) {
      employees.value = employeesData.value
    }

  } catch (error) {
    console.error('Error loading master data:', error)
    // Fallback to default values if API fails
    products.value = [
      { product: 'Mobile 0.1', count: 0 },
      { product: 'Mobile 0.2', count: 0 },
      { product: 'Mobile 0.3', count: 0 }
    ]
    shifts.value = [
      { shift: 'A', count: 0 },
      { shift: 'B', count: 0 },
      { shift: 'C', count: 0 }
    ]
  }
}

const loadMaterials = async (): Promise<void> => {
  loadingMaterials.value = true
  try {
    const { data: materialsData, error } = await useFetch<MateralInventory[]>(`${apiBase}/api/SI25006`, {
      server: false,
      default: () => []
    })

    if (!error.value && materialsData.value) {
      materials.value = materialsData.value.map(material => ({
        ...material,
        matUnit: material.matUnit || 'kg', // Default unit if not specified
        MatQuantity: material.MatQuantity || 0
      }))

      // Set default material if available
      if (materials.value.length > 0 && !selectedMaterial.value) {
        const defaultMat = materials.value.find(m => m.MatName === 'FO1500') || materials.value[0]
        selectedMaterial.value = defaultMat
        formData.value.MatName = defaultMat.MatName
        formData.value.materialId = defaultMat.Id
      }
    } else {
      throw new Error(error.value?.message || 'Failed to load materials')
    }
  } catch (error) {
    console.error('Error loading materials:', error)
    showError('Failed to load materials from server')
  } finally {
    loadingMaterials.value = false
  }
}

const onMaterialChange = (): void => {
  const selected = materials.value.find(m => m.MatName === formData.value.MatName)
  selectedMaterial.value = selected || null
  formData.value.materialId = selected?.Id || ''
  clearMessages()
}

const checkStock = async (): Promise<void> => {
  if (!selectedMaterial.value) return

  loadingStock.value = true
  try {
    // Refresh individual material data
    const { data: materialData, error } = await useFetch<MateralInventory>(`${apiBase}/api/SI25006/${selectedMaterial.value.Id}`, {
      server: false
    })

    if (!error.value && materialData.value) {
      // Update local material data
      const materialIndex = materials.value.findIndex(m => m.Id === selectedMaterial.value?.Id)
      if (materialIndex !== -1) {
        materials.value[materialIndex] = {
          ...materialData.value,
          matUnit: materialData.value.matUnit || 'kg',
          MatQuantity: materialData.value.MatQuantity || 0
        }
        selectedMaterial.value = materials.value[materialIndex]
      }

      showSuccess('Stock data refreshed successfully')
    } else {
      throw new Error(error.value?.message || 'Failed to fetch material data')
    }
  } catch (error) {
    console.error('Error checking stock:', error)
    // Fallback to reloading all materials if individual fetch fails
    try {
      await loadMaterials()
      showSuccess('Stock data refreshed from main inventory')
    } catch (fallbackError) {
      showError('Failed to refresh stock data')
    }
  } finally {
    loadingStock.value = false
  }
}

const validateForm = (): boolean => {
  validationErrors.value = []

  if (!formData.value.shift) validationErrors.value.push('Please select a shift')
  if (!formData.value.product) validationErrors.value.push('Please select a product')
  if (!formData.value.MatName) validationErrors.value.push('Please select a material')
  if (!formData.value.machine) validationErrors.value.push('Please select a machine')
  if (!formData.value.cause) validationErrors.value.push('Please select a cause')
  if (!formData.value.employee) validationErrors.value.push('Please enter employee ID')
  if (!formData.value.quantity || formData.value.quantity <= 0) {
    validationErrors.value.push('Please enter a valid quantity')
  }

  if (selectedMaterial.value && formData.value.quantity > (selectedMaterial.value.MatQuantity || 0)) {
    validationErrors.value.push(
      `Insufficient stock. Available: ${selectedMaterial.value.MatQuantity} ${selectedMaterial.value.matUnit}`
    )
  }

  return validationErrors.value.length === 0
}

const handleWithdrawal = async (): Promise<void> => {
  clearMessages()

  if (!validateForm() || !selectedMaterial.value) {
    if (!selectedMaterial.value) {
      validationErrors.value.push('No material selected')
    }
    return
  }

  isSubmitting.value = true

  try {
    // Calculate new quantity
    const currentQuantity = selectedMaterial.value.MatQuantity || 0
    const newQuantity = currentQuantity - formData.value.quantity

    if (newQuantity < 0) {
      throw new Error('Insufficient stock for withdrawal')
    }

    // Update material quantity via API - ใช้ useFetch แทน $fetch
    const { error } = await useFetch(`${apiBase}//api/SI25006/${selectedMaterial.value.Id}/quantity`, {
      method: 'PATCH',
      body: newQuantity,
      server: false
    })

    if (error.value) {
      throw new Error(error.value.message || 'Failed to update material quantity')
    }

    // TODO: เมื่อมี Withdrawal API จึงจะใช้
    // สำหรับตอนนี้ comment ไว้ก่อนเนื่องจาก API ยังไม่มี endpoint นี้
    /*
    const withdrawalRecord = {
      materialId: selectedMaterial.value.id,
      MatName: formData.value.MatName,
      quantity: formData.value.quantity,
      machine: formData.value.machine,
      cause: formData.value.cause,
      empId: formData.value.employee,
      shift: formData.value.shift,
      product: formData.value.product,
      withdrawalDate: new Date().toISOString()
    }

    const { error: withdrawalError } = await useFetch(`${apiBase}/api/SI25006/withdrawal`, {
      method: 'POST',
      body: withdrawalRecord,
      server: false
    })

    if (withdrawalError.value) {
      throw new Error(withdrawalError.value.message || 'Failed to record withdrawal')
    }
    */

    // Update local state
    selectedMaterial.value.MatQuantity = newQuantity
    const materialIndex = materials.value.findIndex(m => m.Id === selectedMaterial.value?.Id)
    if (materialIndex !== -1) {
      materials.value[materialIndex].MatQuantity = newQuantity
    }

    // Add to recent withdrawals (local storage for demo)
    const withdrawal: Withdrawal = {
      id: Date.now(),
      timestamp: new Date(),
      MatName: formData.value.MatName,
      quantity: formData.value.quantity,
      machine: formData.value.machine,
      cause: formData.value.cause,
      employee: formData.value.employee,
      shift: formData.value.shift,
      product: formData.value.product
    }

    recentWithdrawals.value.unshift(withdrawal)
    if (recentWithdrawals.value.length > 10) {
      recentWithdrawals.value = recentWithdrawals.value.slice(0, 10)
    }

    // Save to localStorage for persistence (เนื่องจากยังไม่มี withdrawal API)
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('recentWithdrawals', JSON.stringify(recentWithdrawals.value))
    }

    showSuccess(`Successfully withdrew ${formData.value.quantity} ${selectedMaterial.value.matUnit} of ${formData.value.MatName}`)

    // Reset quantity
    formData.value.quantity = 0

  } catch (error: any) {
    console.error('Error processing withdrawal:', error)
    if (error && typeof error === 'object' && 'status' in error) {
      const status = error.status
      if (status === 404) {
        showError('Material not found')
      } else if (status === 400) {
        showError('Invalid request data')
      } else if (status >= 500) {
        showError('Server error. Please try again later.')
      } else {
        showError('Failed to process withdrawal. Please try again.')
      }
    } else {
      showError('Network error. Please check your connection.')
    }
  } finally {
    isSubmitting.value = false
  }
}

const initializeData = async (): Promise<void> => {
  try {
    const { error } = await useFetch(`${apiBase}/api/SI25006/initialize-data`, {
      method: 'POST',
      server: false
    })

    if (!error.value) {
      showSuccess('System data initialized successfully')
      // Reload materials after initialization
      await loadMaterials()
      await loadMasterData()
    } else {
      throw new Error(error.value.message || 'Failed to initialize data')
    }
  } catch (error) {
    console.error('Error initializing data:', error)
    showError('Failed to initialize system data')
  }
}

const resetForm = (): void => {
  formData.value = {
    shift: '',
    product: '',
    MatName: '',
    machine: '',
    quantity: 0,
    cause: '',
    employee: '',
    materialId: ''
  }
  selectedMaterial.value = null
  clearMessages()
}

const loadRecentWithdrawals = (): void => {
  try {
    if (typeof localStorage !== 'undefined') {
      const saved = localStorage.getItem('recentWithdrawals')
      if (saved) {
        const parsed = JSON.parse(saved)
        recentWithdrawals.value = parsed.map((w: any) => ({
          ...w,
          timestamp: new Date(w.timestamp)
        }))
      }
    }
  } catch (error) {
    console.error('Error loading recent withdrawals:', error)
    recentWithdrawals.value = []
  }
}

const clearMessages = (): void => {
  validationErrors.value = []
  successMessage.value = ''
}

const showSuccess = (message: string): void => {
  successMessage.value = message
  setTimeout(() => {
    successMessage.value = ''
  }, 5000)
}

const showError = (message: string): void => {
  validationErrors.value = [message]
}

const formatDateTime = (date: Date | string | null | undefined): string => {
  if (!date) return 'N/A'
  try {
    return new Date(date).toLocaleString('th-TH')
  } catch {
    return 'N/A'
  }
}

const formatDate = (dateString: string | null | undefined): string => {
  if (!dateString) return 'N/A'
  try {
    // DateOnly จาก C# จะมาเป็น "YYYY-MM-DD" format
    return new Date(dateString + 'T00:00:00').toLocaleDateString('th-TH')
  } catch {
    return 'N/A'
  }
}

// Lifecycle
onMounted(async (): Promise<void> => {
  try {
    // Load master data first
    await loadMasterData()

    // Load materials from API
    await loadMaterials()

    // If no materials found, offer to initialize
    if (materials.value.length === 0) {
      const shouldInitialize = confirm('No materials found in inventory. Would you like to initialize sample data?')
      if (shouldInitialize) {
        await initializeData()
      }
    }

    // Load recent withdrawals from localStorage
    loadRecentWithdrawals()
  } catch (error) {
    console.error('Error during component initialization:', error)
    showError('Failed to load initial data. Some features may not work properly.')
  }
})
</script>

<template>
  <div class="material-withdrawal-page">
    <!-- Header with Navigation -->
    <div class="header-bar d-flex justify-content-between align-items-center">
      <h4 class="mb-0 text-white fw-bold">Material Withdrawal System</h4>
      <div class="nav-buttons">
        <NuxtLink to="/material-receive" class="btn btn-outline-light btn-sm me-2">
          รับเข้า
        </NuxtLink>
        <button type="button" class="btn-close btn-close-white" aria-label="Close"></button>
      </div>
    </div>

    <!-- Main Form Container -->
    <div class="form-container">
      <div class="page-title">
        <h5 class="text-danger fw-bold mb-3">เบิกออก - Material Withdrawal</h5>
      </div>

      <form @submit.prevent="handleWithdrawal" class="material-form">

        <!-- Row 1: Shift, Product -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Shift <span class="text-danger">*</span></label>
            <select v-model="formData.shift" class="form-control" required>
              <option value="">Select Shift</option>
              <option v-for="shift in shifts" :key="shift.shift" :value="shift.shift">
                {{ shift.shift }} ({{ shift.count }} employees)
              </option>
              <!-- Fallback options -->
              <option v-if="shifts.length === 0" value="A">A</option>
              <option v-if="shifts.length === 0" value="B">B</option>
              <option v-if="shifts.length === 0" value="C">C</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Product <span class="text-danger">*</span></label>
            <select v-model="formData.product" class="form-control" required>
              <option value="">Select Product</option>
              <option v-for="product in products" :key="product.product" :value="product.product">
                {{ product.product }}
                <span v-if="product.count !== undefined">({{ product.count }} materials)</span>
              </option>
              <!-- Fallback options -->
              <option v-if="products.length === 0" value="Mobile 0.1">Mobile 0.1</option>
              <option v-if="products.length === 0" value="Mobile 0.2">Mobile 0.2</option>
              <option v-if="products.length === 0" value="Mobile 0.3">Mobile 0.3</option>
            </select>
          </div>
        </div>

        <!-- Row 2: Mat Name, Machine -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Mat Name <span class="text-danger">*</span></label>
            <select v-model="formData.MatName" @change="onMaterialChange" class="form-control" required
              :disabled="loadingMaterials">
              <option value="">{{ loadingMaterials ? 'Loading materials...' : 'Select Material' }}</option>
              <option v-for="material in materials" :key="material.Id" :value="material.MatName">
                {{ material.MatName }} ({{ material.MatQuantity || 0 }} {{ material.matUnit || 'kg' }})
                <span v-if="material.Location"> - {{ material.Location }}</span>
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Machine <span class="text-danger">*</span></label>
            <select v-model="formData.machine" class="form-control" required>
              <option value="">Select Machine</option>
              <option v-for="machine in DEFAULT_MACHINES" :key="machine" :value="machine">
                {{ machine }}
              </option>
            </select>
          </div>
        </div>

        <!-- Row 3: Cause/Reason, Withdrawal Quantity -->
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Cause/Reason <span class="text-danger">*</span></label>
            <select v-model="formData.cause" class="form-control" required>
              <option value="">Select Cause</option>
              <option v-for="cause in DEFAULT_CAUSES" :key="cause" :value="cause">
                {{ cause }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Withdrawal Quantity <span class="text-danger">*</span></label>
            <div class="input-group">
              <input v-model.number="formData.quantity" type="number" step="0.1" min="0" :max="availableStock"
                class="form-control" placeholder="0.0" required />
              <span class="input-group-text">{{ selectedMaterial?.matUnit || 'kg' }}</span>
            </div>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Employee ID <span class="text-danger">*</span></label>
            <select v-model="formData.employee" class="form-control" required>
              <option value="">Select Employee</option>
              <option v-for="employee in employees" :key="employee.empId" :value="employee.empId">
                {{ employee.empId }} - {{ employee.empName || employee.employeename }}
                <span v-if="employee.department">({{ employee.department }})</span>
              </option>
            </select>
            <!-- Fallback input if no employees loaded -->
            <input v-if="employees.length === 0" v-model="formData.employee" type="text" class="form-control mt-2"
              placeholder="Enter Employee ID manually" />
          </div>

          <!-- Empty space to maintain layout -->
          <div class="form-group"></div>
        </div>

        <!-- Stock Information -->
        <div class="stock-info-section">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Current Balance Stock</label>
              <div class="stock-display">
                <span v-if="selectedMaterial" class="stock-value">
                  {{ selectedMaterial.MatQuantity || 0 }} {{ selectedMaterial.matUnit || 'kg' }}
                </span>
                <span v-else-if="loadingMaterials" class="stock-placeholder">Loading...</span>
                <span v-else class="stock-placeholder">Select material first</span>
              </div>
              <!-- แสดงข้อมูลเพิ่มเติม -->
              <div v-if="selectedMaterial" class="material-details">
                <small class="text-muted">
                  <div v-if="selectedMaterial.lotNumber">Lot: {{ selectedMaterial.lotNumber }}</div>
                  <div v-if="selectedMaterial.Location">Location: {{ selectedMaterial.Location }}</div>
                  <div v-if="selectedMaterial.expDate">Exp: {{ formatDate(selectedMaterial.expDate) }}</div>
                  <div v-if="selectedMaterial.supplier">Supplier: {{ selectedMaterial.supplier }}</div>
                </small>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Refresh Stock Data</label>
              <button type="button" @click="checkStock" :disabled="!formData.MatName || loadingStock"
                class="btn btn-check">
                <span v-if="loadingStock" class="spinner-border spinner-border-sm me-1"></span>
                {{ loadingStock ? 'Checking...' : 'Check Stock' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Alert Messages -->
        <div v-if="validationErrors.length > 0" class="alert alert-danger">
          <strong>Please fix the following errors:</strong>
          <ul class="mb-0 mt-2">
            <li v-for="error in validationErrors" :key="error">{{ error }}</li>
          </ul>
        </div>

        <div v-if="successMessage" class="alert alert-success">
          <strong>Success!</strong> {{ successMessage }}
        </div>

        <!-- Action Buttons -->
        <div class="action-buttons">
          <button type="submit" :disabled="isSubmitting" class="btn btn-withdraw-primary">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
            {{ isSubmitting ? 'Processing Withdrawal...' : 'เบิกออก (Withdraw)' }}
          </button>

          <button type="button" @click="resetForm" class="btn btn-secondary">
            Reset Form
          </button>

          <button v-if="materials.length === 0" type="button" @click="initializeData" class="btn btn-info">
            Initialize Sample Data
          </button>
        </div>

      </form>
    </div>

    <!-- Recent Withdrawals Section -->
    <div class="withdrawals-section">
      <div class="section-header">
        <h6 class="section-title">Recent Withdrawals History</h6>
        <span class="record-count">{{ recentWithdrawals.length }} records</span>
      </div>

      <div class="withdrawals-content">
        <div v-if="recentWithdrawals.length === 0" class="no-data">
          <div class="no-data-icon">📦</div>
          <p class="mb-0">No withdrawal records found</p>
          <small class="text-muted">Start withdrawing materials to see history</small>
        </div>

        <div v-else class="table-container">
          <table class="withdrawals-table">
            <thead>
              <tr>
                <th>Date/Time</th>
                <th>Material</th>
                <th>Quantity</th>
                <th>Machine</th>
                <th>Cause</th>
                <th>Employee</th>
                <th>Shift</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="withdrawal in recentWithdrawals" :key="withdrawal.id">
                <td class="fw-medium">{{ formatDateTime(withdrawal.timestamp) }}</td>
                <td>
                  <span class="material-badge">{{ withdrawal.MatName }}</span>
                </td>
                <td>
                  <span class="quantity-badge">-{{ withdrawal.quantity }}</span>
                </td>
                <td>
                  <span class="machine-badge">{{ withdrawal.machine }}</span>
                </td>
                <td class="cause-text">{{ withdrawal.cause }}</td>
                <td>
                  <div class="employee-info">
                    <span class="employee-avatar">{{ withdrawal.employee.charAt(0) }}</span>
                    {{ withdrawal.employee }}
                  </div>
                </td>
                <td>
                  <span class="shift-badge shift-{{ withdrawal.shift.toLowerCase() }}">{{ withdrawal.shift }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* === Page Layout === */
.material-withdrawal-page {
  min-height: 100vh;
  background: var(--color-bg);
}

.header-bar {
  background: var(--color-lapping);
  color: #fff;
  padding: var(--space-4) var(--space-6);
  box-shadow: var(--shadow-md);
}

.nav-buttons { display: flex; align-items: center; }

.btn-close {
  width: 28px;
  height: 28px;
  background: rgba(255,255,255,0.15);
  border: 1px solid rgba(255,255,255,0.3);
  border-radius: var(--radius-sm);
  color: #fff;
  font-size: 18px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background var(--transition-fast);
}
.btn-close:hover { background: rgba(255,255,255,0.25); }
.btn-close::before { content: '×'; font-weight: bold; }

/* === Form === */
.form-container {
  padding: var(--space-6);
  max-width: 1200px;
  margin: 0 auto;
}

.page-title {
  text-align: center;
  margin-bottom: var(--space-6);
  padding: var(--space-4);
  background: var(--color-surface);
  border-radius: var(--radius-md);
  border-left: 4px solid var(--color-lapping);
}

.material-form {
  background: var(--color-surface);
  padding: var(--space-6);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
  border: 1px solid var(--color-border);
}

.form-row {
  display: flex;
  gap: var(--space-5);
  margin-bottom: var(--space-5);
  align-items: flex-end;
}

.form-group {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.form-label {
  font-size: var(--text-sm);
  color: var(--color-text);
  margin-bottom: var(--space-1);
  font-weight: 500;
}

.form-control {
  padding: var(--space-2) var(--space-3);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text);
  font-size: var(--text-sm);
  height: 40px;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
}
.form-control:focus {
  outline: none;
  border-color: var(--color-lapping);
  box-shadow: 0 0 0 3px rgba(198, 40, 40, 0.12);
}

.input-group { display: flex; }
.input-group .form-control { border-right: none; border-radius: var(--radius-md) 0 0 var(--radius-md); }
.input-group-text {
  background: var(--color-surface-2);
  border: 1px solid var(--color-border);
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  padding: var(--space-2) var(--space-3);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.form-text { font-size: var(--text-xs); color: var(--color-text-muted); margin-top: var(--space-1); }

/* === Stock Info === */
.stock-info-section {
  background: var(--color-surface-2);
  padding: var(--space-5);
  border-radius: var(--radius-md);
  border: 1px dashed var(--color-border);
  margin: var(--space-6) 0;
}

.stock-display {
  padding: var(--space-3) var(--space-4);
  background: var(--color-success-bg);
  border: 1px solid var(--color-success);
  border-radius: var(--radius-md);
  height: 40px;
  display: flex;
  align-items: center;
}
.stock-value { color: var(--color-success); font-weight: 700; font-size: var(--text-base); }
.stock-placeholder { color: var(--color-text-muted); font-size: var(--text-sm); font-style: italic; }
.material-details {
  margin-top: var(--space-2);
  padding: var(--space-2) var(--space-3);
  background: var(--color-surface);
  border-radius: var(--radius-sm);
  border: 1px solid var(--color-border-light);
  font-size: var(--text-sm);
}

/* === Buttons === */
.btn-check {
  width: 100%;
  padding: var(--space-2) var(--space-4);
  background: var(--color-primary);
  color: #fff;
  border: none;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--text-sm);
  font-weight: 600;
  height: 40px;
  transition: background var(--transition-fast), transform var(--transition-fast);
}
.btn-check:hover:not(:disabled) { background: var(--color-primary-dark); transform: translateY(-1px); }
.btn-check:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }

.action-buttons { display: flex; justify-content: center; gap: var(--space-5); margin: var(--space-8) 0; }

.btn {
  padding: var(--space-3) var(--space-8);
  border: none;
  border-radius: var(--radius-md);
  font-size: var(--text-base);
  font-weight: 600;
  cursor: pointer;
  min-width: 160px;
  transition: all var(--transition-fast);
  display: flex;
  align-items: center;
  justify-content: center;
}
.btn:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-withdraw-primary {
  background: var(--color-lapping);
  color: #fff;
  box-shadow: var(--shadow-sm);
}
.btn-withdraw-primary:hover:not(:disabled) { filter: brightness(0.9); transform: translateY(-1px); box-shadow: var(--shadow-md); }

.btn-secondary { background: var(--color-surface-2); color: var(--color-text); border: 1px solid var(--color-border); }
.btn-secondary:hover:not(:disabled) { background: var(--color-border); }

/* === Alerts === */
.alert { margin: var(--space-5) 0; padding: var(--space-4) var(--space-5); border-radius: var(--radius-md); }
.alert-danger { background: var(--color-error-bg); border-left: 4px solid var(--color-error); color: var(--color-error); }
.alert-success { background: var(--color-success-bg); border-left: 4px solid var(--color-success); color: var(--color-success); }
.alert ul { padding-left: var(--space-5); margin-top: var(--space-2); }

/* === Table Section === */
.withdrawals-section {
  margin-top: var(--space-8);
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  overflow: hidden;
  box-shadow: var(--shadow-md);
  border: 1px solid var(--color-border);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-5) var(--space-6);
  background: var(--color-surface-2);
  border-bottom: 1px solid var(--color-border);
}
.section-title { margin: 0; color: var(--color-text); font-size: var(--text-lg); font-weight: 700; }

.record-count {
  background: var(--color-lapping);
  color: #fff;
  padding: 2px var(--space-3);
  border-radius: var(--radius-full);
  font-size: var(--text-xs);
  font-weight: 600;
}

.withdrawals-content { padding: var(--space-6); }

.no-data { text-align: center; padding: var(--space-10) 0; color: var(--color-text-muted); }
.no-data-icon { font-size: 48px; margin-bottom: var(--space-4); }

.table-container { overflow-x: auto; border-radius: var(--radius-md); }

.withdrawals-table { width: 100%; border-collapse: collapse; background: var(--color-surface); font-size: var(--text-sm); }
.withdrawals-table th {
  background: var(--color-table-header);
  padding: var(--space-3) var(--space-4);
  text-align: left;
  font-weight: 600;
  font-size: var(--text-xs);
  text-transform: uppercase;
  color: var(--color-text);
  border-bottom: 2px solid var(--color-border);
}
.withdrawals-table td { padding: var(--space-3) var(--space-4); border-bottom: 1px solid var(--color-border-light); color: var(--color-text); }
.withdrawals-table tbody tr:hover { background: var(--color-surface-2); }

/* === Badges === */
.material-badge { background: var(--color-primary-light); color: var(--color-primary); padding: 2px var(--space-2); border-radius: var(--radius-full); font-size: var(--text-xs); font-weight: 600; }
.quantity-badge  { background: var(--color-error-bg); color: var(--color-error); padding: 2px var(--space-2); border-radius: var(--radius-full); font-size: var(--text-xs); font-weight: 700; }
.machine-badge   { background: var(--color-warning-bg); color: var(--color-warning); padding: 2px var(--space-2); border-radius: var(--radius-full); font-size: var(--text-xs); font-weight: 600; }
.cause-text      { font-size: var(--text-xs); color: var(--color-text-muted); }

.employee-info { display: flex; align-items: center; gap: var(--space-2); }
.employee-avatar {
  width: 28px; height: 28px;
  background: var(--color-lapping);
  color: #fff;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: var(--text-xs); font-weight: 700;
}

.shift-badge { padding: 2px var(--space-2); border-radius: var(--radius-full); font-size: var(--text-xs); font-weight: 700; }
.shift-a { background: var(--color-info-bg); color: var(--color-info); }
.shift-b { background: var(--color-success-bg); color: var(--color-success); }
.shift-c { background: var(--color-warning-bg); color: var(--color-warning); }

/* === Responsive === */
@media (max-width: 768px) {
  .form-row { flex-direction: column; gap: var(--space-4); }
  .action-buttons { flex-direction: column; align-items: center; }
  .btn { width: 100%; max-width: 300px; }
  .section-header { flex-direction: column; gap: var(--space-2); text-align: center; }
}

@media (max-width: 480px) {
  .form-container, .withdrawals-content { padding: var(--space-4); }
  .header-bar { padding: var(--space-3) var(--space-4); }
  .material-form { padding: var(--space-4); }
}
</style>
